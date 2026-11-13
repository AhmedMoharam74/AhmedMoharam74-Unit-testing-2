using Bogus;

namespace Simulation.Checkout.Tests.TestData;

public sealed record TestCustomer(string Name, string Email);

/// <summary>
/// Bogus generator for realistic customers. The seed is fixed, so every run
/// produces exactly the same customer (no flaky tests).
/// </summary>
internal static class CustomerFaker
{
    public const int Seed = 12345;

    public static TestCustomer Create() =>
        new Faker<TestCustomer>()
            .UseSeed(Seed)
            .CustomInstantiator(f => new TestCustomer(f.Name.FullName(), f.Internet.Email()))
            .Generate();
}
