using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Catalog.Application.Contracts;
using PetClinix.Modules.Catalog.Domain.Repositories;

namespace PetClinix.Modules.Catalog.Application.UseCases.DeactivateService;

public sealed class DeactivateServiceCommandHandler : ICommandHandler<DeactivateServiceCommand, Result>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public DeactivateServiceCommandHandler(IServiceRepository serviceRepository, ICatalogUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeactivateServiceCommand command, CancellationToken cancellationToken)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);

        if (service == null || service.ClinicId != command.ClinicId)
        {
            return Result.Failure("catalog.service.not_found", "Serviço não encontrado nesta clínica.");
        }

        service.Deactivate();

        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}