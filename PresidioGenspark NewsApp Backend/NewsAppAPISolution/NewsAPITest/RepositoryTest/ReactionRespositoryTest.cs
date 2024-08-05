using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsAppAPI.Repositories.Interfaces;

namespace NewsAPITest.RepositoryTest
{
    public class ReactionRepositoryTest
    {
        private AppDbContext _context;
        private IReactionRepository _reactionRepository;


        [SetUp]
        public void Setup()
        {
            // Use an in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "tempdata")
                .Options;

            _context = new AppDbContext(options);
            _reactionRepository = new ReactionRepository(_context);

            // Seed initial data for Users and Articles
            SeedData();
        }

        private void SeedData()
        {
            var users = new List<User>
            {
                new User
                    {
                        Id = 1,
                        Email = "user1@example.com",
                        DisplayName = "User1",
                        Role = "User",
                        GoogleId = "google-id-1",
                        GivenName = "John",
                        FamilyName = "Doe",
                        Picture = "http://example.com/user1.jpg",
                        CreatedAt = DateTime.UtcNow.AddYears(-1),
                        UpdatedAt = DateTime.UtcNow
                    },
                     new User
                    {
                        Id = 2,
                        Email = "user1@example2.com",
                        DisplayName = "User12",
                        Role = "User",
                        GoogleId = "google-id-12",
                        GivenName = "John2",
                        FamilyName = "Doe2",
                        Picture = "http://example.com/user1.jpg",
                        CreatedAt = DateTime.UtcNow.AddYears(-2),
                        UpdatedAt = DateTime.UtcNow
                    }
        };

            var articles = new List<NewsArticle>
            {
                new NewsArticle
                        {
                            Id = "article-1",
                            Author = "Jane Doe",
                            Content = "This is the content of the article. It provides detailed information about the topic.",
                            Date = DateTime.UtcNow.AddDays(-10),
                            Title = "Breaking News: Example Article",
                            ImageUrl = "http://example.com/article-image.jpg",
                            ReadMoreUrl = "http://example.com/full-article",
                            Status = "Approved",
                            Category = "Technology"
                        }
        };

            _context.Users.AddRange(users);
            _context.NewsArticles.AddRange(articles);
            _context.SaveChanges();
        }

        [Test]
        public async Task GetReactionAsync_ShouldReturnReaction()
        {
            // Arrange
            var reaction = new Reaction
            {
                ArticleId = "article-1",
                UserId = 1,
                ReactionType = ReactionType.Like
            };

            await _reactionRepository.AddReactionAsync(reaction);

            // Act
            var result = await _reactionRepository.GetReactionAsync(1, "article-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.UserId);
            Assert.AreEqual("article-1", result.ArticleId);
            Assert.AreEqual(ReactionType.Like, result.ReactionType);
        }

        [Test]
        public async Task AddReactionAsync_ShouldAddReaction()
        {
            // Arrange
            var reaction = new Reaction
            {
                ArticleId = "article-1",
                UserId = 1,
                ReactionType = ReactionType.Like
            };

            // Act
            await _reactionRepository.AddReactionAsync(reaction);
            var result = await _reactionRepository.GetReactionAsync(1, "article-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.UserId);
            Assert.AreEqual("article-1", result.ArticleId);
            Assert.AreEqual(ReactionType.Like, result.ReactionType);
        }
        
        [Test]
        public async Task RemoveReactionAsync_ShouldRemoveReaction()
        {
            // Arrange
            var reaction = new Reaction
            {
                ArticleId = "article-1",
                UserId = 1,
                ReactionType = ReactionType.Like
            };

            await _reactionRepository.AddReactionAsync(reaction);

            // Act
            await _reactionRepository.RemoveReactionAsync(1, "article-1");
            var result = await _reactionRepository.GetReactionAsync(1, "article-1");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task UpdateReactionAsync_ShouldUpdateReaction()
        {
            // Arrange
            var reaction = new Reaction
            {
                ArticleId = "article-1",
                UserId = 1,
                ReactionType = ReactionType.Like
            };

            await _reactionRepository.AddReactionAsync(reaction);

            var updatedReaction = new Reaction
            {
                ArticleId = "article-1",
                UserId = 1,
                ReactionType = ReactionType.Dislike
            };

            // Act
            await _reactionRepository.UpdateReactionAsync(updatedReaction);
            var result = await _reactionRepository.GetReactionAsync(1, "article-1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ReactionType.Dislike, result.ReactionType);
        }

        [Test]
        public async Task AddReactionsAsync_ShouldAddMultipleReactions()
        {
            // Arrange
            var reactions = new List<Reaction>
            {
                new Reaction { ArticleId = "article-1", UserId = 1, ReactionType = ReactionType.Like},
                new Reaction { ArticleId = "article-1", UserId = 2, ReactionType = ReactionType.Dislike }
            };

            // Act
            await _reactionRepository.AddReactionsAsync(reactions);
            var result = await _reactionRepository.GetAllReactionsByArticleAsync("article-1");

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.UserId == 1 && r.ReactionType == ReactionType.Like));
            Assert.IsTrue(result.Any(r => r.UserId == 2 && r.ReactionType == ReactionType.Dislike));
        }

        [Test]
        public async Task GetAllReactionsByArticleAsync_ShouldReturnAllReactions()
        {
            // Arrange
            var reactions = new List<Reaction>
            {
                new Reaction { ArticleId = "article-1", UserId = 1, ReactionType = ReactionType.Like },
                new Reaction { ArticleId = "article-1", UserId = 2, ReactionType = ReactionType.Dislike }
            };

            await _reactionRepository.AddReactionsAsync(reactions);

            // Act
            var result = await _reactionRepository.GetAllReactionsByArticleAsync("article-1");

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(r => r.UserId == 1 && r.ReactionType == ReactionType.Like));
            Assert.IsTrue(result.Any(r => r.UserId == 2 && r.ReactionType == ReactionType.Dislike));
        }
    }
}
