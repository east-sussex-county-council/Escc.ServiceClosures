using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A repository where service closure data can be stored
    /// </summary>
    public interface IServiceClosureRepository
    {
        /// <summary>
        /// Saves the closure information to the repository
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="closureInfo">The closure information.</param>
        void SaveClosureInfo(ServiceType serviceType, ClosureInfo closureInfo);
    }
}
