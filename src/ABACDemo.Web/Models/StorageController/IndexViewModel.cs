using ABACDemo.Web.Entities;

namespace ABACDemo.Web.Models.StorageController;

public class IndexViewModel
{
    public IEnumerable<ContainerInfo>? Containers { get; set; }
}

