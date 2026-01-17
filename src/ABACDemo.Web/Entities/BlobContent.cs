namespace ABACDemo.Web.Entities
{
    /// <summary>
    /// Represents the content and metadata of a blob stored in Azure Blob Storage.
    /// </summary>
    public class BlobContent
    {
        /// <summary>
        /// Gets or sets the name of the blob.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the content of the blob as a string.
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the blob as key-value pairs.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Gets or sets the indexes associated with the blob for search and filtering purposes.
        /// </summary>
        public IDictionary<string, string>? Indexes { get; set; }
    }
}
