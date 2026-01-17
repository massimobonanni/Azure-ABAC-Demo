using ABACDemo.Web.Entities;

namespace ABACDemo.Web.Interfaces
{
    /// <summary>
    /// Defines a service contract for managing and accessing Azure Storage containers and blobs content.
    /// </summary>
    public interface IContentsService
    {
        /// <summary>
        /// Retrieves a collection of all storage containers that match the configured container types.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="ContainerInfo"/> objects,
        /// each representing a container with its name, last modified date, and metadata.
        /// </returns>
        Task<IEnumerable<ContainerInfo>> GetContainersAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a collection of blobs from a specified container that match the specified date prefix.
        /// </summary>
        /// <param name="containerName">The name of the container from which to retrieve blobs.</param>
        /// <param name="date">The date used to filter blobs. Blobs are filtered by a prefix in the format "yyyyMMdd".</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="BlobInfo"/> objects,
        /// each containing blob metadata including name, size, tier, replication status, and last modified timestamp.
        /// </returns>
        Task<IEnumerable<BlobInfo>> GetBlobsAsync(string containerName, DateTime date, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the content and metadata of a specific blob from a container.
        /// </summary>
        /// <param name="containerName">The name of the container that contains the blob.</param>
        /// <param name="blobName">The name of the blob to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a <see cref="BlobContent"/> object
        /// with the blob's name, content, metadata, and index information.
        /// </returns>
        Task<BlobContent> GetBlobAsync(string containerName, string blobName, CancellationToken cancellationToken = default);
    }
}
