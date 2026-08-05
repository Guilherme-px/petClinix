using Microsoft.Extensions.Logging;
using PetClinix.BuildingBlocks.Application;
using Resend;

namespace PetClinix.Api.Services;

public class ResendEmailService : IEmailService
{
    private readonly ResendClient _client;
    private readonly string _fromEmail = "PetClinix <onboarding@resend.dev>";
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(ResendClient client, ILogger<ResendEmailService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName, string passwordResetToken, CancellationToken cancellationToken = default)
    {
        var resetLink = $"http://localhost:5173/define-senha?token={passwordResetToken}";

        var htmlContent = $@"
            <h1>Bem-vindo ao PetClinix, {userName}!</h1>
            <p>Sua assinatura foi ativada com sucesso. Sua conta já está pronta para uso.</p>
            <p>Para acessar o sistema, você precisa definir sua senha de acesso através do link abaixo:</p>
            <p><a href='{resetLink}' style='padding: 10px 20px; background-color: #3b82f6; color: white; text-decoration: none; border-radius: 5px;'>Definir Minha Senha</a></p>
            <p>Se o botão não funcionar, copie e cole este link no navegador: {resetLink}</p>
            <br>
            <p>Equipe PetClinix.</p>";

        var message = new EmailMessage
        {
            From = _fromEmail,
            To = new EmailAddressList { toEmail },
            Subject = "Bem-vindo ao PetClinix! Defina sua senha de acesso",
            HtmlBody = htmlContent
        };

        try
        {
            var response = await _client.EmailSendAsync(message, cancellationToken);

            if (response.Content != Guid.Empty)
            {
                _logger.LogInformation("✅ E-mail de boas-vindas enviado com sucesso para {Email} via Resend.", toEmail);
            }
            else
            {
                _logger.LogWarning("Resend retornou um ID vazio ao tentar enviar para {Email}.", toEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exceção ao tentar enviar e-mail via Resend para {Email}", toEmail);
        }
    }
}