using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Section: MonoBehaviour
{
	private SceneController sc;
	private Map map;
	
	public int sectionID;
	private Transform pos;
	private List<Tile> tiles;
	private bool isCP;
	private int maxCapacity;
	public int currentCapacity;
	
	public List<PersonBehavior> peopleAssigned;
	
	public List<Door> doors;
	
	public void InitSection(Map map_, SceneController sc_, int id)
	{
		isCP = false;
		maxCapacity = 0;
		tiles = new List<Tile>();
		doors = new List<Door>();
		peopleAssigned = new List<PersonBehavior>();
		
		map = map_;
		sc = sc_;
		sectionID = id;
	}
	
	public void PaintCP()
	{
		// Re-color room if it's CP
		Color randomColor = Random.ColorHSV(0f, 0.5f, 0.6f, 0.9f, 0.5f, 1f, 0.8f, 0.8f);
		foreach(Tile t in tiles)
		{
			if(isCP) t.GetRenderer().material.color = Color.red;
			else t.GetRenderer().material.color = randomColor;
		}
	}
	
	public Door GetConnection(Section s_)
	{
		foreach(Door d in doors)
		{
			foreach(Section s in d.connections)
			{
				if(s==s_) return d;
			}
		}
		return null;
	}
	
	public void AddTile(Tile t){ tiles.Add(t); maxCapacity++; currentCapacity++;}
	public void RemoveTile(Tile t){ tiles.Remove(t); maxCapacity--; currentCapacity--;}
	public void AddDoor(Door d){doors.Add(d);}
	
	public bool AddPerson(PersonBehavior p){ peopleAssigned.Add(p); return peopleAssigned.Count>currentCapacity;}
	public void RemovePerson(PersonBehavior p){ peopleAssigned.Remove(p);}
	public bool IsFull(){return peopleAssigned.Count>=currentCapacity;}
	
	
	#region GETTERS & SETTERS
	
	public int 			GetSectionID(){return sectionID;}
	public Transform	GetPos(){return pos;}
	public bool 		GetIsCP(){return isCP;}
	public int 			GetMaxCapacity(){return maxCapacity;}
	public List<Tile> 	GetTiles(){return tiles;}
	
	public void 		SetIsCP(bool cp){isCP = cp; PaintCP();if(isCP) map.AddCP(this); else map.RemoveCP(this);}
	public void 		SetPos(Transform pos_){pos = pos_;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" SECTION:  "+msg);}
}
