using Coravel;
using Duende.IdentityServer;
using Evat.IdentityServer.Cors;
using Evat.IdentityServer.Data;
using Evat.IdentityServer.Helper;
using Evat.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Serilog;
using System.Configuration;
using System.Data;

namespace Evat.IdentityServer
{
    internal static class HostingExtensions
    {
        public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
        {
            var idpConnectionString = builder.Configuration.GetConnectionString("IdentityServerConnection");
            var idcoreConnectionString = builder.Configuration.GetConnectionString("IdentityCoreConnection");


            const int considerPwned = 1000;
            builder.Services.AddPwnedPasswordHttpClient(minimumFrequencyToConsiderPwned: considerPwned)
                .AddTransientHttpErrorPolicy(p => p.RetryAsync(3))
                .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(1)));


            builder.Services.AddRazorPages();
            builder.Services.AddCorsOption(builder.Configuration);

            var config = new ApplicationUrls();
            builder.Configuration.Bind("ApplicationUrls", config);
            builder.Services.AddSingleton(config);

            builder.Services.AddScoped<IDbConnection, SqlConnection>(db
                => new SqlConnection(idcoreConnectionString)
                );

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(idcoreConnectionString));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(30);

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.SignIn.RequireConfirmedEmail = false;
                options.User.RequireUniqueEmail = false;



                options.Tokens.ProviderMap.Add("CustomAccountConfirmation",
                   new TokenProviderDescriptor(
                       typeof(CustomAccountConfirmationTokenProvider<ApplicationUser>)));
                options.Tokens.EmailConfirmationTokenProvider = "CustomAccountConfirmation";


            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddPwnedPasswordValidator<ApplicationUser>(options =>
                {
                    options.ErrorMessage =
                        $"Cannot use passwords that have been hacked or pwned more than {considerPwned} times.";
                })
                .AddPasswordValidator<CustomPasswordValidator>()
                .AddUserValidator<CustomUserValidator>()
                .AddDefaultTokenProviders();

            builder.Services.AddTransient<CustomAccountConfirmationTokenProvider<ApplicationUser>>();

            //builder.Services.AddScoped(typeof(ISettingSupplier), typeof(SettingSupplier));


            builder.Services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                    options.IssuerUri = builder.Configuration["ApplicationUrls:SelfUrl"];
                })
                .AddProfileService<ProfileService>()

                .AddAspNetIdentity<ApplicationUser>()
                // this adds the config data from DB (clients, resources, CORS)
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(idpConnectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));
                })
                // .AddProfileService<ProfileService>()
                // this is something you will want in production to reduce load on and requests to the DB
                //.AddConfigurationStoreCache()
                //
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlServer(idpConnectionString, dbOpts => dbOpts.MigrationsAssembly(typeof(Program).Assembly.FullName));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    options.RemoveConsumedTokens = true;
                });


            //builder.Services.AddAuthentication(options =>
            //{
            //    //options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            //})
            //.AddCookie("Cookies", options =>
            //{
            //    options.Cookie.Name = "eVatApps";
            //    options.Cookie.SameSite = SameSiteMode.None;
            //    options.LoginPath = $"/Account/Login";
            //});

            
           builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                   .AddCookie(options =>
                   {
                       // add an instance of the patched manager to the options:
                       options.CookieManager = new ChunkingCookieManager();

                       options.Cookie.HttpOnly = true;
                       options.Cookie.SameSite = SameSiteMode.None;
                       options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                   });

          // builder.Services.AddAuthentication();
            //.AddGoogle(options =>
            //{
            //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

            //    // register your IdentityServer with Google at https://console.developers.google.com
            //    // enable the Google+ API
            //    // set the redirect URI to https://localhost:5001/signin-google
            //    options.ClientId = "copy client ID from Google here";
            //    options.ClientSecret = "copy client secret from Google here";
            //});


            builder.Services.AddMailer(builder.Configuration);

            return builder.Build();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                RequireHeaderSymmetry = false
            };

            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardOptions);
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.UseCorsOption(app.Configuration);
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.MapControllers();

            app.MapRazorPages()
                .RequireAuthorization();




            return app;
        }
    }
}