using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.Mail;

namespace Evat.IdentityServer.Mailables
{
    public class ForgotPasswordMailable : Mailable<ForgotPasswordMailDto>
    {
        private readonly ForgotPasswordMailDto _forgotPasswordDto;

        public ForgotPasswordMailable(ForgotPasswordMailDto forgotPasswordDto)
        {
            _forgotPasswordDto = forgotPasswordDto;
        }

        public override void Build()
        {
            this.To(_forgotPasswordDto.To)
                .From("evat@persol.net")
                .Subject("[PersonaX Standard] Reset Account Password")
                .View("~/Views/Mail/ForgotPasswordMailable.cshtml", _forgotPasswordDto);
        }
    }
}
