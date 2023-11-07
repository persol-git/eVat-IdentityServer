using System.ComponentModel.DataAnnotations;
using System.Text;
using Coravel.Mailer.Mail.Interfaces;
using Evat.IdentityServer.Mailables;
using Evat.IdentityServer.Models;
using Evat.IdentityServer.Models.Mail;
using Evat.IdentityServer.Pages.Account.Login;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace Evat.IdentityServer.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailer _mailer;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager, IMailer mailer, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _mailer = mailer;
            _signInManager = signInManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

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
            public string ReturnUrl { get; set; }
        }


        public async Task<IActionResult> OnGetAsync(string token = null, string username = null, string returnUrl = null)
        {
            if (username == null || token == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, $"Unable to load user");
                return Page();
                // return NotFound($"Unable to load user with Name '{username}'.");
            }


            Input = new InputModel
            {
                Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)),
                UserName = username,
                ReturnUrl = returnUrl ?? "~/",
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByNameAsync(Input.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.UsernameRequiredErrorMessage);
                return Page();
                // Don't reveal that the user does not exist
                //return RedirectToPage("./AccountConfirmationFailed");
            }

            if (user.EmailConfirmed == true)
            {
                ModelState.AddModelError(string.Empty, LoginOptions.AccountAlreadyConfirmedErrorMessage);
                return Page();
            }


            var result = await _userManager.ConfirmEmailAsync(user, Input.Code);
            if (result.Succeeded)
            {
                var change = await _userManager.AddPasswordAsync(user, Input.Password);
                if (change.Succeeded)
                {
                    var login = await _signInManager.PasswordSignInAsync(user, Input.Password, isPersistent: false, true);

                    if (login.Succeeded)
                    {
                        await _mailer.SendAsync(new AccountConfirmedMailable(new AccountConfirmedDto
                        {
                            Name = user.FirstName + " " + user.LastName,
                            To = user.Email
                        }));

                        return RedirectToPage("./AccountConfirmationSucess");
                    }

                    ModelState.AddModelError(string.Empty, "Account confirmed but failed to login.");
                }

                ModelState.AddModelError(string.Empty, "Failed to change password.");
            }

            foreach (var error in result.Errors)
            {
                if (error.Description.Contains("Invalid token."))
                {
                    ModelState.AddModelError(string.Empty, "Confirmation link has expired");
                }else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

    }
}
