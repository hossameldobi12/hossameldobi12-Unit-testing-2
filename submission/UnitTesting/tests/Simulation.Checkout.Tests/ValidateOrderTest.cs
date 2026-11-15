using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ValidateOrderTest
    {


        [Fact]
        public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
        {
            // Arrange
            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), Mock.Of<IInventoryService>(), Mock.Of<INotificationService>());
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };

            //Act
            Func<Task> act = () => checkoutService.CheckoutAsync(order);


            // Assert
            await act.Should().ThrowAsync<EmptyOrderException>();



        }


        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CheckoutAsync_WhenQuantityIsInvalid_ShouldThrowInvalidQuantityException(int quantity)
        {
            // Arrange
            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), Mock.Of<IInventoryService>(), Mock.Of<INotificationService>());
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 1000m,
                Quantity = quantity
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };
            // Act
            Func<Task> act = () => checkoutService.CheckoutAsync(order);
            // Assert
            await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage($"Quantity for '{orderItem.ProductName}' must be greater than zero.");
        }



        [Fact]
        public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
        {
            // Arrange
            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), Mock.Of<IInventoryService>(), Mock.Of<INotificationService>());
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = -0.01m,
                Quantity = 1
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };
            // Act
            Func<Task> act = () => checkoutService.CheckoutAsync(order);
            // Assert
            await act.Should().ThrowAsync<InvalidPriceException>().WithMessage($"Unit price for '{orderItem.ProductName}' cannot be negative.");
        }


        [Fact]
        public async Task CheckoutAsync_WhenPriceIsZero_ShouldCheckoutSucceed()
        {

            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Arrange
            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), inventoryMock.Object, Mock.Of<INotificationService>());
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 0,
                Quantity = 1
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };



            // Act
            var act = await checkoutService.CheckoutAsync(order);

            // Assert

            act.Total.Should().Be(0);
        }




        [Theory]
        [InlineData(999.99, 0)]
        [InlineData(1000, 100)]
        [InlineData(1500, 100)]
        public async Task CheckoutAsync_WhenSubtotalChanges_ShouldApplyCorrectDiscount(decimal SubTotal, decimal Expected)
        {
            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            // Arrange
            var checkoutService = new CheckoutService(Mock.Of<IOrderRepository>(), inventoryMock.Object, Mock.Of<INotificationService>());
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = SubTotal,
                Quantity = 1
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };
            // Act
            var act = await checkoutService.CheckoutAsync(order);

            act.Subtotal.Should().Be(SubTotal);
            act.Discount.Should().Be(Expected);
        }


        [Theory]
        [InlineData(150, 2, 400, 1, 700, 0, 700)]
        [InlineData(500, 3, 0, 1, 1500, 100, 1400)]
        public async Task CheckoutAsync_WhenOrderHasMultipleItems_ShouldCalculateTotalsCorrectly(
            decimal firstPrice,
            int firstQuantity,
            decimal secondPrice,
            int secondQuantity,
            decimal expectedSubtotal,
            decimal expectedDiscount,
            decimal expectedTotal)
        {
            // Arrange
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
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = firstPrice,
                Quantity = firstQuantity
            },
            new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Mouse",
                UnitPrice = secondPrice,
                Quantity = secondQuantity
            }
        }
            };

            // Act
            var result = await checkoutService.CheckoutAsync(order);

            // Assert
            result.Subtotal.Should().Be(expectedSubtotal);
            result.Discount.Should().Be(expectedDiscount);
            result.Total.Should().Be(expectedTotal);
        }
    }
}
