using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Saves closure data to a file
    /// </summary>
    /// <seealso cref="Escc.ServiceClosures.IServiceClosureRepository" />
    public class FileRepository : IServiceClosureRepository
    {
        private readonly ILog _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRepository"/> class.
        /// </summary>
        /// <param name="log">The log.</param>
        public FileRepository(ILog log)
        {
            _log = log;
        }

        /// <summary>
        /// Saves the closure information to the repository
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="closureInfo">The closure information.</param>
        public void SaveClosureInfo(ServiceType serviceType, ClosureInfo closureInfo)
        {
            string filename = XPathClosureData.XmlPath(new ServiceType("school"));
            string tempFile = filename + "." + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".xml";

            // Serialise closures to xml as UTF8
            MemoryStream memoryStream = new MemoryStream();
            _log?.Info("Creating serialiser");
            XmlSerializer xs = new XmlSerializer(typeof(ClosureInfo), "http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/");
            _log?.Info("Creating " + tempFile);
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(tempFile, Encoding.UTF8))
            {
                _log?.Info("Serialising data to " + tempFile);
                xs.Serialize(xmlTextWriter, closureInfo);
            }

            // Overwrite the current data
            _log?.Info("Writing data to file");
            OverwriteXml(filename, tempFile, 5);

        }


        /// <summary>
        /// Overwrites the XML.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="tempFile">The temp file.</param>
        /// <param name="attemptsLeft">The number of attempts left.</param>
        private void OverwriteXml(string filename, string tempFile, int attemptsLeft)
        {
            try
            {
                File.Copy(tempFile, filename, true);
                File.Delete(tempFile);
            }
            catch (IOException)
            {
                // Often when it tries to update the XML there's an IOException, because a web page
                // is in the process of reading the file. All web pages are cached though, so if we
                // keep retrying we should be able to write it. Limited number of attempts though to
                // avoid any risk of an infinite loop, or a conflict with the next scheduled run of this tool.
                if (attemptsLeft > 0)
                {
                    attemptsLeft--;
                    _log?.Info("Unable to write file, trying again in 10 seconds");
                    Thread.Sleep(10000); // wait 10 secs to allow file to become available
                    OverwriteXml(filename, tempFile, attemptsLeft);
                }
                else
                {
                    _log?.Error("Unable to write file, giving up");

                    // Don't leave temp file hanging around
                    File.Delete(tempFile);

                    // Re-throw exception, which will be caught and reported outside this method
                    throw;
                }
            }
        }

    }
}
