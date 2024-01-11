# AzureMapsRouteEtaService

A simple solution to get the estimated time of arrival (ETA) of a vehicle is to call an online routing service, like what is available in Azure Maps. However, if you want to regularly check the ETA to ensure the vehicle is still on track, and/or have a lot of vehicles, calling an online service constantly can quickly generate a lot of financial costs. A more cost effective solution is to cache the route and use the vehicles position over time to interpolate it's position along that route, and calculate the remaining time. 

This project is a simple sample that shows how to leverage a cached Azure Maps route response to interpolate and calculate the remaining travel time and distance, in a scalable and cost effective way.

This solution does the following tasks:

- Creates trips and calculates routes for origin/destination pairs, and caches them using a unique trip ID (sample expects this to be provided, but you could generate GUID when registering a route and use that later when getting updated ETAs).
- Uses Azure Maps routing service to calculate routes, modifies the response object for optimized caching and for faster and easier route ETA interpolation calculations. 
- Compares vehicle position to cached route paths to ensure vehicle is still "on route" within a specified threshold distance.
- Interpolates the position of a vehicle along a cached route object and calculates the remaining travel time and distance.

Check out this [blog post]() related to this project.

## Main sample files

The following is a list of the main files of interest and what's important in them:

- `Global.asax.cs` - To run the sample, there is a placeholder in this file to add your Azure Maps keys. An in-memory cache object is also located here. See the important notes section below for more information.
- `RouteEtaController.cs` - This is a sample web service for registering and removing trips, and calculating ETA's. This includes the logic for determining if a vehicle is still "on route", by calculating the distance from the current position, to the route path and seeing if it is within a certain threshold distance. If it is not within the threshold distance, a new route is calculated for the trip and the cached route info is updated.
- `CachedRouteInfo.cs` - This is a class that represents a modified/optimized version of the Azure Maps route response. This class stores a subset of the response to minimize the route response size. At the bottom of this file is a static method called `CalculateEta` that interpolates the remaining time/distance for a given coordinate along the cached route.
- `AzureMapsRouter.cs` - A thin wrapper around the Azure Maps route service client that requests routes and converts the responses into the optimized cached route objects. In here are you can modify the route request settings such as the travel mode (car vs truck), and other settings, like if real time traffic should be used, or if a future departure date time should be used (go when precalculating routes during ahead of time, like the night before).
- `SpatialMath.cs` - A small class of spatial math calculations for this solution (no external libraries required). Main calculations; distance between points, distance from point to a route path, bearing/heading, and position value comparison.

## Important notes

The primary purpose of this sample is to show the logic for calculating the remaining time/distance using data cached from the Azure Maps routing service. The overall solution in this project is not suitable for production use. The following changes are recommended for production use:

1. The Azure Maps key is currently hard coded in the `Global.asax.cs` file and fine when running this sample locally. In a production application hosted in Azure, it is strongly recommended that managed authenication (Azure Active Directory/ Microsoft Entra ID) is used. Alternatively, store the key in Azure Key Vault. 
2. This sample currently uses a simple in memory cahce (Dictionary in the `Global.asax.cs` file). This will work fine if you run the a single instance of this service and have it it configured to "Always On", however, to be truely scalable, you may want to run multiple instances of the service, possibly across different Azure regions, and thus should used a shared caching service, such as Azure Redis. 
3. For demo purposes, this sample uses an "off route" distance of 1,000 meters (defined in the `RouteEtaController.cs` file). If a vehicle is more than this distance away from the original route, a new route is calculated. In a production app you would likely want to reduce to a range of 50 to 150 meters.
4. This sample uses a simple double array to store coordinates with the format: `[lat, lon]`. 
5. If you don't need or want to calculate a new route if a vehicle goes "off route" and simply want to continue to use the cached route object, there is no need to retrieve and cache the route path. This would significantly reduce the size of the cached route object and also reduce the size of the response from the Azure Maps routing service (slightly faster responses).

## Additional resources

- [Azure Maps product page](https://azure.com/maps)
- [Azure Maps docs](https://learn.microsoft.com/en-us/azure/azure-maps/)

## License

MIT

