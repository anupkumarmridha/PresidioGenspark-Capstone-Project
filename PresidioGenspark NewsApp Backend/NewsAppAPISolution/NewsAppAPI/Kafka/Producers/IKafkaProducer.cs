using Confluent.Kafka;
using NewsAppAPI.DTOs;

namespace NewsAppAPI.Kafka.Producers
{
    public interface IKafkaProducer
    {
       Task<DeliveryResult<string, string>> ProduceAsync(KafkaMessageDto kafkaMessageDto);
    }
}
