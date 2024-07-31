using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NewsAppAPI.Services.Classes
{
    public class ArticleFetchingService : IHostedService, IDisposable
    {
        private readonly ILogger<ArticleFetchingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public ArticleFetchingService(
            ILogger<ArticleFetchingService> logger,
            IHttpClientFactory httpClientFactory,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Schedule the timer to call FetchAndStoreArticlesAsync every hour
            _timer = new Timer(ExecuteAsync, null, TimeSpan.Zero, TimeSpan.FromDays(1));

            return Task.CompletedTask;
        }

        private async void ExecuteAsync(object state)
        {
            try
            {
                _logger.LogInformation("Fetching articles from InShorts API.");

                await FetchAndStoreArticlesAsync();

                _logger.LogInformation("Articles fetched and stored successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching articles.");
            }
        }

        private async Task FetchAndStoreArticlesAsync()
        {
            var categories = new[] { "all", "business", "sports", "technology", "entertainment" };
            var httpClient = _httpClientFactory.CreateClient();

            var fetchTasks = categories.Select(async category =>
            {
                string response = string.Empty; // Declare response variable at appropriate scope
                try
                {
                    response = await httpClient.GetStringAsync($"https://inshortsapi.vercel.app/news?category={category}");
                    var json = JToken.Parse(response);

                    if (json["data"] is not JArray articles)
                    {
                        _logger.LogError($"Expected JArray in 'data' but received {json["data"]?.Type}. Response: {response}");
                        return;
                    }

                    var newsArticles = articles.Select(article => new NewsArticle
                    {
                        Id = (string)article["id"],
                        Author = (string)article["author"],
                        Content = (string)article["content"],
                        Date = DateTime.Parse((string)article["date"]),
                        Title = (string)article["title"],
                        ImageUrl = (string)article["imageUrl"],
                        ReadMoreUrl = (string)article["readMoreUrl"],
                        Status = "Pending",
                        Category = category
                    }).ToList();

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var articleService = scope.ServiceProvider.GetRequiredService<IArticleService>();
                        await articleService.BulkInsertArticlesAsync(newsArticles, category);
                    }
                }
                catch (JsonReaderException ex)
                {
                    _logger.LogError(ex, $"Error parsing JSON for category {category}. Response: {response}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while processing category {category}. Response: {response}");
                }
            });

            await Task.WhenAll(fetchTasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
