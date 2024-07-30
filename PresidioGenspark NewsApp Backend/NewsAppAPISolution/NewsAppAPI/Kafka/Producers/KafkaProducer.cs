using Confluent.Kafka;
using NewsAppAPI.DTOs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace NewsAppAPI.Kafka.Producers
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly ILogger<KafkaProducer> _logger;
        private readonly IProducer<string, string> _producer;

        public KafkaProducer(IConfiguration configuration, ILogger<KafkaProducer> logger)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger = logger;
        }

        public async Task<DeliveryResult<string, string>> ProduceAsync(KafkaMessageDto kafkaMessageDto)
        {
            if (!IsValidKafkaMessageDto(kafkaMessageDto))
            {
                throw new ArgumentException("Invalid KafkaMessageDto");
            }
            var topic = kafkaMessageDto.Topic;
            var operation = kafkaMessageDto.Operation;
            var message = kafkaMessageDto.Message;
            var messageValue = JsonConvert.SerializeObject(new
            {
                Operation = operation,
                Data = message
            });

            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, new Message<string, string> { Value = messageValue });

                // Log the result if needed
                _logger.LogInformation($"Message delivered to {deliveryResult.TopicPartitionOffset}");
                return deliveryResult;
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
                // Handle the exception appropriately
                throw;
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
