namespace TandemSDK.Models
{
    public class System : ElementBase
    {
        public System(ElementBase element)
        : base(element)
        {
        }

        public string? Id { get; set; }
        public string? SystemClass { get; set; }
    }
}
