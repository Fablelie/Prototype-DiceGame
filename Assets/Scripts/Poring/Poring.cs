using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Poring : MonoBehaviour {
	public Node Node;
    public Node PrevNode;

    public Poring Target;

	public PoringBehavior Behavior;
	public Animator Animator;

	public PoringProperty Property;

	public int WinCondition = 0;

	public List<int> OffensiveResultList = new List<int>();
	public List<int> DeffensiveResultList = new List<int>();

	public int OffensiveResult;
	public int DeffensiveResult;

	public Roll MoveRoll;
	public Roll OffensiveRoll;
	public Roll DeffensiveRoll;

	public bool IsAlive => Property.CurrentHp > 0;

	private string playerName;
	public string PlayerName 
	{
		get {
			return playerName;
			}
		set {
			playerName = value;
			PlayerNameText.text = playerName;
		}
	}

	[SerializeField] private Text PlayerNameText;

	[SerializeField] private List<EffectReceiver> m_currentEffects = new List<EffectReceiver>();

	void Awake()
	{
		// Animator = GetComponent<Animator>();
		// Behavior = gameObject.AddComponent<PoringBehavior>();
		// Behavior.Poring = this;
	}

	public void Init(PoringProperty baseProperty)
	{
		Property = ScriptableObject.CreateInstance<PoringProperty>();
        Property.Init(baseProperty);
		// Debug.Log("Current hp : " + Property.CurrentHp);
	}

	void HeadTo(Vector3 position) {
		transform.LookAt(position, transform.up);
	}

	public bool OnReceiverEffect(List<EffectReceiver> effectList)
	{
		foreach (EffectReceiver baseEffect in effectList)
		{
			EffectReceiver newEffect = new EffectReceiver(baseEffect);
			if((int)newEffect.Status == 0) 
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(newEffect.OwnerId), newEffect.Damage))
					return false;
				
				continue;
			}

			var v = m_currentEffects.Find(fx => fx.Status == newEffect.Status);
			if(v != null)
			{
				m_currentEffects[m_currentEffects.IndexOf(v)] = newEffect;
			}
			else
			{
				m_currentEffects.Add(newEffect);
			}
		}

		return true;
	}

	public bool TakeDamage(Poring ownerDamage, float damageResult, GameObject particle = null)
	{
		Property.CurrentHp -= damageResult;
		if(particle != null) InstantiateParticleEffect.CreateFx(particle, transform.position);

		if (Property.CurrentHp > 0)
		{
			Animator.Play((damageResult == 0) ? "Dodge" : "take_damage");
			return true;
		}
		else
		{
			Animator.Play("die");
			ownerDamage.Property.CurrentPoint += Property.CurrentPoint / 2;
			ownerDamage.WinCondition += 1;
			m_currentEffects.Clear();
			PrototypeGameMode.Instance.CheckEndGame();
			return false;
		}
	}

	WaitForSeconds wait = new WaitForSeconds(0.5f);

	public IEnumerator OnStartTurn()
	{
		List<EffectReceiver> removeKey = new List<EffectReceiver>();
		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Burn;
			if(ExtensionSkillStatus.CheckResultInCondition((int)item.Status, (int)e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, item.Particle))
				{
					Behavior.Respawn();
					PrototypeGameMode.Instance.CurrentGameState = eStateGameMode.EndTurn;
					yield break;
				}
				yield return wait;
			}
			item.EffectDuration--;
			if(item.EffectDuration <= 0) removeKey.Add(item);
		}

		removeKey.ForEach(statusKey => 
		{
			m_currentEffects.Remove(statusKey);
		});
		yield break;
	}

	public IEnumerator OnEndTurn()
	{
		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Posion;
			if(ExtensionSkillStatus.CheckResultInCondition((int)item.Status, (int)e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, item.Particle))
				{
					Behavior.Respawn();
					PrototypeGameMode.Instance.CurrentGameState = eStateGameMode.EndTurn;
					yield break;
				}
				yield return wait;
			}
		}
	}

	public bool OnMove()
	{
		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Bleed;
			if(ExtensionSkillStatus.CheckResultInCondition((int)item.Status, (int)e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, item.Particle))
				{
					return false;
				}

			}
		}
		return true;
	}

	public int GetCurrentStatus()
	{
		SkillStatus status = 0;

		foreach (var dict in m_currentEffects)
		{
			status |= dict.Status;
		}
		return (int)status;
	}
}
