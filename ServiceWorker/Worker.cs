using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Model.Delivery;
using Model.ParcelDeliveryTracking;
using MongoDB.Driver;

namespace ServiceWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: "delivery",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            Console.WriteLine(" MESSAGE RECIEVED.");
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Delivery? delivery = JsonSerializer.Deserialize<Delivery>(message);
            InsertToMongoDB(delivery);
            // WriteToCsv(delivery);
        };

        channel.BasicConsume(queue: "delivery",
                             autoAck: true,
                             consumer: consumer);

            static void CreateTracking(Delivery delivery) {
                Console.WriteLine("Create Tracking started");
            var TrackingElement = new ParcelDeliveryTracking
            {
                ParcelDeliveryID = delivery.pakkeID,
                Status = "In Transit",
                Tracking = new ParcelDeliveryTracking.TrackingInformation
                {
                    TrackingId = GenerateUniqueTrackingId(),
                    Carrier = "Vesterhavsekspressen",
                    StatusUpdates = new List<ParcelDeliveryTracking.TrackingInformation.StatusUpdate>
                {
                    new ParcelDeliveryTracking.TrackingInformation.StatusUpdate
                    {
                        Date = DateTime.Now,
                        Description = "Order received from HaaV member"
                    }
                }
                }        
            };
            // Define the MongoDB connection string
            string mongoDBConnectionString = "mongodb://localhost:27018/";

            // Create a new MongoClient
            var client = new MongoClient(mongoDBConnectionString);

            // Get the IMongoDatabase
            var database = client.GetDatabase("ParcelDelivery");

            // Get the IMongoCollection
            var collection = database.GetCollection<ParcelDeliveryTracking>("ParcelDeliveryTracking");

            // Insert the Worker information to the MongoDB
            collection.InsertOne(TrackingElement);
        }
            

        static void InsertToMongoDB(Delivery delivery)
        {
            Console.WriteLine("Insert started");
            // Define the MongoDB connection string
            string mongoDBConnectionString = "mongodb://localhost:27018/";

            // Create a new MongoClient
            var client = new MongoClient(mongoDBConnectionString);

            // Get the IMongoDatabase
            var database = client.GetDatabase("ParcelDelivery");

            // Get the IMongoCollection
            var collection = database.GetCollection<Delivery>("Deliveries");

            // Insert the Worker information to the MongoDB
            collection.InsertOne(delivery);

            CreateTracking(delivery);

        }

        static string GenerateUniqueTrackingId()
{
    // Generate a random number
    Random random = new Random();
    int randomNumber = random.Next(1000, 9999); // Generate a 4-digit random number

    // Get the current timestamp
    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Combine timestamp and random number to create a unique tracking ID
    string trackingId = $"TRACK{timestamp}{randomNumber}";

    return trackingId;
}

        /*
        static void WriteToCsv(Delivery delivery)
        {
            Console.WriteLine("Write started");
            // Define the CSV file path
            string csvFilePath = "deliveries.csv";

            // Check if the CSV file exists, if not, create it and write header
            if (!File.Exists(csvFilePath))
            {
                using (var writer = new StreamWriter(csvFilePath))
                {
                    writer.WriteLine("pakkeID,medlemsNavn,pickupAdresse,afleveringsAdresse");
                    writer.WriteLine($"{delivery.pakkeID},{delivery.medlemsNavn},{delivery.pickupAdresse},{delivery.afleveringsAdresse}");
                }
            }
            else
            {
                // Append the Worker information to the CSV file
                using (var writer = new StreamWriter(csvFilePath, true))
                {
                    writer.WriteLine($"{delivery.pakkeID},{delivery.medlemsNavn},{delivery.pickupAdresse},{delivery.afleveringsAdresse}");
                }
            }
        }
        */
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(10000, stoppingToken);
        }
        
    }
}
