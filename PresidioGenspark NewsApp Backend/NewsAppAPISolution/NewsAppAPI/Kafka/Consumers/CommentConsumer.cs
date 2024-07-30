using Confluent.Kafka;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using NewsAppAPI.Services.Interfaces;

namespace NewsAppAPI.Kafka.Consumers
{
    public class CommentConsumer : BackgroundService
    {
        private readonly string _bootstrapServers;
        private readonly string _commentsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CommentConsumer> _logger;

        public CommentConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<CommentConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _commentsTopic = configuration["Kafka:CommentsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "comment-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false // Manage offsets manually
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_commentsTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        var kafkaMessage = JsonSerializer.Deserialize<KafkaMessageDto>(cr.Value);
                        var deserializedComment = JsonSerializer.Deserialize<Comment>(kafkaMessage.Message);

                        if (kafkaMessage != null && IsValidKafkaMessageDto(kafkaMessage) && deserializedComment != null)
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var commentService = scope.ServiceProvider.GetRequiredService<ICommentService>();

                            await PerformOperationAsync(commentService, deserializedComment, kafkaMessage.Operation);

                            consumer.Commit(cr); // Commit the offset after processing the message
                        }
                        else
                        {
                            _logger.LogWarning("Invalid message received: {Message}", cr.Value);
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError($"JSON deserialization error: {jsonEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error processing message: {ex.Message}");
                    }
                }
            }
            catch (ConsumeException e)
            {
                _logger.LogError($"Consumption failed: {e.Error.Reason}");
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task PerformOperationAsync(ICommentService commentService, Comment comment, string operation)
        {
            switch (operation)
            {
                case "add":
                    await commentService.AddCommentAsync(comment);
                    break;
                case "update":
                    await commentService.UpdateCommentAsync(comment);
                    break;
                case "delete":
                    await commentService.DeleteCommentAsync(comment.Id);
                    break;
                default:
                    _logger.LogWarning("Unknown operation: {Operation}", operation);
                    break;
            }
        }

        private bool IsValidKafkaMessageDto(KafkaMessageDto kafkaMessageDto)
        {
            return !string.IsNullOrEmpty(kafkaMessageDto.Topic)
                && !string.IsNullOrEmpty(kafkaMessageDto.Message)
                && !string.IsNullOrEmpty(kafkaMessageDto.Operation);
        }
    }
}
