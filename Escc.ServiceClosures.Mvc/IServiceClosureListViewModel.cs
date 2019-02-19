using System;
using System.Collections.ObjectModel;

namespace Escc.ServiceClosures.Mvc
{
     /// <summary>
    /// View model to display a list of service closures
    /// </summary>
    public interface IServiceClosureListViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether to show only closures in the future.
        /// </summary>
        /// <value><c>true</c> to show future closures only; otherwise, <c>false</c>.</value>
        bool FutureOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to group by closure status.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if grouping by closure status; otherwise, <c>false</c>.
        /// </value>
        bool GroupByClosureStatus { get; set; }

        /// <summary>
        /// Gets the services containing the closure data.
        /// </summary>
        /// <value>The closures.</value>
        Collection<Service> Services { get; }

        /// <summary>
        /// Gets or sets whether to show a heading when <see cref="GroupByClosureStatus"/> is <c>false</c>, indicating the kind of closures being shown
        /// </summary>
        bool ShowHeading { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show emergency closures.
        /// </summary>
        /// <value><c>true</c> to show emergency closures; otherwise, <c>false</c>.</value>
        bool ShowEmergencyClosures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show closures which are planned in advance and are not emergencies.
        /// </summary>
        /// <value><c>true</c> to show planned closures; otherwise, <c>false</c>.</value>
        bool ShowPlannedClosures { get; set; }

        /// <summary>
        /// Gets or sets the target day for which closures should be shown, if any.
        /// </summary>
        /// <value>
        /// The target day.
        /// </value>
        DateTime? TargetDay { get; set; }

        /// <summary>
        /// Determines whether the <see cref="TargetDay"/> is today.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is today; otherwise, <c>false</c>.
        /// </returns>
        bool IsToday();

        /// <summary>
        /// Determines whether the <see cref="TargetDay"/> is tomorrow.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is tomorrow; otherwise, <c>false</c>.
        /// </returns>
        bool IsTomorrow();
    }
}