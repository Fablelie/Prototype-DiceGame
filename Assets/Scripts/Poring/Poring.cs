using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poring : MonoBehaviour {
	public Node Node;
    public Node PrevNode;

	public PoringBehavior Behavior;
	public Animator Animator;

	[SerializeField] private PoringProperty m_property;

	void Awake()
	{
		// Animator = GetComponent<Animator>();
		// Behavior = gameObject.AddComponent<PoringBehavior>();
		// Behavior.Poring = this;
	}

	public void Init(PoringProperty property)
	{
		m_property = property.CopyTo(m_property);
	}

	void HeadTo(Vector3 position) {
		transform.LookAt(position, transform.up);
	}
}
