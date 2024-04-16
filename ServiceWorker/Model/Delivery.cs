
namespace Model.Delivery
{

    public class Delivery
    {
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