using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Escc.ServiceClosures;
using EsccWebTeam.Data.Xml;
using Microsoft.ApplicationBlocks.ExceptionManagement;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using proxy = EsccWebTeam.ServiceClosures.AdministrationTool.SchoolsInformationService;

namespace EsccWebTeam.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Admin tool to manage service closures
    /// </summary>
    /// <remarks>
    /// This should all be part of EsccWebTeam.ServiceClosures.AdministrationTool project, which is why the namespace has been left as that.
    /// This tool is the original app built against .NET 2.0. The new one uses .NET 3.5 to get access to the Twitterizer library, but this has to
    /// remain as .NET 2.0 so it can run on the public website.
    /// </remarks>
    class Program
    {
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
        /// Gets the school closure data from SID and saves it to a file
        /// </summary>
        private static void PullSchoolData()
        {
            using (SchoolsInformationService.SchoolsInformationWebService webService = new SchoolsInformationService.SchoolsInformationWebService())
            {
                webService.Proxy = XmlHttpRequest.CreateProxy();
                webService.Credentials = System.Net.CredentialCache.DefaultCredentials;
                string sidNamespace = "http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/";

                string filename = XPathClosureData.XmlPath(new ServiceType("school"));
                string tempFile = filename + "." + DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day + "-" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + "-" + DateTime.Now.Millisecond + ".xml";

                // Serialise closures to xml as UTF8
                Console.WriteLine("Requesting data using " + webService.Url);
                proxy.ClosureInfo dataToSerialise = webService.ClosureInfoAllSchools();
                MemoryStream memoryStream = new MemoryStream();
                Console.WriteLine("Creating serialiser");
                XmlSerializer xs = new XmlSerializer(typeof(proxy.ClosureInfo), sidNamespace);
                Console.WriteLine("Creating " + tempFile);
                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(tempFile, Encoding.UTF8))
                {
                    Console.WriteLine("Serialising data to " + tempFile);
                    xs.Serialize(xmlTextWriter, dataToSerialise);
                }

                // Overwrite the current data
                Console.WriteLine("Writing data to file");
                OverwriteXml(filename, tempFile, 5);

                // Log success
                LogEntry entry = new LogEntry();
                entry.Severity = TraceEventType.Information;
                entry.Message = String.Format("School data refreshed using {0}", webService.Url);
                Logger.Write(entry);

                Console.WriteLine(entry.Message);
            }
        }
        #endregion

        /// <summary>
        /// Overwrites the XML.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="tempFile">The temp file.</param>
        /// <param name="attemptsLeft">The number of attempts left.</param>
        private static void OverwriteXml(string filename, string tempFile, int attemptsLeft)
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
                    Console.WriteLine("Unable to write file, trying again in 10 seconds");
                    Thread.Sleep(10000); // wait 10 secs to allow file to become available
                    OverwriteXml(filename, tempFile, attemptsLeft);
                }
                else
                {
                    Console.WriteLine("Unable to write file, giving up");

                    // Don't leave temp file hanging around
                    File.Delete(tempFile);

                    // Re-throw exception, which will be caught and reported outside this method
                    throw;
                }
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
            Console.WriteLine("EsccWebTeam.ServiceClosures.AdministrationTool /pulldata ServiceType");
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
