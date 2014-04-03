using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using eastsussexgovuk.webservices.TextXhtml.HouseStyle;

namespace EsccWebTeam.ServiceClosures
{
    /// <summary>
    /// Manage and process subscriptions to closure information for services
    /// </summary>
    public static class SubscriptionManager
    {
        /// <summary>
        /// Sends notifications to people who have subscribed to emails
        /// </summary>
        /// <param name="service">The service which is subject to closure.</param>
        /// <param name="closure">The closure to notify subscribers about.</param>
        /// <param name="closureEmailTemplateFile">The closure email template file.</param>
        /// <param name="subscriptions">The subscriptions.</param>
        /// <param name="unsubscribeUrl">The unsubscribe URL, including a {0} token for the subscription code.</param>
        public static void SendEmailNotifications(Service service, Closure closure, string closureEmailTemplateFile, Collection<Subscription> subscriptions, Uri unsubscribeUrl)
        {
            // All parameters are required
            if (service == null) throw new ArgumentNullException("service");
            if (closure == null) throw new ArgumentNullException("closure");
            if (closureEmailTemplateFile == null) throw new ArgumentNullException("closureEmailTemplateFile");
            if (subscriptions == null) throw new ArgumentNullException("subscriptions");

            // Get the closure template from the specified file - allow exceptions to be 
            // thrown to calling method which was responsible for specifying this file
            XPathDocument template = new XPathDocument(closureEmailTemplateFile);
            XPathNavigator templateNavigator = template.CreateNavigator();

            // Create an email for each subscriber, because it contains a unique unsubscribe link
            // Set subject and body, inserting details of closure
            MailAddress standardFrom = new MailAddress(templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/From").Value);
            string standardSubject = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/Subject").Value;
            string standardBody = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/Body").Value;
            standardSubject = InsertDataIntoTemplate(service, closure, standardSubject);
            standardBody = InsertDataIntoTemplate(service, closure, standardBody);

            MailAddress officialFrom = new MailAddress(templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/From").Value);
            string officialSubject = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/Subject").Value;
            string officialBody = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/Body").Value;
            officialSubject = InsertDataIntoTemplate(service, closure, officialSubject);
            officialBody = InsertDataIntoTemplate(service, closure, officialBody);

            List<string> addressesAdded = new List<string>();
            foreach (Subscription sub in subscriptions)
            {
                if (sub.Type == SubscriptionType.Email && !addressesAdded.Contains(sub.Address))
                {
                    MailMessage email = new MailMessage();
                    email.From = sub.OfficialNotification ? officialFrom : standardFrom;
                    email.To.Add(new MailAddress(sub.Address));

                    // Track which ones are done so we don't send two emails to the same address if they happen to
                    // have a global and a individual service subscription
                    addressesAdded.Add(sub.Address);

                    email.Subject = sub.OfficialNotification ? officialSubject : standardSubject;
                    email.Body = sub.OfficialNotification ? officialBody : standardBody;

                    // Add the personalised unsubscribe link
                    if (sub.Code != null && sub.Code != new Guid())
                    {
                        string individualUrl = String.Format(unsubscribeUrl.ToString(), sub.Code.ToString());
                        email.Body +=
                            String.Format("{1}++ Unsubscribe{1}{1}To stop receiving emails like this one, click on the link below{1}" +
                            "or paste it into your browser's address bar.{1}{1}" +
                            "{0}{1}{1}", individualUrl, Environment.NewLine);

                    }

                    // All ESCC emails get a standard disclaimer, so add some spacing before it
                    email.Body += String.Format("{0}{0}++ Disclaimer", Environment.NewLine);

                    // Send the email
                    SmtpClient smtp = new SmtpClient();
                    smtp.Send(email);
                }
            }
        }

        /// <summary>
        /// Inserts the closure data into the email template.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="closure">The closure.</param>
        /// <param name="templateText">The template text.</param>
        /// <returns></returns>
        private static string InsertDataIntoTemplate(Service service, Closure closure, string templateText)
        {
            string dateText = DateTimeFormatter.DateRange(closure.StartDate, closure.EndDate, false, false);
            string notes = String.IsNullOrEmpty(closure.Notes) ? "None" : closure.Notes;

            templateText = templateText.Replace("{Service}", service.Name);
            templateText = templateText.Replace("{ServiceCode}", service.Code);
            templateText = templateText.Replace("{Status}", Regex.Replace(closure.Status.ToString(), "([A-Z])", " $1").TrimStart().ToLower(CultureInfo.CurrentCulture));
            templateText = templateText.Replace("{Date}", dateText);
            templateText = templateText.Replace("{Reason}", closure.Reason.Reason);
            templateText = templateText.Replace("{Notes}", notes);

            // Remove any trailing punctuation from missing info
            templateText.TrimEnd(' ', ',', ':');

            return templateText;
        }

    }
}
