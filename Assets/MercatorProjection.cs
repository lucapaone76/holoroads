using System;
//source http://wiki.openstreetmap.org/wiki/Mercator
/*
Most tiled web maps (such as the standard OSM maps and Google Maps) use this Mercator projection.

The map area of such maps is a square with x and y coordiates both between -20,037,508.34 and 20,037,508.34 meters. As a result data north of about 85.1° and south of about -85.1° latitude can not be shown and has been cut off.

The correct EPSG code for this data is 3857 and this is what the data files show. Before this code was allocated other codes such as 900913 were used. If your software does not understand the 3857 code you might need to upgrade. See this page for all the details.

This is the right choice for you if you are creating tiled web maps.
*/
namespace AssemblyCSharp
{
	public static class MercatorProjection
	{
		private static readonly double R_MAJOR = 6378137.0;
		private static readonly double R_MINOR = 6356752.3142;
		private static readonly double RATIO = R_MINOR / R_MAJOR;
		private static readonly double ECCENT = Math.Sqrt(1.0 - (RATIO * RATIO));
		private static readonly double COM = 0.5 * ECCENT;

		private static readonly double DEG2RAD = Math.PI / 180.0;
		private static readonly double RAD2Deg = 180.0 / Math.PI;
		private static readonly double PI_2 = Math.PI / 2.0;

		public static double[] toPixel(double lon, double lat)
		{
			return new double[] { lonToX(lon), latToY(lat) };
		}

		public static double[] toGeoCoord(double x, double y)
		{
			return new double[] { xToLon(x), yToLat(y) };
		}

		public static double lonToX(double lon)
		{
			return R_MAJOR * DegToRad(lon);
		}

		public static double latToY(double lat)
		{
			lat = Math.Min(89.5, Math.Max(lat, -89.5));
			double phi = DegToRad(lat);
			double sinphi = Math.Sin(phi);
			double con = ECCENT * sinphi;
			con = Math.Pow(((1.0 - con) / (1.0 + con)), COM);
			double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
			return 0 - R_MAJOR * Math.Log(ts);
		}

		public static double xToLon(double x)
		{
			return RadToDeg(x) / R_MAJOR;
		}

		public static double yToLat(double y)
		{
			double ts = Math.Exp(-y / R_MAJOR);
			double phi = PI_2 - 2 * Math.Atan(ts);
			double dphi = 1.0;
			int i = 0;
			while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
			{
				double con = ECCENT * Math.Sin(phi);
				dphi = PI_2 - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), COM)) - phi;
				phi += dphi;
				i++;
			}
			return RadToDeg(phi);
		}

		private static double RadToDeg(double rad)
		{
			return rad * RAD2Deg;
		}

		private static double DegToRad(double deg)
		{
			return deg * DEG2RAD;
		}
	}}

