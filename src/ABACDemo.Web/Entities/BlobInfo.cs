namespace ABACDemo.Web.Entities
{
    /// <summary>
    /// Represents metadata and properties of a blob stored in Azure Storage.
    /// </summary>
    public class BlobInfo
    {
        /// <summary>
        /// Gets the name of the blob.
        /// </summary>
        public string? Name { get; internal set; }

        /// <summary>
        /// Gets the date and time when the blob was last modified.
        /// </summary>
        public DateTimeOffset? LastModified { get; internal set; }

        /// <summary>
        /// Gets the size of the blob in bytes.
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// Gets the access tier of the blob (e.g., Hot, Cool, Archive).
        /// </summary>
        public string? Tier { get; internal set; }

        /// <summary>
        /// Gets the metadata associated with the blob.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; internal set; }

        /// <summary>
        /// Gets a string representation of the blob metadata, formatted as key-value pairs.
        /// </summary>
        /// <remarks>
        /// Returns metadata as a semicolon-separated string where each pair is formatted as "key=value".
        /// Returns null if the metadata dictionary is null or empty.
        /// </remarks>
        /// <example>
        /// For metadata with keys "Department" and "Project", the result would be: "Department=IT ; Project=Demo"
        /// </example>
        public string? MetadataAsString
        {
            get
            {
                if (Metadata == null || !Metadata.Any())
                    return null;
                return Metadata
                         .Select(k => $"{k.Key}={k.Value}")
                         .Aggregate((a, b) => $"{a};{b}");
            }
        }

    }
}
