namespace RouteEtaService.Models
{
    /// <summary>
    /// A class representing the response from the Route ETA service.
    /// </summary> 
    public class EtaResponse
    {
        /// <summary>
        /// Unique ID for a trip.
        /// </summary>
        public long TripId { get; set; }

        /// <summary>
        /// Remaining estimated time to the destination in seconds.
        /// </summary>
        public int RemainingTime { get; set; }
        
        /// <summary>
        /// Remaining estimated distance to the destination in meters.
        /// </summary>
        public double RemainingDistance { get; set; }
    }
}