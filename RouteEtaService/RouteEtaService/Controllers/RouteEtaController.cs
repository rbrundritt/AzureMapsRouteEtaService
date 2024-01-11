using RouteEtaService.Helpers;
using RouteEtaService.Models;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RouteEtaService.Controller
{
    /// <summary>
    /// Controller for calculating the ETA for a route.
    /// Return JSON
    /// </summary>
    public class RouteEtaController : ApiController
    {
        /// <summary>
        /// The maximum distance in meters that a vehicle can deviate from the route before a new route is calculated.
        /// Using 1000 meters for this sample. This value should be adjusted based on the accuracy of the GPS device. 
        /// Recommend setting this to 20 - 50 meters for a real world application.
        /// </summary>
        private const int RouteDeviationThreshold = 1000;

        /// <summary>
        /// Creates a trip and calculates the ETA for the route.
        /// </summary>
        [HttpGet]
        [Route("api/RouteEta/createTrip/{tripId}/{originLat}/{originLon}/{destinationLat}/{destinationLon}")]
        public EtaResponse CreateTrip(long tripId, double originLat, double originLon, double destinationLat, double destinationLon)
        {
            var origin = new double[] { originLat, originLon };
            var destination = new double[] { destinationLat, destinationLon };

            RouteStepInfo firstRouteStep;

            if (WebApiApplication.CachedRoutes.ContainsKey(tripId))
            {
                //A trip already exists with this ID. 
                var cachedRoute = WebApiApplication.CachedRoutes[tripId];

                //Check to see if the origin and destination are the same. And that is has route steps.
                if (cachedRoute.RouteSteps != null && cachedRoute.RouteSteps.Length > 0 &&
                    SpatialMath.AreCoordinatesEqual(origin, cachedRoute.Origin) &&
                    SpatialMath.AreCoordinatesEqual(destination, cachedRoute.Destination))
                {
                    //The trip is the same. Return cached ETA.
                    firstRouteStep = cachedRoute.RouteSteps[0];
                    return new EtaResponse()
                    {
                        TripId = tripId,
                        RemainingTime = firstRouteStep.RemainingTime,
                        RemainingDistance = firstRouteStep.RemainingDistance
                    };
                }
                else
                {
                    //The trip is different. Remove the existing trip.
                    WebApiApplication.CachedRoutes.Remove(tripId);
                }
            } 

            return GetNewRoute(tripId, origin, destination);
        }

        /// <summary>
        /// Calculates the ETA for a route.
        /// </summary>
        [HttpGet]
        [Route("api/RouteEta/getEta/{tripId}/{currentLat}/{currentLon}")]
        public EtaResponse GetEta(long tripId, double currentLat, double currentLon)
        {
            if(WebApiApplication.CachedRoutes.ContainsKey(tripId))
            {
                var cachedRoute = WebApiApplication.CachedRoutes[tripId];
                var currentCoordinate = new double[] { currentLat, currentLon };

                //Check to see if the vehicle has deviated from the route.
                if(SpatialMath.DistanceToPath(currentCoordinate, cachedRoute.Path) > RouteDeviationThreshold)
                {
                    //The vehicle has deviated from the route. Add logic to handle this scenario.
                    //If you decide this check isn't needed, you can remove the Path property from the CachedRouteInfo class and make the cached data size smaller.

                    //For this sample, we will recalulate the route.
                    return GetNewRoute(tripId, currentCoordinate, cachedRoute.Destination);
                }

                //Calculate the ETA.
                return CachedRouteInfo.CalculateEta(currentCoordinate, cachedRoute);
            }
            else
            {
                //No trip with this ID exists.
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("No trip with this ID exists."),
                    ReasonPhrase = "Trip Not Found"
                });                    
            }
        }
        
        /// <summary>
        /// Removes a trip from the cache.
        /// </summary>
        /// <param name="tripId">Unique ID for a trip.</param>
        /// <returns>Returns true if trip was found and removed.</returns>
        [HttpGet]
        [Route("api/RouteEta/removeTrip/{tripId}")]
        public bool RemoveTrip(long tripId)
        {
            if(WebApiApplication.CachedRoutes.ContainsKey(tripId))
            {
                WebApiApplication.CachedRoutes.Remove(tripId);

                return true;
            }

            return false;
        }

        #region Private Methods

        /// <summary>
        /// Calculates a new route and returns the ETA.
        /// </summary>
        /// <param name="tripId">The unique ID for the trip.</param>
        /// <param name="origin">Origin of route.</param>
        /// <param name="destination">Destination of route.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        private EtaResponse GetNewRoute(long tripId, double[] origin, double[] destination)
        {
            //Calculate the route.
            var route = AzureMapsRouter.CalculateRouteAsync(tripId, origin, destination);

            if (route == null)
            {
                //Unable to calculate route.
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Unable to calculate route."),
                    ReasonPhrase = "No Route Found"
                });
            }

            //Remove any cached route with this ID.
            if (WebApiApplication.CachedRoutes.ContainsKey(tripId))
            {
                WebApiApplication.CachedRoutes.Remove(tripId);
            }

            //Add the route to the cache.
            WebApiApplication.CachedRoutes.Add(tripId, route);

            //Return the ETA.
            var firstRouteStep = route.RouteSteps[0];
            return new EtaResponse()
            {
                TripId = tripId,
                RemainingTime = firstRouteStep.RemainingTime,
                RemainingDistance = firstRouteStep.RemainingDistance
            };
        }

        #endregion
    }
}
