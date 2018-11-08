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
	[SerializeField] private Image JobImage;
	[SerializeField] private Transform iconRoot;

	[SerializeField] private List<EffectReceiver> m_currentEffects = new List<EffectReceiver>();
	[SerializeField] private List<StatusIconData> m_currentIconEffects = new List<StatusIconData>();

	public int AttackResultIndex = 0;
	public int DefendResultIndex = 0;

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
		JobImage.sprite = Property.JobImage;
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
			if(newEffect.Status.CheckHasStatus(SkillStatus.Posion) &&
				PrototypeGameMode.Instance.IsMineTurn()) 
				newEffect.EffectDuration -= 1;
				
			var v = m_currentEffects.Find(fx => fx.Status == newEffect.Status);
			if(v != null)
			{
				m_currentEffects[m_currentEffects.IndexOf(v)] = newEffect;
			}
			else
			{
				m_currentEffects.Add(newEffect);
				StatusIconData icon = new StatusIconData(){
					status = newEffect.Status,
					image = StatusIconCreater.Instance.CreateIcon(newEffect.Status, iconRoot)
				};
				m_currentIconEffects.Add(icon);
			}
		}

		return true;
	}

	public bool TakeDamage(Poring ownerDamage, float damageResult, bool ignoreUltimate = false, GameObject particle = null)
	{
		if(!ignoreUltimate && damageResult != 0)
		{
			PrototypeGameMode.Instance.AddPoringCanGetUltimatePoint(this);
			PrototypeGameMode.Instance.AddPoringCanGetUltimatePoint(ownerDamage);
		}
		
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
			if(ownerDamage != null)
			{
				ownerDamage.Property.CurrentPoint += Property.CurrentPoint / 2;
				ownerDamage.WinCondition += 1;
			}
			m_currentEffects.Clear();
			m_currentIconEffects.ForEach(icon => Destroy(icon.image.gameObject));
			m_currentIconEffects.Clear();
			PrototypeGameMode.Instance.CheckEndGame();
			return false;
		}
	}

	WaitForSeconds wait = new WaitForSeconds(0.5f);

	public IEnumerator OnStartTurn()
	{
		CountDownEffectDuration();

		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Burn;
			if(item.Status.CheckHasStatus(e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, true, item.Particle))
				{
					Behavior.Respawn();
					PrototypeGameMode.Instance.CurrentGameState = eStateGameMode.EndTurn;
					yield break;
				}
				yield return wait;
			}
			
		}

		
		yield break;
	}

	public IEnumerator OnEndTurn()
	{
		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Posion;
			if(item.Status.CheckHasStatus(e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, true, item.Particle))
				{
					Behavior.Respawn();
					PrototypeGameMode.Instance.CurrentGameState = eStateGameMode.EndTurn;
					yield break;
				}
				yield return wait;
			}
		}
	}

	private void CountDownEffectDuration()
	{
		List<EffectReceiver> removeKey = new List<EffectReceiver>();

		m_currentEffects.ForEach(item => 
		{
			item.EffectDuration--;
			if(item.EffectDuration <= 0) removeKey.Add(item);
		});
	
		removeKey.ForEach(statusKey => 
		{
			m_currentEffects.Remove(statusKey);

			StatusIconData i = m_currentIconEffects.Find(ci => ci.status == statusKey.Status);
			Destroy(i.image.gameObject);
			m_currentIconEffects.Remove(i);
		});
	}

	public bool OnMove()
	{
		for (int i = 0; i < m_currentEffects.Count; i++)
		{
			var item = m_currentEffects[i];

			SkillStatus e = SkillStatus.Bleed;
			if(item.Status.CheckHasStatus(e))
			{
				if(!TakeDamage(PrototypeGameMode.Instance.GetPoringByIndex(item.OwnerId), item.Damage, item.Particle))
				{
					return false;
				}

			}
		}
		return true;
	}

	public SkillStatus GetCurrentStatus()
	{
		SkillStatus status = 0;

		foreach (var dict in m_currentEffects)
		{
			status |= dict.Status;
		}
		return status;
	}

	public EffectReceiver GetStatus(SkillStatus status)
	{
		EffectReceiver fx = m_currentEffects.Find( s => status == s.Status);
		if(fx != null) return fx;
		else return null;
	}

	public float GetBlessingBuff()
	{
		EffectReceiver fx = m_currentEffects.Find( s => SkillStatus.Blessing == s.Status);
		if(fx != null) return fx.Damage;
		else return 1;
	}

	struct StatusIconData
	{
		public SkillStatus status;
		public Image image;
	}
}
