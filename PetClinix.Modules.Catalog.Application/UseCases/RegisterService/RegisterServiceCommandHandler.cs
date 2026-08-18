using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Catalog.Application.Contracts;
using PetClinix.Modules.Catalog.Domain.Entities;
using PetClinix.Modules.Catalog.Domain.Exceptions;
using PetClinix.Modules.Catalog.Domain.Repositories;

namespace PetClinix.Modules.Catalog.Application.UseCases.RegisterService;

public sealed class RegisterServiceCommandHandler : ICommandHandler<RegisterServiceCommand, Result>
{
    private readonly IServiceRepository _serviceRepository;
    private readonly ICatalogUnitOfWork _unitOfWork;

    public RegisterServiceCommandHandler(IServiceRepository serviceRepository, ICatalogUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RegisterServiceCommand command, CancellationToken cancellationToken)
    {
        if (await _serviceRepository.ExistsByNameAsync(command.ClinicId, command.Name, cancellationToken))
        {
            return Result.Failure("catalog.service.name_already_exists", "Já existe um serviço cadastrado com este nome.");
        }

        try
        {
            var service = Service.Create(
                command.ClinicId,
                command.CreatedByUserId,
                command.Name,
                command.Description,
                command.DurationInMinutes,
                command.Price,
                command.RequiresVeterinarian
            );

            await _serviceRepository.AddAsync(service, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (CatalogDomainException ex)
        {
            return Result.Failure(ex.Code, ex.Message);
        }
    }
}