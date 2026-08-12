using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Identity.Application.UseCases.GetStaff;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class GetStaffByIdQueryHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly GetStaffByIdQueryHandler _handler;

    public GetStaffByIdQueryHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _handler = new GetStaffByIdQueryHandler(_userRepositoryMock);
    }

    private static User CreateValidUser(Guid clinicId)
    {
        return User.CreateStaff(
            clinicId, "Staff Teste", "staff@teste.com", "hash", "12345678900",
            "11999990000", new DateOnly(1990, 1, 1), UserRole.Veterinarian);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Does_Not_Exist()
    {
        var query = new GetStaffByIdQuery(Guid.NewGuid(), Guid.NewGuid());
        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_User_Belongs_To_Another_Clinic()
    {
        var myClinicId = Guid.NewGuid();
        var otherClinicId = Guid.NewGuid();
        var user = CreateValidUser(otherClinicId);

        var query = new GetStaffByIdQuery(myClinicId, user.Id);
        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.ErrorCode.Should().Be("identity.user.not_found");
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccess_When_User_Belongs_To_Clinic()
    {
        var clinicId = Guid.NewGuid();
        var user = CreateValidUser(clinicId);

        var query = new GetStaffByIdQuery(clinicId, user.Id);
        _userRepositoryMock.GetByIdAsync(query.UserId, Arg.Any<CancellationToken>()).Returns(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(user.Id);
        result.Value.Name.Should().Be(user.Name);
    }
}