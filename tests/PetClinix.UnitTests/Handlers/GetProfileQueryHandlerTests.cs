using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Identity.Application.UseCases.GetProfile;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Repositories;
using PetClinix.Modules.Identity.Domain.ValueObjects;
using System;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class GetProfileQueryHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IClinicRepository _clinicRepositoryMock;
    private readonly GetProfileQueryHandler _handler;

    public GetProfileQueryHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _clinicRepositoryMock = Substitute.For<IClinicRepository>();
        _handler = new GetProfileQueryHandler(_userRepositoryMock, _clinicRepositoryMock);
    }

    private static User CreateValidUser()
    {
        return User.CreateAdmin(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Admin Teste",
            "admin@teste.com",
            "hash_senha",
            "12345678900",
            "11999990000",
            new DateOnly(1990, 1, 1)
        );
    }

    private static Clinic CreateValidClinic(Guid clinicId)
    {
        return Clinic.Create(
            "Clinica Teste", "Teste LTDA", "12345678000199",
            ClinicSlug.Create("clinica-teste"),
            "clinica@teste.com", "11988887777",
            "01001000", "Rua Teste", "123", "Centro",
            null, "SP", "SP");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Does_Not_Exist()
    {
        var query = new GetProfileQuery(Guid.NewGuid());
        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_Clinic_Does_Not_Exist()
    {
        var user = CreateValidUser();
        var query = new GetProfileQuery(user.Id);

        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _clinicRepositoryMock.GetByIdAsync(user.ClinicId, Arg.Any<CancellationToken>()).Returns((Clinic?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.clinic.not_found");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_And_Mapped_Profile_When_Valid()
    {
        var user = CreateValidUser();
        var clinic = CreateValidClinic(user.ClinicId);
        var query = new GetProfileQuery(user.Id);

        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns(user);
        _clinicRepositoryMock.GetByIdAsync(user.ClinicId, Arg.Any<CancellationToken>()).Returns(clinic);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().Be(user.Id);
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email.Value);
        result.Value.Clinic.Should().NotBeNull();
        result.Value.Clinic.ClinicId.Should().Be(clinic.Id);
        result.Value.Clinic.TradeName.Should().Be(clinic.TradeName);
    }
}