using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class SceneController : MonoBehaviour
{
	public GameObject floor, personPrefab, tilePrefab, sectionPrefab;
	private GameObject personGO;
	public TMP_Text hoverText;
	public MenuController menu;
	
	private DataController dataController;
	private MapData mapData;
	
	private Map map;
	private int width, height;
	private Camera cam;
	
	public enum Modes{BUILD, SECTION, PEOPLE, CAM}
	private Modes mode;
	private int numberOfModes;
	
	private List<PersonBehavior> people;
	private PersonBehavior nextPerson;
	private int personIDcounter = 0;
	private bool peopleBaked;
	
    void Start()
    {
		cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		cam.GetComponent<CameraBehavior>().Initialize(this);
		
		width = 45; height = 35;
		map = new Map(this, width, height, floor, tilePrefab, sectionPrefab);
		
		mode = Modes.CAM;
		numberOfModes = System.Enum.GetNames(typeof(Modes)).Length;
		
		people = new List<PersonBehavior>();
		
		dataController = new DataController(this);
		
    }

	
    void Update()
    {
		ManageMouse();
		ManageKeyboard();
    }
	

	
	public void ManageMouse()
	{
		// Left click DOWN
		if(mode == Modes.BUILD && Input.GetMouseButtonDown(0))
		{
			if(map.SetMouseOnGridPosition())
			{
				map.validInitClickedPos = true;
				map.initClickedPos = map.ToMap(map.actualClickedPos);
			}
			else {map.validInitClickedPos = false;}
		}
		
		// Left held
		else if(mode == Modes.BUILD && map.validInitClickedPos && Input.GetMouseButton(0))
		{
			map.HighlightSelectedTiles();
		}
		
		// Left click UP
		else if (Input.GetMouseButtonUp(0))
        {
			GameObject go = null;
			
			switch(mode)
			{
				case Modes.BUILD: 	if(map.validInitClickedPos) map.CreateWallsOrDoor(); break;
				case Modes.SECTION:	
					go = map.GetElemOnCurrentMousePosition();
					if(go!= null && go.name == "Tile")
					{
						Section s = go.GetComponent<Tile>().GetSection();
						menu.sectionEdit.SetSection(s);
					}
					break;
				case Modes.PEOPLE: 
					go = map.GetElemOnCurrentMousePosition();
					if(go!= null)
					{
						if(go.name == "Tile")
						{
							Section s = go.GetComponent<Tile>().GetSection();
							PlacePerson(map.actualClickedPos, s); 
						}
						else if(go.name == "Person")
						{
							menu.personEdit.SetPerson(go.GetComponent<PersonBehavior>());
						}
					}						
					break;
				default: break;
			}
        }
		
		// Right click UP
		else if(Input.GetMouseButtonUp(1))
		{
			GameObject go = map.GetElemOnCurrentMousePosition();
			
			switch(mode)
			{
				case Modes.BUILD: 
					if(go!=null)
					{
						if(go.name == "Door") {go.transform.parent.GetComponent<Tile>().DestroyDoor();}
						else if(go.name == "Wall") {go.transform.parent.GetComponent<Tile>().DestroyWall();}
					}					
				break;
				case Modes.PEOPLE: 
					if(go!=null)
					{
						if(go.name == "Person") 
						{
							people.Remove(go.GetComponent<PersonBehavior>());
							Destroy(go);
						}
					}					
				break;
				default: break;
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			Debug.Log(map.validInitClickedPos);
		}
		
		
	}
	
	public void ManageKeyboard()
	{
		// Change mode
		if (Input.GetKeyUp(KeyCode.P))
		{
			mode = (Modes)(((int) mode+1)%numberOfModes);
			Print("Ahora estamos en modo "+mode);
		}
		
		else if(Input.GetKeyUp(KeyCode.Space))
		{
			switch(mode)
			{
				case Modes.BUILD: map.BuildNavMesh(); map.BuildSections(); map.MakeDoorsConnections(); break;
				case Modes.PEOPLE: /*GenerateRandomSectionsPath();*/ break;
				default: break;
			}
		}
		
		else if(mode==Modes.PEOPLE && Input.GetKeyUp(KeyCode.F))
		{
			foreach(PersonBehavior p in people)
			{
				p.SetStartedMovement(true);
			}
		}
		else if(Input.GetKeyUp(KeyCode.G))
		{
			foreach(PersonBehavior p in people)
			{
				p.ToggleMovement();
			}
		}
	}
	
	public void PlacePerson(Vector3 pos_, Section s_)
	{
		NavMeshHit hit;
		
		// Place the person in the NavMesh
		if (NavMesh.SamplePosition(pos_, out hit, 2.0f, NavMesh.AllAreas)) {
			//personGO = GameObject.Instantiate(personPrefab);
			//personGO.name = "Person";
			//PersonBehavior newPerson = personGO.GetComponent<PersonBehavior>();
			//newPerson.SetSceneController(this);
			nextPerson.gameObject.SetActive(true);
			nextPerson.SetOnFloor(hit.position);
			//newPerson.SetID(personIDcounter);
			nextPerson.initSection = s_;
			//personIDcounter++;
			people.Add(nextPerson);
			
			nextPerson = GetNextPerson();
			menu.personEdit.SetPerson(nextPerson);
			
			peopleBaked = false;
		} 
	}
	
	// REAL (assuming section by section)
	public void CreatePathsWaypoints()
	{
		Section actualSection = null;
		Door nextDoor = null;
		Vector3 finalPos;
		
		foreach(PersonBehavior p in people)
		{
			actualSection = p.initSection;
			for(int i = 0; i<p.sectionsPath.Count; i++)
			{
				nextDoor = actualSection.GetConnection(p.sectionsPath[i]);
				
				if(nextDoor != null)
				{
					p.pointsPath.Add(map.FromMap(nextDoor.GetPos()));
				}
				else 
				{
					p.pointsPath.Add(map.FromMap(GetClosestDoor(p, p.sectionsPath[i]).GetPos()));
				}	
				
				actualSection = p.sectionsPath[i];
			}
			finalPos = actualSection.GetTiles()[Random.Range(0,actualSection.GetTiles().Count-1)].GetPos();
			p.pointsPath.Add(map.FromMap(finalPos));
		}
	}
	
	/* RANDOM
	public void GenerateRandomSectionsPath()
	{
		Section actualSection = null;
		int pathLength;
		int doorsCount;
		Door randomDoor;
		List<Section> sectionsConnectedToDoor;
		Vector3 finalPos;
		
		Door nextDoor = null; // Delete later, belongs to upper method
		
		foreach(PersonBehavior p in people)
		{
			
			p.SetSpeed();
			
			if(p.randomPath){// Delete later, belongs to upper method
			actualSection = p.initSection;
			pathLength = Random.Range(3,10);
			randomDoor = null;
			
			for(int i=0; i<pathLength; i++)
			{
				doorsCount = actualSection.doors.Count;
				if(doorsCount>0)
				{
					randomDoor = actualSection.doors[Random.Range(0,doorsCount-1)];
					p.pointsPath.Add(map.FromMap(randomDoor.GetPos()));
					
					sectionsConnectedToDoor = new List<Section>(randomDoor.connections);
					sectionsConnectedToDoor.Remove(actualSection);
					actualSection = sectionsConnectedToDoor[Random.Range(0, sectionsConnectedToDoor.Count-1)];
				}
				else return;
			}
			finalPos = actualSection.GetTiles()[Random.Range(0,actualSection.GetTiles().Count-1)].GetPos();
			p.pointsPath.Add(map.FromMap(finalPos));
			}
			
			// Delete later from here, belongs to upper method
			else{
			actualSection = p.initSection;
			for(int i = 0; i<p.sectionsPath.Count; i++)
			{
				nextDoor = actualSection.GetConnection(p.sectionsPath[i]);
				actualSection = p.sectionsPath[i];
				if(nextDoor != null)
				{
					p.pointsPath.Add(map.FromMap(nextDoor.GetPos()));
				}
				else Print("Esto hay que arreglarlo");	
			}
			}
			// To here
		}
	}
	
	*/
	
	public void ToggleText(string msg)
	{
		hoverText.gameObject.transform.position = Input.mousePosition;
		hoverText.text = msg;
	}
	
	
	
	public void ChangeMode(int mode_)
	{
		if((mode_==1 || mode_==2) & !map.alreadyBaked)
		{
			Print("You have to bake the map");
		}
		else if(mode_==2 && map.cpSections.Count==0)
		{
			Print("You need at least one CP");
		}
		else
		{
			if(!menu.GetSubMenu(mode_).activeSelf)
			{
				if(mode_==0 && map.alreadyBaked)
				{
					map.alreadyBaked = false;
					Print("Destroying the previous bake!");
					map.DeleteSections();
				}
				else if(mode_==2)
				{
					// Set next person to create
					if(nextPerson==null) nextPerson = GetNextPerson();
					menu.personEdit.SetPerson(nextPerson);
				}
				mode = (Modes)mode_;
			}
			
			menu.ClickedButton(mode_);
		}
	}
	
	public void DeleteAllWalls()
	{
		map.DeleteDoors();
		map.DeleteWalls();
	}
	
	public void DeleteAllDoors()
	{
		map.DeleteDoors();
	}
	
	public void Bake()
	{
		if(map.alreadyBaked)
		{
			Print("Destroying the previous bake!");
			map.DeleteSections();
		}
		
		map.alreadyBaked = true;
		map.BuildNavMesh(); 
		map.BuildSections(); 
		map.MakeDoorsConnections();
		
		ChangeMode(1);
	}
	
	public void DeleteAllPersons()
	{
		peopleBaked = false;
		foreach (PersonBehavior p in people) {
            Destroy(p.gameObject);
        }
		people.Clear();
	}
	
	public PersonBehavior GetNextPerson()
	{
		personGO = GameObject.Instantiate(personPrefab);
		personGO.SetActive(false);
		personGO.name = "Person";
		PersonBehavior p = personGO.GetComponent<PersonBehavior>();
		p.SetSceneController(this);
		//newPerson.SetOnFloor(hit.position);
		p.SetID(personIDcounter);
		//newPerson.initSection = s_;
		personIDcounter++;
		return p;
	}
	
	
	
	public void BakePeople()
	{
		peopleBaked = true;
		
		people.Sort((a, b) => a.type.CompareTo(b.type));
		
		var distancesToCPs = new List<KeyValuePair<Section, float>>();
		
		foreach(PersonBehavior p in people)
		{
			foreach(Section cp in map.cpSections)
			{
				distancesToCPs.Add(new KeyValuePair<Section, float>(cp, DistanceToCP(p, cp)));
			}
			distancesToCPs.Sort((a, b) => (a.Value.CompareTo(b.Value)));
			
			foreach(var pair in distancesToCPs)
			{
				if(!pair.Key.IsFull())
				{
					pair.Key.AddPerson(p);
					p.sectionsPath.Add(pair.Key);
					break;
				}
			}
			
			distancesToCPs = new List<KeyValuePair<Section, float>>();
		}
	}
	
	public Door GetClosestDoor(PersonBehavior p_, Section cp_)
	{
		var distancesToDoors = new List<KeyValuePair<Door, float>>();
		
		foreach(Door d in cp_.doors)
		{
			distancesToDoors.Add(new KeyValuePair<Door, float>(d, DistanceToDoor(p_, d)));
		}
		distancesToDoors.Sort((a, b) => (a.Value.CompareTo(b.Value)));
		
		return distancesToDoors[0].Key;
	}
	
	private float DistanceToCP(PersonBehavior p, Section cp)
	{
		if(cp.doors.Count == 0) return Mathf.Infinity;
		
		NavMeshHit hit;
		
		NavMeshAgent agent = p.GetComponent<NavMeshAgent>();
		NavMeshPath path = new NavMeshPath();
		float distance;
		
		if (NavMesh.SamplePosition(cp.doors[0].transform.position, out hit, 2.0f, NavMesh.AllAreas)) 
		{
			agent.CalculatePath(hit.position, path);
			agent.SetPath(path);
			agent.isStopped = true;
			
			if(agent.pathStatus == NavMeshPathStatus.PathComplete)
			{
				distance = map.PathDistance(agent);
				return distance;
			}
		}
		
		// Cannot reach the goal
		return Mathf.Infinity;
	}
	
	private float DistanceToDoor(PersonBehavior p, Door d)
	{
		NavMeshHit hit;
		
		NavMeshAgent agent = p.GetComponent<NavMeshAgent>();
		NavMeshPath path = new NavMeshPath();
		float distance;
		
		if (NavMesh.SamplePosition(d.transform.position, out hit, 2.0f, NavMesh.AllAreas)) 
		{
			agent.CalculatePath(hit.position, path);
			agent.SetPath(path);
			agent.isStopped = true;
			
			if(agent.pathStatus == NavMeshPathStatus.PathComplete)
			{
				distance = map.PathDistance(agent);
				return distance;
			}
		}
		
		// Cannot reach the goal
		return Mathf.Infinity;
	}
	
	public void Play()
	{
		if(peopleBaked)
		{
			if(!people[0].GetStartedMovement())
			{
				CreatePathsWaypoints();
				foreach(PersonBehavior p in people)
				{
					p.SetStartedMovement(true);
					p.ToggleMovement();
				}
			}
			else
			{
				foreach(PersonBehavior p in people)
				{
					p.ToggleMovement();
				}
			}
			
		} else Print("gotta bake ppl");
		
	}
	
	public void Pause()
	{
		if(peopleBaked && people[0].GetStartedMovement())
		{
			foreach(PersonBehavior p in people)
			{
				p.ToggleMovement();
			}
		}
	}
	
	
	
	
	public void SaveData()
	{
		List<TileData> tileData = new List<TileData>();
		for(int i=0; i<map.width; i++)
		{
			for(int j=0; j<map.height; j++)
			{
				tileData.Add(new TileData(map.tiles[i][j]));
			}
		}
		
		List<PersonData> personData = new List<PersonData>();
		foreach(PersonBehavior p in people)
		{
			personData.Add(new PersonData(p));
		}
		
		dataController.mapData = new MapData(tileData, map.width, map.height, personData);
		dataController.SaveData();
	}
	
	public void LoadMap()
	{
		map.alreadyBaked = false;
		dataController.LoadMap();
	}
	
	public void LoadPeople()
	{
		Print("Not avaiable");
		/*
		if(map.alreadyBaked)
		{
			dataController.LoadPeople();
		}
		else Print("Map has not been baked");*/
	}
	
	/*
	public void SetPeopleFromData(List<PersonData> PD)
	{
		foreach(PersonData pd in PD)
		{
			personGO = GameObject.Instantiate(personPrefab);
			PersonBehavior newPerson;
			NavMeshHit hit;
			
			// Place the person in the NavMesh
			if (NavMesh.SamplePosition(pd.initPos, out hit, 2.0f, NavMesh.AllAreas)) {
				newPerson = personGO.GetComponent<PersonBehavior>();
				newPerson.GetComponent<NavMeshAgent>().Warp(hit.position);
				people.Add(newPerson);
			} 
		}
	}
	*/
	
	public void DeleteAll()
	{
		DeleteAllPersons();
		
		map.DeleteAll();
		foreach(Section s in map.sections)
		{
			Destroy(s.gameObject);
		}
		map.sections.Clear();
		
		for(int i=0; i<map.width; i++)
		{
			for(int j=0; j<map.height; j++)
			{
				Destroy(map.tiles[i][j].gameObject);
			}
		}
		map.tiles.Clear();
	}
	
	
	#region GETTERS & SETTERS
	
	public Camera GetCam(){ return cam;}
	public Map GetMap(){ return map;}
	public Modes GetMode(){return mode;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" CONTROLLER:  "+msg);}
}
