using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.DTOs;
using NewsAppAPI.Repositories.Classes;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsAppAPI.Repositories.Interfaces;

namespace NewsAPITest.RepositoryTest
{
    public class ArticleRepositoryTests
    {
        private Mock<ILogger<ArticleRepository>> _loggerMock;
        private DbContextOptions<AppDbContext> _dbContextOptions;
        private AppDbContext _context;
        private IArticleRepository _articleRepository;

        [SetUp]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ArticleRepository>>();
            _dbContextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new AppDbContext(_dbContextOptions);
            _articleRepository = new ArticleRepository(_context, _loggerMock.Object);
        }

        [TearDown]
        public void Teardown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task BulkInsertArticlesAsync_ShouldInsertNewArticles()
        {
            // Arrange
            var newArticles = new List<NewsArticle>
            {
                new NewsArticle
            {
                Id = "1",
                Title = "New Article1",
                Content = "New Content1",
                Date = DateTime.Now,
                Author = "Author Name1",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            },
            new NewsArticle
            {
                Id = "2",
                Title = "New Article",
                Content = "New Content",
                Date = DateTime.Now,
                Author = "Author Name",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            }
        };

            // Act
            await _articleRepository.BulkInsertArticlesAsync(newArticles, "Tech");

            // Assert
            var articlesInDb = _context.NewsArticles.ToList();
            Assert.AreEqual(2, articlesInDb.Count);
        }

        [Test]
        public async Task AddArticleAsync_ShouldAddArticle()
        {
            // Arrange
            var article = new NewsArticle
            {
                Id = "1",
                Title = "New Article",
                Content = "New Content",
                Date = DateTime.Now,
                Author = "Author Name",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            };

            // Act
            await _articleRepository.AddArticleAsync(article);

            // Assert
            var articleInDb = await _context.NewsArticles.FindAsync("1");
            Assert.IsNotNull(articleInDb);
            Assert.AreEqual("New Article", articleInDb.Title);
        }


        [Test]
        public async Task BulkUpdateArticlesStatusAsync_ShouldUpdateArticleStatus()
        {
            // Arrange
            var articles = new List<NewsArticle>
            {
                new NewsArticle
            {
                Id = "1",
                Title = "New Article1",
                Content = "New Content1",
                Date = DateTime.Now,
                Author = "Author Name1",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            },
            new NewsArticle
            {
                Id = "2",
                Title = "New Article",
                Content = "New Content",
                Date = DateTime.Now,
                Author = "Author Name",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            }
            };
            await _context.NewsArticles.AddRangeAsync(articles);
            await _context.SaveChangesAsync();

            var articleIds = new List<string> { "1", "2" };

            // Act
            await _articleRepository.BulkUpdateArticlesStatusAsync(articleIds, "Approved");

            // Assert
            var articlesInDb = _context.NewsArticles.Where(a => articleIds.Contains(a.Id)).ToList();
            Assert.True(articlesInDb.All(a => a.Status == "Approved"));
        }

        [Test]
        public async Task DeleteArticleAsync_ShouldDeleteArticle()
        {
            // Arrange
            var article = new NewsArticle
            {
                Id = "1",
                Title = "New Article",
                Content = "New Content",
                Date = DateTime.Now,
                Author = "Author Name",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Pending"
            };
            await _context.NewsArticles.AddAsync(article);
            await _context.SaveChangesAsync();

            // Act
            await _articleRepository.DeleteArticleAsync("1");

            // Assert
            var articleInDb = await _context.NewsArticles.FindAsync("1");
            Assert.IsNull(articleInDb);
        }

        [Test]
        public async Task BulkDeleteArticlesAsync_ShouldDeleteArticles()
        {
            // Arrange
            var articles = new List<NewsArticle>
            {
                new NewsArticle
                {
                    Id = "1",
                    Title = "New Article1",
                    Content = "New Content1",
                    Date = DateTime.Now,
                    Author = "Author Name1",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Pending"
                },
                new NewsArticle
                {
                    Id = "2",
                    Title = "New Article",
                    Content = "New Content",
                    Date = DateTime.Now,
                    Author = "Author Name",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Pending"
                }
            };
            await _context.NewsArticles.AddRangeAsync(articles);
            await _context.SaveChangesAsync();

            var articleIds = new List<string> { "1", "2" };

            // Act
            await _articleRepository.BulkDeleteArticlesAsync(articleIds);

            // Assert
            var articlesInDb = _context.NewsArticles.Where(a => articleIds.Contains(a.Id)).ToList();
            Assert.IsEmpty(articlesInDb);
        }

        [Test]
        public async Task GetAllArticlesByStatusAsync_ShouldReturnArticlesByStatus()
        {
            // Arrange
            var articles = new List<NewsArticle>
            {
                new NewsArticle
                {
                    Id = "1",
                    Title = "Article 1",
                    Content = "New Content1",
                    Date = DateTime.Now,
                    Author = "Author Name1",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Approved"
                },
                new NewsArticle
                {
                    Id = "2",
                    Title = "New Article",
                    Content = "New Content",
                    Date = DateTime.Now,
                    Author = "Author Name",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Pending"
                }
            };
            await _context.NewsArticles.AddRangeAsync(articles);
            await _context.SaveChangesAsync();

            // Act
            var approvedArticles = await _articleRepository.GetAllArticlesByStatusAsync("Approved");

            // Assert
            Assert.AreEqual(1, approvedArticles.Count());
            Assert.AreEqual("Article 1", approvedArticles.First().Title);
        }

        [Test]
        public async Task GetArticleByIdAsync_ShouldReturnArticleDto()
        {
            // Arrange
            var article = new NewsArticle
            {
                Id = "1",
                Title = "Article",
                Content = "Content",
                Date = DateTime.Now,
                Author = "Author",
                Category = "Category",
                ImageUrl = "ImageUrl",
                ReadMoreUrl = "ReadMoreUrl",
                Status = "Approved",
                Reactions = new List<Reaction>
                {
                    new Reaction { ReactionType = ReactionType.Like },
                    new Reaction { ReactionType = ReactionType.Dislike }
                },
                Comments = new List<Comment>
                {
                    new Comment { Content = "Comment", CreatedAt = DateTime.Now }
                }
            };
            await _context.NewsArticles.AddAsync(article);
            await _context.SaveChangesAsync();

            // Act
            var result = await _articleRepository.GetArticleByIdAsync("1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Article", result.Title);
            Assert.AreEqual(1, result.TotalLikes);
            Assert.AreEqual(1, result.TotalDislikes);
            Assert.AreEqual(1, result.TotalComments);
        }

        [Test]
        public async Task GetFilteredArticlesAsync_ShouldReturnFilteredArticles()
        {
            // Arrange
            var articles = new List<NewsArticle>
            {
                new NewsArticle
                {
                    Id = "1",
                    Title = "New Article1",
                    Content = "New Content1",
                    Date = DateTime.Now,
                    Author = "Author Name1",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Approved"
                },
                new NewsArticle
                {
                    Id = "2",
                    Title = "New Article",
                    Content = "New Content",
                    Date = DateTime.Now,
                    Author = "Author Name",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Pending"
                },
                new NewsArticle
                {
                    Id = "3",
                    Title = "New Article3",
                    Content = "New Content3",
                    Date = DateTime.Now,
                    Author = "Author Name3",
                    Category = "Tech",
                    ImageUrl = "http://example.com/image.jpg",
                    ReadMoreUrl = "http://example.com/readmore",
                    Status = "Approved"
                }
            };
            await _context.NewsArticles.AddRangeAsync(articles);
            await _context.SaveChangesAsync();

            var filter = new ArticleFilter { Category = "Tech", Status = "Approved" };

            // Act
            var result = await _articleRepository.GetFilteredArticlesAsync(filter, 1, 2);

            // Assert
            Assert.AreEqual(2, result.TotalCount);
            Assert.IsTrue(result.Articles.All(a => a.Category == "Tech" && a.Status == "Approved"));
        }
    }
}
