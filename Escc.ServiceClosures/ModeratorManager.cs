using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Escc.Dates;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Manage moderators of closure information for services, and send notifications to them
    /// </summary>
    public static class ModeratorManager
    {
        /// <summary>
        /// Sends notifications to moderators about multi-day emergency closures which should have been confirmed, but haven't been
        /// </summary>
        /// <param name="serviceType">Type of the services.</param>
        /// <param name="services">Services which may include overdue closures.</param>
        /// <param name="moderators">The moderators.</param>
        /// <returns>Number of overdue closures found</returns>
        public static int NotifyAboutOverdueConfirmations(ServiceType serviceType, Collection<Service> services, Collection<Moderator> moderators)
        {
            // The supplied services may have closures which are not overdue, so first job is to get
            // a collection of just the overdue closures
            List<Service> overdueClosures = new List<Service>();

            if (services != null)
            {
                foreach (Service service in services)
                {
                    foreach (Closure closure in service.Closures)
                    {
                        if (closure.ConfirmationOverdue)
                        {
                            Service closedService = new Service();
                            closedService.Name = service.Name;
                            closedService.Closures.Add(closure);
                            overdueClosures.Add(closedService);
                        }
                    }
                }
            }

            // Have we got some overdue closures, and someone to tell about them?
            if (overdueClosures.Count > 0 && moderators.Count > 0)
            {
                MailMessage email = new MailMessage();
                foreach (Moderator mod in moderators)
                {
                    email.To.Add(new MailAddress(mod.EmailAddress.EmailAddress));
                }
                email.From = new MailAddress("do-not-reply@eastsussex.gov.uk");
                email.Subject = String.Format("Confirmation overdue for {0} multi-day emergency {1} closures", overdueClosures.Count.ToString(), serviceType.SingularText);

                email.Body = String.Format("The following multi-day emergency {0} closures have not been confirmed today:{1}{1}", serviceType.SingularText, Environment.NewLine);
                foreach (Service closedService in overdueClosures)
                {
                    email.Body += String.Format("{0} {1} due to {2}, {3}{4}{4}", closedService.Name, Regex.Replace(closedService.Closures[0].Status.ToString(), "([A-Z])", " $1").TrimStart().ToLower(CultureInfo.CurrentCulture), closedService.Closures[0].Reason.Reason, closedService.Closures[0].StartDate.ToBritishDateRangeFromThisDateUntil(closedService.Closures[0].EndDate, false, false), Environment.NewLine);
                }
                email.Body += String.Format("++ Disclaimer{0}{0}", Environment.NewLine);
                SmtpClient client = new SmtpClient();
                client.Send(email);
            }

            return overdueClosures.Count;
        }

    }
}
