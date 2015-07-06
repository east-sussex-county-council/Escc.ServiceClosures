using System;
using System.Collections.Generic;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A subscription to closure notifications
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        public Subscription()
        {
            this.Type = SubscriptionType.None;
        }

        /// <summary>
        /// Gets or sets the id of the subscription
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the type of subscription
        /// </summary>
        /// <value>The type.</value>
        public SubscriptionType Type { get; set; }

        /// <summary>
        /// Gets or sets the address to send notifications to
        /// </summary>
        /// <value>The address.</value>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the service being subscribed to
        /// </summary>
        /// <value>The service, or <c>null</c> if the subscription is for every service of a given type.</value>
        public Service Service { get; set; }

        /// <summary>
        /// Gets or sets the code used to activate or deactivate the subscription
        /// </summary>
        /// <value>The code.</value>
        public Guid Code { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Subscription"/> is active.
        /// </summary>
        /// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
        public bool Active { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Subscription"/> is for all services of a given type.
        /// </summary>
        /// <value><c>true</c> if for all services of a given type; otherwise, <c>false</c>.</value>
        public bool Global
        {
            get
            {
                return (this.Service == null || this.Service.Id < 1);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether communications to the subscriber should include some mark indicating an genuine notification message.
        /// </summary>
        /// <value><c>true</c> if official notification; otherwise, <c>false</c>.</value>
        /// <remarks>For example, a child subscribes to school notifications and gets an email. Next time they want a day off they
        /// change the dates in the email and send it to the local radio station. But the radio station gets an "official notification"
        /// which is in a slightly different format, so they ignore the fake email.</remarks>
        public bool OfficialNotification { get; set; }
    }
}
