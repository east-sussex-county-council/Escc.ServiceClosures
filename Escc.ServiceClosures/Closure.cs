using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A scheduled closure of a council service
    /// </summary>
    public class Closure
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets whether the service is closed.
        /// </summary>
        /// <value>The status.</value>
        public ClosureStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>The start date.</value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>The end date.</value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the reason for closure.
        /// </summary>
        /// <value>The reason.</value>
        /// <remarks>Important that this property is not read-only because that would prevent it from being serialised</remarks>
        public ClosureReason Reason { get; set; } = new ClosureReason();

        /// <summary>
        /// Gets or sets notes about the closure.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the date the closure information was added.
        /// </summary>
        /// <value>The date added.</value>
        public DateTime DateAdded { get; set; }

        /// <summary>
        /// Gets or sets the date the closure information was last modified.
        /// </summary>
        /// <value>The date modified.</value>
        public DateTime DateModified { get; set; }

        /// <summary>
        /// Gets or sets the linked data URI representing the closure.
        /// </summary>
        /// <value>The URI.</value>
        [XmlIgnore]
        public Uri LinkedDataUri { get; set; }

        /// <summary>
        /// Gets or sets the linked data URI representing the closure. Synonym for <seealso cref="LinkedDataUri"/> which is compatible with serialisation.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings"), XmlElement("LinkedDataUri")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string LinkedDataUriSerialisable
        {
            get { return (this.LinkedDataUri != null) ? this.LinkedDataUri.ToString() : null; }
            set { this.LinkedDataUri = String.IsNullOrEmpty(value) ? null : new Uri(value, UriKind.Absolute); }
        }
    }
}
