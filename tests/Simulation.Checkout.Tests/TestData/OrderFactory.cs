using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

/// <summary>
/// Small hand-written helpers that build orders with fixed, readable data.
/// </summary>
internal static class OrderFactory
{
    public static OrderItem Item(decimal unitPrice, int quantity, string productName = "Mechanical Keyboard") =>
        new()
        {
            ProductId = Guid.NewGuid(),
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity
        };

    public static Order OrderWith(params OrderItem[] items) =>
        new()
        {
            CustomerName = "Ahmed Ali",
            CustomerEmail = "ahmed.ali@example.com",
            Items = items
        };

    public static Order EmptyOrder() => OrderWith();

    /// <summary>Example A from the assignment: 2 x 150 + 1 x 400 = 700.</summary>
    public static Order ExampleA() => OrderWith(Item(150m, 2, "Mouse"), Item(400m, 1, "Headset"));

    /// <summary>Example C from the assignment: 3 x 500 = 1,500.</summary>
    public static Order ExampleC() => OrderWith(Item(500m, 3, "Monitor"));
}
