using ABACDemo.Web.Entities;
using ABACDemo.Web.Interfaces;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace StorageContentPlatform.Web.Services
{
    /// <summary>
    /// Service for managing and accessing Azure Blob Storage contents.
    /// </summary>
    public class StorageContentsService : IContentsService
    {
        /// <summary>
        /// Configuration settings for the storage service.
        /// </summary>
        private class Configuration
        {
            /// <summary>
            /// Gets or sets a value indicating whether to force the use of Managed Identity for authentication.
            /// </summary>
            public bool ForceManagedIdentity { get; set; } = false;

            /// <summary>
            /// Gets or sets the Azure Storage account name.
            /// </summary>
            public string? StorageAccountName { get; set; }

            /// <summary>
            /// Gets or sets the collection of container types to filter.
            /// </summary>
            public IEnumerable<string>? ContainerTypes { get; set; }

            /// <summary>
            /// Gets the Azure Blob Storage URL based on the storage account name.
            /// </summary>
            public string StorageBlobUrl
            {
                get => $"https://{this.StorageAccountName}.blob.core.windows.net";
            }
        }

        private readonly IConfiguration configuration;
        private readonly Configuration configurationValues;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageContentsService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public StorageContentsService(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.configurationValues = new Configuration();
        }

        #region [ Public Methods - IContentsService implementation ]
        /// <summary>
        /// Retrieves a list of blob containers from Azure Storage.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="ContainerInfo"/> objects representing the containers.</returns>
        public async Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default)
        {
            // Initialize result collection
            var result = new List<ContainerInfo>();
            
            // Load configuration settings
            LoadConfig();
            
            // Create the blob service client for Azure Storage operations
            BlobServiceClient blobServiceClient = CreateBlobServiceClient();

            // Retrieve containers with metadata in pages of 100 items
            var resultSegment = blobServiceClient
                    .GetBlobContainersAsync(BlobContainerTraits.Metadata, null, cancellationToken)
                    .AsPages(default, 100);

            // Iterate through each page of containers
            await foreach (Azure.Page<BlobContainerItem> containerPage in resultSegment)
            {
                // Process each container in the current page
                foreach (BlobContainerItem containerItem in containerPage.Values)
                {
                    // Filter containers based on configured container types (if specified)
                    if (!this.configurationValues.ContainerTypes.Any() || containerItem.HasMetadataValues("containerType", this.configurationValues.ContainerTypes))
                    {
                        // Create container info object
                        var containerInfo = new ContainerInfo();
                        containerInfo.Name = containerItem.Name;
                        containerInfo.LastModified = containerItem.Properties.LastModified;
                        
                        // Format metadata as key=value pairs separated by semicolons
                        if (containerItem.Properties.Metadata != null && containerItem.Properties.Metadata.Any())
                            containerInfo.Metadata = containerItem.Properties.Metadata
                                     .Select((k, v) => $"{k}={v}").Aggregate((a, b) => $"{a};{b}");

                        // Add container to result collection
                        result.Add(containerInfo);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Retrieves a list of blobs from a specific container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="date">The date filter for blobs (currently not used in implementation).</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A collection of <see cref="BlobInfo"/> objects representing the blobs, ordered by name.</returns>
        public async Task<IEnumerable<ABACDemo.Web.Entities.BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default)
        {
            // Initialize result collection
            var result = new List<ABACDemo.Web.Entities.BlobInfo>();
            
            // Load configuration settings
            LoadConfig();

            // Create container client for the specified container
            BlobContainerClient containerClient = CreateBlobContainerClient(containerName);

            // Retrieve blobs with metadata in paginated format
            var blobPages = containerClient
                .GetBlobsAsync(new GetBlobsOptions() { Traits = BlobTraits.Metadata })
                .AsPages();

            // Iterate through each page of blobs
            await foreach (var blobPage in blobPages)
            {
                // Process each blob in the current page
                foreach (var blob in blobPage.Values)
                {
                    // Create blob info object and populate properties
                    var blobInfo = new ABACDemo.Web.Entities.BlobInfo();
                    blobInfo.Name = blob.Name;
                    blobInfo.LastModified = blob.Properties.LastModified;
                    blobInfo.Size = blob.Properties.ContentLength;
                    
                    // Set access tier if available, otherwise null
                    blobInfo.Tier = blob.Properties.AccessTier.HasValue ?
                        blob.Properties.AccessTier.Value.ToString() : null;
                    blobInfo.Metadata = blob.Metadata;

                    // Add blob to result collection
                    result.Add(blobInfo);
                }
            }
            
            // Return blobs ordered alphabetically by name
            return result.OrderBy(b => b.Name);
        }

        /// <summary>
        /// Retrieves the content and metadata of a specific blob.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>A <see cref="BlobContent"/> object containing the blob's content, metadata, and tags.</returns>
        public async Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken)
        {
            // Initialize result object with blob name
            var result = new BlobContent();
            result.Name = blobName;
            
            // Load configuration settings
            LoadConfig();

            // Create blob client for the specific blob
            BlobClient blobClient = CreateBlobClient(containerName, blobName);

            // Download blob content
            var blobContent = await blobClient.DownloadContentAsync(cancellationToken);

            // Extract content and metadata from download response
            if (blobContent.HasValue)
            {
                // Convert blob content to string
                result.Content = blobContent.Value.Content.ToString();
                
                // Get metadata from download details if available
                if (blobContent.Value.Details.Metadata != null)
                {
                    result.Metadata = blobContent.Value.Details.Metadata;
                }
            }

            // If metadata was not retrieved from download, get it from properties
            if (result.Metadata == null)
            {
                var properties = await blobClient.GetPropertiesAsync(null, cancellationToken);

                if (properties.HasValue)
                    result.Metadata = properties.Value.Metadata;
            }
            
            // Attempt to retrieve blob index tags
            try
            {
                var tags = await blobClient.GetTagsAsync(cancellationToken: cancellationToken);
                if (tags.HasValue && tags.Value.Tags != null && tags.Value.Tags.Any())
                {
                    result.Indexes = tags.Value.Tags;
                }
            }
            catch (Exception ex)
            {
                // If tags cannot be retrieved (e.g., permissions issue), set empty dictionary
                result.Indexes = new Dictionary<string, string>();
            }
            return result;
        }

        #endregion [ Public Methods - IContentsService implementation ]

        #region [ Private Methods ]
        /// <summary>
        /// Loads configuration values from the application configuration.
        /// </summary>
        private void LoadConfig()
        {
            // Load authentication mode (Managed Identity vs Default Azure Credential)
            this.configurationValues.ForceManagedIdentity = this.configuration.GetValue<bool>("ForceManagedIdentity");
            
            // Load storage account name
            this.configurationValues.StorageAccountName = this.configuration.GetValue<string>("StorageAccountName");
            
            // Load and parse container types filter (pipe-separated values)
            // Convert to lowercase, split by pipe, remove empty entries and trim whitespace
            this.configurationValues.ContainerTypes = this.configuration.GetValue<string>("ContainerTypes")?
                    .ToLower()
                    .Split("|", StringSplitOptions.RemoveEmptyEntries & StringSplitOptions.TrimEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlobServiceClient"/> with configured credentials and options.
        /// </summary>
        /// <returns>A configured <see cref="BlobServiceClient"/> instance.</returns>
        private BlobServiceClient CreateBlobServiceClient()
        {
            BlobServiceClient? blobServiceClient = null;

            // Create blob service client with storage URL, credentials, and retry options
            blobServiceClient = new BlobServiceClient(
                new Uri(this.configurationValues.StorageBlobUrl),
                GetCredential(),
                GetClientOptions());

            return blobServiceClient;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlobContainerClient"/> for a specific container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>A configured <see cref="BlobContainerClient"/> instance.</returns>
        private BlobContainerClient CreateBlobContainerClient(string containerName)
        {
            BlobContainerClient? blobContainerClient = null;

            // Create blob container client with container URL, credentials, and retry options
            blobContainerClient = new BlobContainerClient(
                new Uri($"{this.configurationValues.StorageBlobUrl}/{containerName}"),
                GetCredential(),
                GetClientOptions());

            return blobContainerClient;
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlobClient"/> for a specific blob.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob.</param>
        /// <returns>A configured <see cref="BlobClient"/> instance.</returns>
        private BlobClient CreateBlobClient(string containerName, string blobName)
        {
            BlobClient? blobClient = null;

            // Create blob client with blob URL, credentials, and retry options
            blobClient = new BlobClient(
                new Uri($"{this.configurationValues.StorageBlobUrl}/{containerName}/{blobName}"),
                GetCredential(),
                GetClientOptions());

            return blobClient;
        }

        /// <summary>
        /// Gets the appropriate Azure token credential based on configuration.
        /// </summary>
        /// <returns>A <see cref="TokenCredential"/> for Azure authentication.</returns>
        private TokenCredential GetCredential()
        {
            TokenCredential credential = null;
            
            // Choose authentication method based on configuration
            if (this.configurationValues.ForceManagedIdentity)
            {
                // Use Managed Identity (for Azure-hosted apps)
                credential = new ManagedIdentityCredential();
            }
            else
            {
                // Use Default Azure Credential (tries multiple authentication methods)
                credential = new DefaultAzureCredential();
            }
            return credential;
        }

        /// <summary>
        /// Creates and configures blob client options with retry policies.
        /// </summary>
        /// <returns>A configured <see cref="BlobClientOptions"/> instance with exponential retry strategy.</returns>
        private BlobClientOptions GetClientOptions()
        {
            var options = new BlobClientOptions();
            
            // Configure retry policy for blob operations
            options.Retry.Delay = TimeSpan.FromMilliseconds(250);  // Initial delay between retries
            options.Retry.MaxRetries = 5;                          // Maximum number of retry attempts
            options.Retry.Mode = RetryMode.Exponential;            // Exponential backoff strategy
            options.Retry.MaxDelay = TimeSpan.FromSeconds(5);      // Maximum delay between retries
            
            return options;
        }

        #endregion [ Private Methods ]
    }
}
