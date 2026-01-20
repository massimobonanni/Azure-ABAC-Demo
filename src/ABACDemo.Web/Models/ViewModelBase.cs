namespace ABACDemo.Web.Models
{
    public abstract class ViewModelBase
    {
        public bool HasError => Exception != null || !string.IsNullOrEmpty(Message);

        public Exception? Exception { get; set; } = null;

        public string? Message { get; set; } = null;
    }
}
