using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.GetStaff;

public sealed class GetStaffByIdQueryHandler : ICommandHandler<GetStaffByIdQuery, Result<StaffResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetStaffByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<StaffResponse>> Handle(GetStaffByIdQuery query, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null || user.ClinicId != query.ClinicId)
        {
            return Result<StaffResponse>.Failure("identity.user.not_found", "Funcionário não encontrado nesta clínica.");
        }

        var response = new StaffResponse(
            user.Id,
            user.Name,
            user.Email.Value,
            user.PhoneNumber.Value,
            user.DocumentNumber,
            user.BirthDate,
            user.Role,
            user.IsActive);

        return Result<StaffResponse>.Success(response);
    }
}