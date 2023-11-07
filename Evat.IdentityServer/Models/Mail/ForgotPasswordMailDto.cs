namespace Evat.IdentityServer.Models.Mail
{
    public class ForgotPasswordMailDto
    {
        public string Name { get; set; }
        public string To { get; set; }
        public string CallbackUrl { get; set; }
    }
}
