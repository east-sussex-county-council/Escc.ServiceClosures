using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using Escc.AddressAndPersonalDetails;

namespace EsccWebTeam.ServiceClosures
{
    /// <summary>
    /// Someone responsible for checking that closure information is up-to-date and correct
    /// </summary>
    public class Moderator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Moderator"/> class.
        /// </summary>
        public Moderator()
        {
            this.EmailAddress = new ContactEmail();
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>The email address.</value>
        public ContactEmail EmailAddress { get; set; }
    }
}
