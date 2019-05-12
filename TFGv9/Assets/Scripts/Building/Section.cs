using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Section: MonoBehaviour
{
	//private SceneController sc;
	private Map map;
	
	private int ID;
	public Vector3 pos;
	private List<Tile> tiles;
	[SerializeField] private bool isCP;
	[SerializeField] private int maxCapacity;
	[SerializeField] private int currentCapacity;
	private List<Vector3> corners;
	
	private List<PersonBehavior> peopleAssigned = new List<PersonBehavior>();
	
	private List<Door> doors;
	
	public Vector3 min, max;

	[SerializeField] private float timer;
	private int peopleArrivedCounter;
	
	public void Setup(Map map_, int ID_)
	{
		isCP = false;
		maxCapacity = 0;
		tiles = new List<Tile>();
		doors = new List<Door>();
		//peopleAssigned = new List<PersonBehavior>();
		corners = new List<Vector3>();
		
		map = map_;
		//sc = sc_;
		ID = ID_;

		name = "Section"+ID;
	}

	public void Setup(Map map_, int ID_, int maxCapacity_, int currentCapacity_, Vector3 pos_, bool isCP_, List<Tile> tiles_, List<Vector3> corners_)
	{
		doors = new List<Door>();
		corners = new List<Vector3>();

		map = map_;
		ID = ID_;
		maxCapacity = maxCapacity_;
		currentCapacity = currentCapacity_;
		pos = pos_;
		transform.position = pos;
		isCP = isCP_;
		tiles = tiles_;
		corners = corners_;

		name = "Section"+ID;
	}

	public void PersonArrived(int ID_, float timer_)
	{
		Utils.Print("P"+ID_+" is here!");
        peopleArrivedCounter++;
        if(peopleArrivedCounter>=peopleAssigned.Count)
		{ 
			Utils.Print("Everyone has arrived to section "+ID+" in "+timer_+" seconds!");
			timer = timer_;
		}
	}
	
	public void Paint()
	{
		// Re-color room if it's CP
		Color randomColor = Random.ColorHSV(0.2f, 0.8f, 0.6f, 0.9f, 0.5f, 0.8f, 0.8f, 0.8f);
		foreach(Tile t in tiles)
		{
			if(isCP){ t.GetRenderer().material.color = Color.red;}
			else t.GetRenderer().material.color = randomColor;
		}
	}
	
	public void CalculatePosition()
	{
		if(min!=max){pos = min + new Vector3(Mathf.Round((max.x-min.x)/2), min.y, Mathf.Round((max.z-min.z)/2));}
		else{pos = max;}
        transform.position = pos;
	}
	
	public Door GetConnection(Section s_)
	{
		Section[] doorConnections;
		foreach(Door d in doors)
		{
			doorConnections = d.GetConnections();
			if(doorConnections[0]==s_ || doorConnections[1]==s_) return d;
		}
		return null;
	}
	
	public void AddTile(Tile t){ tiles.Add(t); maxCapacity++; currentCapacity++;}
	public void RemoveTile(Tile t){ tiles.Remove(t); maxCapacity--; currentCapacity--;}
	public void AddDoor(Door d){doors.Add(d);}
	public void AddCorner(Vector3 c_){corners.Add(c_);}
	public void PrintCorners()
	{
		string r = "Section"+ID+": ";
		foreach(Vector3 c in corners)
		{
			r+=c+" || ";
		}
		Debug.Log(r);
	}
	
	public bool AddPerson(PersonBehavior p){ peopleAssigned.Add(p); return peopleAssigned.Count>currentCapacity;}
	//public void RemovePerson(PersonBehavior p){ peopleAssigned.Remove(p);}
	//public bool IsFull(){return peopleAssigned.Count>=currentCapacity;}
	
	
	#region GETTERS & SETTERS
	
	public int 			GetID(){return ID;}
	public Vector3		GetPos(){return pos;}
	public bool 		GetIsCP(){return isCP;}
	public int 			GetMaxCapacity(){return maxCapacity;}
	public List<Tile> 	GetTiles(){return tiles;}
	public int 			GetCurrentCapacity(){return currentCapacity;}
	public List<Door> 	GetDoors(){return doors;}
	public List<Vector3> GetCorners(){return corners;}
	public float 		GetTimer(){ return timer;}
	
	//public void 		SetIsCP(bool cp){isCP = cp; Paint();if(isCP) map.AddCP(this); else map.RemoveCP(this);}
	public void 		SetIsCP(bool cp_){isCP = cp_; Paint();}
	public void 		SetPos(Vector3 pos_){pos = pos_;}
	public void 		SetCurrentCapacity(int currentCapacity_){currentCapacity = currentCapacity_;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" SECTION:  "+msg);}
	public override string ToString(){ return "Section"+ID;}
}
