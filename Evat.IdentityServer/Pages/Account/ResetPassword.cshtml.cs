using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coravel.Mailer.Mail.Interfaces;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using Evat.IdentityServer.Mailables;
using Evat.IdentityServer.Models;
using Evat.IdentityServer.Models.DTO;
using Evat.IdentityServer.Models.Mail;
using Evat.IdentityServer.Pages.Account.Login;

namespace Evat.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailer _mailer;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IIdentityProviderStore _identityProviderStore;

        public ResetPasswordModel(UserManager<ApplicationUser> userManager, IMailer mailer, IIdentityServerInteractionService interaction, IClientStore clientStore, IEventService events, IAuthenticationSchemeProvider schemeProvider, IIdentityProviderStore identityProviderStore, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _mailer = mailer;
            _interaction = interaction;
            _clientStore = clientStore;
            _events = events;
            _schemeProvider = schemeProvider;
            _identityProviderStore = identityProviderStore;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public ViewModel View { get; set; }

        public class InputModel
        {

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }

            public string UserName { get; set; }
            public string Phone { get; set; }
            public string Type { get; set; }

            public string ReturnUrl { get; set; }
        }

        public async Task<IActionResult> OnGet(string code = null, string username = null, string type = null, string returnUrl = null)
        {
            if (code == null)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.CodeRequiredErrorMessage);
                return Page();
            }
            else if (username == null)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.UsernameRequiredErrorMessage);
                return Page();
            }
            else if (returnUrl == null)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.RedirectRequiredErrorMessage);
                return Page();
            }
            else
            {
                await BuildModelAsync(returnUrl);

                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)),
                    UserName = username,
                    Type = type ?? "",
                    ReturnUrl = returnUrl ?? "~/",
                };

                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByNameAsync(Input.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.UsernameRequiredErrorMessage);
                return Page();

                //user does not exist
                //return RedirectToPage("./ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                if (Input.Type == "unblock")
                {
                    var lockoutDisable = await _userManager.SetLockoutEndDateAsync(user, DateTime.Now);
                    if (lockoutDisable.Succeeded)
                    {
                        var enable2fa = await _userManager.SetTwoFactorEnabledAsync(user, true);
                        if (enable2fa.Succeeded)
                        {

                            await _mailer.SendAsync(new OnSucessAccountActivationMailable(new OnSucessActivationDto
                            {
                                Name = user.FirstName + " " + user.LastName,
                                To = user.Email
                            }));

                            return RedirectToPage("./TwoFactorConfirmed");
                        }
                        {
                            return RedirectToPage("./TwoFactorFailed");
                        }
                    }
                    else
                    {
                        //lock out is not enable 
                        ModelState.AddModelError(string.Empty, LoginOptions.LockOutFailedErrorMessage);
                        return Page();
                        //return RedirectToPage("./LockoutFailed");
                    }



                }
                else
                {

                    await _signInManager.SignInAsync(user, false);

                    await _mailer.SendAsync(new OnResetSucessMailable(new OnResetSucessDto
                    {
                        Name = user.FirstName + " " + user.LastName,
                        To = user.Email
                    }));

                    if (context != null)
                    {
                        if (context.IsNativeClient())
                        {
                            // The client is native, so this change in how to
                            // return the response is for better UX for the end user.
                            return this.LoadingPage(Input.ReturnUrl);
                        }

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        return Redirect(Input.ReturnUrl);
                    }

                    // request for a local page
                    if (Url.IsLocalUrl(Input.ReturnUrl))
                    {
                        return Redirect(Input.ReturnUrl);
                    }
                    else if (string.IsNullOrEmpty(Input.ReturnUrl))
                    {
                        return Redirect("~/");
                    }
                    else
                    {
                        // user might have clicked on a malicious link - should be logged
                        throw new Exception("invalid return URL");
                    }

                    // return RedirectToPage("./ResetPasswordConfirmation");
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
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

                Input.UserName = context.LoginHint;

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
