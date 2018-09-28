using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum DokaponGameState {
	focus,
	roll,
	plan,
	beforeMove,
	move,
	afterMove,
}

public class DokaponGameMode : GameMode {
	public DokaponGameState state;
	public float TimeForRoll = 10;

	[Header("Materials Preset")]
	public MaterialPreset[] MaterialPresets;
	
	[Header("Poring")]
	public GameObject PoringPrefab;
	public Poring currentPoring;
	public List<Poring> porings = new List<Poring>();

	[Header("Camera")]
	public GameObject currentCamera;
	public GameObject[] cameras;
	
	[Header("Object Reference")]
	public GameObject panelStepRemain;
	public Roll panelRoll;
	public PanelPoringUI panelPoringUI;
	public List<PoringUI> listPoringUI = new List<PoringUI>();
	public PoringUI poringUI;
	
	static DokaponGameMode Dokapon;
	public Node[] nodes;
	private int stepWalking = 0;
	private Resource resource;
	private SoundObject bgm;

	private List<List<Node>> Route = new List<List<Node>>();
	private List<Node> NodeStack;

	private float m_timeForRoll = 10;
	// private List<Node> _resultPointer;
	public override void StartGameMode() {
		Dokapon = this;

		this.Name = "Dokapon/Bank";
		this.Turn = 0;

		cameras = GameObject.FindGameObjectsWithTag("MainCamera");
		GameObject[] NodeObjects = GameObject.FindGameObjectsWithTag("Node");
		nodes = new Node[NodeObjects.Length];
		for(int i = 0; i < NodeObjects.Length; i++){
			nodes[i] = NodeObjects[i].GetComponent<Node>();
		}

		for(int i = 0; i < MaxPlayer; i++){
			Poring poring = Instantiate(PoringPrefab).GetComponent<Poring>();
			porings.Add(poring);//print(Random.Range(0, nodes.Length));print(porings.Count);
			// porings[0].transform.position = nodes[0].transform.position;
			Node node = nodes[UnityEngine.Random.Range(0, nodes.Length)];
			node.AddPoring(poring);

			panelPoringUI.Add("Poring " + (i+1), 0, 0);
		}

		currentCamera = cameras[0];
		currentPoring = porings[0];


		currentCamera.GetComponent<CameraController>().SetTarget(currentPoring);

		// Initiailize sounds 
		resource = GetComponent<Resource>();
		bgm = SFX.PlayClip(resource.bgm[0], 1, true);
		bgm.GetComponent<AudioSource>().time = 3;
		
		print("Dokapon Start !!");
	}

	public void SetCamera(CameraType type) {
		foreach (GameObject camera in cameras) {
			camera.GetComponent<CameraController>().Show(type);
		}
	}

	public override void UpdateGameMode() {
		switch (state) {
			case DokaponGameState.focus:
			if (currentPoring == null) return;
			float distance = Vector3.Distance(currentPoring.transform.position, currentCamera.transform.position);//print(distance);
				if (distance < 11.2f) {
					state = DokaponGameState.roll;
					m_timeForRoll = TimeForRoll;

					panelRoll.SetRoll(6);
				}
			break;

			case DokaponGameState.roll:
				m_timeForRoll -= Time.deltaTime;

				if (m_timeForRoll < 0) {
					Roll();
				}
			break;

			case DokaponGameState.plan:
				if (Input.GetMouseButtonDown(0)){ 
					RaycastHit hit; 
					Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
					if (Physics.Raycast(ray, out hit, 100.0f)) {
						Node node = hit.transform.parent.GetComponent<Node>();
						if (node) {
							Debug.Log("You selected the " + node.nid);
							SFX.PlayClip(resource.sound[0]).GetComponent<AudioSource>().time = 0.3f;
							node.PointRenderer.SetPropertyBlock(MaterialPreset.GetMaterialPreset(EMaterialPreset.selected));

							if (node.steps.Count > 0) {
								MagicCursor.Instance.MoveTo(node);
								string s = "";
								Route.Clear();
								// PathToNode(node);
								// foreach (Node n in PathToNode(node)) {
								// 	s += n.nid + ", ";
								// }
								RouteToNode(node);
								// foreach (List)
								//print(GetNodeString(PathToNode(node)));
								// print(s);
								foreach (List<Node> r in Route) {
									print(GetNodeString(r));
								}
								
							}
							
						}
					}
				}
			break;
		}
	}

	public void Roll() {
		panelRoll.SetRoll(6);
	}

	public override void OnRollEnd(int number) {
		stepWalking = number;

		state = DokaponGameState.plan;
		SetCamera(CameraType.TopDown);

		panelStepRemain.SetActive(true);
		ParseMovableNode();
		DisplayNodeHeat();

		bgm.ChangeVolume(0.5f);
	}

	public void ParseMovableNode(int max=0, int step=0, Node node=null, Node pNode=null) {
		if (max == 0) max = stepWalking;
		if (node == null) node = currentPoring.node;
		if (pNode == null) pNode = node;

		foreach(Neighbor neighbor in node.NeighborList) {
			if (pNode.nid == neighbor.Node.nid) continue;
			/*if (step == max) {
				//n.PointRenderer.SetPropertyBlock(MaterialPreset.GetMaterialPreset(EMaterialPreset.movable));
				//return true;
			} else {
				ParseMovableNode(max, step+1, n, node);
			}*/
			if (step < max) {
				neighbor.Node.steps.Add(step+1);
				ParseMovableNode(max, step+1, neighbor.Node, node);
			} else {
				//n.steps.Add(step);
			}
		}

		//return true;
	}

	void AvaliableNodeHeat(int max=0) {
		if (max == 0) max = stepWalking;
		foreach(Node n in nodes) {print(n);
			//Color color = new Color(0,0,0,1);
			if(n.steps.Count == 0) continue;
			n.steps.Sort();
         	float step = n.steps [n.steps.Count - 1];
			float val = Mathf.Floor(step/max); //print(val);
			n.PointRenderer.material.SetColor("_Color", Color.Lerp(Color.white, Color.green, val));
			n.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
		}
	}

	void DisplayNodeHeat(int max=0) {
		if (max == 0) max = stepWalking;
		foreach(Node n in nodes) {print(n);
			//Color color = new Color(0,0,0,1);
			if(n.steps.Count == 0) continue;
			n.steps.Sort();
         	float step = n.steps [n.steps.Count - 1];
			float val = Mathf.Floor(step/max); //print(val);
			n.PointRenderer.material.SetColor("_Color", Color.Lerp(Color.yellow, Color.green, val));
			n.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
		}
	}

	string GetNodeString(List<Node> nodeList) {
		if (nodeList == null) return "NULL";
		if (nodeList.Count == 0) return "[]";

		List<string> strings = new List<string>();
		foreach(Node n in nodeList) {
			strings.Add(n.nid.ToString());
		}
		return "["+ string.Join(", ", strings.ToArray()) + "]";
		// return string.Join(",".Join(), intList.Select(x => x.ToString()).ToArray());
	}

	string GetIndent(int indent, int count=8) {
		string result = "";
		//if (indent > 0) result = new String(' ', count-1) + "|";
		for (int i=0; i<indent; i++) {
			result += new String(' ', (count*2)-1) + "|";
		}
		if (indent > 0) result += "------->";
		return result;
	}

	void RouteToNode(Node target, Node node=null, Node pNode=null, int step=0, List<Node> result=null) {
		if (step > stepWalking) return;

		if (result == null) result = new List<Node>();
		if (pNode == null) pNode = currentPoring.node;
		if (node == null) node = pNode;

		result.Add(node);
		if (node.nid == target.nid) {
			if (step == stepWalking) Route.Add(result);
		}
		
		step++;
		foreach (Neighbor neighbor in node.NeighborList) {
			if (neighbor.Node.nid == pNode.nid) continue;
			//if (n.dir == pNode.dir) continue;

			List<Node> result2 = new List<Node>();
			result2.AddRange(result);
			RouteToNode(target, neighbor.Node, node, step, result2);
		}
	}

	// List<List<Node>> FindPath(Node target, Node node=null, Node pNode=null, int step=0, List<List<Node>> result=null) {
	// 	if (result == null) result = new List<List<Node>>();
	// 	if (pNode == null) pNode = currentPoring.node;
	// 	if (node == null) node = pNode;

	// 	if (step > stepWalking) return new List<Node>();

	// 	if (node.nid == target.nid) {
	// 		result.Add(node);
	// 		Debug.LogError(String.Format(GetIndent(step)+"[FOUND!!] {0} == {1}", node.nid, target.nid, GetNodeString(result)));
	// 		return result;
	// 	}
	// }

	

	List<Node> PathToNode(Node target, Node node=null, Node pNode=null, int step=0, List<Node> result=null) {
		//print( String.Format("{0} is {1} and thinks pi = {2:F2}", "Dave", 42, 3.14159) );

		
		
		// Init for first loop.
		if (result == null) result = new List<Node>();
		if (pNode == null) pNode = currentPoring.node;
		if (node == null) node = pNode;
			
		Debug.Log(String.Format(GetIndent(step)+"[START {1}] {0}/{2} ", step, node.nid, stepWalking));

		// Reject is no more step to walk.
		if (step > stepWalking) return new List<Node>(); 
		// if (step == 0) Route.Clear();

		// Force return if reach target by it self.
		if (node.nid == target.nid) {
			result.Add(node);
			Debug.LogError(String.Format(GetIndent(step)+"[FOUND!!] {0} == {1}", node.nid, target.nid, GetNodeString(result)));
			return result;
		}

		// Foreach to find another connected node.
		step++;
		bool noEntry = true;
		List<List<Node>> nodeList = new List<List<Node>>();
		foreach (Neighbor neighbor in node.NeighborList) {
			// Prevent backward node. 
			if (neighbor.Node.nid == pNode.nid) continue;

			List<Node> subResult = PathToNode(target, neighbor.Node, node, step, result);
			Debug.LogError(String.Format(GetIndent(step)+"[END {0}] {1}", neighbor.Node.nid, GetNodeString(subResult)));
			if (subResult.Count == 0) continue;
			/*{
				if (result.Count > 0)	result.RemoveAt(result.Count-1);
				continue;
			}*/
			//result = subResult;
			//result.Insert(0, node);
			subResult.Insert(0, node);
			noEntry = false;
			Debug.LogWarning(String.Format(GetIndent(step)+"[ForwardCollect] {0} <- {1} == {2}", neighbor.Node.nid, node.nid, GetNodeString(subResult)));
			
			
			// return result;
			nodeList.Add(subResult);
		}
		if (noEntry) return new List<Node>();
		
		/*foreach (List<Node> ln in nodeList) {
			if (ln.Count == stepWalking) print(GetNodeString(ln));
		}*/

		if (nodeList.Count == 1) { 
			return nodeList[0];
		} else { 
			// Debug.Log(GetIndent(step)+"[FR] ---: " + nodeList.Count + " / " + nodes.Length);
			int maxCount = nodes.Length;
			List<Node> shortestNode = null;
			foreach (List<Node> ln in nodeList) {
				// print(GetNodeString(ln));
				if (ln.Count < maxCount) {
					maxCount = ln.Count;
					//shortestNode = new List<Node>();
					//shortestNode.AddRange(ln);
					shortestNode = ln;
				}
			}
			Debug.LogWarning(String.Format(GetIndent(step)+"[ForwardResult] {0} == {1}", node.nid, GetNodeString(shortestNode)));
			if (shortestNode != null) {
				//Route.Add(nodeList);
				return shortestNode;
			} else {
				return new List<Node>();
			}
		}
	// 	List<List<Node>> resultChunk = new List<List<Node>>();
	// 	List<int> resultChunkStep = new List<int>();
	// 	foreach (Node n in node.nodes) {
	// 		if (n.nid == pNode.nid) continue;
	// 		if (n.nid == target.nid) { 
	// 			return result;
	// 		} else {
	// 			if (step < stepWalking) {
	// 				List<Node> subResult = PathToNode(target, n, node, ++step, result);
	// 				resultChunk.Add(subResult);
	// 				resultChunkStep.Add(subResult.Count);
	// 			}
	// 		}
	// 	}

	// 	if (resultChunk.Count == 0) return result;

	// 	int minimum = nodes.Length;
	// 	int indexOfResultChunk = 0;
	// 	int i = 0;
	// 	foreach (int r in resultChunkStep)
	// 	{
	// 		if (i < minimum) {
	// 			minimum = r;
	// 			indexOfResultChunk = i++;
	// 		} 
	// 	}
	// 	return resultChunk[indexOfResultChunk];
		
		return result;
	}

	// List<Node> PathToNode(Node target, Node node=null, Node pNode=null, int step=1, List<Node> result=null) {
	// 	// Reject is no more step to walk.
	// 	if (step == stepWalking) return null; 
		
	// 	// Init for first loop.
	// 	if (result == null) result = new List<Node>();
	// 	if (pNode == null) pNode = currentPoring.node;
	// 	if (node == null) node = pNode;

	// 	// Force return if reach target by it self.
	// 	result.Add(node);
	// 	if (node.nid == target.nid) {
	// 		return result;
	// 	}

	// 	// Foreach to find another connected node.
	// 	bool noEntry = true;
	// 	foreach (Node n in node.nodes) {
	// 		// Prevent backward node. 
	// 		if (n.nid == pNode.nid) continue;

	// 		noEntry = false;
	// 		List<Node> subResult = PathToNode(target, n, node, ++step, result);
	// 		if (subResult != null) result = subResult;
	// 	}
	// 	if (noEntry) return null;
		
	// 	return result;
	// }

	// List<Node> PathToNode(Node target, Node node=null, Node pNode=null, int step=0, List<Node> result=null) {
	// 	if (result == null) result = new List<Node>();
	// 	if (pNode == null) pNode = currentPoring.node;
	// 	if (node == null) node = pNode;
		
	// 	result.Add(node);

	// 	List<List<Node>> resultChunk = new List<List<Node>>();
	// 	List<int> resultChunkStep = new List<int>();
	// 	foreach (Node n in node.nodes) {
	// 		if (n.nid == pNode.nid) continue;
	// 		if (n.nid == target.nid) { 
	// 			return result;
	// 		} else {
	// 			if (step < stepWalking) {
	// 				List<Node> subResult = PathToNode(target, n, node, ++step, result);
	// 				resultChunk.Add(subResult);
	// 				resultChunkStep.Add(subResult.Count);
	// 			}
	// 		}
	// 	}

	// 	if (resultChunk.Count == 0) return result;

	// 	int minimum = nodes.Length;
	// 	int indexOfResultChunk = 0;
	// 	int i = 0;
	// 	foreach (int r in resultChunkStep)
	// 	{
	// 		if (i < minimum) {
	// 			minimum = r;
	// 			indexOfResultChunk = i++;
	// 		} 
	// 	}
	// 	return resultChunk[indexOfResultChunk];
	// }

	// List<Node> PathToNode(Node node, Node mNode=null, Node pNode=null, int step=0, List<Node> result = null) {
	// 	if (pNode == null) pNode = currentPoring.node;
	// 	if (result == null)	result = new List<Node>();

	// 	if (mNode == null) {
	// 		foreach(Node n in pNode.nodes) {
	// 			result.AddRange(PathToNode(node, n, pNode, ++step));
	// 		}
	// 	} else {
	// 		foreach(Node n in mNode.nodes) {
	// 			if (pNode.nid == n.nid) continue;
	// 			if (n.nid == node.nid){
	// 				result.Add(n);
	// 				return result;
	// 			} else {
	// 				if (step < stepWalking) {
	// 					result.AddRange(PathToNode(node, n, mNode, ++step));
	// 				}
	// 			}
	// 		}
	// 	}
		
	// 	return result;
	// }

	// List<Node> PathToNode(Node node, Node pNode=null, int step=0, List<Node> result=null) {
	// 	if (pNode == null) pNode = currentPoring.node;
	// 	if (result == null)	{
	// 		result = new List<Node>();
	// 		result.Add(pNode);
	// 	}

	// 	List<Node> localResult = new List<Node>();
	// 	foreach(Node n in node.nodes) {
	// 		if (pNode.nid == n.nid) continue;
	// 		if (n.nid == node.nid){
	// 			result.Add(n);
	// 			return result;
	// 		} else {
	// 			if (step < stepWalking) {
	// 				result.Add(n);
	// 				return PathToNode(node, n, ++step, result);
	// 			}
	// 		}
	// 	}
	// 	return result;
	// }
	
}
