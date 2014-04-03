using System;
using System.Collections.Generic;
using System.Text;

namespace EsccWebTeam.ServiceClosures
{
    /// <summary>
    /// Settings determining when to show closure status of all services
    /// </summary>
    public enum ShowAllServicesView
    {
        /// <summary>
        /// Always show the status of all services
        /// </summary>
        Always,

        /// <summary>
        /// Only ever show the status of services which have declared they are closed
        /// </summary>
        Never,

        /// <summary>
        /// Show all services if at least one closure has the "MayAffectAllServices" setting
        /// </summary>
        Auto
    }
}
