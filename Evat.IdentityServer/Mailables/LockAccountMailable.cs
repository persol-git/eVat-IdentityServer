using Coravel.Mailer.Mail;
using Evat.IdentityServer.Models.Mail;

namespace Evat.IdentityServer.Mailables
{
    public class LockAccountMailable : Mailable<LockAccountDto>
    {
        private readonly LockAccountDto _lockAccountDto;

        public LockAccountMailable(LockAccountDto lockAccountDto)
        {
            _lockAccountDto = lockAccountDto;
        }

        

        public override void Build()
        {
            this.To(_lockAccountDto.To)
               .From("evat@persol.net")
               .Subject("[PersonaX Standard] Account locked")
                .View("~/Views/Mail/LockAccountMailable.cshtml", _lockAccountDto);
        }
    }
}
