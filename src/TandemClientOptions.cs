namespace Autodesk.Tandem.Client
{
    public class TandemClientOptions
    {
        public TandemClientOptions()
        {
            BaseAddress = "https://tandem.autodesk.com/";
        }

        public string BaseAddress { get; set; }
    }
}
