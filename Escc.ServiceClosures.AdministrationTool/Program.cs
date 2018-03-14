using System;
using System.Collections.ObjectModel;
using proxy = Escc.ServiceClosures.AdministrationTool.SchoolsInformationService;
using Escc.Net;
using Exceptionless;
using log4net;
using log4net.Config;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Admin tool to manage service closures
    /// </summary>
    class Program
    {
        private static Collection<Service> closedServices = null;
        private static Collection<Moderator> moderators = null;
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// Main entry point for the programme
        /// </summary>
        /// <param name="args">The args.</param>
        static void Main(string[] args)
        {
            ExceptionlessClient.Default.Startup();
            XmlConfigurator.Configure();

            try
            {
                if (args.Length != 2)
                {
                    ShowHelp();
                    return;
                }

                switch (args[0])
                {
                    case "/pulldata":
                        // Switch statement is just future-proofing, should be more types of service in future    
                        switch (args[1])
                        {
                            case "school":
                                PullSchoolData();
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
                ex.ToExceptionless().Submit();

                log.Error(ex.Message);
            }
        }




        #region Schools

        /// <summary>
        /// Gets the data for school overdue confirmations.
        /// </summary>
        /// <remarks>Closed services and moderators are populated using static vars because when passed as parameters
        /// the values were wiped by garbage collector when this method finished, rather than being passed back out again.</remarks>
        private static void GetDataForSchoolOverdueConfirmations()
        {
            using (SchoolsInformationService.SchoolsInformationWebService webService = new SchoolsInformationService.SchoolsInformationWebService())
            {
                webService.Proxy = new ConfigurationProxyProvider().CreateProxy();
                webService.Credentials = new ConfigurationWebApiCredentialsProvider().CreateCredentials();
                string sidNamespace = "http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/";

                proxy.Service[] proxyServices = webService.ClosureList(true);
                var proxyServiceConverter = new WebServiceProxyConverter<Service[], proxy.Service[]>(sidNamespace);
                closedServices = new Collection<Service>(proxyServiceConverter.ConvertProxyToOriginalType(proxyServices));

                proxy.Moderator[] proxyModerators = webService.ClosureModeratorsList();
                var proxyModeratorConverter = new WebServiceProxyConverter<Moderator[], proxy.Moderator[]>(sidNamespace);
                moderators = new Collection<Moderator>(proxyModeratorConverter.ConvertProxyToOriginalType(proxyModerators));

                var message = String.Format("Data for overdue school closures retrieved using {0}", webService.Url);
                log.InfoFormat(message);
            }
        }


        /// <summary>
        /// Gets the school closure data from SID and saves it to a file
        /// </summary>
        private static void PullSchoolData()
        {
            var dataSource = new SchoolWebServiceDataSource(new ConfigurationWebApiCredentialsProvider(), new ConfigurationProxyProvider(), log);
            var closureInfo = dataSource.ReadClosureInfo(new ServiceType("school"));

            var repository = new FileRepository(log);
            repository.SaveClosureInfo(new ServiceType("school"), closureInfo);
            log.InfoFormat("School data refreshed using {0}", dataSource.GetType().ToString());
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
                log.InfoFormat("Notified moderators about {0} overdue {1} closures", overdueClosures, serviceType.SingularText);
            }
            else
            {
                log.InfoFormat("No overdue {0} closures found", serviceType.SingularText);
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
            Console.WriteLine("Escc.ServiceClosures.AdministrationTool /pulldata ServiceType");
            Console.WriteLine("or");
            Console.WriteLine("Escc.ServiceClosures.AdministrationTool /overdueconfirmations ServiceType");
            Console.WriteLine();
            Console.WriteLine("where ServiceType is one of the following options: school");
            Console.WriteLine();
        }
    }
}
