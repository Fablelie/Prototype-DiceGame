using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eStateGameMode
{
    None,
    StartTurn,
    ActiveTurn,
    Encounter,
    EndTurn,
}

public struct CurrentPlayer
{
    public Poring Poring;
    public int Index;
}

public class PrototypeGameMode : GameMode
{
    public eStateGameMode CurrentGameState = eStateGameMode.StartTurn;
    public eStateGameMode PrevGameState = eStateGameMode.None;
    public Node StartNode;
    public Node[] Nodes;
    [SerializeField] private Roll m_roll;
    [SerializeField] private List<PoringProperty> m_propertyStarter;
    [SerializeField] private List<Poring> m_player = new List<Poring>();

    private CurrentPlayer m_currentPlayer;
    private CameraController m_cameraController;
    private int m_step = 0;    

    private List<List<Node>> RouteList = new List<List<Node>>();

    private void Start() 
    {
        m_cameraController = CameraController.Instance;
        StartGameMode();
    }

    public override void OnRollEnd(int number)
    {
        m_step = number;

        m_cameraController.Show(CameraType.TopDown);
		ParseMovableNode();
        DisplayNodeHeat();
        StartCoroutine(WaitForSelectNode());
    }

    private IEnumerator WaitForSelectNode()
    {
        bool isSelected = false;
        while (!isSelected)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            { 
                RaycastHit hit; 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
                if (Physics.Raycast(ray, out hit, 100.0f)) 
                {
                    Debug.Log(hit.transform.name);
                    Node node = hit.transform.parent.GetComponent<Node>();
                    if (node) 
                    {
                        Debug.Log("You selected the " + node.nid);
                        // SFX.PlayClip(resource.sound[0]).GetComponent<AudioSource>().time = 0.3f;
                        node.PointRenderer.SetPropertyBlock(MaterialPreset.GetMaterialPreset(EMaterialPreset.selected));

                        if (node.steps.Count > 0) 
                        {
                            MagicCursor.Instance.MoveTo(node);
                            
                            RouteList.Clear();
                            RouteToNode(node);

                            // TODO send result route to rendar path with UI
                            m_currentPlayer.Poring.Behavior.SetupJumpToNodeTarget(RouteList[Random.Range(0, RouteList.Count - 1)]);
                            isSelected = true;
                            m_cameraController.Show(CameraType.Default);
                            foreach (var item in Nodes)
                            {
                                item.steps.Clear();
                                item.PointRenderer.material.SetColor("_Color", Color.white);
                                item.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
                            } 
                        }
                    }
                }
            }
        }
    }

    private void RouteToNode(Node target, Node node = null, Node prevNode = null, int step = 0, List<Node> result = null) {
		if (step > m_step) return;

		if (result == null) result = new List<Node>();
		if (prevNode == null) prevNode = m_currentPlayer.Poring.Node;
		if (node == null) node = prevNode;

		result.Add(node);
		if (node.nid == target.nid) 
		{
			if (step == m_step) RouteList.Add(result);
		}
		
		step++;
		foreach (Neighbor neighbor in node.NeighborList) {
			if (neighbor.Node.nid == prevNode.nid) continue;
			//if (n.dir == pNode.dir) continue;

			List<Node> result2 = new List<Node>();
			result2.AddRange(result);
			RouteToNode(target, neighbor.Node, node, step, result2);
		}
	}

    private void ParseMovableNode(int max=0, int step=0, Node node=null, Node pNode=null) {
		if (max == 0) max = m_step;
		if (node == null) node = m_currentPlayer.Poring.Node;
		if (pNode == null) pNode = node;

		foreach(Neighbor neighbor in node.NeighborList) {
			if (pNode.nid == neighbor.Node.nid || neighbor.eDirection == eDirection.INTO) continue;
			
			if (step < max) 
            {
				neighbor.Node.steps.Add(step+1);
				ParseMovableNode(max, step+1, neighbor.Node, node);
			} 
		}
	}

    private void DisplayNodeHeat(int max = 0) 
    {
		if (max == 0) max = m_step;
		foreach(Node node in Nodes) 
        {
			if(node.steps.Count == 0) continue;
			node.steps.Sort();
         	float step = node.steps [node.steps.Count - 1];
			float val = Mathf.Floor(step / max);
			node.PointRenderer.material.SetColor("_Color", Color.Lerp(Color.yellow, Color.green, val));
			node.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
		}
    }

    public override void StartGameMode()
    {
        Turn = 0;
        Spawn();
        m_cameraController.Show(CameraType.Default);
        m_cameraController.SetTarget(m_currentPlayer.Poring);

    }

    public override void UpdateGameMode()
    {
        if(PrevGameState != CurrentGameState)
        {
            PrevGameState = CurrentGameState;
            switch (CurrentGameState)
            {
                case eStateGameMode.StartTurn:
                    StartTurn();
                break;
                case eStateGameMode.ActiveTurn:
                    ActiveTurn();
                break;
                case eStateGameMode.Encounter:
                    Encounter();
                break;
                case eStateGameMode.EndTurn:
                    EndTurn();
                break;
            }
        }
    }

    #region State process 

    private void StartTurn()
    {
        if(m_currentPlayer.Poring == null) return;
        
        m_cameraController.SetTarget(m_currentPlayer.Poring);

        //TODO enable UI Roll/another action.


        CurrentGameState = eStateGameMode.ActiveTurn;
    }

    private void ActiveTurn()
    {
        // this state active when StartTurn enable UI for this.
        // TODO wait for animation roll end and user select path.
        m_roll.SetRoll(6);

        // CurrentGameState = eStateGameMode.Encounter;
    }

    private void Encounter()
    {
        // this state has perpose for wait all animation finish after user select path.
        // TODO wait for animation finish.

        CurrentGameState = eStateGameMode.EndTurn;
    }

    private void EndTurn()
    {
        m_currentPlayer.Index = (m_currentPlayer.Index + 1 >= m_player.Count) ? 0 : m_currentPlayer.Index + 1;
        m_currentPlayer.Poring = m_player[m_currentPlayer.Index];

        CurrentGameState = eStateGameMode.StartTurn;
    }

    #endregion



    private void Spawn()
    {
        foreach (var item in m_propertyStarter)
        {
            Poring poring = GameObject.Instantiate(item.Prefab, StartNode.transform.position, Quaternion.identity).GetComponent<Poring>();
            m_player.Add(poring);
            poring.Node = StartNode;
        }
        m_currentPlayer.Poring = m_player[0];
        m_currentPlayer.Index = 0;
    }
}
