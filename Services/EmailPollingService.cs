using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ATSProject.Services
{
    public class EmailPollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EmailPollingService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("📡 EmailPollingService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                        await emailService.FetchEmailsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error fetching emails: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            Console.WriteLine("📡 EmailPollingService stopped.");
        }
    }
}
