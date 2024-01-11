namespace RouteEtaService.Models
{
    /// <summary>
    /// A modi
    /// </summary>
    internal class RouteStepInfo
    {
        /// <summary>
        /// A filtered and modified version of the Azure Maps route response instruction that is cached in the Azure Function.
        /// Instead of time and distance relative to the start of the route, it is relative to the remaining time/distance to the destination.
        /// </summary>
        /// <param name="coordinate">Location of the route instruction step: [latitude, longitude]</param>
        /// <param name="remainingTime">Remaing time to destination</param>
        /// <param name="remainingDistance">Remaing distance to destination</param>
        public RouteStepInfo(double[] coordinate, int remainingTime, int remainingDistance)
        {
            Coordinate = coordinate;
            RemainingTime = remainingTime;
        }

        /// <summary>
        /// The coordinate of the route step.
        /// </summary>
        public double[] Coordinate { get; set; }

        /// <summary>
        /// The remaing time to the destination in seconds.
        /// </summary>
        public int RemainingTime { get; set; }

        /// <summary>
        /// The remaining distance to the destination in meters.
        /// </summary>
        public int RemainingDistance { get; set; }
    }
}
