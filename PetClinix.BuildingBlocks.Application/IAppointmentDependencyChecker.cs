namespace PetClinix.BuildingBlocks.Application;

public interface IAppointmentDependencyChecker
{
    Task<bool> HasFutureAppointmentsForTutorAsync(Guid tutorId, CancellationToken cancellationToken = default);
    Task<bool> HasFutureAppointmentsForPetAsync(Guid petId, CancellationToken cancellationToken = default);
    Task<bool> HasFutureAppointmentsForVetAsync(Guid vetId, CancellationToken cancellationToken = default);
    Task<bool> HasFutureAppointmentsForServiceAsync(Guid serviceId, CancellationToken cancellationToken = default); // ADICIONADO PARA SERVIÇOS
}