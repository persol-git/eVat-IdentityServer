namespace Evat.IdentityServer.Cors.Option
{
    public class CorsOptions
    {
        public bool Enabled { get; set; } = false;
        public string Name { get; set; } = "EasySharp";

        public string[] Links { get; set; }
    }
}