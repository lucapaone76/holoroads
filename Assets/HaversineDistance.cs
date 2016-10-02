using System;

namespace AssemblyCSharp
{
	public static class HaversineDistance
	{
		/*
		Damien Dennehy
		About
		Projects

		Search...
		Anonymous 
		Haversine Algorithm in C#

		15th January 2011  Learning  Geocode, GIS

		The Haversine Algorithm is used to calculate the great circle (“as the crow flies”) distance between two points. In Geographic Information Systems it is used to calculate the distance between two co-ordinates on Earth.

		I’ve written an implementation of the algorithm in C# as part of my thesis project. My implementation is inspired by Movable Type’s JavaScript sample.

		It uses a Latitude/Longitude struct as a parameter.
*/
		public struct LatLon
		{
			public double Latitude;
			public double Longtitude;

			public LatLon(double latitude, double longtitude)
			{
				Latitude = latitude;
				Longtitude = longtitude;
			}
		}


		/// <summary>
		/// Radius of the Earth in Kilometers.
		/// </summary>
		private const double EARTH_RADIUS_KM = 6371;

		/// <summary>
		/// Converts an angle to a radian.
		/// </summary>
		/// <param name="input">The angle that is to be converted.</param>
		/// <returns>The angle in radians.</returns>
		private static double ToRad(double input)
		{
			return input * (Math.PI / 180);
		}

		/// <summary>
		/// Calculates the distance between two geo-points in kilometers using the Haversine algorithm.
		/// </summary>
		/// <param name="point1">The first point.</param>
		/// <param name="point2">The second point.</param>
		/// <returns>A double indicating the distance between the points in KM.</returns>
		public static double GetDistanceKM(float lat1,float lon1,float lat2,float lon2 )
		{
			LatLon one = new LatLon (lat1, lon1);
			LatLon two = new LatLon (lat2, lon2);
			return GetDistanceKM (one, two);
		}

		public static double GetDistanceKM(LatLon point1, LatLon point2)
		{
			double dLat = ToRad(point2.Latitude - point1.Latitude);
			double dLon = ToRad(point2.Longtitude - point1.Longtitude);

			double a = Math.Pow(Math.Sin(dLat / 2), 2) +
				Math.Cos(ToRad(point1.Latitude)) * Math.Cos(ToRad(point2.Latitude)) *
				Math.Pow(Math.Sin(dLon / 2), 2);

			double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			double distance = EARTH_RADIUS_KM * c;
			return distance;
		}

/*
		Usage

		double distance = GetDistanceKM(new LatLon(51.8983398377895, -8.47277440130711),
			new LatLon(53.3437004685402, -6.24956980347633));
		3 comments

*/
	}
}

