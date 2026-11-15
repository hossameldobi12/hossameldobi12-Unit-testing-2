using FluentAssertions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace Simulation.Checkout.Tests
{
    public class InMemoryOrderTests
    {
        [Fact]
        public async Task SaveAsync_WhenOrderValid_ShouldSaveOrder()
        {
            // Arrange
            var repository = new InMemoryOrderRepository();

            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };
            // Act
            await repository.SaveAsync(order);

            //Assert
            repository.GetAll().Should().ContainSingle().Which.Should().BeSameAs(order);
        }
        [Fact]
        public async Task SaveAsync_WhenOrderNull_ThrowArgumentNullException()
        {
            // Arrange
            var repository = new InMemoryOrderRepository();

            // Act
            Func<Task> act = () => repository.SaveAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>();

        }
        [Fact]
        public async Task SaveAsync_WhenCancellationRequested_ShouldThrowOperationCanceledException()
        {
            // Arrange
            var repository = new InMemoryOrderRepository();
            var order = new Order
            {
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };
            // Act
            Func<Task> act = () => repository.SaveAsync(order, new CancellationToken(true));

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task SaveAsync_WhenOrderIdAlreadyExists_ShouldReplaceExistingOrder()
        {
            //Arrange
            var repository = new InMemoryOrderRepository();
            var firstOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Ahmed",
                CustomerEmail = "ahmed@example.com",
                Items = new List<OrderItem>()
            };

            var secondOrder = new Order
            {
                Id = firstOrder.Id,
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };

            //act 
            await repository.SaveAsync(firstOrder);
            await repository.SaveAsync(secondOrder);


            // Assert
            repository.GetAll().Should().ContainSingle().Which.Should().BeSameAs(secondOrder);
            repository.GetAll().Should().NotContain(firstOrder);


        }
        [Fact]
        public void GetALL_WhenNoOrders_ShouldReturnEmptyList()
        {
            //Arrange
            var repository = new InMemoryOrderRepository();

            //Act
            var emptylist = repository.GetAll();

            emptylist.Should().NotBeNull();
            emptylist.Should().BeEmpty();


        }
        [Fact]
        public async Task GetAll_WhenThreeOrdersAreSaved_ShouldReturnThreeOrders()
        {
            //Arrange
            var repository = new InMemoryOrderRepository();


            var firstOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Ahmed",
                CustomerEmail = "ahmed@example.com",
                Items = new List<OrderItem>()
            };

            var secondOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Hossam",
                CustomerEmail = "hossam@example.com",
                Items = new List<OrderItem>()
            };

            var thirdOrder = new Order
            {
                Id = Guid.NewGuid(),
                CustomerName = "Mohamed",
                CustomerEmail = "mohamed@example.com",
                Items = new List<OrderItem>()
            };

            //act 
            await repository.SaveAsync(firstOrder);
            await repository.SaveAsync(secondOrder);
            await repository.SaveAsync(thirdOrder);



            
            var orders = repository.GetAll();


            
            orders.Should().HaveCount(3);

        }
    }
}
