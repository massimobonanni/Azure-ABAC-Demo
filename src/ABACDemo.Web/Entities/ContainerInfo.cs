namespace ABACDemo.Web.Entities
{
    /// <summary>
    /// Represents information about a storage container.
    /// </summary>
    public class ContainerInfo
    {
        /// <summary>
        /// Gets or sets the name of the container.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the container was last modified.
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the container.
        /// </summary>
        public string? Metadata { get; set; }
    }
}
