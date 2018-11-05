using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Container for closure data when serialised as XML
    /// </summary>
    /// <remarks>Serialising this class rather than an instance of <see cref="Collection&lt;Service&gt;"/> 
    /// allows extra data to be included in the XML alongside the closures.</remarks>
    public class ClosureInfo
    {
        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>The services.</value>
        public Collection<Service> Services { get; private set; } = new Collection<Service>();
    }
}
