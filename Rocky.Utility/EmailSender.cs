using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private MailSettings _mailSettings;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _mailSettings = _configuration.GetSection("MailJet").Get<MailSettings>();

            Debug.WriteLine("Mail has been sent!");
            // throw new System.NotImplementedException();
        }
    }
}
