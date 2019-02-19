using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Escc.Dates;

namespace Escc.ServiceClosures.Mvc
{
    /// <summary>
    /// View model to display a list of service closures
    /// </summary>
    public class ServiceClosureListViewModel : IServiceClosureListViewModel
    {
        private DateTime _today;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClosureListViewModel"/> class.
        /// </summary>
        public ServiceClosureListViewModel() { _today = DateTime.Now.ToUkDateTime().Date;  }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceClosureListViewModel"/> class.
        /// </summary>
        /// <param name="today">The date to be treated as 'today' when calculating which closures to show.</param>
        public ServiceClosureListViewModel(DateTime today) { _today = today; }

        /// <summary>
        /// Determines whether the <see cref="TargetDay"/> is today.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is today; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsToday() { return TargetDay.HasValue && TargetDay.Value == _today; }

        /// <summary>
        /// Determines whether the <see cref="TargetDay"/> is tomorrow.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is tomorrow; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsTomorrow() { return TargetDay.HasValue && TargetDay.Value == _today.AddDays(1); }

        /// <summary>
        /// Gets or sets the target day for which closures should be shown, if any.
        /// </summary>
        /// <value>
        /// The target day.
        /// </value>
        public virtual DateTime? TargetDay { get; set; }

        /// <summary>
        /// Gets the services containing the closure data.
        /// </summary>
        /// <value>The closures.</value>
        public Collection<Service> Services { get; } = new Collection<Service>();

        /// <summary>
        /// Gets or sets a value indicating whether to show only closures in the future.
        /// </summary>
        /// <value><c>true</c> to show future closures only; otherwise, <c>false</c>.</value>
        public bool FutureOnly { get; set; }

        /// <summary>
        /// Gets or sets whether to show a heading when <see cref="GroupByClosureStatus"/> is <c>false</c>, indicating the kind of closures being shown
        /// </summary>
        public bool ShowHeading { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show emergency closures.
        /// </summary>
        /// <value><c>true</c> to show emergency closures; otherwise, <c>false</c>.</value>
        public bool ShowEmergencyClosures { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to show closures which are planned in advance and are not emergencies.
        /// </summary>
        /// <value><c>true</c> to show planned closures; otherwise, <c>false</c>.</value>
        public bool ShowPlannedClosures { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to group by closure status.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if grouping by closure status; otherwise, <c>false</c>.
        /// </value>
        public bool GroupByClosureStatus { get; set; }
    }
}