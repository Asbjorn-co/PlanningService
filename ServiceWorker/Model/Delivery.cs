using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Model.Delivery
{

    public class Delivery
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)] // Ensure representation as ObjectId
        public string? Id { get; set; }
        public int pakkeID { get; set; }
        public string medlemsNavn { get; set; }
        public string pickupAdresse { get; set; }
        public string afleveringsAdresse { get; set; }

        public Delivery(int pakkeid, string medlemsnavn, string pickupadresse, string afleveringsadresse)
        {
            this.pakkeID = pakkeid;
            this.medlemsNavn = medlemsnavn;
            this.pickupAdresse = pickupadresse;
            this.afleveringsAdresse = afleveringsadresse;
        }
    }

}