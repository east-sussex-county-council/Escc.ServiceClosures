using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using Escc.ServiceClosures.Properties;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Information about service closures read from XML data
    /// </summary>
    public class XPathClosureData : XPathDocument
    {
        private XmlNamespaceManager nsManager;
        private string xmlNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathClosureData"/> class.
        /// </summary>
        /// <param name="uri">The path of the file that contains the XML data.</param>
        public XPathClosureData(string uri) : base(uri) { }

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

        /// <summary>
        /// Gets whether to show the status all services.
        /// </summary>
        /// <returns></returns>
        public ShowAllServicesView ShowAllServicesView
        {
            get
            {
                XPathNavigator showAllServicesNode = this.SelectNode("/ns:ClosureInfo/ns:ShowAllServicesView");
                if (showAllServicesNode == null) return ShowAllServicesView.Auto;

                return (ShowAllServicesView)Enum.Parse(typeof(ShowAllServicesView), showAllServicesNode.Value);
            }
        }

        /// <summary>
        /// Gets or sets the number days in advance a closure must be logged, otherwise it is regarded as logged at short notice.
        /// </summary>
        /// <returns></returns>
        public int ShortNoticeDays
        {
            get
            {
                XPathNavigator shortNoticeNode = this.SelectNode("/ns:ClosureInfo/ns:ShortNoticeDays");
                if (shortNoticeNode == null) return 5; // default value

                return Int32.Parse(shortNoticeNode.Value);
            }
        }

        /// <summary>
        /// Creates an XPathClosureData document from closure data for the specified service type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public static XPathClosureData Create(ServiceType serviceType)
        {
            return new XPathClosureData(XmlPath(serviceType));
        }

        /// <summary>
        /// Gets a namespace manager which sets up the namespace with an "ns" prefix
        /// </summary>
        private XmlNamespaceManager NamespaceManager
        {
            get
            {
                if (this.nsManager == null)
                {
                    XPathNavigator nav = this.CreateNavigator();
                    this.nsManager = new XmlNamespaceManager(nav.NameTable);
                    nav.MoveToRoot();
                    nav.MoveToFirstChild();
                    this.nsManager.AddNamespace("ns", nav.NamespaceURI);
                }

                return this.nsManager;
            }
        }

        /// <summary>
        /// Gets the main XML namespace from the XML data
        /// </summary>
        private string XmlNamespace
        {
            get
            {
                if (String.IsNullOrEmpty(this.xmlNamespace))
                {
                    XPathNavigator nav = this.CreateNavigator();
                    nav.MoveToRoot();
                    nav.MoveToFirstChild();
                    this.xmlNamespace = nav.NamespaceURI;
                }

                return this.xmlNamespace;
            }
        }

        /// <summary>
        /// Selects a node from the closures XML matching the supplied XPath expression
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <returns></returns>
        private XPathNavigator SelectNode(string xpath)
        {
            XPathNavigator nav = this.CreateNavigator();
            nav.MoveToRoot();
            nav.MoveToFirstChild();

            return this.SelectNode(xpath, nav);
        }

        /// <summary>
        /// Selects a node from the closures XML matching the supplied XPath expression
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <param name="nav">The position in the document to select from</param>
        /// <returns></returns>
        private XPathNavigator SelectNode(string xpath, XPathNavigator nav)
        {
            XPathExpression expr = nav.Compile(xpath);
            expr.SetContext(this.NamespaceManager);
            return nav.SelectSingleNode(expr);
        }

        /// <summary>
        /// Selects nodes from the closures XML matching the supplied XPath expression
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <returns></returns>
        private XPathNodeIterator SelectNodes(string xpath)
        {
            XPathNavigator nav = this.CreateNavigator();
            nav.MoveToRoot();
            nav.MoveToFirstChild();

            return this.SelectNodes(xpath, nav);
        }

        /// <summary>
        /// Selects nodes from the closures XML matching the supplied XPath expression
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <param name="nav">The position in the document to select from</param>
        /// <returns></returns>
        private XPathNodeIterator SelectNodes(string xpath, XPathNavigator nav)
        {
            XPathExpression expr = nav.Compile(xpath);
            expr.SetContext(this.NamespaceManager);
            return nav.Select(expr);
        }

        /// <summary>
        /// Gets an XPath expression to select closures in effect tomorrow
        /// </summary>
        /// <value>XPath expression</value>
        private string GetXPathForClosuresOnSingleDay(DateTime day)
        {
            // Need to compare closure dates to date as a number, 
            // because .NET only does XPath 1.0 which doesn't understand dates
            string closureDay = day.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            // Concatenate the year, month and date parts of the closure date and compare to "closureDay" variable
            // Don't check that ns:Status = 'Closed' because this has to work even if the only declaration is one service saying anything about its status
            return String.Format(CultureInfo.InvariantCulture, "/ns:ClosureInfo/ns:Services/ns:Service/ns:Closures/ns:Closure[number(concat(substring(ns:StartDate,1,4), substring(ns:StartDate,6,2), substring(ns:StartDate,9,2)))<={0} and number(concat(substring(ns:EndDate,1,4), substring(ns:EndDate,6,2), substring(ns:EndDate,9,2)))>={0}]", closureDay);
        }

        /// <summary>
        /// Checks whether a emergency closure exists on a specific day.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public bool EmergencyClosureExists(DateTime day)
        {
            // First check if there are any closures with a reason that's marked as an emergency
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(day) + "/ns:Reason/ns:Emergency[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether a emergency closure exists today.
        /// </summary>
        /// <returns></returns>
        public bool EmergencyClosureExistsToday()
        {
            // First check if there are any closures with a reason that's marked as an emergency
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(DateTime.Today) + "/ns:Reason/ns:Emergency[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether a emergency closure exists tomorrow.
        /// </summary>
        /// <returns></returns>
        public bool EmergencyClosureExistsTomorrow()
        {
            // First check if there are any closures with a reason that's marked as an emergency
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(DateTime.Today.AddDays(1)) + "/ns:Reason/ns:Emergency[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether a closure has been announced at short notice.
        /// </summary>
        /// <param name="daysNotice">The maximum number of days' notice.</param>
        /// <returns></returns>
        public bool ClosedTodayAtShortNotice(int daysNotice)
        {
            // Check if there are any closures marked as short notice
            XPathNodeIterator it = this.SelectNodes(String.Format(CultureInfo.InvariantCulture, GetXPathForClosuresOnSingleDay(DateTime.Today) + "/ns:DaysNotice[number(.)<={0}]", daysNotice));
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether a closure has been announced at short notice.
        /// </summary>
        /// <param name="daysNotice">The maximum number of days' notice.</param>
        /// <returns></returns>
        public bool ClosedTomorrowAtShortNotice(int daysNotice)
        {
            // Check if there are any closures marked as short notice
            XPathNodeIterator it = this.SelectNodes(String.Format(CultureInfo.InvariantCulture, GetXPathForClosuresOnSingleDay(DateTime.Today.AddDays(1)) + "/ns:DaysNotice[number(.)<={0}]", daysNotice));
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether all services may be affected by a closure announced for a specific day.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public bool AllServicesAffected(DateTime day)
        {
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(day) + "/ns:Reason/ns:MayAffectAllServices[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether all services may be affected by a closure announced for today.
        /// </summary>
        /// <returns></returns>
        public bool AllServicesAffectedToday()
        {
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(DateTime.Today) + "/ns:Reason/ns:MayAffectAllServices[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Checks whether all services may be affected by a closure announced for tomorrow.
        /// </summary>
        /// <returns></returns>
        public bool AllServicesAffectedTomorrow()
        {
            XPathNodeIterator it = this.SelectNodes(GetXPathForClosuresOnSingleDay(DateTime.Today.AddDays(1)) + "/ns:Reason/ns:MayAffectAllServices[.='true']");
            return (it.Count > 0);
        }

        /// <summary>
        /// Gets the closures for a service identified by its code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        public Collection<Closure> ClosuresByServiceCode(string code)
        {
            if (String.IsNullOrEmpty(code)) return new Collection<Closure>();

            XPathNodeIterator it = this.SelectNodes(String.Format(CultureInfo.InvariantCulture, "/ns:ClosureInfo/ns:Services/ns:Service[ns:Code='{0}']", code));
            if (it.Count == 1)
            {
                it.MoveNext(); // Move from the root node to the matched service

                XmlSerializer deserialiser = new XmlSerializer(typeof(Service), this.XmlNamespace);
                XmlTextReader xmlReader = new XmlTextReader(new StringReader(it.Current.OuterXml));
                Service service = (Service)deserialiser.Deserialize(xmlReader);
                xmlReader.Close();
                return service.Closures;
            }
            else return new Collection<Closure>();
        }

        /// <summary>
        /// Gets the closures for a specific day for a service identified by its code.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="code">The code.</param>
        /// <param name="emergencyOnly">if set to <c>true</c> get emergency closures only.</param>
        /// <returns></returns>
        private Collection<Closure> ClosuresByServiceCodeOnSingleDay(DateTime day, string code, bool emergencyOnly)
        {
            if (String.IsNullOrEmpty(code)) return new Collection<Closure>();

            // Need to compare closure dates to today's date as a number, 
            // because .NET only does XPath 1.0 which doesn't understand dates
            string closureDay = day.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            // Concatenate the year, month and date parts of the closure date and compare to "today" variable
            string xpath = String.Format(CultureInfo.InvariantCulture, "/ns:ClosureInfo/ns:Services/ns:Service[ns:Code='{0}']/ns:Closures/ns:Closure[number(concat(substring(ns:StartDate,1,4), substring(ns:StartDate,6,2), substring(ns:StartDate,9,2)))<={1} and number(concat(substring(ns:EndDate,1,4), substring(ns:EndDate,6,2), substring(ns:EndDate,9,2)))>={1}", code, closureDay);
            if (emergencyOnly) xpath += " and ns:Reason/ns:Emergency = 'true'";
            xpath += "]";

            Collection<Closure> closures = new Collection<Closure>();
            XPathNodeIterator it = this.SelectNodes(xpath);
            while (it.MoveNext())
            {
                XmlSerializer deserialiser = new XmlSerializer(typeof(Closure), this.XmlNamespace);
                XmlTextReader xmlReader = new XmlTextReader(new StringReader(it.Current.OuterXml));
                Closure closure = (Closure)deserialiser.Deserialize(xmlReader);
                xmlReader.Close();
                closures.Add(closure);
            }
            return closures;
        }

        /// <summary>
        /// Gets the closures for today for a service identified by its code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="emergencyOnly">if set to <c>true</c> get emergency closures only.</param>
        /// <returns></returns>
        public Collection<Closure> ClosuresTodayByServiceCode(string code, bool emergencyOnly)
        {
            return this.ClosuresByServiceCodeOnSingleDay(DateTime.Today, code, emergencyOnly);
        }

        /// <summary>
        /// Gets the closures for tomorrow for a service identified by its code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="emergencyOnly">if set to <c>true</c> get emergency closures only.</param>
        /// <returns></returns>
        public Collection<Closure> ClosuresTomorrowByServiceCode(string code, bool emergencyOnly)
        {
            return this.ClosuresByServiceCodeOnSingleDay(DateTime.Today.AddDays(1), code, emergencyOnly);
        }

        /// <summary>
        /// Gets all services from the XML
        /// </summary>
        /// <returns></returns>
        public Collection<Service> Services()
        {
            // Select all services
            XPathNavigator nav = this.CreateNavigator();
            XPathExpression expr = nav.Compile("/ns:ClosureInfo/ns:Services/ns:Service");
            expr.SetContext(this.NamespaceManager);

            // Sort by name of service
            XPathExpression sortExpr = nav.Compile("ns:Name");
            sortExpr.SetContext(this.NamespaceManager);
            expr.AddSort(sortExpr, XmlSortOrder.Ascending, XmlCaseOrder.None, String.Empty, XmlDataType.Text);

            // Add services to collection and return
            XPathNodeIterator it = nav.Select(expr);
            Collection<Service> services = new Collection<Service>();
            while (it.MoveNext())
            {
                // Deserialise the service
                XmlSerializer deserialiser = new XmlSerializer(typeof(Service), this.XmlNamespace);
                XmlTextReader xmlReader = new XmlTextReader(new StringReader(it.Current.OuterXml));
                services.Add((Service)deserialiser.Deserialize(xmlReader));
                xmlReader.Close();
            }
            return services;
        }
    }
}
