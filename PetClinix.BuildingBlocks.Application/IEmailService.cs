namespace PetClinix.BuildingBlocks.Application;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string toEmail, string userName, string passwordResetToken, CancellationToken cancellationToken = default);
}