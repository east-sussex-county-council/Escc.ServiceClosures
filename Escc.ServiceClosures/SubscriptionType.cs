using System;
using System.Collections.Generic;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Media used to subscribe to closure notifications
    /// </summary>
    public enum SubscriptionType
    {
        /// <summary>
        /// No subscription
        /// </summary>
        None = 0,
        /// <summary>
        /// Subscription by email
        /// </summary>
        Email = 1,
        /// <summary>
        /// Subscription by SMS text message
        /// </summary>
        TextMessage = 2
    }
}
