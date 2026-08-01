using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.GetProfile;

public sealed class GetProfileQueryHandler : ICommandHandler<GetProfileQuery, Result<ProfileResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IClinicRepository _clinicRepository;

    public GetProfileQueryHandler(IUserRepository userRepository, IClinicRepository clinicRepository)
    {
        _userRepository = userRepository;
        _clinicRepository = clinicRepository;
    }

    public async Task<Result<ProfileResponse>> Handle(GetProfileQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null)
        {
            return Result<ProfileResponse>.Failure("identity.user.not_found", "Usuário não encontrado.");
        }

        var clinic = await _clinicRepository.GetByIdAsync(user.ClinicId, cancellationToken);
        if (clinic == null)
        {
            return Result<ProfileResponse>.Failure("identity.clinic.not_found", "Clínica não encontrada.");
        }

        var profile = new ProfileResponse(
            user.Id,
            user.Name,
            user.Email.Value,
            user.DocumentNumber,
            user.PhoneNumber.Value,
            user.BirthDate,
            user.Role.ToString(),
            new ClinicResponse(
                clinic.Id,
                clinic.TradeName,
                clinic.LegalName,
                clinic.DocumentNumber,
                clinic.Email.Value,
                clinic.PhoneNumber.Value,
                clinic.ZipCode,
                clinic.Street,
                clinic.Number,
                clinic.Neighborhood,
                clinic.Complement,
                clinic.City,
                clinic.State));

        return Result<ProfileResponse>.Success(profile);
    }
}