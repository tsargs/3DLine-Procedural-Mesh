using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// hemisphere construction and vertex indexing has been implemented based on code from http://wiki.unity3d.com/index.php/ProceduralPrimitives by Bérenger

public class LineMesh : MonoBehaviour 
{
    // components that are local to the gameobject this script is attached to
    MeshFilter filter;
    Mesh mesh;

    // arrays
    Vector3[] vertices;
    Vector3[] normals;
    int[] triangles;

    // next available index for the triangles array
    int index = 0;

    // next available index for the vertices array
    int nextVertexIndex = 0;

    // next latitude that could be created
    int nextLatitude = 0;

    // constant values
    const float PI = Mathf.PI;
    const float TWO_PI = PI * 2f;

    public void InitializeMesh()
    {
        filter = gameObject.GetComponent<MeshFilter>();

        if (!filter)
            filter = gameObject.AddComponent<MeshFilter>();

        mesh = filter.mesh;
        mesh.Clear();
	}

    // adds the opening hemisphere for the line
	public void AddUpperHemiSphere(ref Matrix4x4 trsMatrix, ref Vector3 position)
    {
        int longitudes = Repository.longitudes;
        int latitudes = Repository.latitudes;
        int vertexCount = Repository.UpperHemisphereSize();
        int newVertexCount = nextVertexIndex + vertexCount;
        int newIndexCount = newVertexCount * 2 * 3;

        // resize the arrays and copy existing mesh data from repository
        System.Array.Resize<Vector3>(ref vertices, newVertexCount);
        System.Array.Resize<Vector3>(ref normals, newVertexCount);
        System.Array.Resize<int>(ref triangles, newIndexCount);
        System.Array.Copy(Repository.upperHemisphere, 0, vertices, nextVertexIndex, vertexCount);

        // top cap - set triangle indices
        for (int lon = 0; lon < longitudes; lon++)
        {
            triangles[index++] = lon + 2;
            triangles[index++] = lon + 1;
            triangles[index++] = 0;
        }

        // rest of the hemisphere - set triangle indices
        for (int lat = 0; lat < latitudes - 1; lat++)
        {
            for (int lon = 0; lon < longitudes; lon++)
            {
                int current = lon + lat * (longitudes + 1) + 1;
                int next = current + longitudes + 1;

                triangles[index++] = current;
                triangles[index++] = current + 1;
                triangles[index++] = next + 1;

                triangles[index++] = current;
                triangles[index++] = next + 1;
                triangles[index++] = next;
            }
        }

        // transform the vertices from model coordinate frame to world coordinate frame
        // also compute the normals
        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = trsMatrix.MultiplyPoint3x4(vertices[i]);           
            normals[i] = (vertices[i] - position).normalized;
        }

        // assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;

        // save values to be used by the next mesh to be drawn
        nextVertexIndex = newVertexCount;
        nextLatitude = latitudes;
    }

    // adds the closing hemisphere for the line
    public void AddLowerHemiSphere(ref Matrix4x4 trsMatrix, ref Vector3 position)
    {
        int longitudes = Repository.longitudes;
        int latitudes = Repository.latitudes;
        int vertexCount = Repository.LowerHemisphereSize();
        int newVertexCount = nextVertexIndex + vertexCount;
        int newIndexCount = newVertexCount * 2 * 3;

        // resize the arrays and copy existing mesh data from repository
        System.Array.Resize<Vector3>(ref vertices, newVertexCount);
        System.Array.Resize<Vector3>(ref normals, newVertexCount);
        System.Array.Resize<int>(ref triangles, newIndexCount);
        System.Array.Copy(Repository.lowerHemisphere, 0, vertices, nextVertexIndex, vertexCount);

        // part of the hemisphere except the bottom cap - set triangle indices
        for (int lat = nextLatitude - 1; lat < (nextLatitude + latitudes - 1); lat++)
        {
            for (int lon = 0; lon < longitudes; lon++)
            {
                int current = lon + lat * (longitudes + 1) + 1;
                int next = current + longitudes + 1;

                triangles[index++] = current;
                triangles[index++] = current + 1;
                triangles[index++] = next + 1;

                triangles[index++] = current;
                triangles[index++] = next + 1;
                triangles[index++] = next;
            }
        }

        // bottom cap - set triangle indices
        for( int lon = 0; lon < longitudes; lon++ )
        {
	        triangles[index++] = vertices.Length - 1;
	        triangles[index++] = vertices.Length - (lon+2) - 1;
	        triangles[index++] = vertices.Length - (lon+1) - 1;
        }

        // transform the vertices from model coordinate frame to world coordinate frame
        // also compute the normals
        for(int i = nextVertexIndex; i < vertices.Length; i++)
        {
            vertices[i] = trsMatrix.MultiplyPoint3x4(vertices[i]);           
            normals[i] = (vertices[i] - position).normalized;
        }

        // assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
    }

    public void AddOpenCylinder(ref Matrix4x4 trsMatrix, ref Vector3 position, ref Vector3 lineDirection, ref float distance)
    {
        int longitudes = Repository.longitudes;
        int vertexCount = Repository.CircleSize();
        int newVertexCount = nextVertexIndex + vertexCount;
        int newIndexCount = newVertexCount * 2 * 3;

        // resize the arrays and copy existing mesh data from repository
        System.Array.Resize<Vector3>(ref vertices, newVertexCount);
        System.Array.Resize<Vector3>(ref normals, newVertexCount);
        System.Array.Resize<int>(ref triangles, newIndexCount);
        System.Array.Copy(Repository.circle, 0, vertices, nextVertexIndex, vertexCount);

        // transform the vertices from model coordinate frame to world coordinate frame
        // also compute the normals
        for (int lon = 0; lon <= longitudes; lon++)
        {
            int vIndex = lon + nextLatitude * (longitudes + 1) + 1;
            vertices[vIndex] = trsMatrix.MultiplyPoint3x4(vertices[vIndex]);
            normals[vIndex] = (vertices[vIndex] - position).normalized;
        }

        int lat = nextLatitude - 1;

        // set triangle indices connecting the circle to previous latitude
        // this forms an open cylinder structure attaching it to the exisiting mesh
        for (int lon = 0; lon < longitudes; lon++)
        {
            int current = lon + lat * (longitudes + 1) + 1;
            int next = current + longitudes + 1;

            triangles[index++] = current;
            triangles[index++] = current + 1;
            triangles[index++] = next + 1;

            triangles[index++] = current;
            triangles[index++] = next + 1;
            triangles[index++] = next;
        }

        // assign arrays to the mesh
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;

        // save values to be used by the next mesh to be drawn
        nextVertexIndex = newVertexCount;
        ++nextLatitude;
    }
}
