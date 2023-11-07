namespace Evat.IdentityServer.Data
{
    public class ApplicationUrls
    {
        public static ApplicationUrls Current;

        public ApplicationUrls()
        {
            Current = this;
        }

        public string SelfUrl { get; set; }
        public string CentralUrl { get; set; }
    }
}
