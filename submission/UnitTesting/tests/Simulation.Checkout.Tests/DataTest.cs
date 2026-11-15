using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class DataTest
    {
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsGeneratedByAutoFixture_ShouldCheckoutSucceed()
        {
            // Arrange
            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), inventoryMock.Object, Mock.Of<INotificationService>());

            var fixture = new Fixture();

            var orderItem = fixture.Build<OrderItem>()
                .With(x => x.Quantity, 3)
                .With(x => x.UnitPrice, 100m)
                .Create();

            var order = fixture.Build<Order>()
                .With(o => o.Items, new List<OrderItem> { orderItem })
                .Create();

            // Act
            Func<Task> act = () => checkoutService.CheckoutAsync(order);
            var receipt = await checkoutService.CheckoutAsync(order);

            // Assert
            await act.Should().NotThrowAsync();
            receipt.Should().NotBeNull();
        }

        [Fact]

        public async Task CheckoutAsync_WhenCustomerIsGeneratedByBogus_ShouldReturnReceiptWithSameCustomerData()
        {
            // Arrange
            var faker = new Faker<Order>().UseSeed(2026)
            .RuleFor(x => x.CustomerName, f => f.Name.FullName())
            .RuleFor(x => x.CustomerEmail, f => f.Internet.Email());

            var Fakeorder = faker.Generate();


            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var checkoutService = new CheckoutService(
                Mock.Of<IOrderRepository>(),
                inventoryMock.Object,
                Mock.Of<INotificationService>());

            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = Fakeorder.CustomerName,
                CustomerEmail = Fakeorder.CustomerEmail,
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 100m,
                Quantity = 1
            }
        }
            };

            // Act
            var receipt = await checkoutService.CheckoutAsync(order);

            // Assert
            receipt.CustomerName.Should().Be(Fakeorder.CustomerName);
            receipt.CustomerEmail.Should().Be(Fakeorder.CustomerEmail);
        }
    }
}
