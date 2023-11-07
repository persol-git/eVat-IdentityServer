using Newtonsoft.Json;

namespace Evat.IdentityServer.Models.DTO
{
    public class ForgotPasswordDto
    {
        [JsonProperty("username")]
        public string username { get; set; }

        [JsonProperty("email")]
        public string email { get; set; }

        [JsonProperty("returnUrl")]
        public string returnUrl { get; set; }
    }
}
