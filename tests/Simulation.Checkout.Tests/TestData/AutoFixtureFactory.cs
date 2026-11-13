using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

/// <summary>
/// AutoFixture setup that only generates orders the checkout rules accept:
/// quantity &gt; 0 and unit price &gt;= 0.
/// </summary>
internal static class AutoFixtureFactory
{
    public static Fixture CreateFixture()
    {
        var fixture = new Fixture();
        var random = new Random(42); // fixed seed: same data on every run

        fixture.Customize<OrderItem>(composer => composer
            .With(item => item.Quantity, () => random.Next(1, 10))
            .With(item => item.UnitPrice, () => random.Next(0, 100_000) / 100m));

        fixture.Customize<Order>(composer => composer
            .With(order => order.Items, () => fixture.CreateMany<OrderItem>(3).ToList()));

        return fixture;
    }
}
