using ABACDemo.Web.Entities;

namespace ABACDemo.Web.Models.StorageController;

public class IndexViewModel : ViewModelBase
{
    public IEnumerable<ContainerInfo>? Containers { get; set; }
}

