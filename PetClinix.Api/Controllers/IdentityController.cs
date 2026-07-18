using Microsoft.AspNetCore.Mvc;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;

namespace PetClinix.Api.Controllers;

[ApiController]
[Route("api/clinics")]
public class IdentityController : ControllerBase
{
    private readonly ICommandHandler<RegisterClinicWithAdminCommand, Result<RegisterClinicWithAdminResponse>> _handler;

    public IdentityController(ICommandHandler<RegisterClinicWithAdminCommand, Result<RegisterClinicWithAdminResponse>> handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> RegisterClinic([FromBody] RegisterClinicRequest request, CancellationToken cancellationToken)
    {
        var command = new RegisterClinicWithAdminCommand
        {
            TradeName = request.TradeName,
            LegalName = request.LegalName,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ZipCode = request.ZipCode,
            Street = request.Street,
            Number = request.Number,
            Neighborhood = request.Neighborhood,
            Complement = request.Complement,
            City = request.City,
            State = request.State,
            AdminName = request.AdminName,
            AdminEmail = request.AdminEmail,
            AdminDocumentNumber = request.AdminDocumentNumber,
            AdminPhoneNumber = request.AdminPhoneNumber,
            AdminBirthDate = request.AdminBirthDate
        };

        var result = await _handler.Handle(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { result.ErrorCode, result.ErrorMessage });
        }

        return Created("api/clinics", result.Value);
    }
}

public record RegisterClinicRequest(
    string TradeName, string LegalName, string DocumentNumber,
    string Email, string PhoneNumber,
    string ZipCode, string Street, string Number, string Neighborhood,
    string? Complement, string City, string State,
    string AdminName, string AdminEmail, string Password,
    string AdminDocumentNumber, string AdminPhoneNumber, DateOnly AdminBirthDate);