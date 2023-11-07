using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.Mail;

namespace Evat.IdentityServer.Mailables
{
    public class OnResetSucessMailable : Mailable<OnResetSucessDto>
    {
        private readonly OnResetSucessDto _onResetSucessDto;

        public OnResetSucessMailable(OnResetSucessDto onResetSucessDto)
        {
            _onResetSucessDto = onResetSucessDto;
        }

        public override void Build()
        {
            this.To(_onResetSucessDto.To)
               .From("evat@persol.net")
               .Subject("[PersonaX Standard] Reset Account Password Changed")
               .View("~/Views/Mail/OnResetSucessMailable.cshtml", _onResetSucessDto);
        }
    }
}
