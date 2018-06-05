using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// hemisphere construction has been implemented based on code from http://wiki.unity3d.com/index.php/ProceduralPrimitives by Bérenger

public static class Repository
{
	// array of positions corresponding to local coordinate system
	// the vertices are positioned so that the origin (0,0,0) is at their centre
	public static Vector3[] upperHemisphere;
	public static Vector3[] lowerHemisphere;
	public static Vector3[] circle;

	public static int latitudes{get; private set;}
	public static int longitudes{get; private set;}
	static float radius;

	const float PI = Mathf.PI;
    const float TWO_PI = PI * 2f;

	public static void Initialize(int longCount, float thickness)
	{
		latitudes = longCount/3;
		longitudes = longCount;
		radius = thickness * 0.5f;
	}

	// generate upper hemisphere vertices
	public static void GenerateUpperHemisphere()
	{
		upperHemisphere = new Vector3[(longitudes + 1) * latitudes + 1];

        upperHemisphere[0] = Vector3.up * radius;	// top most vertex
        for (int lat = 0; lat < latitudes; lat++)
        {
            float a1 = PI * (float)((lat * 0.5f) + 1) / (latitudes + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= longitudes; lon++)
            {
                float a2 = TWO_PI * (float)(lon == longitudes ? 0 : lon) / longitudes;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                upperHemisphere[lon + lat * (longitudes + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
	}

	// generate lower hemisphere vertices
	public static void GenerateLowerHemisphere()
	{
		lowerHemisphere = new Vector3[(longitudes + 1) * latitudes + 1];

        for (int lat = 0; lat < latitudes; lat++)
        {
            float a1 = PI * (float)((lat * 0.5f) + (latitudes*0.5f) + 1) / (latitudes + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= longitudes; lon++)
            {
                float a2 = TWO_PI * (float)(lon == longitudes ? 0 : lon) / longitudes;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                lowerHemisphere[lon + lat * (longitudes + 1)] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
		lowerHemisphere[lowerHemisphere.Length-1] = -Vector3.up * radius;	// vertex at the bottom of hemisphere
	}

	// generate vertices on a circle
	public static void GenerateCircle()
	{
		circle = new Vector3[longitudes + 1];

		for (int lon = 0; lon <= longitudes; lon++)
        {
            float a2 = TWO_PI * (float)(lon == longitudes ? 0 : lon) / longitudes;
            float sin2 = Mathf.Sin(a2);
            float cos2 = Mathf.Cos(a2);

            circle[lon] = (new Vector3(cos2, 0, sin2) * radius);
        }
	}

	// returns upper hemisphere vertex count
	public static int UpperHemisphereSize()
	{
		return upperHemisphere.Length;
	}

	// returns lower hemisphere vertex count
	public static int LowerHemisphereSize()
	{
		return lowerHemisphere.Length;
	}

	// returns circle's vertex count
	public static int CircleSize()
	{
		return circle.Length;
	}
}
