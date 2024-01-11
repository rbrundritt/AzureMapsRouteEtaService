using RouteEtaService.Helpers;
using System.Linq;

namespace RouteEtaService.Models
{
    /// <summary>
    /// A filtered version of the Azure Maps route response that is cached in the Azure Function.
    /// This is smaller than the original response and optimized for calculating the ETA.
    /// </summary>
    internal class CachedRouteInfo
    {
        public CachedRouteInfo()
        {
        }

        #region Properties
        
        /// <summary>
        /// The route origin point.
        /// </summary>
        public double[] Origin { get; set; }

        /// <summary>
        /// RRoute destination point.
        /// </summary>
        public double[] Destination { get; set; }

        /// <summary>
        /// Unique identifier for the vehicle route trip.
        /// This ID should be passed into all requests to the service and used to retrieved cached routes.
        /// </summary>
        public long TripId { get; set; }

        /// <summary>
        /// The route line path coordinates in the format: [[latitude, longitude], [latitude, longitude], ...]
        /// </summary>
        public double[][] Path { get; set; }

        /// <summary>
        /// Route instruction steps, limited to the remaining time, distance, and step coordinate.
        /// </summary>
        public RouteStepInfo[] RouteSteps { get; set; }

        #endregion

        #region Static Methods

        /// <summary>
        /// Calculates the ETA for the current coordinate along the route.
        /// </summary>
        /// <param name="currentCoordinate">The current GPS coordinate</param>
        /// <param name="cachedRouteInfo">Cached route information.</param>
        /// <returns></returns>
        public static EtaResponse CalculateEta(double[] currentCoordinate, CachedRouteInfo cachedRouteInfo)
        {
            var routeSteps = cachedRouteInfo.RouteSteps;

            //Loop through the route instructions to find the closest step and its index to the current coordinate.
            var closestPointIndex = 0;
            var closestPointDistance = double.MaxValue;

            for (var i = 0; i < routeSteps.Length; i++)
            {
                var step = routeSteps[i];
                var distance = SpatialMath.HaversineDistance(currentCoordinate, step.Coordinate);

                if (distance < closestPointDistance)
                {
                    closestPointDistance = distance;
                    closestPointIndex = i;
                }
            }

            //Determine the instruction before and after the closet point.
            var previousStep = routeSteps[closestPointIndex];
            RouteStepInfo nextStep;

            if (closestPointIndex == 0)
            {
                nextStep = routeSteps.Last();
            }
            else if (closestPointIndex == routeSteps.Length - 1)
            {
                //If at the last step, swap the previous and next instruction.
                var temp = previousStep;
                previousStep = routeSteps[closestPointIndex - 1];
                nextStep = temp;
            }
            else
            {
                var prevStep = routeSteps[closestPointIndex - 1];
                var closestStep = routeSteps[closestPointIndex];
                nextStep = routeSteps[closestPointIndex + 1];

                //Calculate the distance from the current coordinate to the path between previous and closest instruction and next and closest instruction.
                var pcDistance = SpatialMath.DistanceToSegment(currentCoordinate, prevStep.Coordinate, closestStep.Coordinate);
                var cnDistance = SpatialMath.DistanceToSegment(currentCoordinate, nextStep.Coordinate, closestStep.Coordinate); 

                //Determine which pair of steps the current coordinate falls between.
                if (pcDistance < cnDistance)
                {
                    previousStep = prevStep;
                    nextStep = closestStep;
                }
                else
                {
                    previousStep = closestStep;
                }
            }

            //Get the distance between the instructions.
            var distanceBetweenSteps = SpatialMath.HaversineDistance(previousStep.Coordinate, nextStep.Coordinate);

            //Get distance between the closest point and the next instruction.
            var distanceToNextStep = SpatialMath.HaversineDistance(currentCoordinate, nextStep.Coordinate); 

            //Calculate the percentage of the distance between the steps, that represents the distance to the next step.
            var percentage = distanceToNextStep / distanceBetweenSteps;

            //Calculate the remaining distance/time based on the percentage.
            var remainingDistance = nextStep.RemainingDistance + ((nextStep.RemainingDistance - previousStep.RemainingDistance) * percentage);
            var remainingTime = nextStep.RemainingTime + ((nextStep.RemainingTime - previousStep.RemainingTime) * percentage);

            return new EtaResponse()
            {
                TripId = cachedRouteInfo.TripId,
                RemainingTime = (int)remainingTime,
                RemainingDistance = remainingDistance
            };
        }

        #endregion
    }
}
