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
    public class MockTest
    {
        [Fact]
        public async Task CheckoutAsync_WhenOrderIsValid_ShouldSaveAndSendConfirmationOnce()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var notificationServiceMock = new Mock<INotificationService>();
            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var checkoutService = new CheckoutService(orderRepositoryMock.Object,inventoryMock.Object ,notificationServiceMock.Object );
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 1
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };
            // Act
              await checkoutService.CheckoutAsync(order);

            //Assert
            orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Once);


        }


        [Fact]
        public async Task CheckoutAsync_WhenInventoryIsUnavailable_ShouldNotSaveOrSendConfirmation()
        {
            // Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var notificationServiceMock = new Mock<INotificationService>();
            var inventoryMock = new Mock<IInventoryService>();

            inventoryMock
                .Setup(x => x.IsAvailableAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            var checkoutService = new CheckoutService(orderRepositoryMock.Object, inventoryMock.Object, notificationServiceMock.Object);
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
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

            //Assert
            await act.Should().ThrowAsync<InsufficientInventoryException>();
            orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        [Fact]
        public async Task CheckoutAsync_WhenOrderHasInvalidQuantity_ShouldNotCallDependencies()
        {
            //Arrange
            var orderRepositoryMock = new Mock<IOrderRepository>();
            var notificationServiceMock = new Mock<INotificationService>();
            var inventoryMock = new Mock<IInventoryService>();

            var checkoutService = new CheckoutService(orderRepositoryMock.Object, inventoryMock.Object, notificationServiceMock.Object);
            var orderItem = new OrderItem
            {
                ProductId = Guid.NewGuid(),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 0
            };
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem> { orderItem }
            };
            // Act
            Func<Task> act = () => checkoutService.CheckoutAsync(order);

            //Assert
            await act.Should().ThrowAsync<InvalidQuantityException>().WithMessage($"Quantity for '{orderItem.ProductName}' must be greater than zero.");
            orderRepositoryMock.Verify(x => x.SaveAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
            notificationServiceMock.Verify(x => x.SendOrderConfirmationAsync(It.IsAny<CheckoutReceipt>(), It.IsAny<CancellationToken>()), Times.Never);
            inventoryMock.Verify(x => x.IsAvailableAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }


   
}
