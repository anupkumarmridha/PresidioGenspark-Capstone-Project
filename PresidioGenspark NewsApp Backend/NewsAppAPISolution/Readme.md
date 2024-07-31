To create a Kafka topic named "comments" with 1 partition and a replication factor of 1, use the following command:

```
kafka-topics --create --topic comments --partitions 1 --replication-factor 1 --if-not-exists --bootstrap-server kafka:9092
```

To create a Kafka topic named "reactions" with 1 partition and a replication factor of 1, use the following command:

```
kafka-topics --create --topic reactions --partitions 1 --replication-factor 1 --if-not-exists --bootstrap-server kafka:9092
```

After executing these commands, the topic "reactions" will be created.