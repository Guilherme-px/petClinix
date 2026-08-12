using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Domain.Repositories;

namespace PetClinix.Modules.Identity.Application.UseCases.GetStaff;

public sealed class GetStaffQueryHandler : ICommandHandler<GetStaffQuery, Result<List<StaffResponse>>>
{
    private readonly IUserRepository _userRepository;

    public GetStaffQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<List<StaffResponse>>> Handle(GetStaffQuery query, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllByClinicIdAsync(query.ClinicId, cancellationToken);

        var response = users.Select(u => new StaffResponse(
            u.Id,
            u.Name,
            u.Email.Value,
            u.PhoneNumber.Value,
            u.DocumentNumber,
            u.BirthDate,
            u.Role,
            u.IsActive)).ToList();

        return Result<List<StaffResponse>>.Success(response);
    }
}