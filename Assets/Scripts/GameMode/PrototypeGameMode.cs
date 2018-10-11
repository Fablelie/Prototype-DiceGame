﻿using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using ExitGames.Client.Photon;

public class PoringPrototype
{
    public const string PLAYER_LIVES = "PlayerLives";
    public const string PLAYER_READY = "IsPlayerReady";
    public const string PLAYER_LOADED_LEVEL = "PlayerLoadedLevel";
}

public enum EventCode
{
    SelectNode = 0,
    RollEnd = 1,
}

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

public class PrototypeGameMode : MonoBehaviourPunCallbacks
{
    public static PrototypeGameMode Instance;
    public eStateGameMode CurrentGameState = eStateGameMode.StartTurn;
    public eStateGameMode PrevGameState = eStateGameMode.None;
    public GameObject PrefabValue;
    public Node StartNode;
    public Node[] Nodes;
    public int Turn;
    [SerializeField] private GameObject halo;
    
    [SerializeField] private List<PoringProperty> m_propertyStarter;
    [SerializeField] private List<Poring> m_player = new List<Poring>();

    private CurrentPlayer m_currentPlayer;
    public int IndexCurrentPlayer;
    private CameraController m_cameraController;
    private int m_step = 0;    

    private List<List<Node>> RouteList = new List<List<Node>>();

    void Awake()
    {
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();

        CountdownTimer.OnCountdownTimerHasExpired += StartGameMode;
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        CountdownTimer.OnCountdownTimerHasExpired -= StartGameMode;
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    private void Start() 
    {
        Application.targetFrameRate = 120;
        m_cameraController = CameraController.Instance;

        Hashtable props = new Hashtable
        {
            {PoringPrototype.PLAYER_LOADED_LEVEL, true}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void OnEvent(EventData photonEvent)
    {
        object[] data = (object[])photonEvent.CustomData;
        switch (photonEvent.Code)
        {
            case (byte)EventCode.SelectNode:
                foreach (var node in Nodes)
                {
                    if ((int)data[0] == node.nid)
                    {
                        ReceiveNodeSelected(node);        
                        break;
                    }
                }
            break;
            case (byte)EventCode.RollEnd:
                Poring poring = m_player[(int)data[2]];
                OnRollEnd((int)data[0], (DiceType)((int)data[1]), poring);
            break;
        }
    }

    private void ReceiveNodeSelected(Node node)
    {
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
                isSelectedNode = true;
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
                isSelectedNode = true;
                ResetNodeColor();
            }
        }
    }

    public void OnRollEnd(int index, DiceType type, Poring poring)
    {
        switch (type)
        {
            case DiceType.Move:
                m_step = m_currentPlayer.Poring.Property.MoveDices[0].GetDiceFace(index);
                // Debug.LogFormat("Roll move number : {0}", number);
                m_cameraController.Show(CameraType.TopDown);
                ParseMovableNode();
                DisplayNodeHeat();
                isSelectedNode = false;
                if (PlayerNumberingExtensions.GetPlayerNumber(PhotonNetwork.LocalPlayer) == m_currentPlayer.Index)
                    StartCoroutine(WaitForSelectNode());
            break;
            case DiceType.Offensive:
                // Debug.LogFormat("Roll offensive number : {0}", number - 1);
                poring.OffensiveResult += poring.Property.OffensiveDices[0].GetDiceFace(index);
                poring.OffensiveResultList.Add(index);
            break;
            case DiceType.Deffensive:
                // Debug.LogFormat("Roll deffensive number : {0}", number- 1);
                poring.DeffensiveResult += poring.Property.DeffensiveDices[0].GetDiceFace(index);
                poring.DeffensiveResultList.Add(index);
            break;
        }
    }

    private bool isSelectedNode = false;
    private IEnumerator WaitForSelectNode()
    {
        MagicCursor.Instance.gameObject.SetActive(false);
        while (!isSelectedNode)
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
                    if(node != null)
                    {
                        object[] content = new object[] { node.nid };
                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{ Receivers = ReceiverGroup.All, };
                        SendOptions sendOptions = new SendOptions{ Reliability = true};

                        PhotonNetwork.RaiseEvent((byte)EventCode.SelectNode, content, raiseEventOptions, sendOptions);
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

    public void StartGameMode()
    {
        Turn = 1;
        RespawnValueOnTile(true);
        Spawn();
        HUDController.Instance.Init(m_player, this);
        m_cameraController.Show(CameraType.Default);
        m_cameraController.SetTarget(m_currentPlayer.Poring);

        StartCoroutine(UpdateState());
    }

    public IEnumerator UpdateState()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (PrevGameState != CurrentGameState)
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
        m_currentPlayer.Poring.MoveRoll.SetRoll(m_currentPlayer.Poring.Property.MoveDices[0].FaceDiceList, m_currentPlayer.Index);
        
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
            m_currentPlayer.Index = IndexCurrentPlayer = 0;
            RespawnValueOnTile(false);
            yield return new WaitForSeconds(1);
        }
        else
        {
            m_currentPlayer.Index += 1;
            IndexCurrentPlayer = m_currentPlayer.Index;
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
        foreach (var player in PhotonNetwork.PlayerList)
        {
            Poring poring = GameObject.Instantiate(m_propertyStarter[0].Prefab, StartNode.transform.position, Quaternion.identity).GetComponent<Poring>();
            m_player.Add(poring);
            StartNode.AddPoring(poring);
            poring.Init(m_propertyStarter[0]);
        }
        // foreach (var item in m_propertyStarter)
        // {
        //     Poring poring = GameObject.Instantiate(item.Prefab, StartNode.transform.position, Quaternion.identity).GetComponent<Poring>();
        //     m_player.Add(poring);
        //     StartNode.AddPoring(poring);
        //     poring.Init(item);
        // }
        SetCurrentPlayer(m_player[0]);
        m_currentPlayer.Index = IndexCurrentPlayer = 0;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("DemoAsteroids-LobbyScene");
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PoringPrototype.PLAYER_LIVES))
        {
            // CheckEndOfGame();
            return;
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (changedProps.ContainsKey(PoringPrototype.PLAYER_LOADED_LEVEL))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                Hashtable props = new Hashtable
                {
                    {CountdownTimer.CountdownStartTime, (float) PhotonNetwork.Time}
                };
                PhotonNetwork.CurrentRoom.SetCustomProperties(props);
            }
        }
    }

    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (p.CustomProperties.TryGetValue(PoringPrototype.PLAYER_LOADED_LEVEL, out playerLoadedLevel))
            {
                if ((bool) playerLoadedLevel)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }

    public int GetPoringIndexByPoring(Poring poring)
    {
        for (int i = 0; i < m_player.Count; i++)
            if (m_player[i] == poring) return i;

        return -1;
    }
}
