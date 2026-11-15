
using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Application")
            .And()
            .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(CheckoutService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Interfaces_ShouldStartWithI()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(IOrderRepository).Assembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void OrderRepositoryImplementations_ShouldEndWithRepository()
    {
        // Arrange & Act
        var result = Types
            .InAssembly(typeof(InMemoryOrderRepository).Assembly)
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
