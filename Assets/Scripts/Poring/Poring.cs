using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public void OnReceiveStatus(int statusResult)
	{
		
	}
}
