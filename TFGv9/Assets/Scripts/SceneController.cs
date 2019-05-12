using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;


public class SceneController : MonoBehaviour
{
	public GameObject mapPrefab, graphPrefab, peoplePrefab;
	[SerializeField] private string filename = "test";
	
	// Building's dimensions
	private int width = 25, height = 16; // Number of tiles
	private float size = 1f; // Size of each tile
	
	private List<ISceneElement> sceneElements;
	private enum SceneElementEnum {map, graph, people}
	private SceneElementEnum currentSceneElement;
	private ISceneElement currentEC;
	private int totalModes = 3;

	private Map map;
	private Graph graph;
	private People people;
	private SceneInfo sceneInfo;

	private float timer;
	private bool simulationStarted;

	private Camera cam;
	private Vector3 originalCameraPosition, isoCameraPosition;
	private Quaternion originalCameraRotation, isoCameraRotation;
	private bool isoCam = false;

	private IAlgorithm algorithm;

	//private TextInfoController textInfoController;

	void Awake()
	{
		cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		originalCameraPosition = cam.transform.position; 
		originalCameraRotation = cam.transform.rotation;
		//textInfoController = GameObject.Find("TextInfoController").GetComponent<TextInfoController>();
	}

	void Start()
	{
		// Create the building/map passing the specified dimensions
		map = GameObject.Instantiate(mapPrefab, this.transform).GetComponent<Map>();
		map.Setup(this, width, height, size);

		graph = GameObject.Instantiate(graphPrefab, this.transform).GetComponent<Graph>();
		graph.Setup(this);

		people = GameObject.Instantiate(peoplePrefab, this.transform).GetComponent<People>();
		people.Setup(this);

		sceneInfo = new SceneInfo();
		algorithm = new DummyAlgorithm();

		sceneElements = new List<ISceneElement>();
		sceneElements.Add(map);
		sceneElements.Add(graph);
		sceneElements.Add(people);

		currentSceneElement = SceneElementEnum.map;
		currentEC = sceneElements[(int)currentSceneElement];

		isoCameraPosition = new Vector3 (0,14,0);
		isoCameraRotation = Quaternion.Euler(new Vector3(45,45,0));

	}

	void Update()
    {
		// Capture input events
		ManageMouse();
		ManageKeyboard();

		if(simulationStarted) timer += Time.deltaTime;
    }

	public void ManageMouse()
	{
		// Name mouse events and pass them
		
		if(Input.GetMouseButtonDown(0)) currentEC.MouseEvent(Utils.MouseInputEvents.left_down);
		else if(Input.GetMouseButtonUp(0)) currentEC.MouseEvent(Utils.MouseInputEvents.left_up);
		else if(Input.GetMouseButton(0)) currentEC.MouseEvent(Utils.MouseInputEvents.left_held);
		else if(Input.GetMouseButtonUp(1)) currentEC.MouseEvent(Utils.MouseInputEvents.right_up);
	}

	public void ManageKeyboard()
	{
		// Change mode
		if (Input.GetKeyUp(KeyCode.P))
		{
			currentSceneElement = (SceneElementEnum)(((int) currentSceneElement+1)%totalModes);
			currentEC = sceneElements[(int)currentSceneElement];
			Debug.Log("Now "+currentSceneElement);
		}

		// Change camera view
		else if(Input.GetKeyUp(KeyCode.Q))
		{
			isoCam = !isoCam;
			if(isoCam){ cam.transform.position = isoCameraPosition; cam.transform.rotation = isoCameraRotation;}
			else { cam.transform.position = originalCameraPosition; cam.transform.rotation = originalCameraRotation;}
		}

		else if(Input.GetKeyUp(KeyCode.S))
		{
			Save();
		}
		else if(Input.GetKeyUp(KeyCode.L))
		{
			Load();
		}

		currentEC.KeyboardEvent();
	}

	public void RunAlgorithm()
	{
		List<Path> paths = algorithm.FindPaths(graph, people.GetPeople());

		foreach(Path p in paths)
		{
			p.person.SetNodesPath(p.path);
			p.path[p.path.Count-1].GetData().AddPerson(p.person);
		}
	}

	public void SimulationFinished()
	{
		simulationStarted = false;
		
		sceneInfo.totalTime = timer;
		sceneInfo.maximumCPTime = people.GetLastArrivedPerson().GetTimer();
		sceneInfo.timesPerPerson = people.GetTimesInfo();

		// Info
		DataController.SetInfoData(sceneInfo);
		DataController.SaveInfo();
	}

	public void DeleteAll()
	{
		map.DeleteAll();
		people.DeleteAll();
		graph.DeleteAll();
	}

	public void Load()
	{
		DeleteAll();

		DataController.SetProjectName(filename);

		DataController.LoadMap();
		DataController.LoadGraph();
		DataController.LoadPeople();
		DataController.LoadPaths();
		DataController.LoadInfo();
		
		map.Load();
		graph.Load();
		people.Load();
		people.LoadPathsFromData(DataController.GetPathsData());
		sceneInfo = DataController.GetInfoData();
		timer = 0;
	}

	public void Save()
	{
		DataController.SetProjectName(filename);

		// Topology
		map.Save();
		DataController.SaveMap();

		// Graph
		graph.Save();
		DataController.SaveGraph();

		// People and paths
		// Maybe the algorithm should create the file when it's run, 
		// instead of creating the file on save
		people.Save();
		DataController.SavePeople();
		DataController.SavePaths();

		
	}


	public void DoAction(int elem_)
	{
		switch(elem_)
		{
			case 0: currentEC = sceneElements[0]; currentEC.DoAction(Utils.Actions.bake); break; // Create sections
			case 1: currentEC = sceneElements[1]; currentEC.DoAction(Utils.Actions.bake); break; // Create graph
			case 2: currentEC = sceneElements[2]; currentEC.DoAction(Utils.Actions.bake); break; // Create paths
			case 3: // Change camera
				isoCam = !isoCam;
				if(isoCam){ cam.transform.position = isoCameraPosition; cam.transform.rotation = isoCameraRotation;}
				else { cam.transform.position = originalCameraPosition; cam.transform.rotation = originalCameraRotation;}
				break; 
			case 4: Save(); break; // Save
			case 5: currentEC = sceneElements[2]; Load(); break; // Load
			case 6: currentEC = sceneElements[2]; currentEC.DoAction(Utils.Actions.run); simulationStarted = true; break; // Run
		}
	}

	// public TMP_Text AddInfoText(string t_, Vector3 pos_)
	// {
	// 	return textInfoController.AddInfoText(t_, pos_);
	// }

	public Map GetMap(){return map;}
	public Graph GetGraph(){return graph;}
	public SceneInfo GetSceneInfo(){return sceneInfo;}
	public float GetTimer(){return timer;}





	#region UIMETHODS

	public void ScenarioNew(){ Debug.Log("New scenario");}
	public void ScenarioLoad(){ Debug.Log("Load scenario");currentEC = sceneElements[2]; Load();}
	public void ScenarioSave(){ Debug.Log("Save scenario");Save(); }
	public void ScenarioSaveAs(){ Debug.Log("Save scenario as");}

	public void TopologyCreate(){ Debug.Log("Topology create mode");}
	public void TopologyEdit(){ Debug.Log("Topology edit mode");}
	public void TopologyClearAll(){ Debug.Log("Clear all topology");}
	public void TopologyClearDoors(){ Debug.Log("Clear all doors");}
	public void TopologyCreateSections(){ Debug.Log("Create sections");currentEC = sceneElements[0]; currentEC.DoAction(Utils.Actions.bake);}
	public void TopologyFinish(){ Debug.Log("Finish topology");}

	public void GraphCreate(){ Debug.Log("Graph create mode");}
	public void GraphEdit(){ Debug.Log("Graph edit mode");}
	public void GraphMove(){ Debug.Log("Graph move mode");}
	public void GraphAuto(){ Debug.Log("Create graph automatically");currentEC = sceneElements[1]; currentEC.DoAction(Utils.Actions.bake); }
	public void GraphClearAll(){ Debug.Log("Clear all graph");}
	public void GraphClearEdges(){ Debug.Log("Clear all edges");}
	public void GraphFinish(){ Debug.Log("Finish graph");currentEC = sceneElements[2];}

	public void PeopleCreate(){ Debug.Log("People create mode");}
	public void PeopleEdit(){ Debug.Log("People edit mode");}
	public void PeopleMove(){ Debug.Log("People move mode");}
	public void PeopleClearAll(){ Debug.Log("Clear all people");}
	public void PeopleFinish(){ Debug.Log("Finish people");}

	public void PathsEdit(){ Debug.Log("Paths edit mode");}
	public void PathsAuto(){ Debug.Log("Create paths automatically");currentEC = sceneElements[2]; currentEC.DoAction(Utils.Actions.bake);}
	public void PathsFinish(){ Debug.Log("Finish paths");}

	public void CamNavigate(){ Debug.Log("Navigate with cam");}
	public void CamChangeView(){ Debug.Log("Change cam view");isoCam = !isoCam;
				if(isoCam){ cam.transform.position = isoCameraPosition; cam.transform.rotation = isoCameraRotation;}
				else { cam.transform.position = originalCameraPosition; cam.transform.rotation = originalCameraRotation;}
				}
	public void CamShowTopology(){ Debug.Log("Toggle show topology");}
	public void CamShowGraph(){ Debug.Log("Toggle show graph");}
	public void CamShowPeople(){ Debug.Log("Toggle show people");}
	public void CamShowLabels(){ Debug.Log("Toggle show labels");}

	public void Play(){ Debug.Log("Play");currentEC = sceneElements[2]; currentEC.DoAction(Utils.Actions.run); simulationStarted = true;}
	public void Pause(){ Debug.Log("Pause");}
	public void Stop(){ Debug.Log("Stop");}

	#endregion







}


























/*
public class SceneController : MonoBehaviour
{
	public GameObject floor, personPrefab, tilePrefab, sectionPrefab;
	private GameObject personGO;
	public TMP_Text hoverText;
	public MenuController menu;
	public Sprite circleSprite;
	public RectTransform graphContainer;
	
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
		
		width = 20; height = 15;
		map = new Map(this, width, height, floor, tilePrefab, sectionPrefab);
		
		mode = Modes.BUILD;
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
				case Modes.PEOPLE: /*GenerateRandomSectionsPath(); break;
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
		
		map.PrintGraph();

		DrawGraph();
		
		ChangeMode(1);
	}
	

	private void DrawGraph()
	{
		GameObject gameObject = new GameObject("nodeGO", typeof(Image));
		gameObject.transform.parent= graphContainer;
		gameObject.GetComponent<Image>().sprite = circleSprite;
		RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
		Vector3 newVector = map.FromMap(map.GetGraph().GetNodes()[0].GetData().GetPos());
		Debug.Log(newVector);
		rectTransform.anchoredPosition = new Vector2(newVector.x, newVector.z);
		rectTransform.sizeDelta = new Vector2(20,20);
		rectTransform.anchorMin = new Vector2(0, 0);
		rectTransform.anchorMax = new Vector2(0, 0);
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
	
	
	/*
	
	void OnDrawGizmos()
    {
        foreach(Node n in map.GetGraph().GetNodes())
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(map.FromMap(n.GetData().GetPos())+new Vector3(0,5f,0), 0.6f);
		}
		
		foreach(Edge e in map.GetGraph().GetEdges())
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(map.FromMap(e.GetNodes()[0].GetData().GetPos())+new Vector3(0,5f,0), map.FromMap(e.GetNodes()[1].GetData().GetPos())+new Vector3(0,5f,0));
		}
    }
	*/
	
	
	
	
	/*
	public Door GetClosestDoor(PersonBehavior p_, Section cp_)
	{
		var distancesToDoors = new List<KeyValuePair<Door, float>>();
		
		foreach(Door d in cp_.GetDoors())
		{
			distancesToDoors.Add(new KeyValuePair<Door, float>(d, DistanceToDoor(p_, d)));
		}
		distancesToDoors.Sort((a, b) => (a.Value.CompareTo(b.Value)));
		
		return distancesToDoors[0].Key;
	}
	
	private float DistanceToCP(PersonBehavior p, Section cp)
	{
		if(cp.GetDoors().Count == 0) return Mathf.Infinity;
		
		NavMeshHit hit;
		
		NavMeshAgent agent = p.GetComponent<NavMeshAgent>();
		NavMeshPath path = new NavMeshPath();
		float distance;
		
		if (NavMesh.SamplePosition(cp.GetDoors()[0].transform.position, out hit, 2.0f, NavMesh.AllAreas)) 
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
	//}
	
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
	
// 	public void DeleteAll()
// 	{
// 		DeleteAllPersons();
		
// 		map.DeleteAll();
// 		foreach(Section s in map.sections)
// 		{
// 			Destroy(s.gameObject);
// 		}
// 		map.sections.Clear();
		
// 		for(int i=0; i<map.width; i++)
// 		{
// 			for(int j=0; j<map.height; j++)
// 			{
// 				Destroy(map.tiles[i][j].gameObject);
// 			}
// 		}
// 		map.tiles.Clear();
// 	}
	
	
// 	#region GETTERS & SETTERS
	
// 	public Camera GetCam(){ return cam;}
// 	public Map GetMap(){ return map;}
// 	public Modes GetMode(){return mode;}
	
// 	#endregion
	
// 	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" CONTROLLER:  "+msg);}
// }
