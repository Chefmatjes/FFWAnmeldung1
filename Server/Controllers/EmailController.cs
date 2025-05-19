using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly ILogger<EmailController> _logger;
        private readonly IConfiguration _configuration;

        public EmailController(ILogger<EmailController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                _logger.LogInformation("Email send request received: FilePath={FilePath}", request.FilePath);

                if (string.IsNullOrEmpty(request.FilePath) || !System.IO.File.Exists(request.FilePath))
                {
                    _logger.LogWarning("File not found: {FilePath}", request.FilePath);
                    return BadRequest(new { Error = "File not found" });
                }

                // Get email settings from configuration
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPortStr = _configuration["Email:SmtpPort"];
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderName = _configuration["Email:SenderName"];
                var senderPassword = _configuration["Email:SenderPassword"];
                var defaultRecipientEmail = _configuration["Email:DefaultRecipientEmail"];

                // Validate required configuration
                if (string.IsNullOrEmpty(smtpServer) || 
                    string.IsNullOrEmpty(smtpPortStr) || 
                    string.IsNullOrEmpty(senderEmail) || 
                    string.IsNullOrEmpty(senderPassword))
                {
                    _logger.LogError("Missing required email configuration settings");
                    return StatusCode(500, new { Error = "Email configuration is incomplete" });
                }

                var smtpPort = int.Parse(smtpPortStr);
                
                // Always use default recipient (vorstand@ff-apfeltrang.de)
                var recipient = defaultRecipientEmail ?? "vorstand@ff-apfeltrang.de";

                _logger.LogInformation("Sending email to {Recipient} with file {FilePath}", recipient, request.FilePath);

                // Create the mail message
                using (var mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail, senderName ?? "Feuerwehr Anmeldung");
                    mail.To.Add(recipient);
                    mail.Subject = "Neue Beitrittserklärung zur Freiwilligen Feuerwehr";
                    mail.Body = "Im Anhang finden Sie eine neue Beitrittserklärung zur Freiwilligen Feuerwehr.";
                    mail.IsBodyHtml = false;

                    // Add the file attachment
                    var attachment = new Attachment(request.FilePath);
                    mail.Attachments.Add(attachment);

                    // Send the email
                    using (var smtp = new SmtpClient(smtpServer, smtpPort))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true;

                        await smtp.SendMailAsync(mail);
                        _logger.LogInformation("Email sent successfully to {Recipient}", recipient);
                    }
                }

                return Ok(new { Success = true, RecipientEmail = recipient });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email");
                return StatusCode(500, new { Error = "Failed to send email", Message = ex.Message });
            }
        }
    }

    public class EmailRequest
    {
        public string? FilePath { get; set; }
        // RecipientEmail property is kept for backward compatibility but will be ignored
        public string? RecipientEmail { get; set; }
    }
} 