using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A council service which is subject to closures
    /// </summary>
    public class Service
    {
        #region Fields

        private Collection<Closure> closures = new Collection<Closure>();
        private Dictionary<int, ClosureReason> reasons = new Dictionary<int, ClosureReason>();

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        public Service()
        {
            this.Type = new ServiceType("service", "services");
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the database id of the service.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets an internal code identifying the service, such as a DfES school code or library branch code.
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the name of the service.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the linked data URI representing the service.
        /// </summary>
        /// <value>The URI.</value>
        [XmlIgnore]
        public Uri LinkedDataUri { get; set; }

        /// <summary>
        /// Gets or sets the linked data URI representing the service. Synonym for <seealso cref="LinkedDataUri"/> which is compatible with serialisation.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), XmlElement("LinkedDataUri")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string LinkedDataUriSerialisable
        {
            get { return (this.LinkedDataUri != null) ? this.LinkedDataUri.ToString() : null; }
            set { this.LinkedDataUri = String.IsNullOrEmpty(value) ? null : new Uri(value, UriKind.Absolute); }
        }

        /// <summary>
        /// Gets or sets the URL where details of the service can be found.
        /// </summary>
        /// <value>The URL.</value>
        [XmlIgnore]
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the URL where details of the service can be found. Synonym for <seealso cref="Url"/> which is compatible with serialisation.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), XmlElement("Url")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string UrlSerialisable
        {
            get { return (this.Url != null) ? this.Url.ToString() : null; }
            set { this.Url = String.IsNullOrEmpty(value) ? null : new Uri(value, UriKind.RelativeOrAbsolute); }
        }

        /// <summary>
        /// Gets or sets the type of service. Defaults to "service".
        /// </summary>
        /// <value>The type.</value>
        /// <example>school</example>
        /// <example>library</example>
        /// <remarks>Important that this property is not read-only because that would prevent it from being serialised</remarks>
        public ServiceType Type { get; set; }

        /// <summary>
        /// Gets the closures currently scheduled for the service.
        /// </summary>
        /// <value>The closures currently scheduled.</value>
        public Collection<Closure> Closures { get { return this.closures; } }

        /// <summary>
        /// Gets the reasons for which the service may be closed.
        /// </summary>
        /// <value>The reasons for closure.</value>
        [XmlIgnore]
        public Dictionary<int, ClosureReason> ReasonsForClosure { get { return this.reasons; } }

        #endregion // Properties

        #region Methods

        /// <summary>
        /// Checks whether the service has closure information for a specific day
        /// </summary>
        /// <returns>Closure information if present</returns>
        public Collection<Closure> CheckForClosures(DateTime day)
        {
            Collection<Closure> closuresOnDay = new Collection<Closure>();
            foreach (Closure closure in this.Closures)
            {
                if (closure.StartDate <= day && closure.EndDate >= day)
                {
                    closuresOnDay.Add(closure);
                }
            }

            return closuresOnDay;
        }

        /// <summary>
        /// Checks whether the service has closure information for today
        /// </summary>
        /// <returns>Closure information if present</returns>
        public Collection<Closure> CheckForClosuresToday()
        {
            return CheckForClosures(DateTime.Today);
        }

        /// <summary>
        /// Checks whether the service has closure information for tomorrow
        /// </summary>
        /// <returns>Closure information if present</returns>
        public Collection<Closure> CheckForClosuresTomorrow()
        {
            return CheckForClosures(DateTime.Today.AddDays(1));
        }

        #endregion // Methods
    }
}
