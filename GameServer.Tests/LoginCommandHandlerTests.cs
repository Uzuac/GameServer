using GameServer.Domain.Commands;
using GameServer.Domain.Model;

namespace GameServer.Tests
{
    public class LoginCommandHandlerTests
    {
        private readonly AppDbContextFixture _fixture;

        public LoginCommandHandlerTests()
        {
            _fixture = new AppDbContextFixture();
        }

        [Fact]
        public async Task Handle_PlayerNotFound_ThrowsException()
        {
            // Arrange
            using var context = _fixture.CreateContext();
            var handler = new LoginCommandHandler(context);

            var command = new LoginCommand
            {
                DeviceId = Guid.NewGuid()
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PlayerExists_AuthRecordExists_ReturnsAlreadyLoggedIn()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            // Seed data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            context.PlayerAuth.Add(new PlayerAuth
            {
                DeviceId = deviceId,
                AuthExpiration = DateTime.UtcNow.AddMinutes(30)
            });

            await context.SaveChangesAsync();

            var handler = new LoginCommandHandler(context);
            var command = new LoginCommand { DeviceId = deviceId };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Already logged in", result);
        }

        [Fact]
        public async Task Handle_PlayerExists_AuthRecordDoesNotExist_CreatesAuthAndReturnsPlayerId()
        {
            // Arrange
            using var context = _fixture.CreateContext();

            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            // Seed data
            context.Players.Add(new Player
            {
                Id = playerId,
                Name = "TestPlayer",
                Devices = new[] { new Device { Id = deviceId } }.ToList()
            });

            await context.SaveChangesAsync();

            var handler = new LoginCommandHandler(context);
            var command = new LoginCommand { DeviceId = deviceId };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(playerId.ToString(), result);

            var authRecord = context.PlayerAuth.FirstOrDefault(a => a.DeviceId == deviceId);
            Assert.NotNull(authRecord);
            Assert.Equal(deviceId, authRecord.DeviceId);
        }
    }
}
