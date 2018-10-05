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

    public GameObject PrefabValue;
    public Node StartNode;
    public Node[] Nodes;
    [SerializeField] private GameObject halo;
    [SerializeField] private Roll m_rollMove;
    [SerializeField] private Roll m_rollOffsive;
    [SerializeField] private Roll m_rollDeffsive;
    
    [SerializeField] private List<PoringProperty> m_propertyStarter;
    [SerializeField] private List<Poring> m_player = new List<Poring>();

    private CurrentPlayer m_currentPlayer;
    private CameraController m_cameraController;
    private int m_step = 0;    

    private List<List<Node>> RouteList = new List<List<Node>>();

    private void Start() 
    {
        Application.targetFrameRate = 120;
        m_cameraController = CameraController.Instance;
        StartGameMode();
    }

    public override void OnRollEnd(int number, DiceType type)
    {
        switch (type)
        {
            case DiceType.Move:
                m_step = number;
                // Debug.LogFormat("Roll move number : {0}", number);
                m_cameraController.Show(CameraType.TopDown);
                ParseMovableNode();
                DisplayNodeHeat();
                StartCoroutine(WaitForSelectNode());
            break;
            case DiceType.Offensive:
                // Debug.LogFormat("Roll offensive number : {0}", number - 1);
                m_currentPlayer.Poring.OffensiveResult += m_currentPlayer.Poring.Property.OffensiveDices[0].GetDiceFace(number-1);
                m_currentPlayer.Poring.OffensiveResultList.Add(number- 1);
            break;
            case DiceType.Deffensive:
                // Debug.LogFormat("Roll deffensive number : {0}", number- 1);
                m_currentPlayer.Poring.DeffensiveResult += m_currentPlayer.Poring.Property.DeffensiveDices[0].GetDiceFace(number-1);
                m_currentPlayer.Poring.DeffensiveResultList.Add(number- 1);
            break;
        }
    }

    private IEnumerator WaitForSelectNode()
    {
        bool isSelected = false;
        MagicCursor.Instance.gameObject.SetActive(false);
        while (!isSelected)
        {
            yield return null;
            if (Input.GetMouseButtonDown(0))
            { 
                RaycastHit hit; 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
                if (Physics.Raycast(ray, out hit, 100.0f)) 
                {
                    // Debug.Log(hit.transform.name);
                    Node node = hit.transform.parent.GetComponent<Node>();
                    if (node) 
                    {
                        // Debug.Log("You selected the " + node.nid);
                        // SFX.PlayClip(resource.sound[0]).GetComponent<AudioSource>().time = 0.3f;
                        node.PointRenderer.SetPropertyBlock(MaterialPreset.GetMaterialPreset(EMaterialPreset.selected));

                        if (node == m_currentPlayer.Poring.Node && CheckPoringInTargetNode(node) > 0 && node.TileProperty.Type != TileType.Sanctuary)
                        {
                            List<Poring> porings = node.porings.FindAll(poring => poring != m_currentPlayer.Poring);
                            m_currentPlayer.Poring.Target = porings[Random.Range(0, porings.Count - 1)];

                            m_cameraController.Show(CameraType.Default);
                            isSelected = true;
                            ResetNodeColor();
                            CurrentGameState = eStateGameMode.Encounter;
                        }
                        else if (node.steps.Count > 0 && (CheckPoringInTargetNode(node) > 0 || node.steps.Find(step => step == m_step) == m_step))
                        {
                            MagicCursor.Instance.MoveTo(node);

                            if (CheckPoringInTargetNode(node) > 0 && node.TileProperty.Type != TileType.Sanctuary)
                            {
                                List<Poring> porings = node.porings.FindAll(poring => poring != m_currentPlayer.Poring);
                                m_currentPlayer.Poring.Target = porings[Random.Range(0, porings.Count - 1)];

                                RouteList.Clear();
                                FindRouteNode(m_step, 0, m_currentPlayer.Poring.Node, m_currentPlayer.Poring.PrevNode);

                                RouteList = FindTargetRoute(RouteList, node);
                            }
                            else if (node.steps.Find(step => step == m_step) == m_step)
                            {
                                m_currentPlayer.Poring.Target = null;

                                RouteList.Clear();
                                RouteToNode(node);
                            }

                            int indexRoute = Random.Range(0, RouteList.Count - 1);
                            // Debug.LogFormat("index >>>>>>>>>> {0}", indexRoute);
                            // Debug.LogFormat("RouteList >>>>>>>>>> {0}", RouteList.Count);

                            // TODO send result route to rendar path with UI
                            // print(GetNodeString(RouteList[indexRoute]));
                            m_currentPlayer.Poring.Behavior.SetupJumpToNodeTarget(RouteList[indexRoute]);

                            m_cameraController.Show(CameraType.Default);
                            isSelected = true;
                            ResetNodeColor();
                        }
                    }
                }
            }
        }
    }

    private int CheckPoringInTargetNode(Node node)
    {
        int count = node.porings.Count;

        if (node.porings.Find(poring => poring == m_currentPlayer.Poring) != null) count -= 1;

        return count;
    }

    private void ResetNodeColor()
    {
        foreach (var item in Nodes)
        {
            item.steps.Clear();
            item.PointRenderer.material.SetColor("_Color", new Color(1, 1, 1, 0.37f));
            item.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
        }
    }

    private string GetNodeString(List<Node> nodeList) 
    {
		if (nodeList == null) return "NULL";
		if (nodeList.Count == 0) return "[]";

		List<string> strings = new List<string>();
		foreach(Node n in nodeList) {
			strings.Add(n.nid.ToString());
		}
		return "["+ string.Join(", ", strings.ToArray()) + "]";
		// return string.Join(",".Join(), intList.Select(x => x.ToString()).ToArray());
	}

    private void RouteToNode(Node target, Node node = null, Node prevNode = null, int step = 0, List<Node> result = null) 
    {
		if (result == null) result = new List<Node>();
		if (prevNode == null) prevNode = m_currentPlayer.Poring.PrevNode;
        bool isFirstNode = false;
        if (node == null)
        {
            isFirstNode = true;
            node = m_currentPlayer.Poring.Node;
        }
        
        if (step > 0)
            result.Add(node);

        step += (isFirstNode) ? 1 : node.TileProperty.WeightStep;

        if (node.nid == target.nid) 
			if (step >= m_step) RouteList.Add(result);
            
		if (step > m_step) return;
		foreach (Neighbor neighbor in node.NeighborList) 
        {
            if (prevNode != null)
			    if (neighbor.Node.nid == prevNode.nid)
                    continue;

			List<Node> result2 = new List<Node>();
			result2.AddRange(result);
			RouteToNode(target, neighbor.Node, node, step, result2);
		}
	}

    private void FindRouteNode(int maxStep, int currentStep, Node currentNode, Node prevNode, List<Node> result = null)
    {
        bool isFirstNode = false;
        if (result == null)
        {
            result = new List<Node>();
            isFirstNode = true;
        }

        if (!isFirstNode)
            result.Add(currentNode);

        if (currentStep >= maxStep)
        {
            RouteList.Add(result);
            return;
        }

        currentStep += (isFirstNode) ? 1 : currentNode.TileProperty.WeightStep;
        
        foreach (Neighbor neighbor in currentNode.NeighborList)
        {
            if (neighbor.eDirection == eDirection.INTO) continue;
            if (prevNode != null && neighbor.Node == prevNode) continue;

            List<Node> result2 = new List<Node>();
            result2.AddRange(result);
            FindRouteNode(maxStep, currentStep, neighbor.Node, currentNode, result2);
        }
    }

    private List<List<Node>> FindTargetRoute(List<List<Node>> route, Node target)
    {
        List<List<Node>> result = new List<List<Node>>();
        for(int i = 0; i < route.Count; i++)
        {
            for(int j = route[i].Count - 1; j >= 0; j--)
            {
                if(route[i][j] == target)
                {
                    result.Add(route[i]);
                    break;
                }
                else
                {
                    route[i].RemoveAt(j);
                }
            }
        }

        return result;
    }

    private void ParseMovableNode(int max=0, int step=0, Node node=null, Node prevNode=null) {
		if (max == 0) max = m_step;
		if (node == null) node = m_currentPlayer.Poring.Node;
		if (prevNode == null) prevNode = (m_currentPlayer.Poring.PrevNode != null) ? m_currentPlayer.Poring.PrevNode : m_currentPlayer.Poring.Node;

		foreach(Neighbor neighbor in node.NeighborList) {
            if (prevNode != null)
                if (prevNode.nid == neighbor.Node.nid) continue;
            if (neighbor.eDirection == eDirection.INTO) continue;
			
			if (step < max) 
            {
                int newStep = Mathf.Min(step + neighbor.Node.TileProperty.WeightStep, max);
				neighbor.Node.steps.Add(newStep);
				ParseMovableNode(max, newStep, neighbor.Node, node);
			} 
		}
	}

    private void DisplayNodeHeat(int max = 0) 
    {
		if (max == 0) max = m_step;
		foreach(Node node in Nodes) 
        {
            if (node == m_currentPlayer.Poring.Node && CheckPoringInTargetNode(node) > 0 && node.TileProperty.Type != TileType.Sanctuary) node.PointRenderer.material.SetColor("_Color", Color.red);
            if (node.steps.Count == 0) continue;
			node.steps.Sort();
            Color color = (CheckPoringInTargetNode(node) > 0 && node.TileProperty.Type != TileType.Sanctuary) ? Color.red : (node.steps[node.steps.Count - 1] == max) ? Color.green : Color.yellow;
            
			node.PointRenderer.material.SetColor("_Color", color);
			node.PointRenderer.material.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
		}
    }

    public override void StartGameMode()
    {
        Turn = 1;
        RespawnValueOnTile(true);
        Spawn();
        HUDController.Instance.Init(m_player, this);
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
        m_currentPlayer.Poring.OffensiveResult = 0;//OffensiveResultList.Clear();
        m_currentPlayer.Poring.DeffensiveResult = 0;//DeffensiveResultList.Clear();
        m_rollMove.SetRoll(6);
        m_rollOffsive.SetRoll(6);
        m_rollDeffsive.SetRoll(6);
        
        // CurrentGameState = eStateGameMode.Encounter;
    }

    private void Encounter()
    {
        // this state has perpose for wait all animation finish after user select path.
        // TODO wait for animation finish.

        if(m_currentPlayer.Poring.Target != null)
        {
            m_currentPlayer.Poring.Behavior.AttackTarget();
        }
        else
        {
            CurrentGameState = eStateGameMode.EndTurn;
        }
    }

    private void EndTurn()
    {
        if (m_currentPlayer.Poring.WinCondition >= 3)
        {
            // TODO endgame
            m_currentPlayer.Poring.Animator.Play("Win");
            m_player.ForEach(poring =>
            {
                if (poring != m_currentPlayer.Poring) poring.Animator.Play("Lose");
            });
        }
        else
        {
            StartCoroutine(CheckEndRoundCondition());
        }
    }

    #endregion

    private IEnumerator CheckEndRoundCondition()
    {
        yield return null;

        if ((m_currentPlayer.Index + 1) >= m_player.Count)
        {
            Turn++;
            m_currentPlayer.Index = 0;
            RespawnValueOnTile(false);
            yield return new WaitForSeconds(1);
        }
        else
        {
            m_currentPlayer.Index += 1;
        }

        
        SetCurrentPlayer(m_player[m_currentPlayer.Index]);


        CurrentGameState = eStateGameMode.StartTurn;
    }

    private void SetCurrentPlayer(Poring poring)
    {
        halo.transform.SetParent(poring.transform);
        halo.transform.localPosition = Vector3.zero;// new Vector3(0, 0.25f, 0);
        HUDController.Instance.UpdateCurrentHUD(m_currentPlayer.Index);
        m_currentPlayer.Poring = poring;
    }

    private void RespawnValueOnTile(bool isStartGame)
    {
        foreach (var node in Nodes)
        {
            if(!isStartGame)
            {
                node.TileProperty.OnEndRound();
            }
        
            for (int i = 0; i < ((isStartGame) ? node.StartValue : 1); i++)
            {
                node.SpawnValue(PrefabValue);    
            }
        }
    }
    
    private void Spawn()
    {
        foreach (var item in m_propertyStarter)
        {
            Poring poring = GameObject.Instantiate(item.Prefab, StartNode.transform.position, Quaternion.identity).GetComponent<Poring>();
            m_player.Add(poring);
            StartNode.AddPoring(poring);
            poring.Init(item);
        }
        SetCurrentPlayer(m_player[0]);
        m_currentPlayer.Index = 0;
    }
}
