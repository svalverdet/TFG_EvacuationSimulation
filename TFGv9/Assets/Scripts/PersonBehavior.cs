using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using TMPro;

public class PersonBehavior : MonoBehaviour
{
	private NavMeshAgent agent;
	private People peopleController;
	[SerializeField] private Node initNode;
	[SerializeField] private Section initSection;
	private Vector3 initPos;
	[SerializeField] private Section destSection;
	
	// CHARACTERISTICS
	public enum PersonType {GROUP_LEADER, MOB_IMP, FAM, BASIC};
	public enum SpeedType {SLOW, NORMAL, FAST};
	[SerializeField] private int ID;
	[SerializeField] private string firstName;
	[SerializeField] private PersonType type;
	[SerializeField] private int priority, age;
	[SerializeField] private SpeedType speedType;
	
	// PATH
	[SerializeField] private bool reachedDestination = true;
	//public bool randomPath = false;
	[SerializeField] private List<Node> nodesPath;
	private List<Vector3> pointsPath;
	[SerializeField] private int pathIndex = 0;
	[SerializeField] private bool startedMovement = false;
	
	[SerializeField] private float timer;

	public TMP_Text myText;
	private Quaternion initialRot;
	//private TextHolder textHolder;
	
	private string[] seriousNames = {"Abby", "Anne", "Alice", "Bronn", "Bram", "Charles", "Cat", "Cris", "Diana", "Dunning", "Ernesto", "Elena", "Earl", "Finn", "Gus", "Gia", "Harry", "Iago", "Javier", "Karen", "Laura", "María", "Nina", "Oscar", "Paco", "Quike", "Ron", "Sara", "Teo", "Uma", "Val"};
	
	private string[] names = {"Lilly", "Miguelón", "Fran", "Sandra", "Harry", "Sara", "Albertini", "Nando", "Felipe", "Buch", "Gatito", "David", "Yols"};
	private string[] surnames = {"Liliana", "Colorao", "Vadillo", "Valverde", "Potter", "Jackson", "Pitarque", "Pitotanque", "Sexto", "Nonda", "Caraculo", "Miau"};
	
	
	void Awake () 
	{
		agent = GetComponent<NavMeshAgent>();
		nodesPath = new List<Node>();
		pointsPath = new List<Vector3>();
		//textHolder = GetComponent<TextHolder>();
		//textHolder.label.text = firstName;
		initialRot = myText.transform.rotation;
		peopleController = transform.parent.GetComponent<People>();

	}

	public void Setup(int ID_, Vector3 pos_, Section s_, Node n_)
	{
		ID = ID_;
		type = PersonType.BASIC;
		if(Random.Range(0,4)>2) type = PersonType.MOB_IMP;
		else if(Random.Range(0,10)>7) type = PersonType.GROUP_LEADER;
		priority = 0;
		age = 30;
		speedType = SpeedType.NORMAL;
		
		//firstName = names[Random.Range(0, names.Length-1)]+" "+surnames[Random.Range(0, surnames.Length-1)];
		firstName = seriousNames[Random.Range(0, seriousNames.Length-1)];

		initPos = pos_;
		initSection = s_;
		initNode = n_;
	}

	public void SetupFromData(Node initNode_, int ID_, int priority_, int age_, int type_, int speedType_, string firstName_)
	{
		ID = ID_; initNode = initNode_; priority = priority_; age = age_; type = (PersonType) type_; speedType = (SpeedType) speedType_; firstName = firstName_;
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
			else if (!reachedDestination && agent.remainingDistance <= agent.stoppingDistance+2f)
			{
				reachedDestination = true;
				pathIndex++;
				if(pathIndex>=pointsPath.Count)
				{
					//Utils.Print("P"+ID+": I finished!!"); //startedMovement = false;
					//agent.isStopped = true;
					//agent.ResetPath();
					timer = peopleController.GetSceneController().GetTimer();
					destSection.PersonArrived(ID, timer);
					peopleController.PersonArrived(this, timer);
					
				}
			}
		}

		myText.text = ""+ID;
		myText.transform.rotation = initialRot;

	}
	
	public void SetOnFloor(Vector3 pos_){ agent.Warp(pos_);}
	
	public void ToggleMovement()
	{
		agent.isStopped = !agent.isStopped;
	}

	public void StopMovement()
	{
		agent.isStopped = true;
		agent.ResetPath();
	}
	
	public void ReStart()
	{
		agent.isStopped = true;
		agent.velocity = Vector3.zero;
		agent.Warp(initNode.GetPos());
	}

	public PersonData TransformData()
	{
		return new PersonData(ID, priority, age, (int)type, (int)speedType, firstName, initNode.GetID(), initSection.GetID(), initPos);
	}
	
	public PathData TransformPathData()
	{
		List<int> nodesIDs = new List<int>();
		foreach(Node n in nodesPath)
		{
			nodesIDs.Add(n.GetID());
		}
		return new PathData(ID, nodesIDs);
	}
	
	/*
	void OnMouseOver()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			return;
		}
		string text = "ID: "+ID+"\nName: "+firstName+"\nType: "+type+"\nSpeed: "+speedType+"\nPath: ";
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
	*/
	
	#region GETTERS & SETTERS
		
	public NavMeshAgent 	GetAgent() {return agent;}
	public bool 			GetMI(){return type == PersonType.MOB_IMP;}
	public int				GetPersonType(){return (int)type;}
	public Vector3 			GetInitPos() { return initNode.GetPos();}
	public int 				GetID(){return ID;}
	public bool 			GetStartedMovement(){ return startedMovement;}
	public Node 			GetInitNode(){return initNode;}
	public List<Node>		GetNodesPath(){return nodesPath;}
	public float 			GetTimer(){return timer;}
	public Section 			GetDestSection(){ return destSection;}
	public float 			GetSpeed(SpeedType s_)
	{
		switch(s_)
		{
			case SpeedType.SLOW: return 1f;
			case SpeedType.NORMAL: return 2.5f;
			case SpeedType.FAST: return 3.5f;
		}
		return 0f;
	}
	
	public void 	SetID(int id_){ID = id_; /*firstName = "person"+ID;*/}
	public void 	SetStartedMovement(bool s_){ startedMovement = s_;}
	public void 	SetInitSection(Section s_){initSection = s_;}
	public void 	SetName(string name_){ firstName = name_; /*textHolder.label.text = name_;*/}
	public void 	SetAgentSpeed(){ agent.speed = GetSpeed(speedType);}
	public void 	SetInitPos(Vector3 pos_){ initPos = pos_;}
	public void 	SetNodesPath(List<Node> np_){nodesPath = np_;}
	public void 	SetPointsPath(List<Vector3> pp_){pointsPath = pp_;}
	public void 	SetText(TMP_Text t_){ myText = t_;}
	public void 	SetDestSection(Section s_){destSection = s_;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" PERSON:  "+msg);}
	
}