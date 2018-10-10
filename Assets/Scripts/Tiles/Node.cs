using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

public class Node : MonoBehaviour {

    public TileProperty TileProperty;
	public int StartValue;
	// public int CurrentValue;
	public int MaxValue;
	public List<GameObject> AppleList = new List<GameObject>();
	public List<Neighbor> NeighborList;
	public List<Poring> porings;
	public List<int> steps = new List<int>();
	public GameObject prefebLine;
	public Renderer PointRenderer;
	public Renderer BlockRenderer;
	static public Dictionary<string, GameObject> objectLines = new Dictionary<string, GameObject>();
	static public int nidIndex = 0;
	public int nid = 0;

	void Awake() {
		nid = nidIndex++;
	}

	[SerializeField]private bool m_setMaterial = false;
	[SerializeField]private bool m_setBlockRenderer = false;
	void OnValidate()
	{
		if(m_setMaterial)
		{
			m_setMaterial = !m_setMaterial;
			BlockRenderer.material = TileProperty.Material;
		}

		if(m_setBlockRenderer)
		{
			m_setBlockRenderer = !m_setBlockRenderer;
			foreach (var item in gameObject.GetComponentsInChildren<MeshRenderer>())
			{
				if(item.name == "block")
				{
					BlockRenderer = item;
				}
			}
		}
	}

	static string GetObjectLineName(int nid1, int nid2) {
		int[] nids = { nid1, nid2 };
        Array.Sort(nids);
		return nids[0] + ":" + nids[1];
	}

	void Start() {
		foreach(Neighbor neighbor in NeighborList){

			string objectLineName = GetObjectLineName(this.nid, neighbor.Node.nid);

			if (!objectLines.ContainsKey(objectLineName)) {

				GameObject line = Instantiate(prefebLine);

				line.transform.parent = transform;
				line.transform.rotation = Quaternion.LookRotation(neighbor.Node.transform.position - transform.position);//Quaternion.LookRotation(node.transform.position, Vector3.up);//(,//node.transform.position.x, transform.position.y, node.transform.position.z);
				line.transform.localScale = new Vector3(0.025f, 1f, (Vector3.Distance(transform.position, neighbor.Node.transform.position)-1.5f)/10);
				line.transform.position = Vector3.Lerp(transform.position, neighbor.Node.transform.position, 0.5f);
				line.transform.localPosition = new Vector3(line.transform.localPosition.x, -0.159f, line.transform.localPosition.z);
				line.gameObject.name = objectLineName;
				objectLines.Add(objectLineName, line);

			}
		}
	}

	public void AddPoring(Poring poring) {
		poring.transform.position = transform.position;
		poring.Node = this;
		porings.Add(poring);
		TileProperty.OnEnter(poring);
	}

	public void RemovePoring(Poring poring)
	{
		porings.Remove(poring);
		TileProperty.OnExit(poring);
	}
#if UNITY_EDITOR
	void OnDrawGizmosSelected() {
		foreach(Neighbor neighbor in NeighborList){
			if (neighbor.Node != null) {
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(transform.position, neighbor.Node.transform.position);
				Gizmos.DrawLine(transform.position+new Vector3(0.01f,0,0.01f), neighbor.Node.transform.position+new Vector3(0.01f,0,0.01f));
				Gizmos.DrawLine(transform.position+new Vector3(-0.01f,0,-0.01f), neighbor.Node.transform.position+new Vector3(-0.01f,0,-0.01f));
				//DrawLine(transform.position, node.transform.position, 1);
			}
		}
    }
#endif
	private float m_arrowHeadLength = 1f;
	private float m_arrowHeadAngle = 20.0f;
	private Vector3 m_forward = new Vector3(0,0,1);

#if UNITY_EDITOR
	void OnDrawGizmos() {
		foreach(Neighbor neighbor in NeighborList){
			if (neighbor.Node != null) {
				Color color = new Color(255, 255, 255, .25f);
				Debug.DrawLine(transform.position, neighbor.Node.transform.position, color);


				Vector3 heading = (neighbor.Node.transform.position - transform.position);
				
				var distance = heading.magnitude;
				var direction = (heading / distance) *3f;
				Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+m_arrowHeadAngle,0) * m_forward ;
				Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-m_arrowHeadAngle,0) * m_forward ;
				switch (neighbor.eDirection)
				{
					case eDirection.INTO:
					Debug.DrawRay(neighbor.Node.transform.position - direction, -right * m_arrowHeadLength, Color.blue);
					Debug.DrawRay(neighbor.Node.transform.position - direction, -left * m_arrowHeadLength, Color.blue);
					break;
					case eDirection.OUT:
					Debug.DrawRay(transform.position + direction, right * m_arrowHeadLength, Color.blue);
					Debug.DrawRay(transform.position + direction, left * m_arrowHeadLength, Color.blue);
					break;
				}
			}
		}

		string str = "";
		foreach (int i in steps) {
			str += i + ", ";
		}
		// Handles.color = Color.white;
		GUIStyle style = new GUIStyle();
		style.normal.textColor = Color.red;
		Handles.Label(transform.position, str);
		Handles.Label(transform.position + Vector3.up * 2, $"NID {nid}", style);
	}
#endif
	private Vector3 basePositionApple = new Vector3(0, 0.62f, 0);
	public void SpawnValue(GameObject prefab)
	{
		if(AppleList.Count < MaxValue && porings.Count == 0)
		{
			Vector3 newPosition = basePositionApple;
			newPosition.x = -0.5f + ( 0.5f * AppleList.Count);
			GameObject apple = GameObject.Instantiate(prefab);
			apple.transform.SetParent(transform);
			apple.transform.localPosition = newPosition;
			AppleList.Add(apple);
		}
	}

	public void PoringKeepValueOnTile(Poring poring)
	{
		poring.Property.CurrentPoint += AppleList.Count;
		AppleList.ForEach(apple => DestroyImmediate(apple));
		AppleList.Clear();

		TileProperty.OnFinish(poring);
	}
}

[System.Serializable]
public struct Neighbor
{
	public string NodeName;
	public Node Node;
	public eDirection eDirection;
}

public enum eDirection
{
	BOTH,
	INTO,
	OUT,
}


			// Bezier3D bezier = new Bezier3D();
			// bezier.start = transform.position;
			// bezier.end = node.transform.position;
			// bezier.thickness = 1;
			// bezier.resolution = 8;

			// bezier.handle1 = transform.position;
			// bezier.handle2 = node.transform.position;

			// line.GetComponent<MeshFilter>().mesh = bezier.CreateMesh();
			// // Vector3.zero;

			// float distance = Vector3.Distance(transform.position, node.transform.position);
			// line.transform.position = Vector3.Lerp(transform.position, node.transform.position, 0.5f);
			// Vector2[] verts;
			// Vector2[] normals;
			// float[] us;

			// int[] lines = new int[]{
			// 	0, 1,
			// 	2, 3,
			// 	3, 4,
			// 	4, 5
			// };

			// Mesh mesh = new Mesh();
			// List<Vector3> vertices = new List<Vector3>();
			// List<int> triangles = new List<int>();
			// List<Vector3> normals = new List<Vector3>();
			
			// Vector3 Start = GetPoint(0f);
			// Quaternion rotation = GetRotation(0);
			// Vector3 left = rotation * Vector3.left;
			// Vector3 right = rotation * Vector3.right;
			// Vector3 up = rotation * Vector3.up;
			// vertices.Add(Start + right);
			// vertices.Add(Start + left);
			// normals.Add(up);
			// normals.Add(up);
			// int triIndex = 0;
			
			// int size = 10; // higher number means smoother but also more verts/tris
			// for (int i = 0; i <= size; i++)
			// {
			// 	float t = (float)i / (float)size;
			// 	Vector3 End = GetPoint(t);
			// 	rotation = GetRotation(t);
			
			// 	left = rotation * Vector3.left;
			// 	right = rotation * Vector3.right;
			// 	up = rotation * Vector3.up;
			
			// 	vertices.Add(End + right);
			// 	vertices.Add(End + left);
			// 	normals.Add(up);
			// 	normals.Add(up);
			
			// 	triangles.Add(triIndex);
			// 	triangles.Add(triIndex + 1);
			// 	triangles.Add(triIndex + 2);
			
			// 	triangles.Add(triIndex + 2);
			// 	triangles.Add(triIndex + 1);
			// 	triangles.Add(triIndex + 3);
			
			// 	triIndex += 2;
			
			// 	Start = End;
			// }
			
			// mesh.SetVertices(vertices);
			// mesh.SetNormals(normals);
			// mesh.SetTriangles(triangles, 0);
			// GetComponent<MeshFilter>().mesh = mesh;