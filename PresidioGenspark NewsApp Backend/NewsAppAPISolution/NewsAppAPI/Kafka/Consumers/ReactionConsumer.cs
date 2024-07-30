using Confluent.Kafka;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Repositories.Interfaces;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewsAppAPI.Kafka.Consumers
{
    public class ReactionConsumer : BackgroundService
    {
        private readonly string _bootstrapServers;
        private readonly string _reactionsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReactionConsumer> _logger;
        private readonly int _batchSize;
        private readonly TimeSpan _batchInterval;

        private readonly List<Reaction> _batch;
        private readonly Timer _timer;

        public ReactionConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<ReactionConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _reactionsTopic = configuration["Kafka:ReactionsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _batchSize = int.Parse(configuration["Kafka:BatchSize"] ?? "100");
            _batchInterval = TimeSpan.FromSeconds(int.Parse(configuration["Kafka:BatchInterval"] ?? "10"));
            _batch = new List<Reaction>();
            _timer = new Timer(ProcessBatch, null, _batchInterval, _batchInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "reaction-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_reactionsTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var kafkaMessage = JsonSerializer.Deserialize<KafkaMessageDto>(cr.Value);
                    var deserializedReaction = JsonSerializer.Deserialize<Reaction>(kafkaMessage.Message);

                    if (kafkaMessage != null && IsValidKafkaMessageDto(kafkaMessage) && deserializedReaction != null)
                    {
                        lock (_batch)
                        {
                            _batch.Add(deserializedReaction);
                            if (_batch.Count >= _batchSize)
                            {
                                ProcessBatch(null);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid message received: {Message}", cr.Value);
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

        private async void ProcessBatch(object state)
        {
            List<Reaction> batchToProcess;

            lock (_batch)
            {
                if (_batch.Count == 0)
                    return;

                batchToProcess = new List<Reaction>(_batch);
                _batch.Clear();
            }

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var reactionRepository = scope.ServiceProvider.GetRequiredService<IReactionRepository>();

                var addTasks = batchToProcess.Select(reaction =>
                    PerformBatchOperation(reactionRepository, reaction, "add")
                );

                await Task.WhenAll(addTasks);

                _logger.LogInformation($"Processed batch of {batchToProcess.Count} reactions.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing batch: {ex.Message}");
                // Optionally, you can re-add failed items to the batch or log them for further inspection.
            }
        }

        private async Task PerformBatchOperation(IReactionRepository reactionRepository, Reaction reaction, string operation)
        {
            switch (operation)
            {
                case "add":
                    await reactionRepository.AddReactionAsync(reaction);
                    break;
                case "remove":
                    await reactionRepository.RemoveReactionAsync(reaction.UserId, reaction.ArticleId);
                    break;
                case "update":
                    await reactionRepository.UpdateReactionAsync(reaction);
                    break;
                    // Add more cases as needed
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
