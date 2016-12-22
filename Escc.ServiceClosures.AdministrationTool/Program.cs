using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using eastsussexgovuk.webservices.TextXhtml.HouseStyle;
using Escc.ServiceClosures;
using EsccWebTeam.Data.Xml;
using EsccWebTeam.ServiceClosures.AdministrationTool.Properties;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Twitterizer;
using proxy = EsccWebTeam.ServiceClosures.AdministrationTool.SchoolsInformationService;

namespace EsccWebTeam.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Admin tool to manage service closures
    /// </summary>
    /// <remarks>
    /// This should have all the code from EsccWebTeam.ServiceClosures.AdministrationToolBasic project, which is why the namespaces are the same.
    /// This tool uses .NET 3.5 to get access to the Twitterizer library, but the old application has to remain as .NET 2.0 so it can run on the 
    /// public website.
    /// </remarks>
    class Program
    {
        private static Collection<Service> closedServices = null;
        private static Collection<Moderator> moderators = null;

        /// <summary>
        /// Main entry point for the programme
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 2)
                {
                    ShowHelp();
                    return;
                }

                switch (args[0])
                {
                    case "/tweet":

                        // Switch statement is just future-proofing, should be more types of service in future    
                        switch (args[1])
                        {
                            case "school":
                                TweetSchoolClosures();
                                break;

                            default:
                                ShowHelp();
                                break;
                        }
                        break;

                    case "/overdueconfirmations":

                        // Check whether any multi-day emergency closures have not been confirmed when they should be

                        // Switch statement is just future-proofing, should be more types of service in future    
                        switch (args[1])
                        {
                            case "school":
                                GetDataForSchoolOverdueConfirmations();
                                break;

                            default:
                                ShowHelp();
                                break;
                        }

                        NotifyAboutOverdueConfirmations(new ServiceType(args[1]), closedServices, moderators);

                        break;

                    default:
                        ShowHelp();
                        return;
                }
            }
            catch (Exception ex)
            {
                ExceptionManager.Publish(ex);

                LogEntry entry = new LogEntry();
                entry.Severity = TraceEventType.Error;
                entry.Message = String.Format(ex.Message);
                Logger.Write(entry);

                Console.WriteLine(entry.Message);
            }
        }




        #region Schools

        /// <summary>
        /// Tweets the school closures.
        /// </summary>
        private static void TweetSchoolClosures()
        {
            using (SchoolsInformationService.SchoolsInformationWebService webService = new SchoolsInformationService.SchoolsInformationWebService())
            {
                webService.Proxy = XmlHttpRequest.CreateProxy();
                webService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                proxy.Service[] schools = webService.ClosuresNotTweeted();

                foreach (proxy.Service school in schools)
                {
                    foreach (proxy.Closure closure in school.Closures)
                    {
                        // Abbreviate the school name where it'll still be easily recognisable
                        string schoolName = school.Name.Replace(" Church of England", String.Empty).Replace(" Community Primary", " Primary");
                        if (schoolName.Contains("Primary") || schoolName.Contains("Infant") || schoolName.Contains("Junior")) schoolName = schoolName.Replace(" School", String.Empty);

                        // Tweet school name, closed or open, and date
                        string tweet = schoolName + " " + Regex.Replace(closure.Status.ToString(), "([A-Z])", " $1").TrimStart().ToLower(CultureInfo.CurrentCulture) + " ";
                        if (closure.StartDate == closure.EndDate)
                        {
                            tweet += DateTimeFormatter.ShortBritishDate(closure.StartDate).Replace(" " + DateTime.Now.Year, String.Empty);
                        }
                        else
                        {
                            tweet += DateTimeFormatter.DateRange(closure.StartDate, closure.EndDate, false, false, true).Replace(" " + DateTime.Now.Year, String.Empty);
                        }

                        // Add reason and notes if room, or link to www.eastsussex.gov.uk if not
                        int reasonLength = 0;
                        if (closure.Reason.Reason.Length > 0) reasonLength = closure.Reason.Reason.Length + 2; // include the leading space
                        int notesLength = 0;
                        if (closure.Notes.Length > 0) notesLength = closure.Notes.Length + 2;

                        if (tweet.Length + reasonLength + notesLength <= 140)
                        {
                            // Safe to include all the info we have in the tweet
                            if (reasonLength > 0) tweet += ". " + closure.Reason.Reason;
                            if (notesLength > 0) tweet += ". " + closure.Notes;
                        }
                        else
                        {
                            // We'll have to miss something out, so squeeze in what we can and link to closures page
                            string moreInfo = ". More info http://bit.ly/7OF7d8";
                            if ((tweet.Length + reasonLength + moreInfo.Length) <= 140) tweet += " " + closure.Reason.Reason;
                            if ((tweet.Length + notesLength + moreInfo.Length) <= 140) tweet += " " + closure.Notes;
                            tweet += moreInfo;
                        }

                        // Get Twitter account
                        OAuthTokens twitterTokens = new OAuthTokens();
                        twitterTokens.ConsumerKey = Settings.Default.SchoolTwitterConsumerKey;
                        twitterTokens.ConsumerSecret = Settings.Default.SchoolTwitterConsumerSecret;
                        twitterTokens.AccessToken = Settings.Default.SchoolTwitterAccessToken;
                        twitterTokens.AccessTokenSecret = Settings.Default.SchoolTwitterAccessTokenSecret;

                        StatusUpdateOptions opt = new StatusUpdateOptions();
                        IWebProxy proxy = XmlHttpRequest.CreateProxy();
                        if (proxy != null)
                        {
                            opt.Proxy = new WebProxy();
                            opt.Proxy.Credentials = proxy.Credentials;
                            opt.Proxy.Address = proxy.GetProxy(new Uri("http://twitter.com"));
                        }

                        // Post tweet
                        Console.Write("Tweeting '" + tweet + "'...");

                        try
                        {
                            TwitterStatus result = TwitterStatus.Update(twitterTokens, tweet, opt);
                            if (result.Id > 0)
                            {
                                // Successful tweet, so mark as done in database
                                webService.ClosureTweeted(closure.Id, String.Format("http://twitter.com/{0}/status/{1}", result.User.ScreenName, result.Id));

                                Console.WriteLine("Success");

                                LogEntry entry = new LogEntry();
                                entry.Severity = TraceEventType.Information;
                                entry.Message = String.Format("Tweeted '" + tweet + "'");
                                Logger.Write(entry);
                            }
                            else
                            {
                                Console.WriteLine("Failed." + result.RequestStatus.ErrorDetails.ErrorMessage);

                                LogEntry entry = new LogEntry();
                                entry.Severity = TraceEventType.Error;
                                entry.Message = String.Format("Failed to tweet '" + tweet + "'. " + result.RequestStatus.ErrorDetails.ErrorMessage);
                                Logger.Write(entry);

                                // Publish exception, but don't stop processing because need to try to get message out about every closure
                                ExceptionManager.Publish(new Exception("Failed to tweet '" + tweet + "'. " + result.RequestStatus.ErrorDetails.ErrorMessage));
                            }
                        }
                        catch (TwitterizerException ex)
                        {
                            ExceptionManager.Publish(ex);

                            LogEntry entry = new LogEntry();
                            entry.Severity = TraceEventType.Error;
                            entry.Message = String.Format(ex.ErrorDetails != null ? ex.ErrorDetails.ErrorMessage : ex.Message);
                            Logger.Write(entry);

                            Console.WriteLine(entry.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the data for school overdue confirmations.
        /// </summary>
        /// <remarks>Closed services and moderators are populated using static vars because when passed as parameters
        /// the values were wiped by garbage collector when this method finished, rather than being passed back out again.</remarks>
        private static void GetDataForSchoolOverdueConfirmations()
        {
            using (SchoolsInformationService.SchoolsInformationWebService webService = new SchoolsInformationService.SchoolsInformationWebService())
            {
                webService.Proxy = XmlHttpRequest.CreateProxy();
                webService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                string sidNamespace = "http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/";

                proxy.Service[] proxyServices = webService.ClosureList(true);
                WebServiceHelper<Service[], proxy.Service[]> proxyServiceConverter = new WebServiceHelper<Service[], proxy.Service[]>(sidNamespace);
                closedServices = new Collection<Service>(proxyServiceConverter.ConvertProxyToOriginalType(proxyServices));

                proxy.Moderator[] proxyModerators = webService.ClosureModeratorsList();
                WebServiceHelper<Moderator[], proxy.Moderator[]> proxyModeratorConverter = new WebServiceHelper<Moderator[], proxy.Moderator[]>(sidNamespace);
                moderators = new Collection<Moderator>(proxyModeratorConverter.ConvertProxyToOriginalType(proxyModerators));

                LogEntry entry = new LogEntry();
                entry.Severity = TraceEventType.Information;
                entry.Message = String.Format("Data for overdue school closures retrieved using {0}", webService.Url);
                Logger.Write(entry);

                Console.WriteLine(entry.Message);
            }
        }
        #endregion

        /// <summary>
        /// Notifies moderators about overdue confirmations for multi-day closures
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="closedServices">The closed services.</param>
        /// <param name="moderators">The moderators.</param>
        private static void NotifyAboutOverdueConfirmations(ServiceType serviceType, Collection<Service> closedServices, Collection<Moderator> moderators)
        {
            int overdueClosures = ModeratorManager.NotifyAboutOverdueConfirmations(serviceType, closedServices, moderators);
            if (overdueClosures > 0)
            {
                LogEntry entry = new LogEntry();
                entry.Severity = TraceEventType.Information;
                entry.Message = String.Format("Notified moderators about {0} overdue {1} closures", overdueClosures, serviceType.SingularText);
                Logger.Write(entry);

                Console.WriteLine(entry.Message);
            }
            else
            {
                LogEntry entry = new LogEntry();
                entry.Severity = TraceEventType.Information;
                entry.Message = String.Format("No overdue {0} closures found", serviceType.SingularText);
                Logger.Write(entry);

                Console.WriteLine(entry.Message);
            }
        }


        /// <summary>
        /// Shows the help.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("EsccWebTeam.ServiceClosures.AdministrationTool /tweet ServiceType");
            Console.WriteLine("or");
            Console.WriteLine("EsccWebTeam.ServiceClosures.AdministrationTool /overdueconfirmations ServiceType");
            Console.WriteLine();
            Console.WriteLine("where ServiceType is one of the following options: school");
            Console.WriteLine();

            LogEntry entry = new LogEntry();
            entry.Severity = TraceEventType.Error;
            entry.Message = "Application started with invalid arguments. Help text shown.";
            Logger.Write(entry);
        }
    }
}
