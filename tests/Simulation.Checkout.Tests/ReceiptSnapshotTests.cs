using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using VerifyXunit;

namespace Simulation.Checkout.Tests;

// Task 4 - Snapshot (Verify.Xunit)
[UsesVerify]
public class ReceiptSnapshotTests
{
    [Fact]
    public async Task CheckoutAsync_WhenExampleC_ShouldMatchReceiptSnapshot()
    {
        var inventory = new Mock<IInventoryService>();
        inventory
            .Setup(i => i.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = new CheckoutService(
            new Mock<IOrderRepository>().Object,
            inventory.Object,
            new Mock<INotificationService>().Object);

        // Fixed data only: example C (3 x 500).
        var order = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerName = "Ahmed Ali",
            CustomerEmail = "ahmed.ali@example.com",
            Items = new[]
            {
                new OrderItem
                {
                    ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ProductName = "Monitor",
                    UnitPrice = 500m,
                    Quantity = 3
                }
            }
        };

        var receipt = await sut.CheckoutAsync(order);

        // CreatedAt changes every run, so it is excluded from the snapshot.
        await Verifier.Verify(receipt).IgnoreMember("CreatedAt");
    }
}
