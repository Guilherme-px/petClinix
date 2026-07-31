using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Billing.Application.UseCases.ActivateSubscription;
using PetClinix.Modules.Billing.Domain.Entities;
using PetClinix.Modules.Billing.Domain.Interfaces;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class ActivateSubscriptionCommandHandlerTests
{
    private readonly ISubscriptionRepository _subscriptionRepositoryMock;
    private readonly ActivateSubscriptionCommandHandler _handler;

    public ActivateSubscriptionCommandHandlerTests()
    {
        _subscriptionRepositoryMock = Substitute.For<ISubscriptionRepository>();
        _handler = new ActivateSubscriptionCommandHandler(_subscriptionRepositoryMock);
    }

    private static ActivateSubscriptionCommand CreateValidCommand() => new(
        Guid.NewGuid(),
        "cus_test_123",
        "sub_test_456");

    [Fact]
    public async Task Handle_Should_Create_Subscription_When_Not_Exists()
    {
        var command = CreateValidCommand();

        _subscriptionRepositoryMock
            .GetByClinicIdAsync(command.ClinicId, Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _subscriptionRepositoryMock.Received(1)
            .AddAsync(Arg.Is<Subscription>(s =>
                s != null &&
                s.ClinicId == command.ClinicId &&
                s.StripeSubscriptionId == command.StripeSubscriptionId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Not_Create_Subscription_When_Already_Exists()
    {
        var command = CreateValidCommand();
        var existingSubscription = Subscription.Create(command.ClinicId, command.StripeCustomerId, command.StripeSubscriptionId);

        _subscriptionRepositoryMock
            .GetByClinicIdAsync(command.ClinicId, Arg.Any<CancellationToken>())
            .Returns(existingSubscription);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _subscriptionRepositoryMock.DidNotReceive()
            .AddAsync(Arg.Any<Subscription>(), Arg.Any<CancellationToken>());
    }
}