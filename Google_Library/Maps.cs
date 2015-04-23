namespace Google_Library
{
    using System;
    using GoogleMapsApi;
    using GoogleMapsApi.Entities.Directions.Request;
    using GoogleMapsApi.Entities.Directions.Response;
    
    public static class Maps
    {
        public static TimeSpan EstimatedTime(string origin, string destination)
        {
            DirectionsRequest directionRequest = new DirectionsRequest()
            {
                Origin = origin,
                Destination = destination
            };

            DirectionsResponse directionResponse = GoogleMaps.Directions.Query(directionRequest);

            foreach (Route r in directionResponse.Routes)
            {
                foreach (Leg l in r.Legs)
                {
                    return l.Duration.Value;
                }
            }

            return new TimeSpan();
        }

        public static TimeSpan EstimatedTime(string origin, string destination, int roundUpMinutes)
        {
            TimeSpan directTime = EstimatedTime(origin, destination);

            int mins = (int)(Math.Ceiling((double)(directTime.Minutes / (float)roundUpMinutes)) * roundUpMinutes);

            return directTime.Add(new TimeSpan(0, mins, 0));
        }
    }
}