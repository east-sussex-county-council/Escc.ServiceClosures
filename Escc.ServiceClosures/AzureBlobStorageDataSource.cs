using Microsoft.ApplicationInsights;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Escc.ServiceClosures
{
    /// <summary>
    /// Reads service closure data from Azure blob storage
    /// </summary>
    /// <seealso cref="Escc.ServiceClosures.IServiceClosureDataSource" />
    public class AzureBlobStorageDataSource : IServiceClosureDataSource
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageDataSource"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="containerName">Name of the container.</param>
        public AzureBlobStorageDataSource(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        /// <summary>
        /// Reads the closure data.
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <returns></returns>
        public async Task<IServiceClosureData> ReadClosureDataAsync(ServiceType serviceType)
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_containerName);
            blobContainer.CreateIfNotExistsAsync().Wait();

            var blob = blobContainer.GetBlockBlobReference(serviceType.SingularText.ToLowerInvariant() + ".xml");

            try
            {
                using (var stream = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(stream);
                    stream.Position = 0;
                    return new XPathClosureData(stream);
                }
            }
            catch (StorageException ex)
            {
                var realException = ex.InnerException as System.Net.WebException;
                if (realException == null) { throw; }

                // Report the error and return a response with no data
                new TelemetryClient().TrackException(ex);
                return null;
            }
        }
    }
}
