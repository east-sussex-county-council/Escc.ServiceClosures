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
    static class Program
    {
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
                PullSchoolData();
            }
            catch (Exception ex)
            {
                ex.ToExceptionless().Submit();
                log.Error(ex.Message);
                throw;
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
    }
}
