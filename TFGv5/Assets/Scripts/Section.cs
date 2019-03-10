using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Section: MonoBehaviour
{
	private Map map;
	private SceneController sc;
	
	private int sectionID;
	private bool isCP;
	private int maxCapacity;
	private int currentOccupancy;
	private Transform pos;
	
	private int numClics;
	private List<Tile> tiles;
	private List<PersonBehavior> peopleAssigned;
	
	public List<KeyValuePair<PersonBehavior, float>> personsDistances;
	
	public TMP_Text textInfo;
	
	public void InitSection(Map map_, SceneController sc_, int id)
	{
		isCP = false;
		maxCapacity = 0;
		numClics = 0;
		tiles = new List<Tile>();
		peopleAssigned = new List<PersonBehavior>();
		personsDistances = new List<KeyValuePair<PersonBehavior, float>>();
		
		map = map_;
		sc = sc_;
		sectionID = id;
		
	}
	
	public void PersonArrived()
	{
		currentOccupancy++;
		sc.totalPeople--;
	}
	
	public int Clic()
	{
		numClics++; 
		if(!isCP && numClics==3) 
		{
			isCP = true; 
			map.cpSections.Add(this);
			Print("LA SECCION "+sectionID+" ES UN CP");
		}
		if(isCP)
		{
			PaintCP();
		}
		
		return numClics;
	}
	
	public void PaintCP()
	{
		// Re-color room if it's CP
		Color randomColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
		foreach(Tile t in tiles)
		{
			t.gameObject.GetComponent<Renderer>().material.color = randomColor;
		}
		if(pos!=null)this.transform.position = pos.position;
		textInfo.text = "CP"+sectionID;
	}
	
	public void AddTile(Tile t){ tiles.Add(t); maxCapacity++;}
	public void RemoveTile(Tile t){ tiles.Remove(t); maxCapacity--;}
	public bool AddPerson(PersonBehavior p){ peopleAssigned.Add(p); return peopleAssigned.Count>maxCapacity;}
	public void RemovePerson(PersonBehavior p){ peopleAssigned.Remove(p);}
	public bool IsFull(){return peopleAssigned.Count>=maxCapacity;}
	public bool IsExceeded(){return peopleAssigned.Count>maxCapacity;}
	public int GetExceed(){ return peopleAssigned.Count-maxCapacity;}
	
	public Section ShallowCpy(){ return (Section)this.MemberwiseClone();}
	
	#region GETTERS & SETTERS
	
	public int 			GetSectionID(){return sectionID;}
	public Transform	GetPos(){return pos;}
	public bool 		GetIsCP(){return isCP;}
	public int 			GetMaxCapacity(){return maxCapacity;}
	public List<Tile> 	GetTiles(){return tiles;}
	public List<KeyValuePair<PersonBehavior, float>>	GetPersonsDistances(){return personsDistances;}
	public List<PersonBehavior> 	GetPeopleAssigned(){ return peopleAssigned;}
	
	public void 		SetIsCP(bool cp){isCP = cp;}
	public void 		SetPersonsDistances(List<KeyValuePair<PersonBehavior, float>> d){personsDistances = d;}
	public void 		SetPos(Transform pos_){pos = pos_;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" SECTION:  "+msg);}
}
