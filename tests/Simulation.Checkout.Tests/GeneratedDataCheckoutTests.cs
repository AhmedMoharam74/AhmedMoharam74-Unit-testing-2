using AutoFixture;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Tests.TestData;

namespace Simulation.Checkout.Tests;

// Task 3 - Test data (AutoFixture, Bogus)
public class GeneratedDataCheckoutTests
{
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly Mock<IInventoryService> _inventory = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly CheckoutService _sut;

    public GeneratedDataCheckoutTests()
    {
        _inventory
            .Setup(i => i.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new CheckoutService(_repository.Object, _inventory.Object, _notifications.Object);
    }

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsCreatedByAutoFixture_ShouldSucceedWithConsistentTotals()
    {
        var fixture = AutoFixtureFactory.CreateFixture();
        var order = fixture.Create<Order>();

        var receipt = await _sut.CheckoutAsync(order);

        order.Items.Should().OnlyContain(i => i.Quantity > 0 && i.UnitPrice >= 0);
        receipt.Items.Should().HaveCount(order.Items.Count);
        receipt.Subtotal.Should().Be(order.Items.Sum(i => i.UnitPrice * i.Quantity));
        receipt.Discount.Should().BeInRange(0m, CheckoutService.MaxDiscount);
        receipt.Total.Should().Be(receipt.Subtotal - receipt.Discount);
    }

    [Fact]
    public async Task CheckoutAsync_WhenCustomerIsCreatedByBogusWithFixedSeed_ShouldCopyNameAndEmailToReceipt()
    {
        var customer = CustomerFaker.Create();
        var order = new Order
        {
            CustomerName = customer.Name,
            CustomerEmail = customer.Email,
            Items = new[] { OrderFactory.Item(250m, 2) }
        };

        var receipt = await _sut.CheckoutAsync(order);

        receipt.CustomerName.Should().Be(customer.Name);
        receipt.CustomerEmail.Should().Be(customer.Email);
    }
}
