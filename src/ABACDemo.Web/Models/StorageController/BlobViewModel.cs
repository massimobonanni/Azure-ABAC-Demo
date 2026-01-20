

using ABACDemo.Web.Entities;

namespace ABACDemo.Web.Models.StorageController;

public class BlobViewModel : ViewModelBase
{
    public string? ContainerName { get; internal set; }
    public BlobContent? Blob { get; internal set; }
}
