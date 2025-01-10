using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Domain.Commands;
using GameServer.Domain.Interfaces;
using GameServer.Domain.Model;
using GameServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace GameServer.Tests
{
    public class SendResourcesCommandHandlerTests
    {
        private readonly AppDbContextFixture _fixture;
        private readonly Mock<INotificationService> _mockNotificationService;

        public SendResourcesCommandHandlerTests()
        {
            _fixture = new AppDbContextFixture();
            _mockNotificationService = new Mock<INotificationService>();
        }

        [Fact]
        public async Task Handle_PlayerOrFriendNotFound_ThrowsException()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var handler = new SendResourcesCommandHandler(context, _mockNotificationService.Object);

            var command = new SendResourcesCommand
            {
                DeviceId = Guid.NewGuid(),
                FriendPlayerId = Guid.NewGuid(),
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InsufficientFunds_ReturnsErrorMessage()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var friendPlayerId = Guid.NewGuid();

            // Seed player and friend player data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            context.Players.Add(new Player
            {
                Id = friendPlayerId,
                Name = "TestPlayer2",
            });

            await context.SaveChangesAsync();

            var handler = new SendResourcesCommandHandler(context, _mockNotificationService.Object);

            var command = new SendResourcesCommand
            {
                DeviceId = deviceId,
                FriendPlayerId = friendPlayerId,
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Insufficient funds", result);
        }

        [Fact]
        public async Task Handle_TransferResources_SuccessfullyTransfersAndReturnsNewAmount()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();
            var friendPlayerId = Guid.NewGuid();

            // Seed player and friend player data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            context.Players.Add(new Player
            {
                Id = friendPlayerId,
                Name = "TestPlayer2",
            });

            context.Resources.Add(new Resource
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Type = ResourceType.Coin,
                Amount = 200
            });

            await context.SaveChangesAsync();

            var handler = new SendResourcesCommandHandler(context, _mockNotificationService.Object);

            var command = new SendResourcesCommand
            {
                DeviceId = deviceId,
                FriendPlayerId = friendPlayerId,
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("100", result);

            var playerResource = context.Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == ResourceType.Coin);
            var friendResource = context.Resources.FirstOrDefault(r => r.PlayerId == friendPlayerId && r.Type == ResourceType.Coin);

            Assert.NotNull(playerResource);
            Assert.Equal(100, playerResource.Amount);

            Assert.NotNull(friendResource);
            Assert.Equal(100, friendResource.Amount);
        }
    }
}
