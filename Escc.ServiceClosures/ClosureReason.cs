using System;
using System.Collections.Generic;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A reason for which a service may be closed
    /// </summary>
    public class ClosureReason
    {
        /// <summary>
        /// Gets or sets the identifier
        /// </summary>
        /// <value>The identifier</value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>The reason.</value>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClosureReason"/> is an emergency closure.
        /// </summary>
        /// <value><c>true</c> if reason is a type of emergency; otherwise, <c>false</c>.</value>
        public bool Emergency { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClosureReason"/> requires the submitter to add further details.
        /// </summary>
        /// <value><c>true</c> if more details required; otherwise, <c>false</c>.</value>
        public bool RequiresNotes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClosureReason"/> is selectable for new closures.
        /// </summary>
        /// <value><c>true</c> if selectable; otherwise, <c>false</c>.</value>
        public bool Selectable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ClosureReason"/> may also affect all similar services.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if reason may affect all services; otherwise, <c>false</c>.
        /// </value>
        public bool MayAffectAllServices { get; set; }
    }
}
