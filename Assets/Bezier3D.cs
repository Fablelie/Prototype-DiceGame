using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bezier3D : MonoBehaviour
{
	public Vector3 start = new Vector3(0, 0, 0);
	public Vector3 end = new Vector3(1, 1, 0);
	public Vector3 handle1 = new Vector3(0, 1, 0);
	public Vector3 handle2 = new Vector3(1, 0, 0);
	public int resolution = 12;
	public float thickness = 0.25f;

	public void Start ()
	{
		GetComponent<MeshFilter>().mesh = CreateMesh();
	}

	//cacluates point coordinates on a quadratic curve
	public static Vector3 PointOnPath(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u, uu, uuu, tt, ttt;
		Vector3 p;

		u = 1 - t;
		uu = u * u;
		uuu = uu * u;

		tt = t * t;
		ttt = tt * t;

		p = uuu * p0;
		p += 3 * uu * t * p1;
		p += 3 * u * tt * p2;
		p += ttt * p3;

		return p;
	}

	public Mesh CreateMesh()
	{
		Mesh mesh;

		mesh = new Mesh();

		float scaling = 1;
		float width = 0.1f;//thickness / 2f;
		List<Vector3> vertList = new List<Vector3>();
		List<int> triList = new List<int>();
		List<Vector2> uvList = new List<Vector2>();
		Vector3 upNormal = new Vector3(0, 0, -1);

		triList.AddRange(new int[] {                     
			2, 1, 0,    //start face
			0, 3, 2
		});

		for (int s = 0; s < resolution; s++)
		{
			float t = ((float)s) / resolution;
			float futureT = ((float)s + 1) / resolution;

			Vector3 segmentStart = PointOnPath(t, start, handle1, handle2, end);
			Vector3 segmentEnd = PointOnPath(futureT, start, handle1, handle2, end);

			Vector3 segmentDirection = segmentEnd - segmentStart;
			if (s == 0 || s == resolution - 1)
				segmentDirection = new Vector3(0, 1, 0);
			segmentDirection.Normalize();
			Vector3 segmentRight = Vector3.Cross(upNormal, segmentDirection);
			segmentRight *= width;
			Vector3 offset = segmentRight.normalized * (width / 2) * scaling;
			Vector3 br = segmentRight + upNormal * width + offset;
			Vector3 tr = segmentRight + upNormal * -width + offset;
			Vector3 bl = -segmentRight + upNormal * width + offset;
			Vector3 tl = -segmentRight + upNormal * -width + offset;

			int curTriIdx = vertList.Count;

			Vector3[] segmentVerts = new Vector3[] 
			{
				segmentStart + br,
				segmentStart + bl,
				segmentStart + tl,
				segmentStart + tr,
			};
			vertList.AddRange(segmentVerts);

			Vector2[] uvs = new Vector2[]
			{
				new Vector2(0, 0), 
				new Vector2(0, 1), 
				new Vector2(1, 1),
				new Vector2(1, 1)
			};
			uvList.AddRange(uvs);

			int[] segmentTriangles = new int[]
			{
				curTriIdx + 6, curTriIdx + 5, curTriIdx + 1, //left face
				curTriIdx + 1, curTriIdx + 2, curTriIdx + 6,
				curTriIdx + 7, curTriIdx + 3, curTriIdx + 0, //right face
				curTriIdx + 0, curTriIdx + 4, curTriIdx + 7,
				curTriIdx + 1, curTriIdx + 5, curTriIdx + 4, //top face
				curTriIdx + 4, curTriIdx + 0, curTriIdx + 1,
				curTriIdx + 3, curTriIdx + 7, curTriIdx + 6, //bottom face
				curTriIdx + 6, curTriIdx + 2, curTriIdx + 3
			};
			triList.AddRange(segmentTriangles);

			// final segment fenceposting: finish segment and add end face
			if (s == resolution - 1)
			{
				curTriIdx = vertList.Count;

				vertList.AddRange(new Vector3[] {
					segmentEnd + br,
					segmentEnd + bl,
					segmentEnd + tl,
					segmentEnd + tr
				});

				uvList.AddRange(new Vector2[] { 
						new Vector2(0, 0), 
						new Vector2(0, 1), 
						new Vector2(1, 1),
						new Vector2(1, 1)
					}
				);
				triList.AddRange(new int[] {
					curTriIdx + 0, curTriIdx + 1, curTriIdx + 2, //end face
					curTriIdx + 2, curTriIdx + 3, curTriIdx + 0
				});
			}
		}

		mesh.vertices = vertList.ToArray();
		mesh.triangles = triList.ToArray();
		mesh.uv = uvList.ToArray();
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		//mesh.Optimize();

		return mesh;
	}

	// private void SetMesh() {
	

	// if (curveOK) {
	// 	int vertsInShape = shape.vertices.Length;
	// 	int edgeLoops = curve.Length;
	// 	int vertCount = vertsInShape * edgeLoops;
	// 	int triCount = shape.lines.Length * segments;
	
	// 	int[] triIndices = new int[triCount * 3];
	// 	Vector3[] vertices = new Vector3[vertCount];
	// 	Vector3[] normals = new Vector3[vertCount];
	// 	Vector2[] uvs = new Vector2[vertCount];
	
	// 	for (int i = 0; i < curve.Length; i++) {
	// 		int offset = i * vertsInShape;
	// 		for (int n = 0; n < vertsInShape; n++) {
	// 			int id = offset + n;
	// 			vertices[id] = curve[i].LocalToWorld(shape.vertices[n]);
	// 			normals[id] = curve[i].LocalToWorldDirection(shape.normals[n]);
	// 			uvs[id] = new Vector2(shape.us[n], i / (float)edgeLoops);
	// 		}
	// 	}
	// 	int ti = 0;
	// 	for (int i = 0; i < segments; i++) {
	// 		int offset = i * vertsInShape;
	// 		for (int n = 0; n < shape.lines.Length; n += 2) {
	// 			int a = offset + shape.lines[n] + vertsInShape;
	// 			int b = offset + shape.lines[n];
	// 			int c = offset + shape.lines[n + 1];
	// 			int d = offset + shape.lines[n + 1] + vertsInShape;
	// 			triIndices[ti] = c; ti++;
	// 			triIndices[ti] = a; ti++;
	// 			triIndices[ti] = b; ti++;
	// 			triIndices[ti] = a; ti++;
	// 			triIndices[ti] = c; ti++;
	// 			triIndices[ti] = d; ti++;
	// 		}
	// 	}
	
	// 	Mesh mesh = new Mesh();
	// 	mesh.name = "CurvedMesh";
	// 	mesh.vertices = vertices;
	// 	mesh.triangles = triIndices;
	// 	mesh.normals = normals;
	// 	mesh.uv = uvs;
	// 	mesh.RecalculateNormals();
	// 	filter.sharedMesh.Clear();
	// 	filter.sharedMesh = mesh;
	// }
}