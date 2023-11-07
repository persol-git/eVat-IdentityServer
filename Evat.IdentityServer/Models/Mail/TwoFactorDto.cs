namespace Evat.IdentityServer.Models.Mail
{
    public class TwoFactorDto
    {
        public string To { get; set; }
        public string Name { get; set; }
        public string TwoFactorCode { get; set; }
    }
}
