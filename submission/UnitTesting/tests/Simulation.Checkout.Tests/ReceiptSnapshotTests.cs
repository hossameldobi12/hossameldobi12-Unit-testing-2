using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class ReceiptSnapshotTests
    {
        [Fact]
        public async Task CheckoutAsync_ExampleC_ShouldMatchSnapshot()
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
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>
        {
            new OrderItem
            {
                ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                ProductName = "Laptop",
                UnitPrice = 500m,
                Quantity = 3
            },
            new OrderItem
            {
                ProductId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                ProductName = "Mouse",
                UnitPrice = 0m,
                Quantity = 1
            }
        }
            };

            // Act
            var receipt = await checkoutService.CheckoutAsync(order);

            // Assert
            var settings = new VerifySettings();
            settings.ScrubMember("CreatedAt");
            settings.ScrubMember("OrderId");

            await Verify(receipt, settings);
        }
    }
}
