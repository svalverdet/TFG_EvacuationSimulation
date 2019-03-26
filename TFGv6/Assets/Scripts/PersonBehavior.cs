using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PersonBehavior : MonoBehaviour
{
	private NavMeshAgent agent;
	private SceneController sc;
	private Vector3 initPos;
	public Section initSection;
	
	// CHARACTERISTICS
	public enum PersonType {GROUP_LEADER, MOB_IMP, BASIC};
	public enum SpeedType {SLOW, NORMAL, FAST};
	private int personID = -1;
	public string firstName;
	public PersonType type;
	public int priority, age;
	public SpeedType speedType;
	
	// PATH
	public bool reachedDestination = true;
	//public bool randomPath = false;
	public List<Section> sectionsPath;
	public List<Vector3> pointsPath;
	private int pathIndex = 0;
	private bool startedMovement = false;
	
	
	private TextHolder textHolder;
	
	private string[] seriousNames = {"Abby", "Anne", "Alice", "Bronn", "Bram", "Charles", "Cat", "Cris", "Diana", "Dunning", "Ernesto", "Elena", "Earl", "Finn", "Gus", "Gia", "Harry", "Iago", "Javier", "Karen", "Laura", "María", "Nina", "Oscar", "Paco", "Quike", "Ron", "Sara", "Teo", "Uma", "Val"};
	
	private string[] names = {"Lilly", "Miguelón", "Fran", "Sandra", "Harry", "Sara", "Albertini", "Nando", "Felipe", "Buch", "Gatito", "David", "Yols"};
	private string[] surnames = {"Liliana", "Colorao", "Vadillo", "Valverde", "Potter", "Jackson", "Pitarque", "Pitotanque", "Sexto", "Nonda", "Caraculo", "Miau"};
	
	private string[] consonants = {"b","c","d","f","g","h","j","k","l","m","n","p","r","s","t","v","y","z"};
	private string[] vocals = {"a","e","i","o","u"};
	
	void Awake () 
	{
		agent = GetComponent<NavMeshAgent>();
		
		type = PersonType.BASIC;
		priority = 0;
		age = 30;
		speedType = SpeedType.NORMAL;
		
		//firstName = names[Random.Range(0, names.Length-1)]+" "+surnames[Random.Range(0, surnames.Length-1)];
		firstName = seriousNames[Random.Range(0, seriousNames.Length-1)];
		
		/*int nameLength = Random.Range(3,7);
		firstName = "";
		for(int i=0; i<nameLength; i++)
		{
			if(Random.Range(0,5)>1)
			{
				firstName+=consonants[Random.Range(0,consonants.Length-1)];
			} else firstName+=vocals[Random.Range(0,vocals.Length-1)];
		}
		*/
	}
	
	void Start()
	{
		initPos = transform.position;
		
		sectionsPath = new List<Section>();
		pointsPath = new List<Vector3>();
		
		textHolder = GetComponent<TextHolder>();
		textHolder.label.text = firstName;
	}
	
	void Update()
	{
		if(startedMovement)
		{
			if(reachedDestination && pathIndex<pointsPath.Count)
			{
				reachedDestination = false;
				agent.SetDestination(pointsPath[pathIndex]);
			}
			else if (!reachedDestination && agent.remainingDistance <= agent.stoppingDistance+0.3f)
			{
				reachedDestination = true;
				pathIndex++;
			}
		}
	}
	
	public void SetOnFloor(Vector3 pos_){ agent.Warp(pos_);}
	
	private float PathDistance()
	{
		float distance = 0.0f;
		Vector3[] corners = agent.path.corners;
		for (int c = 0; c < corners.Length - 1; ++c) {
			distance += Mathf.Abs((corners[c] - corners[c + 1]).magnitude);
		}
		return distance;
	}
	
	public void ToggleMovement()
	{
		agent.isStopped = !agent.isStopped;
	}
	
	public void ReStart()
	{
		agent.isStopped = true;
		agent.velocity = Vector3.zero;
		agent.Warp(initPos);
	}
	
	public float GetSpeed(SpeedType s_)
	{
		switch(s_)
		{
			case SpeedType.SLOW: return 1f;
			case SpeedType.NORMAL: return 2.5f;
			case SpeedType.FAST: return 3.5f;
		}
		return 0f;
	}
	
	public void SetSpeed()
	{
		agent.speed = GetSpeed(speedType);
	}
	
	void OnMouseOver()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			return;
		}
		string text = "ID: "+personID+"\nName: "+firstName+"\nType: "+type+"\nSpeed: "+speedType+"\nPath: ";
		foreach(Section s in sectionsPath)
		{
			text += s.name+", ";
		}
		sc.ToggleText(text);
	}
	
	void OnMouseExit()
	{
		sc.ToggleText("");
	}
	
	#region GETTERS & SETTERS
		
	public NavMeshAgent 	GetAgent() {return agent;}
	public bool 			GetMI(){return type == PersonType.MOB_IMP;}
	public Vector3 			GetInitPos() { return initPos;}
	public int 				GetID(){return personID;}
	public bool 			GetStartedMovement(){ return startedMovement;}
	
	public void 			SetID(int id_){personID = id_; /*firstName = "person"+personID;*/}
	public void 			SetStartedMovement(bool s_){ startedMovement = s_;}
	public void 			SetSceneController(SceneController sc_){sc = sc_;}
	public void 			SetInitSection(Section s_){initSection = s_;}
	public void 			SetName(string name_){ firstName = name_; textHolder.label.text = name_;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" PERSON:  "+msg);}
	
}

/*

*/
