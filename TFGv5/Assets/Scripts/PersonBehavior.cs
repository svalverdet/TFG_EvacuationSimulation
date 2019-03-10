using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PersonBehavior : MonoBehaviour
{
	private NavMeshAgent agent;
	
	private Vector3 initPos;
	private Vector3 goal;
	
	private static int personIDcounter;
	private int personID;
	
	public float DISTANCE;
	public int COLLPOINT;
	public Section destSection;
	public bool reachedDestination = false;
	
	public string firstName;
	//private Section initSection;
	//private List<PersonBehavior> familyMembers;
	//private List<PersonBehavior> friendMembers;
	public bool mobilityImpaired = false;
	//public bool groupLeader = false;
	
	public List<KeyValuePair<Section, float>> possibleCPs;
	
	private TextHolder textHolder;
	
	
	void Awake () 
	{
		agent = GetComponent<NavMeshAgent>();
		int r = Random.Range(1, 5);
		if (r>3) mobilityImpaired = true;
		
		personID = personIDcounter;
		personIDcounter++;
		firstName = "person"+personID;
		
		possibleCPs = new List<KeyValuePair<Section, float>>();
	}
	void Start()
	{
		initPos = transform.position;
		textHolder = GetComponent<TextHolder>();
		textHolder.label.text = firstName;
	}
	
	void Update()
	{
		if (!reachedDestination && destSection!= null && !agent.pathPending)
		{
			if (agent.remainingDistance <= agent.stoppingDistance)
			{
				if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
				{
					 // Agent reached destination
					 Print(firstName+" has reached destination CP"+destSection.GetSectionID());
					 reachedDestination = true;
					 destSection.PersonArrived();
					 gameObject.SetActive(false);
				}
			}
		}
	}
	
	public void SetGoal(Vector3 pos, int cpID, Section s)
	{
		NavMeshHit hit;
		if (NavMesh.SamplePosition(pos, out hit, 2.0f, NavMesh.AllAreas))
		{
			NavMeshPath path = new NavMeshPath();
			agent.CalculatePath(hit.position, path);
			agent.SetPath(path);
			agent.isStopped = true;
			
			DISTANCE = PathDistance();
			COLLPOINT = cpID;
			destSection = s;
			
			textHolder.label.text += "-> CP"+cpID;
		}
		else{Print("unable to find path");}
	}
	
	private float PathDistance()
	{
		float distance = 0.0f;
		Vector3[] corners = agent.path.corners;
		for (int c = 0; c < corners.Length - 1; ++c) {
			distance += Mathf.Abs((corners[c] - corners[c + 1]).magnitude);
		}
		return distance;
		//Print(personID+": here it goes the path distance "+distance);
	}
	
	public void StartMoving()
	{
		agent.isStopped = !agent.isStopped;
	}
	
	public void ReStart()
	{
		agent.isStopped = true;
		agent.velocity = Vector3.zero;
		agent.Warp(initPos);
	}
	
	
	
	#region GETTERS & SETTERS
		
	public NavMeshAgent 	GetAgent() {return agent;}
	public bool 			GetMI(){return mobilityImpaired;}
	public Vector3 			GetInitPos() { return initPos;}
	public int 				GetID(){return personID;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" PERSON:  "+msg);}
	
}
