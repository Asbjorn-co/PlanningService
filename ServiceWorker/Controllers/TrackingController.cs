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
using Model.Delivery;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
public class TrackingController : ControllerBase
{
    private readonly ILogger<TrackingController> _logger;

    private readonly IMongoCollection<Delivery> _deliveryCollection;

    private readonly string _connectionString = "mongodb://localhost:27018/";
    private readonly string _databaseName = "ParcelDelivery";

    public TrackingController(IMongoClient mongoClient, ILogger<TrackingController> logger)
    {
        _logger = logger;
        // Initialize MongoClient with connection string
            var client = new MongoClient(_connectionString);
            var database = client.GetDatabase(_databaseName);

            _deliveryCollection = database.GetCollection<Delivery>("Deliveries");
    }

    [HttpGet]
    public ActionResult<string> GetMessage()
    {
        return Ok("It works!");
    }

    [HttpGet]
    [Route("{name}")]
    public ActionResult<IEnumerable<Delivery>> GetAllDeliveriesName(string name)
    {
        var filter = Builders<Delivery>.Filter.Eq(delivery => delivery.medlemsNavn, name);
        var deliveries = _deliveryCollection.Find(filter).ToList();
        return Ok(deliveries);
    }

    [HttpGet("{date}")]
public ActionResult<IEnumerable<ParcelDeliveryTracking>> GetAllDeliveriesDate(DateTime date)
{
    // Define match filter to find documents with a StatusUpdate matching the provided date
    var matchFilter = Builders<ParcelDeliveryTracking>.Filter.ElemMatch(
        tracking => tracking.Tracking.StatusUpdates,
        statusUpdate => statusUpdate.Date == date
    );

    // Define projection to include only the fields needed in the result
    var projection = Builders<ParcelDeliveryTracking>.Projection.Include(tracking => tracking.Id)
        .Include(tracking => tracking.ParcelDeliveryID)
        .Include(tracking => tracking.Status)
        .Include(tracking => tracking.Tracking)
        .Slice(tracking => tracking.Tracking.StatusUpdates, 1); // Limit StatusUpdates array to 1 element

    // Execute the aggregation pipeline to find deliveries matching the provided date
    var deliveries = _deliveryCollection.Aggregate()
        .Match(matchFilter)
        .Project<ParcelDeliveryTracking>(projection)
        .ToList();

    // Return the list of deliveries as a JSON response
    return Ok(deliveries);
}
}


