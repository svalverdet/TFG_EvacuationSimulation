using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

public class SceneController : MonoBehaviour
{
	public GameObject personPrefab, tilePrefab, floor, sectionPrefab;
	public TMP_Text timerText;
	
	private DataController dataController;
	private MapData mapData;
	
	private GameObject personGO;
	private List<PersonBehavior> people;
	
	private Map map;
	private Camera cam;
	
	private enum Modes{BUILD, PEOPLE}
	private Modes mode = Modes.PEOPLE;
	
	private float timer;
	private bool timerStarted;
	public int totalPeople;
	
	// Forma parte del algoritmo de backtracking (el numero 5)
	float globalCost = Mathf.Infinity;
	List<KeyValuePair<Section, List<PersonBehavior>>> solution;
	int loops = 10000;
	int timesBranchesCut = 0;
	
    void Start()
    {
		cam = GameObject.Find("Main Camera").GetComponent<Camera>();
		
		people = new List<PersonBehavior>();
		map = new Map(this, 28, 18, tilePrefab, sectionPrefab, floor);
		
		
		dataController = new DataController(this);
		
    }

	
    void Update()
    {
		if(timerStarted)
		{
			timer+=Time.deltaTime;
			timerText.text = "Time taken: "+timer+" s";
			
			if(totalPeople<1)
			{
				timerStarted = false;
			}
		}
		
		// Right click UP
		if(Input.GetMouseButtonUp(1))
		{
			switch(mode)
			{
				case Modes.PEOPLE:
					// Create person if clicked on Tile
					SetPerson();
				break;
				
				case Modes.BUILD:
					// Turn sections to CPs
					map.CreateCP();
				break;
				
				default: break;
			}
		}
		
		// Left click DOWN
		else if(Input.GetMouseButtonDown(0))
		{
			map.initClickedPos = map.MouseOnGridPosition();			
		}
		
		// Left click UP
		else if (Input.GetMouseButtonUp(0) && mode == Modes.BUILD)
        {
			map.endClickedPos = map.MouseOnGridPosition();
            map.CreateRoom();
        }
		
		
		
		// Delete people
		else if (Input.GetKeyUp(KeyCode.T))
        {
			foreach(PersonBehavior p in people)
			{
				GameObject.Destroy(p.gameObject);
			}
			people = new List<PersonBehavior>();
        }
		
		// Finish -> start
		else if (Input.GetKeyUp(KeyCode.F))
        {
			//CPAllocation();
			StartSimulation();
        }
		
		else if(Input.GetKeyUp(KeyCode.G))
		{
			foreach(PersonBehavior p in people)
			{
				p.StartMoving();
			}
		}
		
		// Restart
		else if (Input.GetKeyUp(KeyCode.R))
        {
			string str = "";
			foreach(Section cp in map.cpSections)
			{
				str+="CP"+cp.GetSectionID()+" ("+cp.GetMaxCapacity()+"), ";
			}
			Print(str);
			/*
			foreach(PersonBehavior p in people)
			{
				p.ReStart();
			}
			*/
        }
		
		
		// Save data
		else if (Input.GetKeyUp(KeyCode.S)){SaveData();}
		// Load data
		else if (Input.GetKeyUp(KeyCode.L)){LoadData();}
		// Build NavMesh
		else if (Input.GetKeyUp(KeyCode.B)){map.BuildNavMesh();}
		// SetLoadedPeople
		else if (Input.GetKeyUp(KeyCode.Q)){SetPeopleFromData(dataController.mapData.mapPeople);}
		
		// Change mode
		else if (Input.GetKeyUp(KeyCode.P))
		{
			mode++;
			if((int)mode>=System.Enum.GetNames(typeof(Modes)).Length) mode=0;
			Print("Ahora estamos en modo "+mode);
		}
    }
	
	private bool SetPerson()
	{
		if(map.GetMapElement().GetComponent<Tile>() != null)
		{
			map.BuildNavMesh();
			
			personGO = GameObject.Instantiate(personPrefab);
			PersonBehavior newPerson;
			NavMeshHit hit;
			
			// Place the person in the NavMesh
			if (NavMesh.SamplePosition(map.actualClickedPos, out hit, 2.0f, NavMesh.AllAreas)) {
				newPerson = personGO.GetComponent<PersonBehavior>();
				newPerson.GetComponent<NavMeshAgent>().Warp(hit.position);
				people.Add(newPerson);
				return true;
			} 
		}
		return false;		
	}
	
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
	
	
	// NEW NEW NEW NEW ALGORITHM BACKTRACKING
	
	public void CPAllocation()
	{
		List<PersonBehavior> P = new List<PersonBehavior>(people);
		List<KeyValuePair<Section, List<PersonBehavior>>> CP = new List<KeyValuePair<Section, List<PersonBehavior>>>();
		
		// Inicializar la lista
		foreach(Section cp_ in map.cpSections)
		{
			CP.Add(new KeyValuePair<Section, List<PersonBehavior>>(cp_, new List<PersonBehavior>()));
		}
		float maxLocalCost = -1.0f;
		
		ComputeAlgorithm(P, CP, maxLocalCost);
		
		// Asigno las metas a las personas
		foreach(KeyValuePair<Section, List<PersonBehavior>> cp in solution)
		{
			foreach(PersonBehavior p in cp.Value)
			{
				int tileIndex = cp.Key.GetTiles().Count*3/5;
				Vector3 aux = cp.Key.GetTiles()[tileIndex].gameObject.transform.position;
				p.SetGoal(cp.Key.GetPos().position, cp.Key.GetSectionID(), cp.Key);
			}
		}
		
		Print(timesBranchesCut+" veces se han cortado cosas");
	}
	
	public void ComputeAlgorithm(List<PersonBehavior> P_, List<KeyValuePair<Section, List<PersonBehavior>>> CP_, float maxLocalCost_)
	{
		loops--;
		if(loops%500==0)Print(loops+"");
		List<PersonBehavior> P = new List<PersonBehavior>(P_);
		List<KeyValuePair<Section, List<PersonBehavior>>> CP = new List<KeyValuePair<Section, List<PersonBehavior>>>();
		List<KeyValuePair<Section, List<PersonBehavior>>> originalCP = new List<KeyValuePair<Section, List<PersonBehavior>>>();
		PersonBehavior actualPerson = P[0];
		P.RemoveAt(0);
		
		foreach(KeyValuePair<Section, List<PersonBehavior>> cpy in CP_)
		{
			CP.Add(new KeyValuePair<Section, List<PersonBehavior>>(cpy.Key, new List<PersonBehavior>(cpy.Value)));
			originalCP.Add(new KeyValuePair<Section, List<PersonBehavior>>(cpy.Key, new List<PersonBehavior>(cpy.Value)));
		}
		float maxLocalCost = maxLocalCost_;
		float cost;
		
		
		for(int i=0; i<originalCP.Count; i++)
		{
			// Si la lista asociada no es mayor o igual que la capacidad de ese cp
			if(CP[i].Value.Count < CP[i].Key.GetMaxCapacity())
			{
				cost = DistanceToCP(actualPerson, CP[i].Key);
				if(cost >= globalCost) 
				{
					timesBranchesCut++;
					return;
				}
				else
				{
					CP[i].Value.Add(actualPerson);
					if(cost > maxLocalCost) maxLocalCost = cost;
					
					if(P.Count>0 && loops>0)
					{
						ComputeAlgorithm(P, CP, maxLocalCost);
					}
					else
					{
						if(loops>0)
						{
							globalCost = maxLocalCost;
							solution = new List<KeyValuePair<Section, List<PersonBehavior>>>(CP);
						} else 
						{
							Print("EL ALGORITMO NO PUEDE CON TANTO");
						}
					}
				}
				
				for(int j=0; j<originalCP.Count; j++)
				{
					CP[j] = new KeyValuePair<Section, List<PersonBehavior>>(originalCP[j].Key, new List<PersonBehavior>(originalCP[j].Value));
				}
				
			}else timesBranchesCut++;
		}
	}
	
	public void PrintCPs(List<KeyValuePair<Section, List<PersonBehavior>>> CP_)
	{
		foreach(KeyValuePair<Section, List<PersonBehavior>> cp_ in CP_)
		{
			Print("RESULTADOS PARA LA SECCIÓN CP"+cp_.Key.GetSectionID()+": ");
			string str = "{";
			foreach(PersonBehavior p in cp_.Value)
			{
				str += p.firstName+", ";
			}
			str += "}";
			Print(str);
		}
	}
	
	
	
	/* NEW NEW NEW ALGORITHM ASIGNACIÓN POR COMPETENCIA
	
	private void CPAllocation()
	{
		// Primero relleno la info
		foreach(PersonBehavior p in people)
		{
			foreach(Section cp in map.cpSections)
			{
				p.possibleCPs.Add(new KeyValuePair<Section, float>(cp, DistanceToCP(p, cp)));
			}
			p.possibleCPs.Sort((a, b) => (a.Value.CompareTo(b.Value)));
		}
		
		// Imprimo a ver qué tal voy
		PrintPossibleCPsForPeople();
		
		// AQUÍ EMPIEZA EL BUCLE RECURSIVO
		ComputeAlgorithm(people, 0);
	}
	
	public void ComputeAlgorithm(List<PersonBehavior> peopleList, int repNumber)
	{
		Print(repNumber+" REPS");
		bool loop = false;
		List<Section> repSections = new List<Section>();
		
		// Asigno las personas
		foreach(PersonBehavior p in peopleList)
		{
			if(p.possibleCPs.Count>0)
			{
				Section cp = p.possibleCPs[0].Key;
				loop = cp.AddPerson(p);
				if(!repSections.Contains(cp) && cp.IsExceeded()) repSections.Add(cp);
				p.possibleCPs.RemoveAt(0);
			}
			else 
			{
				Print("ERRROOOOOOOOOOOOOR");
			}
		}
		Print("¿hay que repetir?: "+loop+". Hay "+repSections.Count+" CPs que revisar");
		
		if(repNumber>15)
		{
			Print("EL ALGORITMO NO PUEDE CON TANTO");
			Print("EL ALGORITMO NO PUEDE CON TANTO");
			Print("EL ALGORITMO NO PUEDE CON TANTO");
			Print("EL ALGORITMO NO PUEDE CON TANTO");
			Print("EL ALGORITMO NO PUEDE CON TANTO");
		}
		else if(!loop)
		{
			Print("GOOD TO GO. Se ha repartido así:");
			PrintCPPeople();
			
			// Asigno las metas a las personas
			foreach(Section se_ in map.cpSections)
			{
				foreach(PersonBehavior pe_ in se_.GetPeopleAssigned())
				{
					pe_.SetGoal(se_.GetPos().position, se_.GetSectionID(), se_);
				}
			}
		}
		else if(loop)
		{
			List<PersonBehavior> peopleToRearrange = new List<PersonBehavior>();
		
			//Me hago una lista con persona-distancias
			List<KeyValuePair<PersonBehavior, float>> personDistance = new List<KeyValuePair<PersonBehavior, float>>();
			List<KeyValuePair<PersonBehavior, float>> personDistanceAUX;
			
			foreach(Section repCP in repSections)
			{
				// Cojo personas de ese cp
				foreach(PersonBehavior repP in repCP.GetPeopleAssigned())
				{
					float dist = repP.possibleCPs[0].Value;
					personDistance.Add(new KeyValuePair<PersonBehavior, float>(repP,dist));
				}
				
				// Las ordeno
				personDistance.Sort((a, b) => (a.Value.CompareTo(b.Value)));
				Print("La lista decisiva del CP"+repCP.GetSectionID()+" es:");
				foreach(KeyValuePair<PersonBehavior, float> test in personDistance)
				{
					Print(test.Key.firstName+" -> "+test.Value);
				}
				
				// Quito estos elementos del repCP people assigned
				int diff = repCP.GetExceed();
				Print("Sobran "+diff+" personas porque hay "+repCP.GetPeopleAssigned().Count+" y caben "+repCP.GetMaxCapacity());
				personDistanceAUX = personDistance.GetRange(0,diff);
				int contTest = 0;
				foreach(KeyValuePair<PersonBehavior, float> pToRemove in personDistanceAUX)
				{
					contTest++;
					repCP.RemovePerson(pToRemove.Key);
					peopleToRearrange.Add(pToRemove.Key);
				}
				Print("Se han eliminado "+contTest+" personas y ahora hay"+repCP.GetPeopleAssigned().Count+" y caben "+repCP.GetMaxCapacity());
			}
			
			ComputeAlgorithm(peopleToRearrange, repNumber+=1);
		}
		
	}
	
	public void PrintPossibleCPsForPeople()
	{
		foreach(PersonBehavior p1 in people)
		{
			Print("Distances for "+p1.firstName+":");
			
			foreach(KeyValuePair<Section, float> d2 in p1.possibleCPs)
			{
				Print(d2.Value+" metres for CP "+d2.Key.GetSectionID());
			}
		}
	}
	
	public void PrintCPPeople()
	{
		string st = "";
		foreach(Section s3 in map.cpSections)
		{
			st+="("+s3.GetPeopleAssigned().Count+"/"+s3.GetMaxCapacity()+")-> ";
			foreach(PersonBehavior p3 in s3.GetPeopleAssigned())
			{
				st+=p3.firstName+", ";
			}
		}
		Print(st);
	}
	
	*/
	
	/* NEW NEW ALGORITHM WORST FIRST
	
	private void CPAllocation()
	{
		var allDistances = new List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>>();
		var aux = new KeyValuePair<PersonBehavior, float>();
		
		// Asigno a todos los CPs las distancias a las personas
		foreach(Section cp in map.cpSections)
		{
			foreach(PersonBehavior p in people)
			{
				aux = new KeyValuePair<PersonBehavior, float>(p, DistanceToCP(p, cp));
				allDistances.Add(new KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>(cp, aux));
			}
		}
		
		// Ya tengo todas las distancias en la lista así que las ordeno
		
		allDistances.Sort((a, b) => (a.Value.Value.CompareTo(b.Value.Value)));
		
		// Voy a imprimir por pantalla a ver qué tal voy
		float di;
		PersonBehavior pe;
		Section se;
		
		Print("Tenemos un total de "+allDistances.Count+" datos en la lista.");
		foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> k in allDistances)
		{
			di = k.Value.Value;
			pe = k.Value.Key;
			se = k.Key;
			
			Print("La persona "+pe.firstName+" está a "+di+" metros del CP "+se.GetSectionID());
		}
		
		// wtf está bien aunque tarda lo suyo
		
		
		List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>> allDistancesAUX;
		int maxLoop = 100;
		while(allDistances.Count>0 && maxLoop>0)
		{
			maxLoop--;
			allDistancesAUX = new List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>>(allDistances);
			
			
			// Selecciono aquel en peor situación
			se = allDistances[allDistances.Count-1].Key;
			pe = allDistances[allDistances.Count-1].Value.Key;
			
			//Busco el mejor elemento para esa persona
			KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> bestPath;
			bestPath = GetBestPath(allDistances, pe);
			se = bestPath.Key;
			pe = bestPath.Value.Key;
			
			// Asigno ese valor para esa persona
			int tileIndex = se.GetTiles().Count*3/5;
			Vector3 aux2 = se.GetTiles()[tileIndex].gameObject.transform.position;
			pe.SetGoal(aux2);
			se.AddPerson(pe);
			Print(pe.firstName+" has been added to CP "+se.GetSectionID());
			
			int counter = 0;
			// Luego quito a la persona
			foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> ke in allDistancesAUX)
			{ 
				if(ke.Value.Key == pe) { allDistances.Remove(ke); counter++;}
			}
			
			
			// Luego miro si está lleno
			if(se.IsFull())
			{
				allDistancesAUX = new List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>>(allDistances);
				
				foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> k in allDistancesAUX)
				{
					if(k.Key == se) { allDistances.Remove(k); counter++;}
				}
			}
			
			Print(counter+" CPs have been removed");
			
		}
		
	}
	
	
	private KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> GetBestPath(List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>> allDistances, PersonBehavior pe)
	{
		foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> k in allDistances)
		{
			if(k.Value.Key == pe) { return k;}
		}
		
		// MAL FEO CACA
		Print("WTFFFFFFFFFFFFFFFFF");
		return allDistances[0];
	}
	
	*/
	
	/* NEW ALGORITHM BEST FIRST
	
	private void CPAllocation()
	{
		var allDistances = new List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>>();
		var aux = new KeyValuePair<PersonBehavior, float>();
		
		// Asigno a todos los CPs las distancias a las personas
		foreach(Section cp in map.cpSections)
		{
			foreach(PersonBehavior p in people)
			{
				aux = new KeyValuePair<PersonBehavior, float>(p, DistanceToCP(p, cp));
				allDistances.Add(new KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>(cp, aux));
			}
		}
		
		// Ya tengo todas las distancias en la lista así que las ordeno
		
		allDistances.Sort((a, b) => (a.Value.Value.CompareTo(b.Value.Value)));
		
		// Voy a imprimir por pantalla a ver qué tal voy
		float di;
		PersonBehavior pe;
		Section se;
		
		Print("Tenemos un total de "+allDistances.Count+" datos en la lista.");
		foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> k in allDistances)
		{
			di = k.Value.Value;
			pe = k.Value.Key;
			se = k.Key;
			
			Print("La persona "+pe.firstName+" está a "+di+" metros del CP "+se.GetSectionID());
		}
		
		// wtf está bien aunque tarda lo suyo
		
		
		List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>> allDistancesAUX;
		int maxLoop = 50;
		while(allDistances.Count>0 && maxLoop>0)
		{
			maxLoop--;
			allDistancesAUX = new List<KeyValuePair<Section, KeyValuePair<PersonBehavior, float>>>(allDistances);
			
			
			// Primero asigno
			se = allDistances[0].Key;
			pe = allDistances[0].Value.Key;
			
			int tileIndex = se.GetTiles().Count*3/5;
			Vector3 aux2 = se.GetTiles()[tileIndex].gameObject.transform.position;
			pe.SetGoal(aux2);
			se.AddPerson(pe);
			Print(pe.firstName+" has been added to CP "+se.GetSectionID());
			
			int counter = 0;
			// Luego quito a la persona
			foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> ke in allDistancesAUX)
			{ 
				if(ke.Value.Key == pe) { allDistances.Remove(ke); counter++;}
			}
			
			
			// Luego miro si está lleno
			if(se.IsFull())
			{
				foreach(KeyValuePair<Section, KeyValuePair<PersonBehavior, float>> k in allDistancesAUX)
				{
					if(k.Key == se) { allDistances.Remove(k); counter++;}
				}
			}
			
			Print(counter+" CPs have been removed");
			
		}
		
	}
	
	*/
	
	
	/* old algorithm RANDOM
	// OLD ALGORITHM 
	
	private Dictionary<PersonBehavior, Section> CPAllocation()
	{
		Dictionary<PersonBehavior, Section> L = new Dictionary<PersonBehavior, Section>();
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
					
					int tileIndex = pair.Key.GetTiles().Count*3/5;
					Vector3 aux = pair.Key.GetTiles()[tileIndex].gameObject.transform.position;
					p.SetGoal(aux);
					
					L.Add(p, pair.Key);
					break;
				}
			}
			
			distancesToCPs = new List<KeyValuePair<Section, float>>();
		}
		
		return L;
	}
	*/
	
	private float DistanceToCP(PersonBehavior p, Section cp)
	{
		NavMeshHit hit;
		
		NavMeshAgent agent = p.GetComponent<NavMeshAgent>();
		NavMeshPath path = new NavMeshPath();
		float distance;
		
		if (NavMesh.SamplePosition(cp.GetPos().position, out hit, 2.0f, NavMesh.AllAreas)) 
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
	
	
	public void StartSimulation()
	{
		timerStarted = true;
		totalPeople = people.Count;
		
		CPAllocation();
		
		foreach(PersonBehavior p in people)
		{
			p.StartMoving();
		}
		
	}
	
	public void LoadData()
	{
		dataController.LoadData();
	}
	
	public void SaveData()
	{
		List<TileData> tileData = new List<TileData>();
		for(int ti=0; ti<map.tiles.Count; ti++)
		{
			for(int tj=0; tj<map.tiles[ti].Count; tj++)
			{
				tileData.Add(new TileData(map.tiles[ti][tj]));
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
	
	public void DeleteAll()
	{
		foreach (PersonBehavior p in Object.FindObjectsOfType<PersonBehavior>()) {
             Destroy(p.gameObject);
        }
		people.Clear();
		
		foreach (Tile t in Object.FindObjectsOfType<Tile>()) {
			Destroy(t.gameObject);
        }
		map.DeleteAll();
		
		globalCost = Mathf.Infinity;
		if(solution != null) solution.Clear();
		loops = 10000;
		timesBranchesCut = 0;
		
	}
	
	#region GETTERS & SETTERS
	
	public Camera GetCam(){ return cam;}
	public Map GetMap(){ return map;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" CONTROLLER:  "+msg);}
}
