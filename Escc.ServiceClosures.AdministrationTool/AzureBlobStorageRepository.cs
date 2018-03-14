using Microsoft.WindowsAzure.Storage;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Escc.ServiceClosures.AdministrationTool
{
    /// <summary>
    /// Saves service closure data as serialised XML to an Azure blob storage container
    /// </summary>
    /// <seealso cref="Escc.ServiceClosures.IServiceClosureRepository" />
    public class AzureBlobStorageRepository : IServiceClosureRepository
    {
        private readonly string _connectionString;
        private readonly string _containerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobStorageRepository"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="containerName">Name of the container.</param>
        public AzureBlobStorageRepository(string connectionString, string containerName)
        {
            _connectionString = connectionString;
            _containerName = containerName;
        }

        /// <summary>
        /// Saves the closure information to the repository
        /// </summary>
        /// <param name="serviceType">Type of the service.</param>
        /// <param name="closureInfo">The closure information.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void SaveClosureInfo(ServiceType serviceType, ClosureInfo closureInfo)
        {
            var storageAccount = CloudStorageAccount.Parse(_connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(_containerName);
            blobContainer.CreateIfNotExistsAsync().Wait();

            using (var stream = new MemoryStream())
            {
                XmlSerializer xs = new XmlSerializer(typeof(ClosureInfo), "http://czoneapps.eastsussex.gov.uk/Czone.WebService.SchoolsInformation/");
                xs.Serialize(stream, closureInfo);
                stream.Position = 0;

                var blob = blobContainer.GetBlockBlobReference(serviceType.SingularText.ToLowerInvariant() + ".xml");
                blob.UploadFromStream(stream);
            }
        }
    }
}
