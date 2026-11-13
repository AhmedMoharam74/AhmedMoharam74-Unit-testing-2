using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using Simulation.Checkout.Tests.TestData;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    private readonly Mock<IOrderRepository> _repository = new();
    private readonly Mock<IInventoryService> _inventory = new();
    private readonly Mock<INotificationService> _notifications = new();
    private readonly CheckoutService _sut;

    public CheckoutServiceTests()
    {
        // By default every product is in stock. Tests override this when needed.
        _inventory
            .Setup(i => i.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new CheckoutService(_repository.Object, _inventory.Object, _notifications.Object);
    }

    // ---------------------------------------------------------------
    // Task 1 - Business rules
    // ---------------------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
    {
        var order = OrderFactory.EmptyOrder();

        Func<Task> act = () => _sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<EmptyOrderException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_WhenQuantityIsZeroOrNegative_ShouldThrowInvalidQuantityExceptionWithProductName(int quantity)
    {
        var order = OrderFactory.OrderWith(OrderFactory.Item(100m, quantity, "Gaming Mouse"));

        Func<Task> act = () => _sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidQuantityException>()
            .WithMessage("*Gaming Mouse*");
    }

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
    {
        var order = OrderFactory.OrderWith(OrderFactory.Item(-0.01m, 1));

        Func<Task> act = () => _sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsZero_ShouldSucceedWithZeroTotal()
    {
        var order = OrderFactory.OrderWith(OrderFactory.Item(0m, 2, "Free Sticker"));

        var receipt = await _sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(0m);
        receipt.Total.Should().Be(0m);
    }

    public static TheoryData<decimal, decimal> DiscountCases => new()
    {
        { 999.99m, 0m },
        { 1000m, 100m },
        { 1500m, 100m }
    };

    [Theory]
    [MemberData(nameof(DiscountCases))]
    public async Task CheckoutAsync_WhenSubtotalVaries_ShouldApplyExpectedDiscount(decimal subtotal, decimal expectedDiscount)
    {
        var order = OrderFactory.OrderWith(OrderFactory.Item(subtotal, 1));

        var receipt = await _sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(subtotal);
        receipt.Discount.Should().Be(expectedDiscount);
    }

    [Fact]
    public async Task CheckoutAsync_WhenExampleA_ShouldReturnSubtotal700NoDiscountAndTotal700()
    {
        var order = OrderFactory.ExampleA();

        var receipt = await _sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(700m);
        receipt.Discount.Should().Be(0m);
        receipt.Total.Should().Be(700m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenExampleC_ShouldCapDiscountAt100AndReturnTotal1400()
    {
        var order = OrderFactory.ExampleC();

        var receipt = await _sut.CheckoutAsync(order);

        receipt.Subtotal.Should().Be(1500m);
        receipt.Discount.Should().Be(100m);
        receipt.Total.Should().Be(1400m);
    }

    // ---------------------------------------------------------------
    // Task 2 - Mocks (Moq)
    // ---------------------------------------------------------------

    [Fact]
    public async Task CheckoutAsync_WhenAllItemsInStock_ShouldSaveOrderOnceAndSendConfirmationOnce()
    {
        var order = OrderFactory.ExampleA();

        var receipt = await _sut.CheckoutAsync(order);

        receipt.OrderId.Should().Be(order.Id);
        _repository.Verify(r => r.SaveAsync(order, It.IsAny<CancellationToken>()), Times.Once());
        _notifications.Verify(n => n.SendOrderConfirmationAsync(receipt, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task CheckoutAsync_WhenOneItemIsOutOfStock_ShouldThrowAndNeverSaveOrNotify()
    {
        var outOfStock = OrderFactory.Item(200m, 1, "Webcam");
        var order = OrderFactory.OrderWith(OrderFactory.Item(100m, 1, "Cable"), outOfStock);
        _inventory
            .Setup(i => i.IsAvailableAsync(outOfStock.ProductId, outOfStock.Quantity, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = () => _sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InsufficientInventoryException>()
            .WithMessage("*Webcam*");
        _repository.Verify(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());
        _notifications.Verify(n => n.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task CheckoutAsync_WhenValidationFails_ShouldNeverCheckInventorySaveOrNotify()
    {
        var order = OrderFactory.OrderWith(OrderFactory.Item(100m, 0));

        Func<Task> act = () => _sut.CheckoutAsync(order);

        await act.Should().ThrowAsync<InvalidQuantityException>();
        _inventory.Verify(i => i.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never());
        _repository.Verify(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never());
        _notifications.Verify(n => n.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact]
    public async Task CheckoutAsync_WhenCheckoutSucceeds_ShouldCheckInventoryBeforeSavingOrder()
    {
        var calls = new List<string>();
        _inventory
            .Setup(i => i.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("inventory"))
            .ReturnsAsync(true);
        _repository
            .Setup(r => r.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("save"))
            .Returns(Task.CompletedTask);
        var order = OrderFactory.OrderWith(OrderFactory.Item(100m, 1));

        await _sut.CheckoutAsync(order);

        calls.Should().Equal("inventory", "save");
    }
}
