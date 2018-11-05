using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Reads service closure data from a repository
    /// </summary>
    public interface IServiceClosureDataSource
    {
        /// <summary>
        /// Reads the closure data.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        Task<IServiceClosureData> ReadClosureDataAsync(ServiceType serviceType);
    }
}
