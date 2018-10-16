using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class AI : MonoBehaviour
{
    NavMeshAgent Agent;
    private Transform EndLevel;
    private Animator animator;

    //VISION & HEARING
    private GameObject Head;
    public bool TargetInSight;
    private bool CanHear;

    private GameObject[] Targets;
    private Vector3[] TargetPositions;
    private float[] Angles;
    private float[] Dist;
	private AudioSource[] TargetAudio;

    //HEALTH
	private GameObject[] HealthPack;
	private float[] HealthPackDist;
    public float Health = 100;
    private float healthPack = 50;
	private float ClosestHealth;

    //AMMO
    public GameObject Bullet;
    private Transform bulletSpawn;
	private GameObject[] AmmoPack;
	private float[] AmmoPackDist;
    public float Ammo = 8;
    private float ammoPack = 8;
	private float ClosestAmmo;
    private float timer = 0.8f;

    //NOT SEEN
    private GameObject[] NotSeenH;
	private GameObject[] NotSeenA;
	private Vector3[] NotSeenVectorH;
	private Vector3[] NotSeenVectorA;


    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        EndLevel = GameObject.Find("EndLevel").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        //VISION & HEARING
        Head = GameObject.FindWithTag("Head");
        TargetInSight = false;
        CanHear = false;
        //AMMO
        bulletSpawn = GameObject.Find("BulletSpawn").transform;
    }

    void Update()
    {
        DrawRayDirection ();
        DrawRaySight ();
        UpdateArrays ();
        SetDestination ();
        AIHearsTarget ();
        AISeesTarget ();
        DestroyAllBullets ();
    }


    void DrawRayDirection()
    {
        Ray Ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(Ray.origin, Ray.direction * 12, Color.green);
    }


    void DrawRaySight()
    {
        float Speed = 0.04f;
        Vector3 Direction = Agent.destination - Head.transform.position;
        Quaternion ToRotation = Quaternion.LookRotation(Direction);
        Head.transform.rotation = Quaternion.Lerp(Head.transform.rotation, ToRotation, Speed);
        Head.transform.position = Agent.transform.position;

        Ray Ray = new Ray(Head.transform.position, Head.transform.forward);
        Debug.DrawRay(Ray.origin, Ray.direction * 20, Color.blue);
    }


    void UpdateArrays()
    {
		//VISION & HEARING
        Targets = GameObject.FindGameObjectsWithTag("Target");
        TargetPositions = new Vector3[Targets.Length];
        Angles = new float[Targets.Length];
        Dist = new float [Targets.Length];
		TargetAudio = new AudioSource[Targets.Length];

        for (int i = 0; i < Targets.Length; i++)
        {
            TargetPositions [i] = Targets [i].transform.position - transform.position;
            Angles [i] = Vector3.Angle(TargetPositions [i], transform.forward);
            Dist [i] = Vector3.Distance(Targets [i].transform.position, transform.position);
			TargetAudio [i] = Targets [i].GetComponent<AudioSource> ();
        }

		//FINDING HEALTH & AMMO
		NotSeenH = GameObject.FindGameObjectsWithTag("NotSeenH");
		NotSeenA = GameObject.FindGameObjectsWithTag("NotSeenA");
		NotSeenVectorH = new Vector3[NotSeenH.Length];
		NotSeenVectorA = new Vector3[NotSeenA.Length];

		for (int i = 0; i < NotSeenH.Length; i++) 
		{
			NotSeenVectorH [i] = NotSeenH [i].transform.position - transform.position;
		}

		for (int i = 0; i < NotSeenA.Length; i++) 
		{
			NotSeenVectorA [i] = NotSeenA [i].transform.position - transform.position;
		}
			
        if (Targets.Length == 0)
        {
            TargetInSight = false;
            CanHear = false;
        }
			
		//HEALTH AND AMMO
		HealthPack = GameObject.FindGameObjectsWithTag("Health");
		AmmoPack = GameObject.FindGameObjectsWithTag ("Ammo");
		HealthPackDist = new float[HealthPack.Length];
		AmmoPackDist = new float[AmmoPack.Length];

		for (int i = 0; i < HealthPackDist.Length; i++) 
		{
			HealthPackDist [i] = Vector3.Distance(HealthPack [i].transform.position, transform.position);
		}

		for (int i = 0; i < AmmoPackDist.Length; i++) 
		{
			AmmoPackDist [i] = Vector3.Distance(AmmoPack [i].transform.position, transform.position);
		}
    }


    void SetDestination()
    {
		ClosestHealth = Mathf.Min (HealthPackDist);
		ClosestAmmo = Mathf.Min (AmmoPackDist);

        if (Health < 30 && HealthPack.Length != 0f) SetHealthDestiantion();
        else if (Ammo < 4 && AmmoPack.Length != 0f) SetAmmoDestiantion();
        else if (CanHear == true) return;
        else if (TargetInSight == true) return;
        else Agent.SetDestination(EndLevel.position);
    }


    void AIHearsTarget()
    {
        float HearingRange;

		if (Ammo <= 0)
        {
            CanHear = false;
            return;
        }

        for (int i = 0; i < Dist.Length; i++)
        {
			if (TargetAudio [i].volume > 0.5f && Dist [i] <= 30f) 
			{
				HearingRange = 30f;
			} else 
			{
				HearingRange = 20f;
			}

			if (TargetAudio[i].isPlaying)
			{
	            if (HearingRange >= Dist[i])
	            {
	                CanHear = true;
	                Agent.SetDestination(Targets[i].transform.position);

	            } else CanHear = false;
    		}
		}
	}


    void AISeesTarget()
    {
        float FoV = 200;
        RaycastHit Hit;

        if (Ammo <= 0)
        {
            TargetInSight = false;
            return;
        }

		for (int i = 0; i < Angles.Length; i++)
        {
			if (Angles [i] < FoV * 0.5f) {
				if (Physics.Raycast (transform.position + transform.up, TargetPositions [i].normalized, out Hit, 12))
				{
					if (Hit.collider.gameObject == Targets [i]) 
					{
						TargetInSight = true;
						Agent.SetDestination (Targets [i].transform.position);
						Agent.speed = 1f;
                        animator.SetFloat("Forward", 0.5f);
                        animator.SetBool("Crouch", true);
						Invoke ("FireWeapon", 1.0f);
                        return;
					}
                    else TargetInSight = false;
                }
			}
        }

        for (int i = 0; i < NotSeenVectorH.Length; i++) 
		{
			if (Physics.Raycast (transform.position + transform.up, NotSeenVectorH [i].normalized, out Hit, 20)) 
			{
				if (Hit.collider.gameObject == NotSeenH [i])
				{
					NotSeenH[i].gameObject.tag = "Health";
				}
			}
		}

		for (int i = 0; i < NotSeenVectorA.Length; i++) 
		{
			if (Physics.Raycast (transform.position + transform.up, NotSeenVectorA [i].normalized, out Hit, 20)) 
			{
				if (Hit.collider.gameObject == NotSeenA [i])
				{
					NotSeenA[i].gameObject.tag = "Ammo";
				}
			}
		}

        TargetInSight = false;

        if (!TargetInSight)
        {
            animator.SetFloat("Forward", 1f);
            animator.SetBool("Crouch", false);
        }
    }


    void DestroyAllBullets()
    {
         GameObject[] AllBullets;

        AllBullets = GameObject.FindGameObjectsWithTag("Bullet");

        for (int i = 0; i < AllBullets.Length; i++)
        {
            Destroy(AllBullets[i], 5.0f);
        }
    }


    void SetHealthDestiantion ()
    {
		for (int i = 0; i < HealthPack.Length; i++) 
		{
			if (ClosestHealth == HealthPackDist[i])
			{
				Agent.SetDestination (HealthPack [i].transform.position);
			}
		}

		Agent.speed = 1f;
    }


    void SetAmmoDestiantion()
    {
		for (int i = 0; i < AmmoPack.Length; i++) 
		{
			if (ClosestAmmo == AmmoPackDist[i])
			{
				Agent.SetDestination (AmmoPack [i].transform.position);
			}
		}

		Agent.speed = 1f;
    }


    void FireWeapon()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            var bullet = Instantiate (Bullet, bulletSpawn.position, bulletSpawn.rotation);
            bullet.GetComponent<Rigidbody>().velocity = Agent.transform.forward * 6;

            Ammo--;
            timer = 0.8f;
        }
    }


    void OnTriggerEnter (Collider other)
    {
        if (other.tag == "Health")
        {
            Health += healthPack;
        }

        if (other.tag == "Ammo")
        {
            Ammo += ammoPack;
        }
    } 
}
