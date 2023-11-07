using Microsoft.EntityFrameworkCore;
using Serilog;
using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using Duende.IdentityServer.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Evat.IdentityServer.Models;
using IdentityModel;
using Evat.IdentityServer.Data;

namespace Evat.IdentityServer
{
    public class SeedData
    {
        public static void EnsureSeedData(WebApplication app)
        {
            using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();

                //var context = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                //var context2 = scope.ServiceProvider.GetService<ApplicationDbContext>();
                //context.Database.Migrate();
                //context2.Database.Migrate();


                //EnsureSeedData(context);

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                SeedTestUserData(userMgr);
            }
        }

        private static void EnsureSeedData(ConfigurationDbContext context)
        {

            if (!context.Clients.Any())
            {
                Log.Debug("Clients being populated");
                foreach (var client in Config.Clients.ToList())
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("Clients already populated");
            }

            if (!context.IdentityResources.Any())
            {
                Log.Debug("IdentityResources being populated");
                foreach (var resource in Config.IdentityResources.ToList())
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("IdentityResources already populated");
            }

            if (!context.ApiScopes.Any())
            {
                Log.Debug("ApiScopes being populated");
                foreach (var resource in Config.ApiScopes.ToList())
                {
                    context.ApiScopes.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiScopes already populated");
            }

            if (!context.ApiResources.Any())
            {
                Log.Debug("ApiResources being populated");
                foreach (var resource in Config.GetApiResources())
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }
            else
            {
                Log.Debug("ApiResources already populated");
            }

            if (!context.IdentityProviders.Any())
            {
                Log.Debug("OIDC IdentityProviders being populated");
                context.IdentityProviders.Add(new OidcProvider
                {
                    Scheme = "demoidsrv",
                    DisplayName = "IdentityServer",
                    Authority = "https://demo.duendesoftware.com",
                    ClientId = "login",
                }.ToEntity());
                context.SaveChanges();
            }
            else
            {
                Log.Debug("OIDC IdentityProviders already populated");
            }
        }


        public static void SeedTestUserData(UserManager<ApplicationUser> userManager)
        {

            var testUser = userManager.FindByNameAsync("persol").Result;

            if (testUser == null)
            {
                testUser = new ApplicationUser
                {
                    UserName = "persol",
                    Email = "michael.ameyaw@persol.net",
                    EmailConfirmed = true,
                };
                var result = userManager.CreateAsync(testUser, "P@$$w0rd@2023").Result;
                if (!result.Succeeded)
                {
                    Log.Error("something went wrong default user not writed");
                }

                result = userManager.AddClaimsAsync(testUser, new Claim[]{
                            new Claim(JwtClaimTypes.Name, "Michael Ameyaw"),
                            new Claim(JwtClaimTypes.GivenName, "Michael"),
                            new Claim(JwtClaimTypes.FamilyName, "Ameyaw"),
                            new Claim("permissions", "se"),
                            new Claim("permissions", "se-01"),
                            new Claim("permissions", "se-02"),
                            new Claim("permissions", "se-03"),
                            new Claim("permissions", "ac"),
                            new Claim("permissions", "ac-01"),
                            new Claim("permissions", "ac-02"),
                            new Claim("permissions", "ac-03"),
                            new Claim("permissions", "re"),
                            new Claim("permissions", "re-01"),
                            new Claim("permissions", "re-02"),
                            new Claim("permissions", "re-03"),
                            new Claim("permissions", "do"),
                            new Claim("permissions", "do-01"),
                            new Claim("permissions", "do-02"),
                            new Claim("permissions", "do-03"),
                            new Claim("permissions", "cu"),
                            new Claim("permissions", "cu-01"),
                            new Claim("permissions", "cu-02"),
                            new Claim("permissions", "cu-03"),
                            new Claim("permissions", "eq"),
                            new Claim("permissions", "eq-01"),
                            new Claim("permissions", "eq-02"),
                            new Claim("permissions", "eq-03"),
                            new Claim("permissions", "et-01"),
                            new Claim("permissions", "et-02"),
                            new Claim("permissions", "et-03"),
                            new Claim("permissions", "pe"),
                            new Claim("permissions", "pe-01"),
                            new Claim("permissions", "pe-02"),
                            new Claim("permissions", "pe-03"),
                            new Claim("permissions", "jo"),
                            new Claim("permissions", "jo-10"),
                            new Claim("permissions", "jo-11"),
                            new Claim("permissions", "jo-12"),
                            new Claim("permissions", "jo-13"),
                            new Claim("permissions", "jo-14"),
                            new Claim("permissions", "jo-15"),
                            new Claim("permissions", "fi"),
                            new Claim("permissions", "me"),
                            new Claim("permissions", "us"),
                            new Claim("permissions", "tr"),
                            new Claim("permissions", "cs"),
                            new Claim("company_id", new Guid("8c5df4a1-926e-4dc8-9847-7c406ce47fef").ToString()),
                            new Claim("company_mame", "GRA"),
                            new Claim("tin", "C0007220001"),

                        }).Result;

                if (!result.Succeeded)
                {
                    Log.Error("something went wrong default user claaims not writed");
                }
            }
            else
            {
                Log.Information("persol already exists");
            }
        }
    }
}