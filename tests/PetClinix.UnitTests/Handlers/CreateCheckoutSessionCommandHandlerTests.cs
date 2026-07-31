using FluentAssertions;
using NSubstitute;
using PetClinix.Modules.Billing.Application.Contracts;
using PetClinix.Modules.Billing.Application.UseCases.CreateCheckoutSession;
using Xunit;

namespace PetClinix.UnitTests.Handlers;

public class CreateCheckoutSessionCommandHandlerTests
{
    private readonly IStripeService _stripeServiceMock;
    private readonly CreateCheckoutSessionCommandHandler _handler;

    public CreateCheckoutSessionCommandHandlerTests()
    {
        _stripeServiceMock = Substitute.For<IStripeService>();
        _handler = new CreateCheckoutSessionCommandHandler(_stripeServiceMock);
    }

    private static CreateCheckoutSessionCommand CreateValidCommand() => new(
        Guid.NewGuid(),
        "price_123456",
        "admin@petclinic.com",
        "http://localhost:3000/success",
        "http://localhost:3000/cancel"
    );

    [Fact]
    public async Task Handle_Should_Return_Success_And_CheckoutUrl_When_Valid()
    {
        var command = CreateValidCommand();
        var fakeUrl = "https://checkout.stripe.com/c/pay/cs_test_123";

        _stripeServiceMock
            .CreateCheckoutSessionAsync(Arg.Any<CreateCheckoutSessionCommand>(), Arg.Any<CancellationToken>())
            .Returns(fakeUrl);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.CheckoutUrl.Should().Be(fakeUrl);

        await _stripeServiceMock.Received(1)
            .CreateCheckoutSessionAsync(Arg.Is<CreateCheckoutSessionCommand>(c =>
                c != null &&
                c.PriceId == command.PriceId &&
                c.AdminEmail == command.AdminEmail),
                Arg.Any<CancellationToken>());
    }
}