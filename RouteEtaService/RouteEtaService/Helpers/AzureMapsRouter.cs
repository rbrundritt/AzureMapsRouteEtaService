using Azure.Core.GeoJson;
using Azure.Maps.Routing;
using RouteEtaService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RouteEtaService.Helpers
{
    /// <summary>
    /// This class is responsible for calculating a route, and returning a filtered version of the route response.
    /// </summary>
    internal static class AzureMapsRouter
    {
        /// <summary>
        /// The Azure Maps client used to calculate routes.
        /// </summary>
        private static MapsRoutingClient client = null;

        /// <summary>
        /// Calculates the route a given start and end point.
        /// </summary>
        /// <param name="tripId">Unique ID for the trip.</param>
        /// <param name="origin">The origin of the route.</param>
        /// <param name="destination">The destination of the route.</param>
        /// <returns></returns>
        public static CachedRouteInfo CalculateRouteAsync(long tripId, double[] origin, double[] destination)
        {
            if(client == null)
            {
                client = new MapsRoutingClient(new Azure.AzureKeyCredential(WebApiApplication.AzureMapsSubscriptionKey));
            }

            CachedRouteInfo routeInfo = null;

            //Create a list of waypoints in the format that the Azure Maps Route API expects.
            //Preallocate the list size for the origin and destination to prevent the list from allocating additional space initially (lists allocate a size of 4 by default).
            var waypoints = new List<GeoPosition>(2)
            {
                //Note that GeoPosition takes in Longitude/Latitude, not Latitude/Longitude.
                new GeoPosition(origin[1], origin[0]),
                new GeoPosition(destination[1], destination[0])
            }; 
                           
            var response = client.GetDirections(new RouteDirectionQuery(waypoints, new RouteDirectionOptions()
            {
                TravelMode = TravelMode.Truck,
                InstructionsType = RouteInstructionsType.Coded,
                RouteType = RouteType.Fastest,
                UseTrafficData = true,
                RouteRepresentationForBestOrder = RouteRepresentationForBestOrder.Polyline
            }));

            if (response != null && response.Value != null && response.Value.Routes != null && response.Value.Routes.Count > 0)
            {
                var route = response.Value.Routes[0];

                // Convert the route line path to a format that is easier to work with.
                var path = route.Legs[0].Points.Select(coordinate => new double[] { coordinate.Longitude, coordinate.Latitude }).ToArray();

                //Get last instruction.
                var lastInstruction = route.Guidance.Instructions.Last();

                // Convert the route instruction steps to a format that is easier to work with.
                var routeSteps = route.Guidance.Instructions.Select(step => new RouteStepInfo(
                    new double[] { step.Point.Latitude, step.Point.Longitude},
                    (int)(lastInstruction.TravelTimeInSeconds - step.TravelTimeInSeconds),
                    (int)(lastInstruction.RouteOffsetInMeters - step.RouteOffsetInMeters))).ToArray();

                routeInfo = new CachedRouteInfo() {
                    TripId = tripId, 
                    Origin = origin, 
                    Destination = destination,
                    Path = path,
                    RouteSteps = routeSteps
                };
            }

            return routeInfo;
        }
    }
}
