namespace PetClinix.BuildingBlocks.Application;

public interface IEmailService
{
    Task<string?> SendWelcomeEmailAsync(string toEmail, string userName, string passwordResetToken, CancellationToken cancellationToken = default);
}