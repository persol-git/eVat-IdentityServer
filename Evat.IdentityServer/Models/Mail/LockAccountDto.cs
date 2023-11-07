namespace Evat.IdentityServer.Models.Mail
{
    public class LockAccountDto
    {
        public string Name { get; set; }
        public string To { get; set; }
        public string UnblockUrl { get; set; }
    }
}
