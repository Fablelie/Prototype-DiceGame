using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : InstanceObject<HUDController> 
{
	[SerializeField] private Text Turn; 
	private int currentTurn;
	public List<HUDCell> Cells;
	private GameMode m_gameMode;

	public void Init(List<Poring> porings, GameMode gameMode)
	{
		m_gameMode = gameMode;
		Cells.ForEach(cell => cell.gameObject.SetActive(false));

		for (int i = 0; i < porings.Count; i++)
		{
			var cell = Cells[i];

			cell.Init(porings[i]);
			cell.gameObject.SetActive(true);
		}
		
	}

	private void FixedUpdate() 
	{
		if(m_gameMode != null)
		{
			Turn.text = $"Round : {m_gameMode.Turn}";
		}
	}

	public void UpdateCurrentHUD(int index)
	{
		Cells.ForEach(cell => cell.GetComponent<Image>().color = Color.white);
		Cells[index].GetComponent<Image>().color = Color.red;
	}
}
