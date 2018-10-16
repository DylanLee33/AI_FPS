using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Target : MonoBehaviour {

	public GameObject PatrolPoint01;
	public GameObject PatrolPoint02;

	public NavMeshAgent Agent;

	void Start ()
	{
		Agent.SetDestination (PatrolPoint02.transform.position);
	}


	void Update ()
	{
		if (Agent.remainingDistance < 0.5f) 
		{
			Agent.SetDestination (PatrolPoint02.transform.position);
		}

		if (Agent.remainingDistance < 0.5f) 
		{
			Agent.SetDestination (PatrolPoint01.transform.position);
		}
	}
}
