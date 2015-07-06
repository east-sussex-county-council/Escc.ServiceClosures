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
        #region Fields

        private ClosureStatus status = ClosureStatus.Unknown;

        #endregion //Fields

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Closure"/> class.
        /// </summary>
        public Closure()
        {
            this.Reason = new ClosureReason();
        }

        #endregion // Constructor


        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets whether the service is closed.
        /// </summary>
        /// <value>The status.</value>
        public ClosureStatus Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }

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
        public ClosureReason Reason { get; set; }

        /// <summary>
        /// Gets or sets notes about the closure.
        /// </summary>
        /// <value>The notes.</value>
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets the the number of days' advance notice given of the closure.
        /// </summary>
        /// <value>The number of days' notice.</value>
        public int DaysNotice { get; set; }

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
        /// Gets or sets a value indicating whether re-confirmation of a multi-day closure is currently required.
        /// </summary>
        /// <value><c>true</c> if confirmation required; otherwise, <c>false</c>.</value>
        public bool ConfirmationRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether re-confirmation of a multi-day closure is currently overdue.
        /// </summary>
        /// <value><c>true</c> if confirmation overdue; otherwise, <c>false</c>.</value>
        public bool ConfirmationOverdue { get; set; }

        /// <summary>
        /// Gets or sets the date the closure was last confirmed.
        /// </summary>
        /// <value>The date confirmed.</value>
        public DateTime ConfirmedDate { get; set; }

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

        #endregion // Properties
    }
}
