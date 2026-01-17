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
        /// Gets the replication policy identifier associated with the blob.
        /// </summary>
        public string? ReplicationPolicyId { get; internal set; }

        /// <summary>
        /// Gets the replication rule identifier associated with the blob.
        /// </summary>
        public string? ReplicationRuleId { get; internal set; }

        /// <summary>
        /// Gets the current replication status of the blob.
        /// </summary>
        public string? ReplicationStatus { get; internal set; }

        /// <summary>
        /// Gets the size of the blob in bytes.
        /// </summary>
        public long? Size { get; internal set; }

        /// <summary>
        /// Gets the access tier of the blob (e.g., Hot, Cool, Archive).
        /// </summary>
        public string? Tier { get; internal set; }
    }
}
