using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Requests.BatchRequest;

namespace NewsAppAPI.Services.Classes
{
    public class ArticleFetchingService : BackgroundService
    {
        private readonly ILogger<ArticleFetchingService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IServiceScopeFactory _scopeFactory;

        public ArticleFetchingService(
            ILogger<ArticleFetchingService> logger,
            IHttpClientFactory httpClientFactory,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
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

                // Wait for 1 hour before the next fetch
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
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
                        Id = (string)article["id"],  // No parsing needed, assign as string
                        Author = (string)article["author"],
                        Content = (string)article["content"],
                        Date = DateTime.Parse((string)article["date"]),
                        Title = (string)article["title"],
                        ImageUrl = (string)article["imageUrl"],
                        ReadMoreUrl = (string)article["readMoreUrl"],
                        Status = "Pending"
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


    }
}
