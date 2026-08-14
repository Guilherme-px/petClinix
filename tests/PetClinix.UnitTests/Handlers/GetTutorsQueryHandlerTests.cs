using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Pets.Application.UseCases.GetTutors;
using PetClinix.Modules.Pets.Domain.Entities;
using PetClinix.Modules.Pets.Domain.Repositories;
using PetClinix.Modules.Pets.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class GetTutorsQueryHandlerTests
{
    private readonly ITutorRepository _tutorRepositoryMock;
    private readonly GetTutorsQueryHandler _handler;

    public GetTutorsQueryHandlerTests()
    {
        _tutorRepositoryMock = Substitute.For<ITutorRepository>();
        _handler = new GetTutorsQueryHandler(_tutorRepositoryMock);
    }

    private static Tutor CreateValidTutor(Guid clinicId)
    {
        return Tutor.Create(
            clinicId, Guid.NewGuid(), "Tutor Teste", "12345678900", null, "11999990000", null,
            "01001000", "Rua Teste", "123", "Centro", null, "Sao Paulo", "SP", null
        );
    }

    [Fact]
    public async Task Handle_Should_Return_PagedResult_With_Correct_Data()
    {
        var clinicId = Guid.NewGuid();
        var query = new GetTutorsQuery(clinicId, 1, 10);

        var tutors = new List<Tutor>
        {
            CreateValidTutor(clinicId),
            CreateValidTutor(clinicId)
        };

        _tutorRepositoryMock.GetAllByClinicIdAsync(clinicId, 1, 10, Arg.Any<CancellationToken>())
           .Returns((tutors, 2));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Tutors_Exist()
    {
        var clinicId = Guid.NewGuid();
        var query = new GetTutorsQuery(clinicId, 1, 10);

        _tutorRepositoryMock.GetAllByClinicIdAsync(clinicId, 1, 10, Arg.Any<CancellationToken>())
            .Returns((new List<Tutor>(), 0));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}