using ABACDemo.Web.Entities;
using ABACDemo.Web.Interfaces;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace StorageContentPlatform.Web.Services
{
    public class StorageContentsService : IContentsService
    {
        private class Configuration
        {
            public bool ForceManagedIdentity { get; set; } = false;
            public string? StorageAccountName { get; set; }
            public IEnumerable<string>? ContainerTypes { get; set; }

            public string StorageBlobUrl
            {
                get => $"https://{this.StorageAccountName}.blob.core.windows.net";
            }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        public StorageContentsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        #region [ Public Methods - IContentsService implementation ]
        public async Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
        {
            var result = new List<ContainerInfo>();
            LoadConfig();
            BlobServiceClient blobServiceClient = CreateBlobServiceClient();

            var resultSegment = blobServiceClient
                    .GetBlobContainersAsync(BlobContainerTraits.Metadata, null, cancellationToken)
                    .AsPages(default, 100);

            await foreach (Azure.Page<BlobContainerItem> containerPage in resultSegment)
            {
                foreach (BlobContainerItem containerItem in containerPage.Values)
                {
                    if (!this.configurationValues.ContainerTypes.Any() || containerItem.HasMetadataValues("containerType", this.configurationValues.ContainerTypes))
                    {
                        var containerInfo = new ContainerInfo();
                        containerInfo.Name = containerItem.Name;
                        containerInfo.LastModified = containerItem.Properties.LastModified;
                        if (containerItem.Properties.Metadata != null && containerItem.Properties.Metadata.Any())
                            containerInfo.Metadata = containerItem.Properties.Metadata
                                     .Select((k, v) => $"{k}={v}").Aggregate((a, b) => $"{a};{b}");

                        result.Add(containerInfo);
                    }
                }
            }
            return result;
        }

        public async Task<IEnumerable<ABACDemo.Web.Entities.BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default)
        {
            var result = new List<ABACDemo.Web.Entities.BlobInfo>();
            LoadConfig();

            BlobContainerClient containerClient = CreateBlobContainerClient(containerName);

            // Add logging to see which identity is being used
            Console.WriteLine($"Attempting to list blobs in container: {containerName}");
            Console.WriteLine($"Using managed identity: {this.configurationValues.ForceManagedIdentity}");
            Console.WriteLine($"Storage URL: {this.configurationValues.StorageBlobUrl}");

            try
            {
                // Verify container exists and you have access to it
                var containerProperties = await containerClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                Console.WriteLine($"Container properties retrieved successfully. Metadata: {string.Join(", ", containerProperties.Value.Metadata.Select(m => $"{m.Key}={m.Value}"))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get container properties: {ex.Message}");
                throw;
            }

            var blobPages = containerClient.GetBlobsAsync().AsPages();

            await foreach (var blobPage in blobPages)
            {
                foreach (var blob in blobPage.Values)
                {
                    var blobInfo = new ABACDemo.Web.Entities.BlobInfo();
                    blobInfo.Name = blob.Name;
                    blobInfo.LastModified = blob.Properties.LastModified;
                    blobInfo.Size = blob.Properties.ContentLength;
                    blobInfo.Tier = blob.Properties.AccessTier.HasValue ?
                        blob.Properties.AccessTier.Value.ToString() : null;
                    blobInfo.Metadata = blob.Metadata;

                    result.Add(blobInfo);
                }
            }
            return result.OrderBy(b => b.Name);
        }

        public async Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken)
        {
            var result = new BlobContent();
            result.Name = blobName;
            LoadConfig();

            BlobClient blobClient = CreateBlobClient(containerName, blobName);

            var blobContent = await blobClient.DownloadContentAsync(cancellationToken);

            if (blobContent.HasValue)
                result.Content = blobContent.Value.Content.ToString();

            var properties = await blobClient.GetPropertiesAsync(null, cancellationToken);

            if (properties.HasValue)
                result.Metadata = properties.Value.Metadata;

            return result;
        }

        #endregion [ Public Methods - IContentsService implementation ]

        #region [ Private Methods ]
        private void LoadConfig()
        {
            this.configurationValues.ForceManagedIdentity = this.configuration.GetValue<bool>("ForceManagedIdentity");
            this.configurationValues.StorageAccountName = this.configuration.GetValue<string>("StorageAccountName");
            this.configurationValues.ContainerTypes = this.configuration.GetValue<string>("ContainerTypes")?
                    .ToLower()
                    .Split("|", StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
        }

        private BlobServiceClient CreateBlobServiceClient()
        {
            BlobServiceClient? blobServiceClient = null;

            blobServiceClient = new BlobServiceClient(
                new Uri(this.configurationValues.StorageBlobUrl),
                GetCredential(),
                GetClientOptions());

            return blobServiceClient;
        }

        private BlobContainerClient CreateBlobContainerClient(string containerName)
        {
            BlobContainerClient? blobContainerClient = null;

            blobContainerClient = new BlobContainerClient(
                new Uri($"{this.configurationValues.StorageBlobUrl}/{containerName}"),
                GetCredential(),
                GetClientOptions());

            return blobContainerClient;
        }

        private BlobClient CreateBlobClient(string containerName, string blobName)
        {
            BlobClient? blobClient = null;

            blobClient = new BlobClient(
                new Uri($"{this.configurationValues.StorageBlobUrl}/{containerName}/{blobName}"),
                GetCredential(),
                GetClientOptions());

            return blobClient;
        }

        private TokenCredential GetCredential()
        {
            TokenCredential credential = null;
            if (this.configurationValues.ForceManagedIdentity)
            {
                credential = new ManagedIdentityCredential();
            }
            else
            {
                credential = new DefaultAzureCredential();
            }
            return credential;
        }

        private BlobClientOptions GetClientOptions()
        {
            var options = new BlobClientOptions();
            options.Retry.Delay = TimeSpan.FromMilliseconds(250);
            options.Retry.MaxRetries = 5;
            options.Retry.Mode = RetryMode.Exponential;
            options.Retry.MaxDelay = TimeSpan.FromSeconds(5);
            return options;
        }

        #endregion [ Private Methods ]
    }
}
