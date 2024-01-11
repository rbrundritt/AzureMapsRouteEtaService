using System;

namespace RouteEtaService.Helpers
{
    internal static class SpatialMath
    {
        /// <summary>
        /// Earth radius in meters.
        /// </summary>
        private const double EarthRadius = 6378137;

        /// <summary>
        /// Calculates the distance between two coordinates in meters using the Haversine formula.
        /// </summary>
        /// <param name="start">First coordinate [lat, lon]</param>
        /// <param name="end">Second coordinate [lat, lon]</param>
        /// <returns>Distance between coordinates in meters</returns>
        public static double HaversineDistance(double[] start, double[] end)
        {
            var latitude1 = start[0];
            var longitude1 = start[1];
            var latitude2 = end[0];
            var longitude2 = end[1];

            var latitudeRadians1 = latitude1 * (Math.PI / 180);
            var latitudeRadians2 = latitude2 * (Math.PI / 180);
            var longitudeRadians1 = longitude1 * (Math.PI / 180);
            var longitudeRadians2 = longitude2 * (Math.PI / 180);

            var deltaLatitude = latitudeRadians2 - latitudeRadians1;
            var deltaLongitude = longitudeRadians2 - longitudeRadians1;

            var a = Math.Pow(Math.Sin(deltaLatitude / 2), 2) + Math.Cos(latitudeRadians1) * Math.Cos(latitudeRadians2) * Math.Pow(Math.Sin(deltaLongitude / 2), 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return EarthRadius * c;
        }

        /// <summary>
        /// Checks if two coordinates are equal to 6 decimal places (within <10cm).
        /// </summary>
        /// <param name="coordinate1">First coordinate</param>
        /// <param name="coordinate2">Second Coordinate</param>
        /// <returns></returns>
        public static bool AreCoordinatesEqual(double[] coordinate1, double[] coordinate2)
        {
            return Math.Round(Math.Abs(coordinate1[0] - coordinate2[0]), 6) == 0 &&
                Math.Round(Math.Abs(coordinate1[1] - coordinate2[1]), 6) == 0;
        }   

        /// <summary>
        /// Calculates the shortest distance from a coordinate to a path.
        /// </summary>
        /// <param name="coordinate">The coordinate</param>
        /// <param name="path">The path</param>
        /// <returns>Distance in meters.</returns>
        public static int DistanceToPath(double[] coordinate, double[][] path)
        {
            var closestDistance = double.MaxValue;
            for (var i = 0; i < path.Length - 1; i++)
            {
                var distance = DistanceToSegment(coordinate, path[i], path[i + 1]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
            }
            return (int)closestDistance;
        }

        /// <summary>
        /// Calculates the shortest distance from a coordinate to a segment.
        /// </summary>
        /// <param name="coordinate">Coordunate</param>
        /// <param name="segmentStart">Start of line segment</param>
        /// <param name="segmentEnd">End of line segment</param>
        /// <returns></returns>
        public static double DistanceToSegment(double[] coordinate, double[] segmentStart, double[] segmentEnd)
        {
            var segmentDistance = HaversineDistance(segmentStart, segmentEnd);
            var startToCoordinateDistance = HaversineDistance(segmentStart, coordinate);
            var endToCoordinateDistance = HaversineDistance(segmentEnd, coordinate);

            // If the coordinate is beyond the start or end of the segment, use the distance to the start or end of the segment.
            if (startToCoordinateDistance > segmentDistance)
            {
                return endToCoordinateDistance;
            }
            else if (endToCoordinateDistance > segmentDistance)
            {
                return startToCoordinateDistance;
            }

            // Calculate the distance to the segment using the formula for the distance from a point to a line.
            // https://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
            var segmentBearing = Bearing(segmentStart, segmentEnd);
            var startToCoordinateBearing = Bearing(segmentStart, coordinate);
            var angle = Math.Abs(segmentBearing - startToCoordinateBearing);
            var distanceToSegment = Math.Sin(angle) * startToCoordinateDistance;

            return distanceToSegment;
        }

        /// <summary>
        /// Calculates the bearing between two coordinates.
        /// </summary>        
        /// <param name="start">First coordinate [lat, lon]</param>
        /// <param name="end">Second coordinate [lat, lon]</param>
        /// <returns>Bearing in degrees</returns>
        /// <remarks>
        /// The bearing is the angle between the line from the start to the end and true north.
        /// </remarks>
        public static double Bearing(double[] start, double[] end)
        {
            var latitude1 = start[0];
            var longitude1 = start[1];
            var latitude2 = end[0];
            var longitude2 = end[1];

            var latitudeRadians1 = latitude1 * (Math.PI / 180);
            var latitudeRadians2 = latitude2 * (Math.PI / 180);
            var longitudeRadians1 = longitude1 * (Math.PI / 180);
            var longitudeRadians2 = longitude2 * (Math.PI / 180);

            var deltaLongitude = longitudeRadians2 - longitudeRadians1;

            var y = Math.Sin(deltaLongitude) * Math.Cos(latitudeRadians2);
            var x = Math.Cos(latitudeRadians1) * Math.Sin(latitudeRadians2) - Math.Sin(latitudeRadians1) * Math.Cos(latitudeRadians2) * Math.Cos(deltaLongitude);
            var bearingRadians = Math.Atan2(y, x);

            return (bearingRadians * (180 / Math.PI) + 360) % 360;
        }
    }
}
