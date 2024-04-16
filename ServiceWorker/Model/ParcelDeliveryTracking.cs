using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model.ParcelDeliveryTracking
{
    public class ParcelDeliveryTracking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)] // Ensure representation as ObjectId
    public string Id { get; set; }
    public int ParcelDeliveryID { get; set; }
    public string Status { get; set; }
    public TrackingInformation Tracking { get; set; }

    public class TrackingInformation
    {
        public string TrackingId { get; set; }
        public string Carrier { get; set; }
        public List<StatusUpdate> StatusUpdates { get; set; }

        public class StatusUpdate
        {
            public DateTime Date { get; set; }
            public string Description { get; set; }
        }
    }
}
}