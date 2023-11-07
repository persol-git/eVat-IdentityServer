using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using IdentityModel;

namespace Evat.IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
           new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
                new IdentityResource(
                    "evat_profile",
                        new List<string>{
                            "CompanyReference",
                            JwtClaimTypes.Role
                        })
            };
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
               // use resources if we will configure more scopes
                new ApiResource("evat_modular_api", "Evat Resource API")
                {
                    Scopes = { "evat_modular_api" }
                }
            };
        }
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("evat_modular_api")
                {
                    UserClaims = {"permissions" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = "evat-api-swagger",
                    ClientSecrets = { new Secret("yBGX2m)JxZxc^Io7#m12'2{8_oZE.3".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code,
                    AllowedCorsOrigins = new[]
                    {
                        "http://localhost:44328",
                        "http://localhost:9080",
                        "http://psl-app-vm3",
                        "https://evat-api.persolqa.com",

                    },
                    RedirectUris =
                        new[]
                        {
                            "http://localhost:44328/swagger/oauth2-redirect.html",
                            "http://psl-app-vm3/evat-api/swagger/oauth2-redirect.html",
                            "https://evat-api.persolqa.com/swagger/oauth2-redirect.html",
                            "http://localhost:9080/swagger-ui/oauth2-redirect.html",
                            "http://paul-live/swagger-ui/oauth2-redirect.html",
                        },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "evat_modular_api"
                    },
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true
                },
                new Client
                {
                    ClientId = "standard-m2m",
                    ClientName = "Standard Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("#vD|n?rTV*W@FdE+R*wv_d^uU04{Gj".Sha256()) },

                    AllowedScopes = { "evat_modular_api" }
                },
                new Client
                {
                    ClientId = "evat-standard-ui",
                    ClientName = "Evat Standard",

                    AllowedGrantTypes = GrantTypes.Code,

                    ClientSecrets =
                    {
                        new Secret("L#?+;@e&?7D40p0kYO93e>u}18qI7^".Sha256())
                    },

                    RedirectUris =
                    {
                        "http://localhost:3001/signin-callback",
                    },
                    PostLogoutRedirectUris =
                    {
                        "http://localhost:3001/login"
                    },
                    AllowedCorsOrigins = new[]
                    {

                        "http://localhost:3001",
                        "http://psl-app-vm3",
                        "https://evat-api.persolqa.com",

                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "evat_modular_api",
                    },
                    AllowOfflineAccess=true,
                    AlwaysSendClientClaims = true,
                    AlwaysIncludeUserClaimsInIdToken = true
                },
            };
    }
}