using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.Mail;

namespace Evat.IdentityServer.Mailables
{
    public class AccountConfirmedMailable : Mailable<AccountConfirmedDto>
    {
        private readonly AccountConfirmedDto _twoFactorConfirmedDto;

        public AccountConfirmedMailable(AccountConfirmedDto twoFactorConfirmedDto)
        {
            _twoFactorConfirmedDto = twoFactorConfirmedDto;
        }

        public override void Build()
        {
            this.To(_twoFactorConfirmedDto.To)
               .From("evat@persol.net")
               .Subject("[PersonaX Standard] Account Confirmation")
                .View("~/Views/Mail/AccountConfirmedMailable.cshtml", _twoFactorConfirmedDto);
        }
    }
}
