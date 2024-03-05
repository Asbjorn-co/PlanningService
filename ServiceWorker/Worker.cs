using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Model.Delivery;

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
            Delivery? delivery = JsonSerializer.Deserialize<Delivery>(message);        };
        channel.BasicConsume(queue: "delivery",
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();

        static void WriteToCsv(Delivery delivery)
    {
        // Define the CSV file path
        string csvFilePath = "deliveries.csv";

        // Check if the CSV file exists, if not, create it and write header
        if (!File.Exists(csvFilePath))
        {
            using (var writer = new StreamWriter(csvFilePath))
            {
                writer.WriteLine("pakkeID,medlemsNavn,pickupAdresse,afleveringsAdresse");
            }
        }

        // Append the Worker information to the CSV file
        using (var writer = new StreamWriter(csvFilePath, true))
        {
            writer.WriteLine($"{delivery.pakkeID},{delivery.medlemsNavn},{delivery.pickupAdresse},{delivery.afleveringsAdresse}");
        }

        using (var writer = new StreamWriter(csvFilePath))
        {
            writer.WriteLine("medlemsNavn,pickupAdresse,pakkeID,afleveringsAdresse");
            
        }

    }
    while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
}
}
