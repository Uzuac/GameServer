using GameServer.Domain.Commands;
using GameServer.Domain.Model;
using GameServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Tests
{
    public class UpdateResourcesCommandHandlerTests
    {
        private readonly AppDbContextFixture _fixture;

        public UpdateResourcesCommandHandlerTests()
        {
            _fixture = new AppDbContextFixture();
        }

        [Fact]
        public async Task Handle_PlayerNotFound_ThrowsException()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var handler = new UpdateResourcesCommandHandler(context);

            var command = new UpdateResourcesCommand
            {
                DeviceId = Guid.NewGuid(),
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ResourceNotFound_CreatesResourceAndReturnsAmount()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            // Seed player data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            await context.SaveChangesAsync();

            var handler = new UpdateResourcesCommandHandler(context);

            var command = new UpdateResourcesCommand
            {
                DeviceId = deviceId,
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("100", result);

            var resource = context.Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == ResourceType.Coin);
            Assert.NotNull(resource);
            Assert.Equal(100, resource.Amount);
        }

        [Fact]
        public async Task Handle_ResourceExists_UpdatesResourceAndReturnsAmount()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            // Seed player and resource data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            context.Resources.Add(new Resource
            {
                Id = Guid.NewGuid(),
                PlayerId = playerId,
                Type = ResourceType.Coin,
                Amount = 50
            });

            await context.SaveChangesAsync();

            var handler = new UpdateResourcesCommandHandler(context);

            var command = new UpdateResourcesCommand
            {
                DeviceId = deviceId,
                ResourceType = ResourceType.Coin,
                ResourceValue = 100
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("150", result);

            var resource = context.Resources.FirstOrDefault(r => r.PlayerId == playerId && r.Type == ResourceType.Coin);
            Assert.NotNull(resource);
            Assert.Equal(150, resource.Amount);
        }
    }
}
