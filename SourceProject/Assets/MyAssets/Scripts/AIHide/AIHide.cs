using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIHide : MonoBehaviour {

	NavMeshAgent Agent;
	private GameObject Player;

	private GameObject[] HidingPlaces;
	private GameObject[] SafePlaces;
	private GameObject[] UnsafePlaces;
	private Vector3[] SpaceToPlayer; 
	private Vector3 AgentToPlayer;

	private float[] SpaceDistances;
	private float FurthestSpace;
	private float ClosestSpace;

	void Start () 
	{
		Agent = GetComponent<NavMeshAgent> ();
		Agent.SetDestination (GameObject.Find("Location01").transform.position);
		Player = GameObject.Find ("FPSController");

		HidingPlaces = GameObject.FindGameObjectsWithTag ("Hide");
		SpaceToPlayer = new Vector3[HidingPlaces.Length];
		SpaceDistances = new float[HidingPlaces.Length];
	}
	

	void Update () 
	{
		SafePlaces = GameObject.FindGameObjectsWithTag ("Hide");
		UnsafePlaces = GameObject.FindGameObjectsWithTag ("NotHidden");

		FindSafeLocoations ();
		AIToSafeLocation ();
	}


	void FindSafeLocoations ()
	{
		RaycastHit Hit;

		for (int i = 0; i < HidingPlaces.Length; i++) 
		{
			SpaceToPlayer [i] = HidingPlaces[i].transform.position - Player.transform.position;

			if (Physics.Raycast (Player.transform.position, SpaceToPlayer[i], out Hit, 50))
			{
				if (Hit.collider.gameObject == HidingPlaces[i])
				{
					HidingPlaces[i].gameObject.tag = "NotHidden";
				}
				else if (Hit.collider.gameObject != HidingPlaces[i])
				{
					HidingPlaces[i].gameObject.tag = "Hide";
				}
			}

			//RAY FOR TESTING
			Ray Ray = new Ray(Player.transform.position, SpaceToPlayer[i]);
			Debug.DrawRay(Ray.origin, Ray.direction * 50, Color.red);
		}
	}


	void AIToSafeLocation () 
	{
		RaycastHit Hit;

		AgentToPlayer = Agent.transform.position - Player.transform.position;

		if (Physics.Raycast (Player.transform.position, AgentToPlayer, out Hit, 50))
		{
			if (Hit.collider.gameObject.tag == "Obstruction")
			{
				Agent.speed = 0f;
				return;
			}

			FindSpaces ();

			for (int i = 0; i < SafePlaces.Length; i++) 
			{
				if (FurthestSpace == SpaceDistances[i])
				{
					Agent.SetDestination (SafePlaces[i].transform.position);
					Agent.speed = 8f;
				}
			}
		}

		// RAY FOR TESTING
		Ray ray = new Ray(Player.transform.position, AgentToPlayer);
		Debug.DrawRay(ray.origin, ray.direction * 50, Color.blue);
	}


	void FindSpaces ()
	{
		for (int i = 0; i < SafePlaces.Length; i++)
		{
			SpaceDistances [i] = Vector3.Distance (SafePlaces [i].transform.position, Player.transform.position);
		}

		FurthestSpace = Mathf.Max (SpaceDistances);
		ClosestSpace = Mathf.Min (SpaceDistances);
	}
}
