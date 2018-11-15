using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DiceControl : InstanceObject<DiceControl>, IPointerUpHandler, IPointerDownHandler
{
	[SerializeField] private TurnActiveUIController uiController;
	[SerializeField] private Image diceGauge;

	public int PoringIndex;
	public PrototypeGameMode GameMode;
	public bool isRollFinish = true;

	private bool isPress = false;
	private int diceResult;

	public void OnPointerDown(PointerEventData eventData)
    {
		if(isRollFinish)
		{
			gameObject.GetComponent<Button>().interactable = false;
			uiController.OnPressRollDice();
			isPress = true;
			diceResult = UnityEngine.Random.Range(0,2);

			StartCoroutine(OnPressBtn());
		}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		isPress = false;
    }

	private IEnumerator OnPressBtn()
	{
		isRollFinish = false;
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
