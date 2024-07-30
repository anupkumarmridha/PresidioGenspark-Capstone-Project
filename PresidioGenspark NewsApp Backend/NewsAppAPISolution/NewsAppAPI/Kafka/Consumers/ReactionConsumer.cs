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
    public class ReactionConsumer : BackgroundService
    {
        private readonly string _bootstrapServers;
        private readonly string _reactionsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReactionConsumer> _logger;

        public ReactionConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<ReactionConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _reactionsTopic = configuration["Kafka:ReactionsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "reaction-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false // Manage offsets manually
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_reactionsTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = consumer.Consume(stoppingToken);
                        var kafkaMessage = JsonSerializer.Deserialize<KafkaMessageDto>(cr.Value);
                        var deserializedReaction = JsonSerializer.Deserialize<Reaction>(kafkaMessage.Message);

                        if (kafkaMessage != null && IsValidKafkaMessageDto(kafkaMessage) && deserializedReaction != null)
                        {
                            using var scope = _serviceScopeFactory.CreateScope();
                            var reactionService = scope.ServiceProvider.GetRequiredService<IReactionService>();

                            await PerformOperationAsync(reactionService, deserializedReaction, kafkaMessage.Operation);

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

        private async Task PerformOperationAsync(IReactionService reactionService, Reaction reaction, string operation)
        {
            switch (operation)
            {
                case "add":
                    await reactionService.AddReactionAsync(reaction);
                    break;
                case "remove":
                    await reactionService.RemoveReactionAsync(reaction.UserId, reaction.ArticleId);
                    break;
                case "update":
                    await reactionService.UpdateReactionAsync(reaction);
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
