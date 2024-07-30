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
    public class CommentConsumer : BackgroundService
    {
        private readonly string _bootstrapServers;
        private readonly string _commentsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CommentConsumer> _logger;
        private readonly int _batchSize;
        private readonly TimeSpan _batchInterval;

        private readonly List<Comment> _batch;
        private readonly Timer _timer;

        public CommentConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<CommentConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _commentsTopic = configuration["Kafka:CommentsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _batchSize = int.Parse(configuration["Kafka:BatchSize"] ?? "100");
            _batchInterval = TimeSpan.FromSeconds(int.Parse(configuration["Kafka:BatchInterval"] ?? "10"));
            _batch = new List<Comment>();
            _timer = new Timer(ProcessBatch, null, _batchInterval, _batchInterval);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                GroupId = "comment-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_commentsTopic);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var cr = consumer.Consume(stoppingToken);
                    var kafkaMessage = JsonSerializer.Deserialize<KafkaMessageDto>(cr.Value);
                    var deserializedComment = JsonSerializer.Deserialize<Comment>(kafkaMessage.Message);

                    if (kafkaMessage != null && IsValidKafkaMessageDto(kafkaMessage) && deserializedComment != null)
                    {
                        lock (_batch)
                        {
                            _batch.Add(deserializedComment);
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
            List<Comment> batchToProcess;

            lock (_batch)
            {
                if (_batch.Count == 0)
                    return;

                batchToProcess = new List<Comment>(_batch);
                _batch.Clear();
            }

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var commentRepository = scope.ServiceProvider.GetRequiredService<ICommentRepository>();

                var addTasks = batchToProcess.Select(comment =>
                    PerformBatchOperation(commentRepository, comment, "add")
                );

                await Task.WhenAll(addTasks);

                _logger.LogInformation($"Processed batch of {batchToProcess.Count} comments.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing batch: {ex.Message}");
                // Optionally, you can re-add failed items to the batch or log them for further inspection.
            }
        }

        private async Task PerformBatchOperation(ICommentRepository commentRepository, Comment comment, string operation)
        {
            switch (operation)
            {
                case "add":
                    await commentRepository.AddCommentAsync(comment);
                    break;
                case "update":
                    await commentRepository.UpdateCommentAsync(comment);
                    break;
                case "delete":
                    await commentRepository.DeleteCommentAsync(comment.Id);
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
