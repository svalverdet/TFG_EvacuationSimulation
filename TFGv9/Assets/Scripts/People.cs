using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class People : MonoBehaviour, ISceneElement
{
    public GameObject personPrefab;
    [SerializeField] private List<PersonBehavior> people;
    private int nGL, nMI, nFM, nB;

    private SceneController sc;
    private int currentPersonID;

    private bool alreadyBaked = false;

    private int peopleArrivedCounter;
    private float timer;
    private PersonBehavior lastArrivedPerson;

    public void Setup(SceneController sc_)
    {
        sc = sc_;
        people = new List<PersonBehavior>();
    }

    public void MouseEvent(Utils.MouseInputEvents mouseEvent)
	{
        KeyValuePair<Vector3,GameObject> info = Utils.ScreenToWorld();

		switch(mouseEvent)
	 	{
			case Utils.MouseInputEvents.left_up: 
                if(info.Value != null && info.Value.name == "Tile")
                {
                    Tile t = info.Value.GetComponent<Tile>();
                    Section s = t.GetSection();
                    if(s != null)
                    {
                        PlacePerson(s, t.GetPos());
                    }
                }
			break;

            case Utils.MouseInputEvents.right_up:
                if(info.Value != null && info.Value.tag == "person")
                {
                    people.Remove(info.Value.GetComponent<PersonBehavior>());
                    sc.GetSceneInfo().numberOfPeople -= 1;

                    GameObject.Destroy(info.Value);
                }
            break;

			default: break;
		}
	}

	public void KeyboardEvent()
	{
		if(Input.GetKeyUp(KeyCode.B))
		{
			if(!alreadyBaked) Bake();
            else StartMovement();
		}
	}

    public void DoAction(Utils.Actions action_)
	{
		if(action_ == Utils.Actions.bake && !alreadyBaked) Bake();
        else if(action_ == Utils.Actions.run) StartMovement();
	}

    private PersonBehavior PlacePerson(Section s_, Vector3 pos_)
    {
        // Place the person in the NavMesh
        NavMeshHit hit;
        bool validPos = NavMesh.SamplePosition(pos_, out hit, 2.0f, NavMesh.AllAreas);

		if (validPos) {
            PersonBehavior newPerson = GameObject.Instantiate(personPrefab, this.transform).GetComponent<PersonBehavior>();
            newPerson.SetOnFloor(hit.position);
            newPerson.Setup(currentPersonID++, hit.position, s_, sc.GetGraph().GetClosestNode(hit.position, s_));            
            people.Add(newPerson);

            // Add text
            //newPerson.SetText(sc.AddInfoText("person "+newPerson.GetID(), newPerson.GetInitPos()));
            
            sc.GetSceneInfo().numberOfPeople += 1;
            switch(newPerson.GetPersonType()) 
            {
                case 0: nGL++; break; case 1: nMI++; break; case 2: nFM++; break; case 3: nB++; break; default:break;
            }
            sc.GetSceneInfo().RecalculatePercents(nGL, nMI, nFM, nB);

            return newPerson;
		} 
        else return null;
        
    }

    private void Bake()
    {
        sc.RunAlgorithm();
        alreadyBaked = true;
    }

    private void StartMovement()
    {
        peopleArrivedCounter = 0;

        foreach(PersonBehavior p in people)
        {
            List<Vector3> path = new List<Vector3>();
            foreach(Node n in p.GetNodesPath())
            {
                path.Add(n.GetPos());
            }
            p.SetPointsPath(path);

            p.SetDestSection(p.GetNodesPath()[p.GetNodesPath().Count-1].GetData());

            p.SetStartedMovement(true);
        }
    }

    public void PersonArrived(PersonBehavior p_, float timer_)
    {
        peopleArrivedCounter++;
        if(peopleArrivedCounter>=people.Count) 
        {
            timer = timer_;
            Utils.Print("Everyone's safe in "+timer_+"s !!!!!!"); 
            lastArrivedPerson = p_;
            foreach(PersonBehavior p in people) p.StopMovement();
            sc.SimulationFinished();
        }
    }

    public void DeleteAll()
    {
        foreach(PersonBehavior p in people)
        {
            GameObject.Destroy(p.gameObject);
        }
        people.Clear();
        alreadyBaked = false;
    }
    
    public void Save()
    {
		List<PersonData> personDataList = new List<PersonData>();
        List<PathData> pathDataList = new List<PathData>();

        foreach(PersonBehavior p in people)
        {
            personDataList.Add(p.TransformData());
            pathDataList.Add(p.TransformPathData());
        }

		DataController.SetPeopleData(new PeopleData(personDataList));
        DataController.SetPathsData(new PathsData(pathDataList));
    }

    public void Load()
    {
        PeopleData pd_ = DataController.GetPeopleData();
        PersonBehavior newPerson;
        foreach(PersonData p in pd_.people)
        {
            newPerson = PlacePerson(sc.GetMap().FindSectionByID(p.initSectionID), p.initPos);
            newPerson.SetupFromData(sc.GetGraph().FindNodeByID(p.initNodeID), p.ID, p.priority, p.age, p.type, p.speedType, p.firstName);
        }
    }

    public void LoadPathsFromData(PathsData pd_)
    {
        List<Node> nodesPath = new List<Node>();
        PersonBehavior person = null;

        foreach(PathData p in pd_.paths)
        {
            person = FindPersonByID(p.personID);
            if(person != null)
            {
                foreach(int n in p.nodes)
                {
                    nodesPath.Add(sc.GetGraph().FindNodeByID(n));
                }
                person.SetNodesPath(nodesPath);
                nodesPath = new List<Node>();
            }
        }

        alreadyBaked = true;
    }

    public List<PersonTime> GetTimesInfo()
    {
        List<PersonTime> times = new List<PersonTime>();
        foreach(PersonBehavior p in people)
        {
            times.Add(new PersonTime(p.GetID(), p.GetDestSection().GetID(), p.GetTimer()));
        }
        return times;
    }

    public PersonBehavior FindPersonByID(int id_)
    {
        return people.Find(x => x.GetID() == id_);
    }

    public void SetSceneController(SceneController sc_){sc = sc_;}
    public List<PersonBehavior> GetPeople(){return people;}
    public SceneController GetSceneController() { return sc;}
    public PersonBehavior GetLastArrivedPerson(){return lastArrivedPerson;}
}
