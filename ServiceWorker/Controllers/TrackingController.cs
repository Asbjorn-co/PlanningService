using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceWorker;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Model.Delivery;
using Model.ParcelDeliveryTracking;
using System.Text.RegularExpressions;


namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly ILogger<TrackingController> _logger;

    private readonly IMongoCollection<Delivery> _deliveryCollection;
    private readonly IMongoCollection<ParcelDeliveryTracking> _trackingCollection;

    private readonly string _connectionString = "mongodb://localhost:27018/";
    private readonly string _databaseName = "ParcelDelivery";

    public TrackingController(IMongoClient mongoClient, ILogger<TrackingController> logger)
    {
        _logger = logger;
        // Initialize MongoClient with connection string
        var client = new MongoClient(_connectionString);
        var database = client.GetDatabase(_databaseName);

        _deliveryCollection = database.GetCollection<Delivery>("Deliveries");
        _trackingCollection = database.GetCollection<ParcelDeliveryTracking>("ParcelDeliveryTracking");
    }

    [HttpGet]
    public ActionResult<string> GetMessage()
    {
        return Ok("It works!");
    }

    [HttpGet]
    [Route("name/{name}")]
    public ActionResult<IEnumerable<Delivery>> GetAllDeliveriesName(string name)
    {
        var filter = Builders<Delivery>.Filter.Eq(delivery => delivery.medlemsNavn, name);
        var deliveries = _deliveryCollection.Find(filter).ToList();
        return Ok(deliveries);
    }

    [HttpGet]
    [Route("date/{date}")]
    public ActionResult<IEnumerable<ParcelDeliveryTracking>> GetSpecificDate(string date)
    {
        // Convert the date string from the route parameter to a DateTime object
        if (!DateTime.TryParse(date, out var targetDate))
        {
            return BadRequest("Invalid date format. Please provide the date in a valid format.");
        }

        // Fetch all documents from the collection
        var allTrackingObjects = _trackingCollection.Find(new BsonDocument()).ToList();

        // Filter the documents based on the target date
        var filteredTrackingObjects = allTrackingObjects.Where(tracking =>
            tracking.Tracking.StatusUpdates.Any(statusUpdate =>
                statusUpdate.Date.Date == targetDate.Date
            )
        ).ToList();

        // Return the list of filtered deliveries as a JSON response
        return Ok(filteredTrackingObjects);
    }
}













