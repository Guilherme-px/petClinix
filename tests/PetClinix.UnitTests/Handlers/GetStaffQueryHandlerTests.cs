using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Identity.Application.UseCases.GetStaff;
using PetClinix.Modules.Identity.Domain.Entities;
using PetClinix.Modules.Identity.Domain.Enums;
using PetClinix.Modules.Identity.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class GetStaffQueryHandlerTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly GetStaffQueryHandler _handler;

    public GetStaffQueryHandlerTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _handler = new GetStaffQueryHandler(_userRepositoryMock);
    }

    private static User CreateValidUser(Guid clinicId, Guid userId)
    {
        return User.CreateStaff(
            clinicId, "Staff Teste", "staff@teste.com", "hash", "12345678900",
            "11999990000", new DateOnly(1990, 1, 1), UserRole.Veterinarian);
    }

    [Fact]
    public async Task Handle_Should_Return_PagedResult_With_Correct_Data()
    {
        var clinicId = Guid.NewGuid();
        var query = new GetStaffQuery(clinicId, 1, 10);

        var users = new List<User>
        {
            CreateValidUser(clinicId, Guid.NewGuid()),
            CreateValidUser(clinicId, Guid.NewGuid())
        };

        _userRepositoryMock.GetAllByClinicIdAsync(clinicId, 1, 10, Arg.Any<CancellationToken>())
            .Returns((users, 2));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }
}