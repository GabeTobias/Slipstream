using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class SmartTerrain : MonoBehaviour
{
    public int width, height;
	public int vertexStretch;

	[SerializeField]
	public byte[] heightMap;

	private Vector3[] vertices;

	public void MatchTrack()
	{
		int resX = width / vertexStretch;
		int resZ = height / vertexStretch;

		for (int x0 = 0; x0 < resX; x0++)
		{
			for (int y0 = 0; y0 < resZ; y0++)
			{

			}
		}
	}

	public void GenerateMesh() 
    {
		// You can change that line to provide another MeshFilter
		MeshFilter filter = GetComponent<MeshFilter>();
		Mesh mesh = filter.sharedMesh;
		mesh.Clear();

		int resX = width/vertexStretch; // 2 minimum
		int resZ = height/vertexStretch;

		#region Vertices		
		vertices = new Vector3[resX * resZ];
		for (int z = 0; z < resZ; z++)
		{
			// [ -length / 2, length / 2 ]
			float zPos = ((float)z / (resZ - 1) - .5f) * height;
			for (int x = 0; x < resX; x++)
			{
				// [ -width / 2, width / 2 ]
				float xPos = ((float)x / (resX - 1) - .5f) * width;
				vertices[x + z * resX] = new Vector3(xPos, heightMap[x + z * resX], zPos);
			}
		}
		#endregion

		#region UVs		
		Vector2[] uvs = new Vector2[vertices.Length];
		for (int v = 0; v < resZ; v++)
		{
			for (int u = 0; u < resX; u++)
			{
				uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
			}
		}
		#endregion

		#region Triangles
		int nbFaces = (resX - 1) * (resZ - 1);
		int[] triangles = new int[nbFaces * 6];
		int t = 0;
		for (int face = 0; face < nbFaces; face++)
		{
			// Retrieve lower left corner from face ind
			int i = face % (resX - 1) + (face / (resZ - 1) * resX);

			triangles[t++] = i + resX;
			triangles[t++] = i + 1;
			triangles[t++] = i;

			triangles[t++] = i + resX;
			triangles[t++] = i + resX + 1;
			triangles[t++] = i + 1;
		}
		#endregion

		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.Optimize();

		GetComponent<MeshCollider>().sharedMesh = mesh;
	}
}
