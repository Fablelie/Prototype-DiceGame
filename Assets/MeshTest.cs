using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTest : MonoBehaviour {
	public MeshFilter meshFilter;

	void Start () {
		Vector3[] vertices = new Vector3[]{
			new Vector3(1, 0, 1),
			new Vector3(-1, 0, 1),
			new Vector3(1, 0, -1),
			new Vector3(-1, 0, -1),
		};

		Vector3[] normals = new Vector3[]{
			new Vector3(0, 1, 0),
			new Vector3(0, 1, 0),
			new Vector3(0, 1, 0),
			new Vector3(0, 1, 0),
		};

		Vector2[] uvs = new Vector2[]{
			new Vector2(0, 1),
			new Vector2(0, 0),
			new Vector2(1, 1),
			new Vector2(1, 0),
		};

		int[] triangles = new int[]{
			0, 2, 3,
			3, 1, 0,
		};

		/*MeshFilter mf = GetComponent<MeshFilter>();
		if(mf.sharedMesh = null)
			mf.sharedMesh = new Mesh();
		Mesh mesh = mf.sharedMesh;*/

		meshFilter.mesh.Clear();
		meshFilter.mesh.vertices = vertices;
		meshFilter.mesh.normals = normals;
		meshFilter.mesh.uv = uvs;
		meshFilter.mesh.triangles = triangles;

		//mf.sharedMesh = mesh;
		//GetComponent<MeshFilter>().mesh = mesh;
		//meshFilter.mesh = mesh;
	}

}
