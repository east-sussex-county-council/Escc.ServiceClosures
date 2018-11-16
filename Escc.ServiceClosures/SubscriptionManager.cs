using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.XPath;
using Escc.Dates;

namespace Escc.ServiceClosures
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
        /// <param name="reportClosureUrl">The report closure URL.</param>
        /// <param name="unsubscribeUrl">The unsubscribe URL, including a {0} token for the subscription code.</param>
        /// <param name="closureEmailTemplateFile">The closure email template file.</param>
        /// <param name="subscriptions">The subscriptions.</param>
        /// <exception cref="ArgumentNullException">
        /// service
        /// or
        /// closure
        /// or
        /// closureEmailTemplateFile
        /// or
        /// subscriptions
        /// </exception>
        public static void SendEmailNotifications(Service service, Closure closure, Uri reportClosureUrl, Uri unsubscribeUrl, string closureEmailTemplateFile, Collection<Subscription> subscriptions)
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
            var standardFrom = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/From")?.Value;
            var standardBcc = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/Bcc")?.Value;
            var standardSubject = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/Subject")?.Value;
            var standardBody = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='false']/Body")?.Value;
            standardSubject = InsertDataIntoTemplate(service, closure,reportClosureUrl, standardSubject);
            standardBody = InsertDataIntoTemplate(service, closure, reportClosureUrl, standardBody);

            var officialFrom = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/From")?.Value;
            var officialBcc = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/Bcc")?.Value;
            var officialSubject = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/Subject")?.Value;
            var officialBody = templateNavigator.SelectSingleNode("/EmailTemplate/Email[@OfficialNotification='true']/Body")?.Value;
            officialSubject = InsertDataIntoTemplate(service, closure, reportClosureUrl, officialSubject);
            officialBody = InsertDataIntoTemplate(service, closure, reportClosureUrl, officialBody);

            List<string> addressesAdded = new List<string>();
            using (SmtpClient smtp = new SmtpClient())
            {
                foreach (Subscription sub in subscriptions)
                {
                    if (sub.Type == SubscriptionType.Email && !addressesAdded.Contains(sub.Address))
                    {
                        using (MailMessage email = new MailMessage())
                        {
                            email.IsBodyHtml = true;
                            var from = (sub.OfficialNotification ? officialFrom : standardFrom);
                            if (!String.IsNullOrEmpty(from)) {
                                email.From = new MailAddress(from);
                            }
                            email.To.Add(new MailAddress(sub.Address));

                            string addBcc = sub.OfficialNotification ? officialBcc : standardBcc;
                            // Do we have a blind copy recipient?
                            if (!string.IsNullOrEmpty(addBcc))
                            {
                                // Yes, then blind copy them in
                                email.Bcc.Add(new MailAddress(addBcc));
                            }

                            // Track which ones are done so we don't send two emails to the same address if they happen to
                            // have a global and a individual service subscription
                            addressesAdded.Add(sub.Address);

                            email.Subject = sub.OfficialNotification ? officialSubject : standardSubject;
                            email.Body = sub.OfficialNotification ? officialBody : standardBody;

                            email.Body = InsertPersonalisedDataIntoTemplate(service, sub, unsubscribeUrl, email.Body);

                            // Send the email
                            smtp.Send(email);
                        }
                    }
                }
            }
        }

        private static string InsertPersonalisedDataIntoTemplate(Service service, Subscription sub, Uri unsubscribeUrl, string templateText)
        {
            if (!String.IsNullOrEmpty(templateText))
            {
                // Add the personalised unsubscribe link
                if (sub.Code != null && sub.Code != new Guid())
                {
                    string individualUrl = String.Format(unsubscribeUrl.ToString(), sub.Code.ToString());
                    templateText = templateText.Replace("{Unsubscribe}", individualUrl);
                }
                if (service.Url != null)
                {
                    templateText = templateText.Replace("{ServiceUrl}", service.Url.ToString());
                }
            }
            return templateText;
        }

        /// <summary>
        /// Inserts the closure data into the email template.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <param name="closure">The closure.</param>
        /// <param name="reportClosureUrl">The report closure URL.</param>
        /// <param name="templateText">The template text.</param>
        /// <returns></returns>
        private static string InsertDataIntoTemplate(Service service, Closure closure, Uri reportClosureUrl, string templateText)
        {
            if (!String.IsNullOrEmpty(templateText))
            {
                string dateText = closure.StartDate.ToBritishDateRangeFromThisDateUntil(closure.EndDate, false, false);
                string notes = String.IsNullOrEmpty(closure.Notes) ? "None" : closure.Notes;
                string status = Regex.Replace(closure.Status.ToString(), "([A-Z])", " $1").TrimStart();

                templateText = templateText.Replace("{AddedBy}", closure.AddedBy);
                templateText = templateText.Replace("{Service}", service.Name);
                templateText = templateText.Replace("{ServiceCode}", service.Code);
                templateText = templateText.Replace("{Status}", status.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) + status.Substring(1).ToLower(CultureInfo.CurrentCulture));
                templateText = templateText.Replace("{Date}", dateText);
                templateText = templateText.Replace("{Reason}", closure.Reason.Reason);
                templateText = templateText.Replace("{Notes}", HttpUtility.HtmlEncode(notes).Replace(Environment.NewLine + Environment.NewLine, "<br />"));
                if (reportClosureUrl != null)
                {
                    templateText = templateText.Replace("{ReportClosureUrl}", reportClosureUrl.ToString());
                }

                // Remove any trailing punctuation from missing info
                templateText.TrimEnd(' ', ',', ':');
            }
            return templateText;
        }

    }
}
