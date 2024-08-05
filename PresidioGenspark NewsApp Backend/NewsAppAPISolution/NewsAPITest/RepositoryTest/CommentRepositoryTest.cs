using Microsoft.EntityFrameworkCore;
using NewsAppAPI.Contexts;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Classes;
using NewsAppAPI.Repositories.Interfaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsAPITest.RepositoryTest
{
    [TestFixture]
    public class CommentRepositoryTest
    {
        private ICommentRepository _commentRepository;
        private AppDbContext _context;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _context = new AppDbContext(options);
            _commentRepository = new CommentRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region AddCommentAsync Tests

        [Test]
        public async Task AddCommentAsync_ShouldAddTopLevelComment()
        {
            // Arrange
            var comment = new Comment
            {
                ArticleId = "article-1",
                Content = "This is a top-level comment.",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };

            // Act
            await _commentRepository.AddCommentAsync(comment);

            // Assert
            var commentInDb = await _context.Comments.FindAsync(comment.Id);
            Assert.IsNotNull(commentInDb);
            Assert.AreEqual(comment.Content, commentInDb.Content);
        }

        [Test]
        public async Task AddCommentAsync_ShouldAddReply()
        {
            // Arrange
            var parentComment = new Comment
            {
                ArticleId = "article-1",
                Content = "This is a parent comment.",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };

            await _commentRepository.AddCommentAsync(parentComment);

            var replyComment = new Comment
            {
                ArticleId = "article-1",
                Content = "This is a reply.",
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                ParentId = parentComment.Id
            };

            // Act
            await _commentRepository.AddCommentAsync(replyComment);

            // Assert
            var replyInDb = await _context.Comments.FindAsync(replyComment.Id);
            Assert.IsNotNull(replyInDb);
            Assert.AreEqual(replyComment.Content, replyInDb.Content);
            Assert.AreEqual(parentComment.Id, replyInDb.ParentId);
        }

        [Test]
        public void AddCommentAsync_InvalidParentId_ShouldThrowException()
        {
            // Arrange
            var comment = new Comment
            {
                ArticleId = "article-1",
                Content = "Invalid reply",
                CreatedAt = DateTime.UtcNow,
                UserId = 1,
                ParentId = 999 // Invalid ParentId
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _commentRepository.AddCommentAsync(comment));
        }

        #endregion AddCommentAsync Tests

        #region GetCommentsByArticleIdAsync Tests

        [Test]
      public async Task GetCommentsByArticleIdAsync_ShouldReturnCommentsInFILOOrder()
{
    // Arrange
    var now = DateTime.UtcNow;
    var comments = new List<Comment>
{
    new Comment 
    { 
        ArticleId = "article-1", 
        Content = "First Comment", 
        CreatedAt = now.AddMinutes(-3), 
        UserId = 1,
        ParentId = null,
        User = new User 
        { 
            Id = 1, 
            DisplayName = "User1", 
            Email = "user1@example.com",
            FamilyName = "Family1",
            GivenName = "Given1",
            GoogleId = "google-id-1",
            Picture = "picture-url-1",
            Role = "Role1"
        }
    },
    new Comment 
    { 
        ArticleId = "article-1", 
        Content = "Second Comment", 
        CreatedAt = now.AddMinutes(-2), 
        UserId = 2,
        ParentId = null,
        User = new User 
        { 
            Id = 2, 
            DisplayName = "User2", 
            Email = "user2@example.com",
            FamilyName = "Family2",
            GivenName = "Given2",
            GoogleId = "google-id-2",
            Picture = "picture-url-2",
            Role = "Role2"
        }
    },
    new Comment 
    { 
        ArticleId = "article-1", 
        Content = "Third Comment", 
        CreatedAt = now.AddMinutes(-1), 
        UserId = 3,
        ParentId = null,
        User = new User 
        { 
            Id = 3, 
            DisplayName = "User3", 
            Email = "user3@example.com",
            FamilyName = "Family3",
            GivenName = "Given3",
            GoogleId = "google-id-3",
            Picture = "picture-url-3",
            Role = "Role3"
        }
    }
};


    // Add comments to the repository
    foreach (var comment in comments)
    {
        await _commentRepository.AddCommentAsync(comment);
    }

    // Double-check that comments were added
    var allCommentsInDb = await _context.Comments.ToListAsync();
    Console.WriteLine($"Total comments in DB: {allCommentsInDb.Count}");
    foreach (var comment in allCommentsInDb)
    {
        Console.WriteLine($"Comment ID: {comment.Id}, Content: {comment.Content}, CreatedAt: {comment.CreatedAt}, UserId: {comment.UserId}, ParentId: {comment.ParentId}");
    }

    // Act
    var result = await _commentRepository.GetCommentsByArticleIdAsync("article-1");
    Console.WriteLine($"Retrieved comments count: {result.Count()}");

    // Assert
    Assert.AreEqual(3, result.Count());

    var commentsList = result.ToList();
    Assert.AreEqual("Third Comment", commentsList.First().Content); // FILO
    Assert.AreEqual("First Comment", commentsList.Last().Content);   // FILO

    // Verify all attributes
    Assert.AreEqual("article-1", commentsList.First().ArticleId);
    Assert.AreEqual("Third Comment", commentsList.First().Content);
    Assert.AreEqual(now.AddMinutes(-1).ToUniversalTime(), commentsList.First().CreatedAt);
    Assert.AreEqual(3, commentsList.First().UserId);
    Assert.AreEqual("User3", commentsList.First().UserName);
    Assert.IsNull(commentsList.First().ParentId);

    Assert.AreEqual("article-1", commentsList.Last().ArticleId);
    Assert.AreEqual("First Comment", commentsList.Last().Content);
    Assert.AreEqual(now.AddMinutes(-3).ToUniversalTime(), commentsList.Last().CreatedAt);
    Assert.AreEqual(1, commentsList.Last().UserId);
    Assert.AreEqual("User1", commentsList.Last().UserName);
    Assert.IsNull(commentsList.Last().ParentId);
}




        #endregion GetCommentsByArticleIdAsync Tests

        #region GetCommentByIdAsync Tests

        [Test]
        public async Task GetCommentByIdAsync_ExistingId_ShouldReturnComment()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var comment = new Comment
            {
                ArticleId = "article-1",
                Content = "First Comment",
                CreatedAt = now.AddMinutes(-3),
                UserId = 1,
                ParentId = null,
                User = new User
                {
                    Id = 1,
                    DisplayName = "User1",
                    Email = "user1@example.com",
                    FamilyName = "Family1",
                    GivenName = "Given1",
                    GoogleId = "google-id-1",
                    Picture = "picture-url-1",
                    Role = "Role1"
                }
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _commentRepository.GetCommentByIdAsync(comment.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(comment.Content, result.Content);
        }

        [Test]
        public async Task GetCommentByIdAsync_NonExistingId_ShouldReturnNull()
        {
            // Act
            var result = await _commentRepository.GetCommentByIdAsync(999); // Non-existing ID

            // Assert
            Assert.IsNull(result);
        }

        #endregion GetCommentByIdAsync Tests

        #region UpdateCommentAsync Tests

        [Test]
        public async Task UpdateCommentAsync_ShouldUpdateExistingComment()
        {
            // Arrange
            var comment = new Comment
            {
                ArticleId = "article-1",
                Content = "Original Content",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            comment.Content = "Updated Content";

            // Act
            await _commentRepository.UpdateCommentAsync(comment);

            // Assert
            var updatedComment = await _context.Comments.FindAsync(comment.Id);
            Assert.IsNotNull(updatedComment);
            Assert.AreEqual("Updated Content", updatedComment.Content);
        }

        [Test]
        public void UpdateCommentAsync_NonExistingComment_ShouldThrowException()
        {
            // Arrange
            var nonExistingComment = new Comment
            {
                Id = 999,
                ArticleId = "article-1",
                Content = "Non-existing Comment",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _commentRepository.UpdateCommentAsync(nonExistingComment));
        }

        #endregion UpdateCommentAsync Tests

        #region DeleteCommentAsync Tests

        [Test]
        public async Task DeleteCommentAsync_ShouldDeleteCommentAndReplies()
        {
            // Arrange
            var parentComment = new Comment
            {
                ArticleId = "article-1",
                Content = "Parent Comment",
                CreatedAt = DateTime.UtcNow,
                UserId = 1
            };

            await _commentRepository.AddCommentAsync(parentComment);

            var reply = new Comment
            {
                ArticleId = "article-1",
                Content = "Reply",
                CreatedAt = DateTime.UtcNow,
                UserId = 2,
                ParentId = parentComment.Id
            };

            await _commentRepository.AddCommentAsync(reply);

            // Act
            await _commentRepository.DeleteCommentAsync(parentComment.Id);

            // Assert
            var parentInDb = await _context.Comments.FindAsync(parentComment.Id);
            var replyInDb = await _context.Comments.FindAsync(reply.Id);

            Assert.IsNull(parentInDb);
            Assert.IsNull(replyInDb);
        }

        [Test]
        public void DeleteCommentAsync_NonExistingComment_ShouldThrowException()
        {
            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _commentRepository.DeleteCommentAsync(999)); // Non-existing ID
        }

        #endregion DeleteCommentAsync Tests
        
        #region AddCommentsAsync
        [Test]
        public async Task AddCommentsAsync_ValidComments_ShouldAddCommentsToDatabase()
        {
            // Arrange
            var comments = new List<Comment>
            {
                new Comment
                {
                    ArticleId = "article-1",
                    Content = "Comment 1",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 1
                },
                new Comment
                {
                    ArticleId = "article-1",
                    Content = "Comment 2",
                    CreatedAt = DateTime.UtcNow,
                    UserId = 2
                }
            };

            // Act
            await _commentRepository.AddCommentsAsync(comments);

            // Assert
            var addedComments = await _context.Comments.ToListAsync();
            Assert.AreEqual(2, addedComments.Count);
            Assert.IsTrue(addedComments.Any(c => c.Content == "Comment 1"));
            Assert.IsTrue(addedComments.Any(c => c.Content == "Comment 2"));
        }

        [Test]
        public void AddCommentsAsync_NullComments_ShouldThrowArgumentException()
        {
            // Arrange
            IEnumerable<Comment> comments = null;

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _commentRepository.AddCommentsAsync(comments));
        }

        [Test]
        public async Task AddCommentsAsync_EmptyComments_ShouldThrowArgumentException()
        {
            // Arrange
            var comments = new List<Comment>();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentException>(async () =>
                await _commentRepository.AddCommentsAsync(comments));
        }
        #endregion AddCommentsAsync
    }
}
