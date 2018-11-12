using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiceControl : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
	[SerializeField] private TurnActiveUIController uiController;
	[SerializeField] private Image diceGauge;

	public int PoringIndex;
	public PrototypeGameMode GameMode;

	private bool isPress;
	private int diceResult;

	public void OnPointerDown(PointerEventData eventData)
    {
		uiController.OnPressRollDice();
		isPress = true;
		diceResult = UnityEngine.Random.Range(0,2);

		StartCoroutine(OnPressBtn());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		isPress = false;
    }

	private IEnumerator OnPressBtn()
	{
		diceGauge.fillAmount = 0;
		while (isPress)
		{
			if(diceGauge.fillAmount >= 1) diceGauge.fillAmount = 0;
			yield return new WaitForEndOfFrame();
			diceGauge.fillAmount += Time.deltaTime;
		}

		if(diceGauge.fillAmount < 0.35f) diceResult += 0;
		else if(diceGauge.fillAmount < 0.67f) diceResult += 2;
		else diceResult += 4;

		yield return new WaitForSeconds(1);
		diceGauge.fillAmount = 0;
		GameMode.PhotonNetworkRaiseEvent(EventCode.BeginRollMove, new object[] { PoringIndex, diceResult });
	}
}
