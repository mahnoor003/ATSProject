using ATSProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATSProject.Controllers
{
    [ApiController]
    [Route("api/email")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        public EmailController(EmailService emailService) => _emailService = emailService;

        [HttpGet("fetch")]
        public async Task<IActionResult> Fetch()
        {
            await _emailService.FetchEmailsAsync();
            return Ok(new { message = "Fetched & processed unread emails." });
        }

        // keep this around if you want to verify Gmail returns mail
        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            await _emailService.FetchEmailsTestAsync();
            return Ok(new { message = "Listed unread emails to console." });
        }
    }
}
