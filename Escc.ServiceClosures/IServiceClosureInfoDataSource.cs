using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// A repository from where service closure information can be read
    /// </summary>
    public interface IServiceClosureInfoDataSource
    {
        /// <summary>
        /// Reads the closure information for a given service type.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        ClosureInfo ReadClosureInfo(ServiceType serviceType);
    }
}
