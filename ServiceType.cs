using System;
using System.Collections.Generic;
using System.Text;

namespace EsccWebTeam.ServiceClosures
{
    /// <summary>
    /// A type of service which is subject to closures
    /// </summary>
    public class ServiceType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceType"/> class.
        /// </summary>
        public ServiceType() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceType"/> class.
        /// </summary>
        /// <param name="singular">A word or phrase which describes a single instance of this service.</param>
        public ServiceType(string singular)
        {
            this.SingularText = singular;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceType"/> class.
        /// </summary>
        /// <param name="singular">A word or phrase which describes a single instance of this service.</param>
        /// <param name="plural">A word or phrase which describes multiple instances of this service.</param>
        public ServiceType(string singular, string plural)
        {
            this.SingularText = singular;
            this.PluralText = plural;
        }

        /// <summary>
        /// Gets or sets the word or phrase which describes a single instance of this service.
        /// </summary>
        /// <value>The singular text.</value>
        /// <example>
        /// <list type="bullet">
        /// <item>school</item>
        /// <item>library</item>
        /// <item>household waste site</item>
        /// </list>
        /// </example>
        public string SingularText { get; set; }

        /// <summary>
        /// Gets or sets the word or phrase which describes multiple instances of this service.
        /// </summary>
        /// <value>The plural text.</value>
        /// <example>
        /// <list type="bullet">
        /// <item>schools</item>
        /// <item>libraries</item>
        /// <item>household waste sites</item>
        /// </list>
        /// </example>
        public string PluralText { get; set; }
    }
}
