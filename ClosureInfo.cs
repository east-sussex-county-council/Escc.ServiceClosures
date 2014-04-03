using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace EsccWebTeam.ServiceClosures
{
    /// <summary>
    /// Container for closure data when serialised as XML
    /// </summary>
    /// <remarks>Serialising this class rather than an instance of <see cref="Collection&lt;Service&gt;"/> 
    /// allows extra data to be included in the XML alongside the closures.</remarks>
    public class ClosureInfo
    {
        private Collection<Service> services = new Collection<Service>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ClosureInfo"/> class.
        /// </summary>
        public ClosureInfo()
        {
            this.ShowAllServicesView = ShowAllServicesView.Auto;
            this.ShortNoticeDays = 5; // default, apps can change it
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public Collection<Service> Services { get { return this.services; } }

        /// <summary>
        /// Gets or sets when to show the all services view.
        /// </summary>
        /// <value>When to show the all services view.</value>
        public ShowAllServicesView ShowAllServicesView { get; set; }

        /// <summary>
        /// Gets or sets the number days in advance a closure must be logged, otherwise it is regarded as logged at short notice.
        /// </summary>
        /// <value>The number of days.</value>
        public int ShortNoticeDays { get; set; }
    }
}
