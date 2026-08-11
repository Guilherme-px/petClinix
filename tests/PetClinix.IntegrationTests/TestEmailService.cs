using PetClinix.BuildingBlocks.Application;

namespace PetClinix.IntegrationTests;

public class TestEmailService : IEmailService
{
    public Task SendWelcomeEmailAsync(string toEmail, string userName, string passwordResetToken, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}