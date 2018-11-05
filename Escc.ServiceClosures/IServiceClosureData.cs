using System;
using System.Collections.ObjectModel;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Information about service closures
    /// </summary>
    public interface IServiceClosureData
    {
        /// <summary>
        /// Gets the closures for a service identified by its code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        Collection<Closure> ClosuresByServiceCode(string code);

        /// <summary>
        /// Gets the closures for today for a service identified by its code.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="code">The code.</param>
        /// <param name="emergencyOnly">if set to <c>true</c> get emergency closures only.</param>
        /// <returns></returns>
        Collection<Closure> ClosuresByDateAndServiceCode(DateTime date, string code, bool emergencyOnly);

        /// <summary>
        /// Checks whether a emergency closure exists on a specific day.
        /// </summary>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        bool EmergencyClosureExists(DateTime day);

        /// <summary>
        /// Gets all the services for which closure data is available
        /// </summary>
        /// <returns></returns>
        Collection<Service> Services();
    }
}