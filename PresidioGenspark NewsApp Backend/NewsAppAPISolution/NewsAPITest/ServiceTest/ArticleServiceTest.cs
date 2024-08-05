using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using NewsAppAPI.Contexts;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using NewsAppAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NewsAppAPI.Services.Classes;

namespace NewsAPITest.ServiceTest
{
    public class ArticleServiceTest
    {
        private Mock<IArticleRepository> _articleRepositoryMock;
        private IArticleService _articleService;
        private List<NewsArticle> _articles;
        private NewsArticle _article;

        [SetUp]
        public void Setup()
        {
            // Mock the repository
            _articleRepositoryMock = new Mock<IArticleRepository>();

            // Inject the mock into the service (assuming a concrete implementation exists)
            _articleService = new ArticleService(_articleRepositoryMock.Object);

            _articles = new List<NewsArticle>
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

            _article = new NewsArticle
            {
                Id = "3",
                Title = "Article 3",
                Content = "New Content3",
                Date = DateTime.Now,
                Author = "Author Name3",
                Category = "Tech",
                ImageUrl = "http://example.com/image.jpg",
                ReadMoreUrl = "http://example.com/readmore",
                Status = "Approved"
            };

            _articleRepositoryMock
                .Setup(repo => repo.AddArticleAsync(It.IsAny<NewsArticle>()))
                .Callback<NewsArticle>(article => _articles.Add(article))
                .Returns(Task.CompletedTask);


            _articleRepositoryMock
              .Setup(repo => repo.BulkDeleteArticlesAsync(It.IsAny<IEnumerable<string>>()))
              .Callback<IEnumerable<string>>(ids =>
              {
                  _articles.RemoveAll(a => ids.Contains(a.Id));
              })
              .Returns(Task.CompletedTask);


            _articleRepositoryMock
                .Setup(repo => repo.GetArticleByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((string id) =>
                {
                    var article = _articles.FirstOrDefault(a => a.Id == id);
                    if (article == null) return null;
                    return new ArticleDto
                    {
                        Id = article.Id,
                        Title = article.Title,
                        Content = article.Content,
                        Author = article.Author,
                        Category = article.Category,
                        ImageUrl = article.ImageUrl,
                        ReadMoreUrl = article.ReadMoreUrl,
                        CreatedAt = article.Date,
                        TotalLikes = article.Reactions.Count(r => r.ReactionType == ReactionType.Like),
                        TotalDislikes = article.Reactions.Count(r => r.ReactionType == ReactionType.Dislike),
                        TotalComments = article.Comments.Count
                    };
                });

            _articleRepositoryMock
                .Setup(repo => repo.GetAllArticlesByStatusAsync(It.IsAny<string>()))
                .ReturnsAsync((string status) => _articles.Where(a => a.Status == status));

            _articleRepositoryMock
                .Setup(repo => repo.GetFilteredArticlesAsync(It.IsAny<ArticleFilter>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((ArticleFilter filter, int pageNumber, int pageSize) =>
                {
                    var filteredArticles = _articles.Where(a => a.Category == filter.Category && a.Status == filter.Status);
                    return new PaginatedArticlesDto
                    {
                        TotalCount = filteredArticles.Count(),
                        Articles = filteredArticles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
                    };
                });

            _articleRepositoryMock
            .Setup(repo => repo.DeleteArticleAsync(It.IsAny<string>()))
            .Callback<string>(id =>
            {
                _articles.RemoveAll(a => a.Id == id);
            })
            .Returns(Task.CompletedTask);



            _articleRepositoryMock
                .Setup(repo => repo.BulkDeleteArticlesAsync(It.IsAny<IEnumerable<string>>()))
                .Callback<IEnumerable<string>>(ids =>
                {
                    _articles.RemoveAll(a => ids.Contains(a.Id));
                })
                .Returns(Task.CompletedTask);

            _articleRepositoryMock
                .Setup(repo => repo.BulkUpdateArticlesStatusAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        [Test]
        public async Task BulkInsertArticlesAsync_ShouldInsertArticles()
        {
            // Arrange
            // Act
            await _articleService.BulkInsertArticlesAsync(_articles, "Tech");

            // Assert
            var result = await _articleService.GetFilteredArticlesAsync(new ArticleFilter { Category = "Tech" }, 1, 10);
            Assert.AreEqual(2, result.TotalCount);
        }

        [Test]
        public async Task GetAllArticlesByStatusAsync_ShouldReturnArticlesWithStatus()
        {
            // Arrange
            var status = "Approved";
            await _articleService.BulkInsertArticlesAsync(_articles, "Tech");
            // Act
            var result = await _articleService.GetAllArticlesByStatusAsync(status);

            // Assert
            Assert.AreEqual(1, result.Count());
            Assert.IsTrue(result.All(a => a.Status == status));
        }

        [Test]
        public async Task GetFilteredArticlesAsync_ShouldReturnPaginatedArticles()
        {
            // Arrange
            var filter = new ArticleFilter { Category = "Tech", Status = "Approved" };
            // Act
            var result = await _articleService.GetFilteredArticlesAsync(filter, 1, 10);

            // Assert
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(1, result.Articles.Count());
        }

        [Test]
        public async Task GetArticleByIdAsync_ShouldReturnArticle()
        {
            // Arrange
            var articleId = "3";

            await _articleService.AddArticleAsync(_article);

            // Act
            var result = await _articleService.GetArticleByIdAsync(articleId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(articleId, result.Id);
        }

        [Test]
        public async Task AddArticleAsync_ShouldAddArticle()
        {
            // Arrange
            var articleId = "3";

            await _articleService.AddArticleAsync(_article);

            // Act
            var result = await _articleService.GetArticleByIdAsync(articleId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(articleId, result.Id);
        }

        [Test]
        public async Task DeleteArticleAsync_ShouldDeleteArticle()
        {
            // Arrange
            var articleId = "3";

            await _articleService.AddArticleAsync(_article);

            // Act
            await _articleService.DeleteArticleAsync(articleId);

            var result = await _articleService.GetArticleByIdAsync(articleId);
            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task BulkDeleteArticlesAsync_ShouldDeleteMultipleArticles()
        {
            // Arrange
            var articleIds = new List<string> { "1", "2" };
            await _articleService.BulkInsertArticlesAsync(_articles, "Tech");

            // Act
            await _articleService.BulkDeleteArticlesAsync(articleIds);

            // Assert
            var result1 = await _articleService.GetArticleByIdAsync("1");
            var result2 = await _articleService.GetArticleByIdAsync("2");
            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }
    }
}