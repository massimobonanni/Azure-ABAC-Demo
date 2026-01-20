using ABACDemo.Web.Entities;
using ABACDemo.Web.Models;

namespace ABACDemo.Web.Models.StorageController;

public class ContainerViewModel : ViewModelBase
{
    public string? ContainerName { get; set; }
    public DateTime Date { get; set; }
    public IEnumerable<BlobInfo>? Blobs { get; set; }
}

