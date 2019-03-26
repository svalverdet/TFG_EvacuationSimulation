using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Map
{
	private GameObject floor, tilePrefab, sectionPrefab;
	private GameObject wallGO, tileGO, sectionGO;
	
	private SceneController sc;
	
	public int width, height;
	public float tileWidth = 1.0f;
	
	public List<List<Tile>> tiles;
	public List<Section> sections, cpSections;
	public List<Door> doors;
	public bool alreadyBaked = false;
	
	public Vector3 initClickedPos, endClickedPos, actualClickedPos;
	public bool validInitClickedPos;
	
	// For the walls building
	private List<Tile> lastSelectedOutTiles = new List<Tile>();
	private List<Tile> lastSelectedInTiles = new List<Tile>();
	
    public Map(SceneController sc_, int width_, int height_, GameObject floor_, GameObject tilePrefab_, GameObject sectionPrefab_)
	{
		tiles = new List<List<Tile>>();
		sections = new List<Section>();
		cpSections = new List<Section>();
		doors = new List<Door>();
		
		sc = sc_;
		width = width_;
		height = height_;
		floor = floor_;
		tilePrefab = tilePrefab_;
		sectionPrefab = sectionPrefab_;
		
		CreateFloor();
	}
	
	// Fill the tiles list with instances of Tile
	private void CreateFloor()
	{
		for(int i=0; i<width; i++)
		{
			tiles.Add(new List<Tile>());
			for(int j=0; j<height; j++)
			{
				tileGO = GameObject.Instantiate(tilePrefab);
				tileGO.name = "Tile";
				Tile newTile = tileGO.GetComponent<Tile>();
				Vector3 devPos = new Vector3(i*tileWidth, 0, j*tileWidth);
				
				newTile.gameObject.transform.parent = floor.transform;
				newTile.SetPos(devPos);
				newTile.SetMap(this);
				
				tiles[i].Add(newTile);
			}
		}
	}
	
	
	public GameObject GetElemOnCurrentMousePosition()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			Debug.Log("Over UI!");
			return null;
		}
		
		RaycastHit hit;
		Ray ray = sc.GetCam().ScreenPointToRay(Input.mousePosition);
		
		if(Physics.Raycast(ray.origin, ray.direction, out hit, 200f))
		{
			actualClickedPos = new Vector3(Mathf.Floor(hit.point.x+tileWidth/2),0.5f,Mathf.Floor(hit.point.z+tileWidth/2));
			return hit.transform.gameObject;
		}
		else return null;
	}
	
	// Devuelve si encuentra la posicion en el mapa y la asigna en caso afirmativo
	public bool SetMouseOnGridPosition()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			Debug.Log("Over UI!");
			return false;
		}
			
		
		RaycastHit hit;
		bool hasHit;
		Ray ray = sc.GetCam().ScreenPointToRay(Input.mousePosition);
		hasHit = Physics.Raycast(ray.origin, ray.direction, out hit, 200f);
		
		if(hasHit)
		{
			actualClickedPos = new Vector3(Mathf.Floor(hit.point.x+tileWidth/2),0.5f,Mathf.Floor(hit.point.z+tileWidth/2));
			return true;
		}
		return false;
	}
	
	public Vector3 ToMap(Vector3 pos_)
	{
		return pos_ + new Vector3(width/2, 0, height/2);
	}
	
	public Vector3 FromMap(Vector3 pos_)
	{
		return pos_ - new Vector3(width/2, 0, height/2);
	}
	
	public void BuildNavMesh(){ floor.GetComponent<NavMeshSurface>().BuildNavMesh();}
	
	
	public void HighlightSelectedTiles()
	{
		if(SetMouseOnGridPosition())
		{
			foreach(Tile to in lastSelectedOutTiles)
			{
				to.DeHighlight();
			}
			foreach(Tile ti in lastSelectedInTiles)
			{
				ti.DeHighlight();
			}
			lastSelectedOutTiles.Clear();
			lastSelectedInTiles.Clear();
			
			endClickedPos = ToMap(actualClickedPos);
			
			int minX, minY, maxX, maxY;
			minX = Mathf.RoundToInt(Mathf.Min(initClickedPos.x, endClickedPos.x));
			maxX = Mathf.RoundToInt(Mathf.Max(initClickedPos.x, endClickedPos.x));
			minY = Mathf.RoundToInt(Mathf.Min(initClickedPos.z, endClickedPos.z));
			maxY = Mathf.RoundToInt(Mathf.Max(initClickedPos.z, endClickedPos.z));
			
			for(int i=minX; i<=maxX; i++)
			{
				for(int j=minY; j<=maxY; j++)
				{
					// Exterior
					if(i==minX || j==minY || i==maxX || j==maxY)
					{
						tiles[i][j].Highlight(0);
						lastSelectedOutTiles.Add(tiles[i][j]);
					}
					// Interior
					else
					{
						tiles[i][j].Highlight(1);
						lastSelectedInTiles.Add(tiles[i][j]);
					}
				}
			}
		}
	}
	/*
	public void CreateWalls()
	{
		//if(validInitClickedPos && SetMouseOnGridPosition())
		//{
			foreach(Tile to in lastSelectedOutTiles)
			{
				to.DeHighlight();
				to.AddWall();
			}
			foreach(Tile ti in lastSelectedInTiles)
			{
				ti.DeHighlight();
				if(ti.HasDoor())ti.DestroyDoor();
				if(ti.HasWall())ti.DestroyWall();
			}
		//}
	}*/
	
		/*
	public void CreateDoor()
	{
		GameObject go = GetElemOnCurrentMousePosition();
		if(go!=null && go.name == "Wall")
		{
			Tile t = go.transform.parent.GetComponent<Tile>();
			Door d = t.AddDoor();
			if(d!=null) doors.Add(d);
		}
	}
	*/
	
	public void CreateWallsOrDoor()
	{
		GameObject go = GetElemOnCurrentMousePosition();
		if(go!=null)
		{
			if(go.name == "Wall" && lastSelectedOutTiles.Count<=1)
			{
				Tile t = go.transform.parent.GetComponent<Tile>();
				t.DeHighlight();
				Door d = t.AddDoor();
				if(d!=null) doors.Add(d);
			}
			else
			{
				foreach(Tile to in lastSelectedOutTiles)
				{
					to.DeHighlight();
					to.AddWall();
				}
				foreach(Tile ti in lastSelectedInTiles)
				{
					ti.DeHighlight();
					if(ti.HasDoor())ti.DestroyDoor();
					if(ti.HasWall())ti.DestroyWall();
				}
			}
		} 
	}
	
	public void BuildSections()
	{
		int currentSectionID = 0;
		Section currentSection = null;
		
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				if(!tiles[i][j].HasWall() && !tiles[i][j].HasDoor() && tiles[i][j].GetSection()==null)
				{
					sectionGO = GameObject.Instantiate(sectionPrefab);
					sectionGO.name = "Section"+currentSectionID;
					currentSection = sectionGO.GetComponent<Section>();
					currentSection.InitSection(this, sc, currentSectionID);
					
					sections.Add(currentSection);
					
					currentSectionID++;
					ExpandSectionSearch(i, j, currentSection);
					currentSection.PaintCP();
				}
			}
		}
		
	}
	
	// Change for loop? adding items to a list
	public void ExpandSectionSearch(int i_, int j_, Section s_)
	{
		tiles[i_][j_].SetSection(s_);
		s_.AddTile(tiles[i_][j_]);
		
		int[] directions = {1,0, 0,1, -1,0, 0,-1};
		int idx, i, j;
		Tile t = null;
		
		for(idx = 0; idx<directions.Length; idx=idx+2)
		{
			i = i_+directions[idx];
			j = j_+directions[idx+1];
			
			if(i>=0 && j>=0 && i<width && j<height){
				t = tiles[i][j];
				if(t.GetSection()==null && !t.HasWall() && !t.HasDoor())
				{
					ExpandSectionSearch(i, j, s_);
				}
			}
		}
	}
	
	public void MakeDoorsConnections()
	{
		int i, j, i_, j_, idx;
		int[] directions = {1,0, 0,1, -1,0, 0,-1};
		Section s = null;
		Tile t = null;
		
		foreach(Door d in doors)
		{
			Vector3 doorPos = d.GetPos();
			i = (int)Mathf.Round(doorPos.x/tileWidth);
			j = (int)Mathf.Round(doorPos.z/tileWidth);
			
			for(idx = 0; idx<directions.Length; idx=idx+2)
			{
				i_ = i+directions[idx];
				j_ = j+directions[idx+1];
				
				if(i_>=0 && j_>=0 && i_<width && j_<height){
					t = tiles[i_][j_];
					s = t.GetSection();
					if(s!=null && !t.HasWall() && !t.HasDoor())
					{
						d.AddConnection(s);
						s.AddDoor(d);
					}
				}
			}
		}
	}
	
	
	public void AddCP(Section cp_){ cpSections.Add(cp_);}
	public void RemoveCP(Section cp_){ cpSections.Remove(cp_);}
	
	public float PathDistance(NavMeshAgent agent)
	{
		float distance = 0.0f;
		Vector3[] corners = agent.path.corners;
		for (int c = 0; c < corners.Length - 1; ++c) {
			distance += Mathf.Abs((corners[c] - corners[c + 1]).magnitude);
		}
		return distance;
	}
	
	
	public void DeleteWalls()
	{
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				tiles[i][j].DestroyWall();
			}
		}
	}
	
	public void DeleteDoors()
	{
		foreach(Section s in sections)
		{
			s.doors.Clear();
		}
		doors.Clear();
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				tiles[i][j].DestroyDoor();
			}
		}
	}
	
	public void DeleteSections()
	{
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				tiles[i][j].SetSection(null);
				tiles[i][j].DeHighlight();
			}
		}
		foreach(Door d in doors)
		{
			d.connections.Clear();
		}
		
		cpSections.Clear();
	}
	
	public void DeleteAll()
	{
		DeleteSections();
		DeleteDoors();
		DeleteWalls();
	}
	
	public void CreateFloorFromData(MapData md)
	{
		Door d;
		Tile newTile;
		TileData t;
		
		for(int i=0; i<width; i++)
		{
			tiles.Add(new List<Tile>());
			for(int j=0; j<height; j++)
			{
				tileGO = GameObject.Instantiate(tilePrefab);
				tileGO.name = "Tile";
				newTile = tileGO.GetComponent<Tile>();
				t = md.mapTiles[i*height + j];
				
				newTile.gameObject.transform.parent = floor.transform;
				
				newTile.SetPos(t.pos);
				newTile.SetMap(this);
				if(t.hasWall)
				{
					newTile.AddWall();
				}
				else if(t.hasDoor)
				{
					d = newTile.AddDoor();
					if(d!=null) doors.Add(d);
				}
				
				tiles[i].Add(newTile);
			}
		}
	}
	
	
	
	#region GETTERS & SETTERS
	public SceneController GetSceneController(){return sc;}
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" MAP:  "+msg);}
	
}