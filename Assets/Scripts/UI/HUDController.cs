using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class HUDController : InstanceObject<HUDController> 
{
	[SerializeField] private Text Turn; 
	private int currentTurn;
	public List<HUDCell> Cells;
	private PrototypeGameMode m_gameMode;

	public void Init(List<Poring> porings, PrototypeGameMode gameMode)
	{
		m_gameMode = gameMode;
		Cells.ForEach(cell => cell.gameObject.SetActive(false));

		for (int i = 0; i < porings.Count; i++)
		{
			var cell = Cells[i];

			cell.Init(porings[i]);
			cell.gameObject.SetActive(true);
		}
		
		// m_gameMode.ObserveEveryValueChanged(g => g.IndexCurrentPlayer, FrameCountType.FixedUpdate).Subscribe(i =>
		// {
		// 	string s = (PhotonNetwork.LocalPlayer.GetPlayerNumber() == i) ? "You turn!!" : "Wait...";
		// 	Turn.text = $"Round : {m_gameMode.Turn} \n<color=#FF0000>{s}</color>";
		// });

		m_gameMode.ObserveEveryValueChanged(g => g.Turn, FrameCountType.FixedUpdate).Subscribe(i =>
		{
			string s = (PhotonNetwork.LocalPlayer.GetPlayerNumber() == m_gameMode.IndexCurrentPlayer) ? "You turn!!" : "Wait...";
			Turn.text = $"Round : {i} \n<color=#FF0000>{s}</color>";
		});
	}

	public void ShowTextEndGame()
	{
		Turn.text = Turn.text + "\nEnd game...";
	}

	// private void FixedUpdate() 
	// {
	// 	if(m_gameMode != null)
	// 	{
	// 		string s = (PlayerNumberingExtensions.GetPlayerNumber(PhotonNetwork.LocalPlayer) == PrototypeGameMode.Instance.IndexCurrentPlayer) ? "You turn!!" : "";
	// 		Turn.text = $"Round : {m_gameMode.Turn} \n<color=#FF0000>{s}</color>";
	// 	}
	// }

	public void UpdateCurrentHUD(int index)
	{
		Cells.ForEach(cell => cell.BG.enabled = false);
		Cells[index].BG.enabled = true;
	}
}
