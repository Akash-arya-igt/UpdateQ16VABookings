using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Webjet.BusinessEntities
{
    public class FlightInfo
    {
        /// <summary>
        /// Not populated in GetFlightDetails- not important
        /// </summary>
        public string SegNum { get; set; }

        public string CarrierCode { get; set; }
        public string FlightNumber { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string Source { get; set; }//"StartAirp"
        public string Destination { get; set; }//"EndAirp"
        public string Class { get; set; }
        public string Status { get; set; }
        public bool FlightFlownInd { get; set; }
        public string PreviousStatus { get; set; }
        public DateTime LastSegArrival { get; set; }

        //Added by Harish R-33 on 25-Aug-2015
        public DateTime TicketingDate { get; set; }
        public string PaxType { get; set; }
        public string FareBasis { get; set; }
        public string BookigClass { get; set; }
        public string MultipleTravelClass { get; set; }

        public DateTime LocalAirportTime { get; set; }
        //Added by harish R-42 to add Plating carrier
        public string PlatingCarrier { get; set; }
        public int Index { get; set; }
        public string PlatingIATACode { get; set; }
        public string CouponNum { get; set; }
    }
}
