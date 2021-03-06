﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Information about service closures read from XML data
    /// </summary>
    public class XPathClosureData : XPathDocument, IServiceClosureData
    {
        private XmlNamespaceManager nsManager;
        private string xmlNamespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathClosureData"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="T:System.IO.Stream" /> object that contains the XML data.</param>
        public XPathClosureData(Stream stream) : base(stream) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathClosureData"/> class.
        /// </summary>
        /// <param name="uri">The path of the file that contains the XML data.</param>
        public XPathClosureData(string uri) : base(uri) { }

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
        private static string GetXPathForClosuresOnSingleDay(DateTime day)
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
            else { return new Collection<Closure>(); }
        }

        /// <summary>
        /// Gets the closures for a specific day for a service identified by its code.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <param name="code">The code.</param>
        /// <param name="emergencyOnly">if set to <c>true</c> get emergency closures only.</param>
        /// <returns></returns>
        public Collection<Closure> ClosuresByDateAndServiceCode(DateTime day, string code, bool emergencyOnly)
        {
            if (String.IsNullOrEmpty(code)) { return new Collection<Closure>(); }

            // Need to compare closure dates to today's date as a number, 
            // because .NET only does XPath 1.0 which doesn't understand dates
            string closureDay = day.ToString("yyyyMMdd", CultureInfo.InvariantCulture);

            // Concatenate the year, month and date parts of the closure date and compare to "today" variable
            string xpath = String.Format(CultureInfo.InvariantCulture, "/ns:ClosureInfo/ns:Services/ns:Service[ns:Code='{0}']/ns:Closures/ns:Closure[number(concat(substring(ns:StartDate,1,4), substring(ns:StartDate,6,2), substring(ns:StartDate,9,2)))<={1} and number(concat(substring(ns:EndDate,1,4), substring(ns:EndDate,6,2), substring(ns:EndDate,9,2)))>={1}", code, closureDay);
            if (emergencyOnly) { xpath += " and ns:Reason/ns:Emergency = 'true'"; }
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
