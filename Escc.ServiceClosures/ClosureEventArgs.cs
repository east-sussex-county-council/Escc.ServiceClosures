using System;
using System.Collections.Generic;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Contextual information about a service closure
    /// </summary>
    public class ClosureEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the service which is subject to closures.
        /// </summary>
        /// <value>The service.</value>
        public Service Service { get; set; }

        /// <summary>
        /// Gets or sets the closure details.
        /// </summary>
        /// <value>The closure details.</value>
        public Closure Closure { get; set; }

        /// <summary>
        /// Gets or sets the reason a service may be closed.
        /// </summary>
        /// <value>The reason.</value>
        public ClosureReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the subscription.
        /// </summary>
        /// <value>The subscription.</value>
        public Subscription Subscription { get; set; }
    }
}
