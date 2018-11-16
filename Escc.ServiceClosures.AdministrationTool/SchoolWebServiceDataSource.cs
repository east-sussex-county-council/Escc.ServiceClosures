using Escc.Net;
using log4net;
using System;
using proxy = Escc.ServiceClosures.AdministrationTool.SchoolsInformationService;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Reads closure data from the school closures web service
    /// </summary>
    /// <seealso cref="Escc.ServiceClosures.IServiceClosureInfoDataSource" />
    public class SchoolWebServiceDataSource : IServiceClosureInfoDataSource
    {
        private readonly IWebApiCredentialsProvider _credentialsProvider;
        private readonly IProxyProvider _proxyProvider;
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchoolWebServiceDataSource" /> class.
        /// </summary>
        /// <param name="credentialsProvider">The credentials provider.</param>
        /// <param name="proxyProvider">The proxy provider.</param>
        /// <param name="log">The log.</param>
        public SchoolWebServiceDataSource(IWebApiCredentialsProvider credentialsProvider, IProxyProvider proxyProvider, ILog log)
        {
            _credentialsProvider = credentialsProvider;
            _proxyProvider = proxyProvider;
            _log = log;
        }
        /// <summary>
        /// Reads the closure information for a given service type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public ClosureInfo ReadClosureInfo(ServiceType serviceType)
        {
            using (SchoolsInformationService.SchoolsInformationWebService webService = new SchoolsInformationService.SchoolsInformationWebService())
            {
                _log?.Info("Requesting data using " + webService.Url);
                webService.Proxy = _proxyProvider?.CreateProxy();
                webService.Credentials = _credentialsProvider?.CreateCredentials();

                var proxyObject = webService.ClosureInfoAllSchools();
                var converter = new WebServiceProxyConverter<ClosureInfo, proxy.ClosureInfo>("http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/");
                return converter.ConvertProxyToOriginalType(proxyObject);
            }
        }
    }
}
