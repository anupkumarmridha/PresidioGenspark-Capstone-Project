using Confluent.Kafka;
using NewsAppAPI.DTOs;
using NewsAppAPI.Models;
using NewsAppAPI.Services.Interfaces;
using System.Text.Json;


namespace NewsAppAPI.Kafka.Consumers
{
    public class CommentConsumer : IHostedService
    {
        private readonly string _bootstrapServers;
        private readonly string _commentsTopic;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CommentConsumer> _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private Task _executingTask;

        public CommentConsumer(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ILogger<CommentConsumer> logger)
        {
            _bootstrapServers = configuration["Kafka:BootstrapServers"];
            _commentsTopic = configuration["Kafka:CommentsTopic"];
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;

            _logger.LogInformation("Kafka Consumer created with BootstrapServers: {BootstrapServers} and CommentsTopic: {CommentsTopic}", _bootstrapServers, _commentsTopic);
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
                GroupId = "comment-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(_commentsTopic);
            _logger.LogInformation("Subscribed to Kafka topic: {Topic}", _commentsTopic);

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

                            consumer.Commit(cr);
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            return Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }
}
