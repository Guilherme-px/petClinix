using PetClinix.BuildingBlocks.Application;

namespace PetClinix.Modules.Appointments.Application.UseCases.GetAvailableSlots;

public sealed record GetAvailableSlotsQuery(Guid ClinicId, Guid VeterinarianId, Guid ServiceId, DateOnly Date) : ICommand<Result<List<string>>>;