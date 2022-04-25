using CatalogService.Model;
using CatalogService.Model.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace CatalogService.Business
{
    /// <summary>
    /// StockScribe is a background task purpose-built to decrement stock upon a 
    /// new purchase. 
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
    public class StockScribe : BackgroundService
    {
        private ConnectionFactory _connectionFactory;
        private static IModel? _channel;
        private IConnection _conn;
        private const string _queueName = "stockDecrementer";
        private 
        bool durable = true;
        bool exclusive = false;
        bool autoDelete = false;

        
        public string _queueUrl;
        public string _mongoUrl;
        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public StockScribe(string queueStr, string mongoStr)
        {
            _queueUrl = queueStr;
            _mongoUrl = mongoStr;
        }


        /// <summary>
        /// Initiating call. Creates a listener for the RabbitMq queue.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(_queueUrl),
                DispatchConsumersAsync = true
            };

            _conn = _connectionFactory.CreateConnection();

            _channel = _conn.CreateModel();

            _channel.QueueDeclare(
                _queueName,
                durable,
                exclusive,
                autoDelete,
                null
            );

            return base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Execution of an event-driven task. (i.e. when a message is received.)
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += ConsumeMessage;

            _channel.BasicConsume(_queueName, false, consumer);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Main task purpose. DecrementRequest is serialized and used in order to perform a stock item decrement.
        /// 
        /// Used in order flow.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="event"></param>
        /// <returns></returns>
        private static async Task ConsumeMessage(object sender, BasicDeliverEventArgs @event)
        {


            var body = @event.Body.ToArray();

            DecrementRequest request = JsonSerializer.Deserialize<DecrementRequest>(body);

            var clientSettings = MongoClientSettings.FromConnectionString("amqps://fientxhx:GnS4aTKpa7dM-bGOhMhDn9v7qLZJ2tJZ@goose.rmq2.cloudamqp.com/fientxhx");
            clientSettings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(clientSettings);
            var db = client.GetDatabase("sovran");
            var catalogs = db.GetCollection<CatalogEntry>("catalogs");
            try
            {
                var filter = Builders<CatalogEntry>.Filter.Where(x => x.userName == request.userName && x.catalog.Any(i => i.Id == request.itemId));
                var update = Builders<CatalogEntry>.Update.Set(x => x.catalog[-1].itemQty[request.detail], request.quantity);
                UpdateResult result = catalogs.UpdateOneAsync(filter, update).Result;
                if (result.IsAcknowledged)
                {
                    _channel.BasicAck(@event.DeliveryTag, true);
                }
                else
                {
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _channel.BasicAck(@event.DeliveryTag, false);
            }
        }

        /// <summary>
        /// Unused method to cease task. Given task is a necessary background service, it is never called to cease.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _conn.Close();
        }

        /// <summary>
        /// Main container class for stock decrement details.
        /// </summary>
        public class DecrementRequest
        {
            /// <summary>
            /// Username of merchant whom's stock is being accessed.
            /// </summary>
            public string userName { get; set; }
            /// <summary>
            /// Id of their respective stock item.
            /// </summary>
            public string itemId { get; set; }
            /// <summary>
            /// Detail is for the respective stock item's measurements, size, etc.
            /// </summary>
            public string detail { get; set; }
            /// <summary>
            /// The number of this item to decrement. Given current progress, this will be limited to one item.
            /// </summary>
            public int quantity { get; set; }
        }
    }
}
