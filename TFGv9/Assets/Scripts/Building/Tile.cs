using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Tile : MonoBehaviour
{
	public GameObject wallPrefab, doorPrefab;

	private GameObject wallGO, doorGO;

	private Vector3 pos;
	private bool hasWall = false, hasDoor = false;
	private Section section = null;

	private Renderer myRenderer;
	private Color originalColor;


	void Awake()
	{
		myRenderer = this.gameObject.GetComponent<Renderer>();
 		originalColor = myRenderer.material.color;
	}

	public void AddWall()
	{
		// Create a wall on the tile if there's no wall already
		// also if there's a door, deactivate it

		if(!hasWall)
		{
			hasWall = true;
			if(wallGO==null)
			{
				wallGO = GameObject.Instantiate(wallPrefab, this.transform.position+new Vector3(0f, 1.5f, 0f), this.transform.rotation, this.transform);
				wallGO.name = "Wall";
			}
			else wallGO.SetActive(true);
			
			if(hasDoor)
			{
				hasDoor = false;
				doorGO.SetActive(false);
			}
		}
	}

	public void DestroyWall()
	{
		if(hasWall)
		{
			hasWall = false;
			wallGO.SetActive(false);
		}
	}
	
	public Door AddDoor(int ID_)
	{
		if(!hasDoor)
		{
			DestroyWall();
			hasDoor = true;
			if(doorGO==null)
			{
				doorGO = GameObject.Instantiate(doorPrefab, this.transform.position+new Vector3(0f, 1.5f, 0f), this.transform.rotation, this.transform);
				doorGO.name = "Door";
				doorGO.GetComponent<Door>().Setup(ID_,this);
			}
			else doorGO.SetActive(true);
			
			return doorGO.GetComponent<Door>();
		}
		return null;
	}
	
	public void DestroyDoor()
	{
		if(hasDoor)
		{
			AddWall();
		}
	}
	

	public void Highlight(int type)
	{
		if(type==0) myRenderer.material.color = Color.red;
		else if(type==1)myRenderer.material.color = Color.white;
	}
	
	public void DeHighlight()
	{
		myRenderer.material.color = originalColor;
	}

	#region GETTERS & SETTERS
	
	public bool 		HasWall(){return hasWall;}
	public bool 		HasDoor(){return hasDoor;}
	public Vector3		GetPos(){return pos;}
	public Renderer 	GetRenderer(){return myRenderer;}
	public Section 		GetSection(){return section;}
	public Door			GetDoor(){ return doorGO.GetComponent<Door>();}
	
	public void 		SetPos(Vector3 pos_){pos = pos_;}
	public void 		SetSection(Section newSection){section = newSection;}
	
	#endregion
}


























// public class Tile : MonoBehaviour
// {
//     public GameObject wallPrefab, doorPrefab;
// 	private GameObject wallGO, doorGO;
	
// 	private Color originalColor;
// 	private Renderer myRenderer;
	
// 	private Map map;
// 	private Section section = null;
	
// 	public Vector3 pos;
// 	private bool hasWall = false;
// 	private bool hasDoor = false;
	
//     void Start()
//     {
//         transform.position = pos - new Vector3(map.width/2, 0, map.height/2);
		
// 		myRenderer = this.gameObject.GetComponent<Renderer>();
// 		originalColor = myRenderer.material.color;
//     }
	

	
// 	void OnMouseOver()
// 	{
// 		if(EventSystem.current.IsPointerOverGameObject()){
// 			return;
// 		}
// 		if(section!=null) map.GetSceneController().ToggleText(section.name);
// 	}
	
// 	void OnMouseExit()
// 	{
// 		map.GetSceneController().ToggleText("");
// 	}
	
	
// 	#region GETTERS & SETTERS
	
// 	public bool 		HasWall(){return hasWall;}
// 	public bool 		HasDoor(){return hasDoor;}
// 	public Section 		GetSection(){return section;}
// 	public Vector3		GetPos(){return pos;}
// 	public Renderer 	GetRenderer(){return myRenderer;}
// 	public Map			GetMap(){return map;}
	
// 	public void 		SetPos(Vector3 pos_){pos = pos_;}
// 	public void 		SetMap(Map map_){map = map_;}
// 	public void 		SetSection(Section newSection){section = newSection;}
	
// 	#endregion
	
// 	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" TILE:  "+msg);}
// }
