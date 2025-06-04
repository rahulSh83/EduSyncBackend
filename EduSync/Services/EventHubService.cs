﻿using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;

namespace EduSyncAPI.Services
{
    public class EventHubService
    {
        private readonly EventHubProducerClient _producerClient;
        private readonly ILogger<EventHubService> _logger;

        public EventHubService(IConfiguration configuration, ILogger<EventHubService> logger)
        {
            string connectionString = configuration["AzureEventHubs:ConnectionString"];
            string eventHubName = configuration["AzureEventHubs:EventHubName"];

            _producerClient = new EventHubProducerClient(connectionString, eventHubName);

            _logger = logger;

            _logger.LogInformation("Initializing EventHubProducerClient with event hub: " + eventHubName);
        }

        public async Task SendEventAsync<T>(T eventData, string eventType)
        {
            _logger.LogWarning("SendEventAsync called for event type: " + eventType);
            try
            {
                // Create the event data
                var eventBody = JsonSerializer.Serialize(eventData);
                var eventBytes = Encoding.UTF8.GetBytes(eventBody);

                // Create the event
                var eventDataBatch = await _producerClient.CreateBatchAsync();
                var eventDataItem = new EventData(eventBytes);
                eventDataItem.Properties["EventType"] = eventType;

                // Add the event to the batch
                if (!eventDataBatch.TryAdd(eventDataItem))
                {
                    _logger.LogError("Event is too large for the batch");
                    throw new Exception("Event is too large for the batch");
                }

                _logger.LogWarning("About to send event to Event Hub...");
                // Send the batch
                await _producerClient.SendAsync(eventDataBatch);
                _logger.LogInformation($"Event of type {eventType} sent successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending event of type {eventType}");
                throw;
            }
        }

        public async Task DisposeAsync()
        {
            if (_producerClient != null)
            {
                await _producerClient.DisposeAsync();
            }
        }
    }
}
