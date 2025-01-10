using GameServer.Domain.Commands;
using GameServer.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using GameServer.Domain.Model;
using Moq;
using Xunit;

namespace GameServer.Tests
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<AppDbContext> _mockDbContext;
        private readonly LoginCommandHandler _handler;

        public LoginCommandHandlerTests()
        {
            _mockDbContext = new Mock<AppDbContext>();
            _handler = new LoginCommandHandler(_mockDbContext.Object);
        }

        [Fact]
        public async Task Handle_PlayerNotFound_ThrowsException()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            _mockDbContext.Setup(db => db.Players.AsNoTracking()).Returns(MockDbSet.Empty<Player>().Object);

            var command = new LoginCommand { DeviceId = deviceId };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_PlayerExists_AuthRecordExists_ReturnsAlreadyLoggedIn()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var players = new[] {
                new Player {
                    Id = playerId,
                    Devices = new[] { new Device { Id = deviceId } }.ToList()
                }
            };

            var playerAuth = new[] {
                new PlayerAuth {
                    Id = Guid.NewGuid(),
                    DeviceId = deviceId,
                    AuthExpiration = DateTime.UtcNow.AddMinutes(30)
                }
            };

            _mockDbContext.Setup(db => db.Players.AsNoTracking()).Returns(MockDbSet.Create(players).Object);
            _mockDbContext.Setup(db => db.PlayerAuth.AsNoTracking()).Returns(MockDbSet.Create(playerAuth).Object);

            var command = new LoginCommand { DeviceId = deviceId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal("Already logged in", result);
        }

        [Fact]
        public async Task Handle_PlayerExists_AuthRecordDoesNotExist_CreatesAuthAndReturnsPlayerId()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var playerId = Guid.NewGuid();

            var players = new[] {
                new Player {
                    Id = playerId,
                    Devices = new[] { new Device { Id = deviceId } }.ToList()
                }
            };

            _mockDbContext.Setup(db => db.Players.AsNoTracking()).Returns(MockDbSet.Create(players).Object);
            _mockDbContext.Setup(db => db.PlayerAuth.AsNoTracking()).Returns(MockDbSet.Empty<PlayerAuth>().Object);

            _mockDbContext.Setup(db => db.PlayerAuth.Add(It.IsAny<PlayerAuth>()));
            _mockDbContext.Setup(db => db.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new LoginCommand { DeviceId = deviceId };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(playerId.ToString(), result);
            _mockDbContext.Verify(db => db.PlayerAuth.Add(It.IsAny<PlayerAuth>()), Times.Once);
            _mockDbContext.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    // MockDbSet utility for creating mock DbSet<T> instances.
    public static class MockDbSet
    {
        public static Mock<DbSet<T>> Empty<T>() where T : class
        {
            return Create(Enumerable.Empty<T>());
        }

        public static Mock<DbSet<T>> Create<T>(IEnumerable<T> entities) where T : class
        {
            var queryable = entities.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }
    }
}
