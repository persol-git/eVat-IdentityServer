using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.DTO;

namespace Evat.IdentityServer.Mailables
{
    public class OnSucessAccountActivationMailable : Mailable<OnSucessActivationDto>
    {
        private readonly OnSucessActivationDto _onSucessActivationDto;

        public OnSucessAccountActivationMailable(OnSucessActivationDto onSucessActivationDto)
        {
            _onSucessActivationDto = onSucessActivationDto;
        }

        public override void Build()
        {
            this.To(_onSucessActivationDto.To)
               .From("evat@persol.net")
               .Subject("[PersonaX Standard] Account Activated")
                .View("~/Views/Mail/OnSucessAccountActivationMailable.cshtml", _onSucessActivationDto);
        }
    }
}
