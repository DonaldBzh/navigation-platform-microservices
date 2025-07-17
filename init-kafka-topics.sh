#!/bin/bash
set -e

echo "✅ Waiting for Kafka to be ready..."
while ! kafka-topics --bootstrap-server kafka:29092 --list > /dev/null 2>&1; do
echo "Waiting for Kafka..."
sleep 2
done

echo "✅ Creating Kafka topics..."
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic user-created-events --partitions 3 --replication-factor 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic user-status-changed-events --partitions 3 --replication-factor 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic journey-created-events --partitions 3 --replication-factor 1
kafka-topics --bootstrap-server kafka:29092 --create --if-not-exists --topic daily-goal-achieved-events --partitions 3 --replication-factor 1


echo "✅ Kafka topics created successfully!"
echo "✅ Current topics:"
kafka-topics --bootstrap-server kafka:29092 --list
