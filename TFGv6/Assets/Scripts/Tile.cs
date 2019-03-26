using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    public GameObject wallPrefab, doorPrefab;
	private GameObject wallGO, doorGO;
	
	private Color originalColor;
	private Renderer myRenderer;
	
	private Map map;
	private Section section = null;
	
	private Vector3 pos;
	private bool hasWall = false;
	private bool hasDoor = false;
	
    void Start()
    {
        transform.position = pos - new Vector3(map.width/2, 0, map.height/2);
		
		myRenderer = this.gameObject.GetComponent<Renderer>();
		originalColor = myRenderer.material.color;
    }
	
	public void AddWall()
	{
		if(!hasWall)
		{
			hasWall = true;
			if(wallGO==null)
			{
				wallGO = GameObject.Instantiate(wallPrefab);
				wallGO.name = "Wall";
				wallGO.transform.parent = this.transform;
				wallGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
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
	
	public Door AddDoor()
	{
		if(!hasDoor)
		{
			DestroyWall();
			hasDoor = true;
			if(doorGO==null)
			{
				doorGO = GameObject.Instantiate(doorPrefab);
				doorGO.name = "Door";
				doorGO.transform.parent = this.transform;
				doorGO.transform.localPosition = new Vector3(0f, 1.5f, 0f);
				
				doorGO.GetComponent<Door>().SetTileParent(this);
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
			hasDoor = false;
			doorGO.SetActive(false);
			hasWall = true;
			wallGO.SetActive(true);
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
	
	
	void OnMouseOver()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			return;
		}
		if(section!=null) map.GetSceneController().ToggleText(section.name);
	}
	
	void OnMouseExit()
	{
		map.GetSceneController().ToggleText("");
	}
	
	
	#region GETTERS & SETTERS
	
	public bool 		HasWall(){return hasWall;}
	public bool 		HasDoor(){return hasDoor;}
	public Section 		GetSection(){return section;}
	public Vector3		GetPos(){return pos;}
	public Renderer 	GetRenderer(){return myRenderer;}
	public Map			GetMap(){return map;}
	
	public void 		SetPos(Vector3 pos_){pos = pos_;}
	public void 		SetMap(Map map_){map = map_;}
	public void 		SetSection(Section newSection){section = newSection;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" TILE:  "+msg);}
}
