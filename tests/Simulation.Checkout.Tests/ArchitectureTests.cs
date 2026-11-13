using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

// Task 5 - Architecture (NetArchTest.Rules)
public class ArchitectureTests
{
    private const string DomainNamespace = "Simulation.Checkout.Domain";
    private const string ApplicationNamespace = "Simulation.Checkout.Application";
    private const string InfrastructureNamespace = "Simulation.Checkout.Infrastructure";

    private static readonly System.Reflection.Assembly DomainAssembly = typeof(Order).Assembly;
    private static readonly System.Reflection.Assembly ApplicationAssembly = typeof(CheckoutService).Assembly;
    private static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(InMemoryOrderRepository).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Domain must stay independent. Offending types: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Application must not know Infrastructure. Offending types: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        var result = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly })
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "every interface name must start with 'I'. Offending types: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void OrderRepositoryImplementations_ShouldEndWithRepository()
    {
        var result = Types.InAssemblies(new[] { DomainAssembly, ApplicationAssembly, InfrastructureAssembly })
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "IOrderRepository implementations must end with 'Repository'. Offending types: {0}",
            string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
