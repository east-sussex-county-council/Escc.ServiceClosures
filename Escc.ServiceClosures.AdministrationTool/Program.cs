using System;
using System.Collections.ObjectModel;
using Escc.Net;
using Exceptionless;
using log4net;
using log4net.Config;
using System.Configuration;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Admin tool to manage service closures
    /// </summary>
    class Program
    {
        private static Collection<Service> closedServices = null;
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
        /// Gets the school closure data from SID and saves it to a file
        /// </summary>
        private static void PullSchoolData()
        {
            var dataSource = new SchoolWebServiceDataSource(new ConfigurationWebApiCredentialsProvider(), new ConfigurationProxyProvider(), log);
            var closureInfo = dataSource.ReadClosureInfo(new ServiceType("school"));

            log.Info("Saving school data to Azure blob storage");
            var repository = new AzureBlobStorageRepository(ConfigurationManager.ConnectionStrings["Escc.ServiceClosures.AzureStorage"].ConnectionString, "service-closures");
            repository.SaveClosureInfo(new ServiceType("school"), closureInfo);
            log.InfoFormat("School data refreshed using {0}", dataSource.GetType().ToString());
        }
        #endregion

        /// <summary>
        /// Shows the help.
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();
            Console.WriteLine("Escc.ServiceClosures.AdministrationTool /pulldata ServiceType");
            Console.WriteLine();
            Console.WriteLine("where ServiceType is one of the following options: school");
            Console.WriteLine();
        }
    }
}
