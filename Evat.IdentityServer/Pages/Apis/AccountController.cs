using Coravel.Mailer.Mail.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Evat.IdentityServer.Helper;
using Evat.IdentityServer.Mailables;
using Evat.IdentityServer.Models;
using Evat.IdentityServer.Models.DTO;
using Evat.IdentityServer.Models.Mail;
using System.Text;
using System.Text.Encodings.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Evat.IdentityServer.Pages.Apis
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMailer _mailer;

        public AccountController(UserManager<ApplicationUser> userManager, IMailer mailer)
        {
            _userManager = userManager;
            _mailer = mailer;
        }


        [HttpGet("GetAccountEmailWithPhone")]
        public async Task<IActionResult> Get([FromQuery] string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);

                if (user == null) return Ok(new { email = string.Empty, phone = string.Empty });
                if (user.Email == "" || user.PhoneNumber == "") return Ok(new { email = string.Empty, phone = string.Empty });

                user.Email = HelperExtensions.ShortEmailFormatter(user.Email);

                var check = HelperExtensions.PhoneNumberCheck(user.PhoneNumber);

                user.PhoneNumber = !check ? string.Empty : HelperExtensions.PhoneNumberFormatter(user.PhoneNumber);

                return Ok(new { email = user.Email, phone = user.PhoneNumber });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }

        [HttpPost("ForgotPasswordAjaxAsync")]
        public async Task<IActionResult> Post([FromBody] ForgotPasswordDto model)
        {
            string feedback;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.username);
                if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
                {
                    feedback = "user-not-exist-204";
                }
                else
                {
                    if (user.Email != model.email)
                    {
                        feedback = "email-mismatch-code-422";
                    }
                    else
                    {
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                        var callbackUrl = Url.Page(
                            "/Account/ResetPassword",
                            pageHandler: null,
                            values: new { code, username = user.UserName, model.returnUrl },
                            protocol: Request.Scheme);

                        try
                        {
                            await _mailer.SendAsync(new ForgotPasswordMailable(new ForgotPasswordMailDto
                            {
                                Name = user.FirstName + " " + user.LastName,
                                To = user.Email,
                                CallbackUrl = callbackUrl
                            }));

                            feedback = "email-sent-200";
                        }
                        catch (Exception)
                        {
                            feedback = "email-failed-422";
                        }

                    }
                }
            }
            else
            {
                feedback = "validation-error-code-422";
            }

            return Ok(feedback);
        }


        [HttpGet("GenerateAccountConfirmationCode")]
        public async Task<IActionResult> GenerateAccountConfirmationCode([FromQuery] string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null) return NoContent();

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                return Ok(new UserDto { Code = code });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }

    internal class UserDto
    {
        public string Code { get; set; }
    }
}
