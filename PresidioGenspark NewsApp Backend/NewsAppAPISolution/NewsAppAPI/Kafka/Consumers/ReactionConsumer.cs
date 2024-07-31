using Confluent.Kafka;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace NewsAppAPI.Kafka.Consumers
{
    public class ReactionConsumer : IHostedService
    {
        private readonly string _bootstrapServers;
        private readonly string _reactionsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReactionConsumer> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _executingTask;

        public ReactionConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<ReactionConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _reactionsTopic = configuration["Kafka:ReactionsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _logger.LogInformation("Kafka Consumer created with BootstrapServers: {BootstrapServers} and ReactionsTopic: {ReactionsTopic}", _bootstrapServers, _reactionsTopic);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executingTask = Task.Run(() => ExecuteAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            return Task.CompletedTask;
        }

        private async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "reaction-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_reactionsTopic);
            _logger.LogInformation("Subscribed to Kafka topic: {Topic}", _reactionsTopic);

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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
}
