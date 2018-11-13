using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "ReplaceToMapName", menuName = "Monster/MonsterPool")]
public class MonsterPool : ScriptableObject
{
	public List<MonsterTier> MonsterTierList;

	public BaseMonsterScriptableObject GetMonster(int currentTurn)
	{
		MonsterTier monsterTier = null;
		char[] c;

		foreach (var mt in MonsterTierList) // ? = number
		{
			switch (mt.TierType) 
			{
				case TierType.Between: // ?-?
					c = mt.StringFormatTier.ToCharArray();
					for(int i = int.Parse(c[0].ToString()); i <= int.Parse(c[2].ToString()); i++)
					{
						if(i == currentTurn)
							monsterTier = mt;
					}
				break;
				case TierType.MoreThen: // ?+
					c = mt.StringFormatTier.ToCharArray();
					if(currentTurn >= int.Parse(c[0].ToString()))
						monsterTier = mt;
				break;
				case TierType.Extra: // ?%
					c = mt.StringFormatTier.ToCharArray();
					int randomValue = Random.Range(1, 101);
					if(int.Parse(c[0].ToString()) >= randomValue)
					{
						int index = Random.Range(0, mt.Monsters.Count);
						return mt.Monsters[index];
					}
				break;
			}
		}
		
		return monsterTier.Monsters[Random.Range(0, monsterTier.Monsters.Count)];
	}
}

[System.Serializable]
public class MonsterTier
{
	public string TierName;
	public TierType TierType;
	[Tooltip("ex. 1-3 this tier'll create between turn 1-3, 7+ this tier'll create turn 7 upper.")]
	public string StringFormatTier;
	public List<BaseMonsterScriptableObject> Monsters = new List<BaseMonsterScriptableObject>();
}

public enum TierType
{
	Between,
	MoreThen,
	Extra
}