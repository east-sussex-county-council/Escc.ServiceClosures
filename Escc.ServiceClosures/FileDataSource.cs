using Escc.ServiceClosures.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Reads closure data from an XML file
    /// </summary>
    /// <seealso cref="Escc.ServiceClosures.IServiceClosureDataSource" />
    public class FileDataSource : IServiceClosureDataSource
    {
        /// <summary>
        /// Reads the closure data.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public IServiceClosureData ReadClosureData(ServiceType serviceType)
        {
            return new XPathClosureData(XmlPath(serviceType));
        }


        /// <summary>
        /// Gets the path to the XML file containing the closure data for a given type of service.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">Thrown if serviceType is null</exception>
        /// <exception cref="ArgumentException">Thrown if serviceType.SingularText is not set</exception>
        public static string XmlPath(ServiceType serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException("serviceType");
            if (String.IsNullOrEmpty(serviceType.SingularText)) throw new ArgumentException("The SingularText property must be set", "serviceType");
            if (String.IsNullOrEmpty(Settings.Default.XmlFolder)) throw new ConfigurationErrorsException("The XmlFolder configuration setting must be set");

            string filename = Regex.Replace(serviceType.SingularText, "[^A-Za-z0-9]", String.Empty) + ".xml";

            // Get folder from settings. Support original format of UNC path, and recommended format of app-relative URL which resolves to local path.
            string folder = Settings.Default.XmlFolder;
            if (!Path.IsPathRooted(folder))
            {
                folder = HostingEnvironment.MapPath(folder);
            }

            // Convert service type to filename
            filename = folder.TrimEnd('\\') + "\\" + filename;
            return filename;
        }
    }
}
