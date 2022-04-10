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
    public class StockScribe : BackgroundService
    {
        private readonly ILogger<StockScribe> _logger;
        private ConnectionFactory _connectionFactory;
        private static IModel? _channel;
        private IConnection _conn;
        private const string _queueName = "stockDecrementer";

        bool durable = true;
        bool exclusive = false;
        bool autoDelete = false;

        private readonly string _url = "amqps://fientxhx:GnS4aTKpa7dM-bGOhMhDn9v7qLZJ2tJZ@goose.rmq2.cloudamqp.com/fientxhx";

        private ManualResetEvent _resetEvent = new ManualResetEvent(false);

        public StockScribe(ILogger<StockScribe> logger)
        {
            _logger = logger;
        }


        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _connectionFactory = new ConnectionFactory()
            {
                Uri = new Uri(_url),
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.Received += ConsumeMessage;

            Console.WriteLine("Task Complete!");
            _channel.BasicConsume(_queueName, false, consumer);
            await Task.CompletedTask;
        }

        private static async Task ConsumeMessage(object sender, BasicDeliverEventArgs @event)
        {


            var body = @event.Body.ToArray();

            DecrementRequest request = JsonSerializer.Deserialize<DecrementRequest>(body);

            var clientSettings = MongoClientSettings.FromConnectionString("mongodb+srv://overseer:DunwallRats@sovranmerchantcatalog.r38ic.mongodb.net/sovran?retryWrites=true&w=majority");
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


        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _conn.Close();
            _logger.LogInformation("We're quitting out");
        }

        public class DecrementRequest
        {
            public string userName { get; set; }
            public string itemId { get; set; }
            public string detail { get; set; }
            public int quantity { get; set; }
        }
    }
}
