using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.Mail;

namespace Evat.IdentityServer.Mailables
{
    public class TwoFactorMailable : Mailable<TwoFactorDto>
    {
        private readonly TwoFactorDto _twoFactorDto;

        public TwoFactorMailable(TwoFactorDto twoFactorDto)
        {
            _twoFactorDto = twoFactorDto;
        }

        public override void Build()
        {
            this.To(_twoFactorDto.To)
               .From("evat@persol.net")
               .Subject("[PersonaX Standard] Two Factor Authentication")
                .View("~/Views/Mail/TwoFactorMailable.cshtml", _twoFactorDto);
        }
    }
}
