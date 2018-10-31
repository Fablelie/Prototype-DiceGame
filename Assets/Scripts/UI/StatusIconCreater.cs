using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StatusIconCreater : InstanceObject<StatusIconCreater> 
{
	[System.Serializable]
	struct BaseStatusSprite
	{
		public SkillStatus key;
		public Sprite icon;
	}

	public GameObject prefab;

	[SerializeField] private List<BaseStatusSprite> m_statusSourceList = new List<BaseStatusSprite>();

	public Image CreateIcon(SkillStatus key, Transform parent)
	{
		Image i = Instantiate(prefab).GetComponent<Image>();
		i.sprite = m_statusSourceList.Find(s => s.key == key).icon;
		i.transform.SetParent(parent);
		i.transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
		i.transform.localRotation = Quaternion.identity;
		i.transform.localScale = new Vector3(1, 1, 1);
		
		return i;
	}
}
