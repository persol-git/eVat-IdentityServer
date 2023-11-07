using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Evat.IdentityServer.Models;
using Evat.IdentityServer.Mailables;
using Evat.IdentityServer.Models.Mail;
using Coravel.Mailer.Mail.Interfaces;
using Evat.IdentityServer.Helper;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Duende.IdentityServer.Stores;
using Evat.IdentityServer.Pages.Account.Login;

namespace Evat.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailer _mailer;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IIdentityProviderStore _identityProviderStore;



        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, IMailer mailer, IIdentityServerInteractionService interaction, IClientStore clientStore, IEventService events, IAuthenticationSchemeProvider schemeProvider, IIdentityProviderStore identityProviderStore)
        {
            _userManager = userManager;
            _mailer = mailer;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _schemeProvider = schemeProvider;
            _identityProviderStore = identityProviderStore;
        }

        public ViewModel View { get; set; }


        [BindProperty]
        public InputModel Input { get; set; }

        public class ViewModel
        {
            public bool AllowRememberLogin { get; set; } = true;
            public bool EnableLocalLogin { get; set; } = true;

            public IEnumerable<ExternalProvider> ExternalProviders { get; set; } = Enumerable.Empty<ExternalProvider>();
            public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(x => !string.IsNullOrWhiteSpace(x.DisplayName));

            public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
            public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;

            public class ExternalProvider
            {
                public string DisplayName { get; set; }
                public string AuthenticationScheme { get; set; }
            }
        }

        public class InputModel
        {
            [Required]
            public string UserName { get; set; }

            [EmailAddress]
            public string Email { get; set; }
            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            if (returnUrl != null)
            {
                Input = new InputModel
                {
                    ReturnUrl = returnUrl
                };

                await BuildModelAsync(returnUrl);
            }

            return Page();

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                if (returnUrl == null)
                { }


                var user = await _userManager.FindByNameAsync(Input.UserName);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }


                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { code, username = user.UserName },
                    protocol: Request.Scheme);


                await _mailer.SendAsync(new ForgotPasswordMailable(new ForgotPasswordMailDto
                {
                    Name = user.FirstName + " " + user.LastName,
                    To = user.Email,
                    CallbackUrl = callbackUrl
                }));

                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }

        private async Task BuildModelAsync(string returnUrl)
        {
            Input = new InputModel
            {
                ReturnUrl = returnUrl
            };

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
            {
                var local = context.IdP == Duende.IdentityServer.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                View = new ViewModel
                {
                    EnableLocalLogin = local,
                };

                Input.Email = context.LoginHint;

                if (!local)
                {
                    View.ExternalProviders = new[] { new ViewModel.ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes
                .Where(x => x.DisplayName != null)
                .Select(x => new ViewModel.ExternalProvider
                {
                    DisplayName = x.DisplayName ?? x.Name,
                    AuthenticationScheme = x.Name
                }).ToList();

            var dynamicSchemes = (await _identityProviderStore.GetAllSchemeNamesAsync())
                .Where(x => x.Enabled)
                .Select(x => new ViewModel.ExternalProvider
                {
                    AuthenticationScheme = x.Scheme,
                    DisplayName = x.DisplayName
                });
            providers.AddRange(dynamicSchemes);


            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            View = new ViewModel
            {
                AllowRememberLogin = LoginOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && LoginOptions.AllowLocalLogin,
                ExternalProviders = providers.ToArray()
            };
        }

    }
}
