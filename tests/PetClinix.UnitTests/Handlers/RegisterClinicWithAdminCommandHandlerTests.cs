using FluentAssertions;
using NSubstitute;
using PetClinix.BuildingBlocks.Application;
using PetClinix.Modules.Identity.Application.Contracts;
using PetClinix.Modules.Identity.Application.UseCases.RegisterClinicWithAdmin;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class RegisterClinicWithAdminCommandHandlerTests
{
    private readonly IClinicRepository _clinicRepositoryMock;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly RegisterClinicWithAdminCommandHandler _handler;
    private readonly IUnitOfWork _unitOfWorkMock;

    public RegisterClinicWithAdminCommandHandlerTests()
    {
        _clinicRepositoryMock = Substitute.For<IClinicRepository>();
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new RegisterClinicWithAdminCommandHandler(
            _clinicRepositoryMock,
            _userRepositoryMock,
            _passwordHasherMock,
            _unitOfWorkMock);
    }

    private static RegisterClinicWithAdminCommand CreateValidCommand() => new()
    {
        TradeName = "Pet Love",
        LegalName = "Pet Love LTDA",
        DocumentNumber = "12345678000199",
        Email = "contato@petlove.com",
        PhoneNumber = "11988887777",
        ZipCode = "01001000",
        Street = "Rua Teste",
        Number = "123",
        Neighborhood = "Centro",
        City = "SP",
        State = "SP",
        AdminName = "Admin",
        AdminEmail = "admin@petlove.com",
        AdminDocumentNumber = "12345678900",
        AdminPhoneNumber = "11999990000",
        AdminBirthDate = new DateOnly(1990, 1, 1)
    };

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ClinicSlugAlreadyExists()
    {
        var command = CreateValidCommand();

        _clinicRepositoryMock
            .ExistsBySlugAsync(Arg.Any<ClinicSlug>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.slug_already_exists");
        await _clinicRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_SaveData_When_EverythingIsValid()
    {
        var command = CreateValidCommand();

        _clinicRepositoryMock.ExistsBySlugAsync(Arg.Any<ClinicSlug>(), Arg.Any<CancellationToken>()).Returns(false);
        _clinicRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);
        _userRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ClinicEmailAlreadyExists()
    {
        var command = CreateValidCommand();

        _clinicRepositoryMock.ExistsBySlugAsync(Arg.Any<ClinicSlug>(), Arg.Any<CancellationToken>()).Returns(false);
        _clinicRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.email_already_exists");
        await _clinicRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_AdminEmailAlreadyExists()
    {
        var command = CreateValidCommand();

        _clinicRepositoryMock.ExistsBySlugAsync(Arg.Any<ClinicSlug>(), Arg.Any<CancellationToken>()).Returns(false);
        _clinicRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(false);
        _userRepositoryMock.ExistsByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>()).Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.email_already_exists");
        await _clinicRepositoryMock.DidNotReceive().AddAsync(Arg.Any<Clinic>(), Arg.Any<CancellationToken>());
    }
}