using Microsoft.EntityFrameworkCore;
using Moq;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Classes;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAPITest.RepositoryTest
{
    [TestFixture]
    public class UserRepositoryTest
    {
        private UserRepository _userRepository;
        private AppDbContext _context;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AppDbContext(options);
            _userRepository = new UserRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region AddUserAsync Tests

        [Test]
        public async Task AddUserAsync_ShouldAddUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                DisplayName = "Test User",
                Role = "User",
                GoogleId = "google-id-123",
                GivenName = "Test",
                FamilyName = "User",
                Picture = "http://example.com/pic.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var addedUser = await _userRepository.AddUserAsync(user);

            // Assert
            var userInDb = await _context.Users.FindAsync(addedUser.Id);
            Assert.IsNotNull(userInDb);
            Assert.AreEqual(user.Email, userInDb.Email);
            Assert.AreEqual(user.DisplayName, userInDb.DisplayName);
        }
        #endregion AddUserAsync Tests

        #region GetUserByEmailAsync Tests

        [Test]
        public async Task GetUserByEmailAsync_ExistingEmail_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                DisplayName = "Test User",
                Role = "User",
                GoogleId = "google-id-123",
                GivenName = "Test",
                FamilyName = "User",
                Picture = "http://example.com/pic.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByEmailAsync("test@example.com");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result.Email);
        }
        [Test]
        public async Task GetUserByEmailAsync_NonExistingEmail_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetUserByEmailAsync("nonexisting@example.com");

            // Assert
            Assert.IsNull(result);
        }
        #endregion GetUserByEmailAsync Tests
    
        #region GetUserByGoogleIdAsync Tests

        [Test]
        public async Task GetUserByGoogleIdAsync_ExistingGoogleId_ShouldReturnUser()
        {
            // Arrange
            var user = new User
            {
                Email = "test@example.com",
                DisplayName = "Test User",
                Role = "User",
                GoogleId = "google-id-123",
                GivenName = "Test",
                FamilyName = "User",
                Picture = "http://example.com/pic.jpg",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _userRepository.GetUserByGoogleIdAsync("google-id-123");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.GoogleId, result.GoogleId);
        }

        [Test]
        public async Task GetUserByGoogleIdAsync_NonExistingGoogleId_ShouldReturnNull()
        {
            // Act
            var result = await _userRepository.GetUserByGoogleIdAsync("nonexisting-google-id");

            // Assert
            Assert.IsNull(result);
        }

        #endregion GetUserByGoogleIdAsync Tests
    }
}
