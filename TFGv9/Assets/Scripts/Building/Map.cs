using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class Map : MonoBehaviour, ISceneElement
{
	public GameObject tilePrefab, sectionPrefab;

	private SceneController sc;

	// Building's dimensions
	private int width, height; // Number of tiles
	private float size; // Size of each tile

	private List<List<Tile>> tiles;
	[SerializeField] private List<Section> sections, cpSections;
	private List<Door> doors;
	int currentSectionID, currentDoorID;

    private Vector3 initClickedPos = Vector3.positiveInfinity, actualClickedPos = Vector3.positiveInfinity, endClickedPos = Vector3.positiveInfinity;

	// For the walls building
	private List<Tile> lastSelectedOutTiles = new List<Tile>();
	private List<Tile> lastSelectedInTiles = new List<Tile>();

	[SerializeField] private bool alreadyBaked;

	public void Setup(SceneController sc_, int width_, int height_, float size_)
	{
		sc = sc_;

		width = width_;
		height = height_;
		size = size_;

		tiles = new List<List<Tile>>();
		sections = new List<Section>();
 		cpSections = new List<Section>();
		doors = new List<Door>();

		alreadyBaked = false;

		CreateFloor();
	}

	public void MouseEvent(Utils.MouseInputEvents mouseEvent)
	{
		KeyValuePair<Vector3,GameObject> info = Utils.ScreenToWorld();
		switch(mouseEvent)
	 	{
			case Utils.MouseInputEvents.left_down: 
				initClickedPos = info.Key;
			break;

			case Utils.MouseInputEvents.left_held: 
				if(!alreadyBaked && Utils.ValidMousePos(initClickedPos))
				{
					initClickedPos = ToMapPos(initClickedPos);
					HighlightSelectedTiles();
				}
			break;

			case Utils.MouseInputEvents.left_up: 
				if(!alreadyBaked) CreateWallsOrDoor();
				else if(info.Value != null && info.Value.name == "Tile") ToggleSectionCP(info.Value.GetComponent<Tile>().GetSection());
			break;

			case Utils.MouseInputEvents.right_up:
				if(!alreadyBaked)
				{
					if(info.Value != null && info.Value.name == "Wall") Utils.Print("destroy this wall");
				}
			break;

			default: break;
		}
	}

	public void KeyboardEvent()
	{
		if(Input.GetKeyUp(KeyCode.Space))
		{
			Bake();
		}
	}

	public void DoAction(Utils.Actions action_)
	{
		if(action_ == Utils.Actions.bake) Bake();
	}

	// Local to world coordinates and vice-versa
	public Vector3 ToWorldPos(Vector3 mapPos_){	return new Vector3(mapPos_.x*size, 0, mapPos_.z*size);}
	public Vector3 ToMapPos(Vector3 worldPos_){ return new Vector3(Mathf.RoundToInt(worldPos_.x/size), 0, Mathf.RoundToInt(worldPos_.z/size));}


	private void CreateFloor()
	{
		// Create the tiles of the floor

		Tile newTile = null;
		Vector3 mapPos;

		for(int i=0; i<width; i++)
		{
			tiles.Add(new List<Tile>());
			for(int j=0; j<height; j++)
			{
				mapPos = new Vector3(i, 0, j);
				
				newTile = GameObject.Instantiate(tilePrefab, ToWorldPos(mapPos), this.transform.rotation, this.transform).GetComponent<Tile>();
				newTile.gameObject.name = "Tile";
				newTile.SetPos(mapPos);
				
				tiles[i].Add(newTile);
			}
		}
	}

	private void HighlightSelectedTiles()
	{
		actualClickedPos = Utils.ScreenToWorld().Key;

		if(Utils.ValidMousePos(actualClickedPos))
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
			
			endClickedPos = ToMapPos(actualClickedPos);
			
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

	private void CreateWallsOrDoor()
	{
		GameObject go = Utils.ScreenToWorld().Value;

		if(go!=null && lastSelectedOutTiles.Count>0)
		{
			if(go.name == "Wall" && lastSelectedOutTiles.Count<=1)
			{
				Tile t = go.transform.parent.GetComponent<Tile>();
				t.DeHighlight();

				Door d = t.AddDoor(currentDoorID++);
				if(d!=null) doors.Add(d);
			}
			else
			{
				foreach(Tile to in lastSelectedOutTiles)
				{
					to.DeHighlight();
					if(to.HasDoor()) doors.Remove(to.GetDoor());
					to.AddWall();
				}
				foreach(Tile ti in lastSelectedInTiles)
				{
					ti.DeHighlight();
					if(ti.HasDoor()){doors.Remove(ti.GetDoor()); ti.DestroyDoor();}
					if(ti.HasWall())ti.DestroyWall();
				}
			}
		} 
	}

	public void Bake()
	{
		if(alreadyBaked)
		{
			Utils.Print("Already baked!");
			//DeleteSections();
		}
		else
		{
			BuildNavMesh(); 
			BuildSections(); 
			MakeDoorsConnections();
			
			Utils.Print("change to graph mode and build it");
			//ChangeMode(1);
		}
		
	}

	private void BuildNavMesh(){ this.GetComponent<NavMeshSurface>().BuildNavMesh(); alreadyBaked = true;}

	public void BuildSections()
	{
		Section currentSection = null;
		
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				if(!tiles[i][j].HasWall() && !tiles[i][j].HasDoor() && tiles[i][j].GetSection()==null)
				{
					currentSection = GameObject.Instantiate(sectionPrefab, this.transform).GetComponent<Section>();
					currentSection.Setup(this, currentSectionID);
					currentSection.min = tiles[i][j].GetPos();
					currentSection.max = currentSection.min;
					
					sections.Add(currentSection);
					
					currentSectionID++;
					ExpandSectionSearch(i, j, currentSection);
					currentSection.Paint();
					currentSection.CalculatePosition();
					//currentSection.PrintCorners();

					sc.GetSceneInfo().numberOfSections += 1;
				}
			}
		}
		
	}
	
	// ggggggggg change to queue
	public void ExpandSectionSearch(int i_, int j_, Section s_)
	{
		tiles[i_][j_].SetSection(s_);
		s_.AddTile(tiles[i_][j_]);
		
		Vector3 tilePos = tiles[i_][j_].GetPos();
		if(tilePos.x>s_.max.x || tilePos.z>s_.max.z) s_.max = tilePos;
		
		List<Tile> tileCorners = new List<Tile>();
		Vector3 cornerPos;
		int[] directions = {1,0, 0,1, -1,0, 0,-1};
		int idx, i, j;
		Tile t = null;
		
		for(idx = 0; idx<directions.Length; idx=idx+2)
		{
			i = i_+directions[idx];
			j = j_+directions[idx+1];
			
			if(i>=0 && j>=0 && i<width && j<height){
				t = tiles[i][j];
				tileCorners.Add(t);
				if(t.GetSection()==null && !t.HasWall() && !t.HasDoor())
				{
					ExpandSectionSearch(i, j, s_);
				}
			}
		}

		// Save section corners
		if(tileCorners.Count>3)
		{
			int next;
			for(int a=0; a<tileCorners.Count; a++)
			{
				next = (a+1)%tileCorners.Count;
				if(a == tileCorners.Count-1) next = 0;
				if(tileCorners[a].HasWall() == tileCorners[next].HasWall())
				{
					cornerPos = tilePos + (tileCorners[a].GetPos() - tilePos) + (tileCorners[next].GetPos() - tilePos);
					s_.AddCorner(cornerPos);
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
			i = (int)doorPos.x;
			j = (int)doorPos.z;
			
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

	private void ToggleSectionCP(Section s_)
	{
		s_.SetIsCP(!s_.GetIsCP());
		if(s_.GetIsCP()){ cpSections.Add(s_); sc.GetSceneInfo().numberOfCPs += 1;}
		else {cpSections.Remove(s_); sc.GetSceneInfo().numberOfCPs -= 1;}
	}

	public Section GetRandomCP()
	{
		int total = cpSections.Count;
		if(total > 0)
		{
			int rnd = Mathf.RoundToInt(Random.Range(0,total-1));
			return cpSections[rnd];
		}
		else Debug.Log("ERROR. Not a single CP"); return null;
	}

	public Section IsPosSection(Vector3 pos_)
	{
		if(pos_.x != Mathf.Infinity) return tiles[(int)pos_.x][(int)pos_.z].GetSection();
		else return null;
	}

	public Section FindSectionByID(int id_)
	{
		return sections.Find( x => x.GetID()==id_);
	}

	public Door FindDoorByID(int id_)
	{
		return doors.Find( x => x.GetID()==id_);
	}


	#region deleting stuff
	public void DeleteAll()
	{
		DeleteSections();
		DeleteDoors();
		DeleteWalls();

		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				Destroy(tiles[i][j].gameObject);
			}
		}
		RestartAllVars();
	}

	private void DeleteSections()
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
			d.ClearConnections();
		}
		
		cpSections.Clear();

		foreach(Section s in sections)
		{
			Destroy(s.gameObject);
		}
		sections.Clear();
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
			s.GetDoors().Clear();
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

	private void RestartAllVars()
	{
		tiles.Clear();

		initClickedPos = Vector3.positiveInfinity; 
		actualClickedPos = Vector3.positiveInfinity;
		endClickedPos = Vector3.positiveInfinity;
		
		lastSelectedOutTiles.Clear();
		lastSelectedInTiles.Clear();

		alreadyBaked = false;
	}

	#endregion

	#region loading stuff
	public void Load()
	{
		MapData md_ = DataController.GetMapData();

		// Dimens
		width = md_.width;
		height = md_.height;

		// Tiles
		CreateFloor();

		// Walls
		foreach(Vector3 wall in md_.walls)
		{
			tiles[(int)wall.x][(int)wall.z].AddWall();
		}

		// Sections
		Section newSection;
		List<Tile> sectionTiles = new List<Tile>();
		Tile sectionTile;

		//Vector3 diff;
		//int next;
		foreach(SectionData sd in md_.sections)
		{
			newSection = GameObject.Instantiate(sectionPrefab, this.transform).GetComponent<Section>();
			foreach(Vector3 t in sd.tiles)
			{
				sectionTile = tiles[(int)t.x][(int)t.z];
				sectionTile.SetSection(newSection);
				sectionTiles.Add(sectionTile);
			}
			newSection.Setup(this, sd.ID, sd.maxCapacity, sd.currentCapacity, sd.pos, sd.isCP, sectionTiles, sd.corners);
			sections.Add(newSection);
			if(newSection.GetIsCP()) cpSections.Add(newSection);
			newSection.Paint();

			sectionTiles = new List<Tile>();

			// Add section walls
			// for(int a=0; a<sd.corners.Count; a++)
			// {
			// 	next = (a+1)%sd.corners.Count;
			// 	diff = sd.corners[next] - sd.corners[a];
			// 	Debug.Log(diff);
			// 	BuildMultipleWalls(sd.corners[a], diff);
			// }
			 
		}

		// Doors
		Door newDoor;
		foreach(DoorData dd in md_.doors)
		{
			newDoor = tiles[(int)dd.pos.x][(int)dd.pos.z].AddDoor(dd.ID);
			newDoor.SetID(dd.ID);
			if(newDoor!=null) doors.Add(newDoor	);
			else Debug.Log("is new door null??");
		}
		MakeDoorsConnections();

		// De momento no se tienen en cuenta los datos guardados de conexiones 
		// ni de las puertas ni de las secciones, se vuelven a calcular

		BuildNavMesh();
		alreadyBaked = true;

	}

	// private void BuildMultipleWalls(Vector3 actualPos_, Vector3 diff_)
	// {
	// 	Vector3 actualPos = actualPos_;
	// 	Vector3 mult;
	// 	int max;
	// 	if(diff_.x==0f) {max = (int) diff_.z; mult = new Vector3(0f,0f,1f);}
	// 	else {max = (int)diff_.x; mult = new Vector3(1f,0f,0f);}

	// 	for(int i=0; i<max; i++)
	// 	{
	// 		Debug.Log("from "+actualPos+" in direction "+mult);
	// 		actualPos += mult;
	// 		Debug.Log("to "+actualPos);
	// 		tiles[(int)actualPos.x][(int)actualPos.z].AddWall();
	// 	}
	// }

	public void Save()
	{
		// Walls
		List<Vector3> wallsPositions = new List<Vector3>();
		for(int i=0; i<width; i++)
		{
			for(int j=0; j<height; j++)
			{
				if(tiles[i][j].HasWall()){ wallsPositions.Add(new Vector3(i,0,j));}
			}
		}

		// Sections
		List<SectionData> sectionsData = new List<SectionData>();
		List<Vector3> sectionTiles = new List<Vector3>();
		foreach(Section s in sections)
		{
			foreach(Tile t in s.GetTiles())
			{
				sectionTiles.Add(t.GetPos());
			}
			sectionsData.Add(new SectionData(s.GetID(), s.GetMaxCapacity(), s.GetCurrentCapacity(), s.GetPos(), s.GetIsCP(), sectionTiles, s.GetCorners()));
			sectionTiles = new List<Vector3>();
		}

		// Doors
		List<DoorData> doorsData = new List<DoorData>();
		Section[] doorSections;
		int connA, connB;
		foreach(Door d in doors)
		{
			doorSections = d.GetConnections();
			connA = doorSections[0].GetID();
			connB = doorSections[1].GetID();
			doorsData.Add(new DoorData(d.GetID(), connA, connB, d.GetPos()));
		}

		// Save all
		DataController.SetMapData(new MapData(width, height, wallsPositions, sectionsData, doorsData));
	}
	#endregion




	public List<Section> GetSections() { return sections;}
	public List<Door> GetDoors(){ return doors;}
	public int GetWidth(){return width;}
	public int GetHeight(){return height;}

	public void SetWidth(int w_){width = w_;}
	public void SetHeight(int h_){ height = h_;}
}

















	
// 	public int doorIDCounter = 0;
	
// 	public void BuildSections()
// 	{
// 		int currentSectionID = 0;
// 		Section currentSection = null;
		
// 		for(int i=0; i<width; i++)
// 		{
// 			for(int j=0; j<height; j++)
// 			{
// 				if(!tiles[i][j].HasWall() && !tiles[i][j].HasDoor() && tiles[i][j].GetSection()==null)
// 				{
// 					sectionGO = GameObject.Instantiate(sectionPrefab);
// 					sectionGO.name = "Section"+currentSectionID;
// 					currentSection = sectionGO.GetComponent<Section>();
// 					currentSection.Init(this, sc, currentSectionID);
// 					currentSection.min = tiles[i][j].GetPos();
// 					currentSection.max = currentSection.min;
					
// 					sections.Add(currentSection);
// 					graph.InsertNode(currentSection);
					
// 					currentSectionID++;
// 					ExpandSectionSearch(i, j, currentSection);
// 					currentSection.Paint();
// 					currentSection.CalculatePosition();
// 				}
// 			}
// 		}
		
// 	}
	
// 	// Change for loop? adding items to a list
// 	public void ExpandSectionSearch(int i_, int j_, Section s_)
// 	{
// 		tiles[i_][j_].SetSection(s_);
// 		s_.AddTile(tiles[i_][j_]);
		
// 		Vector3 tilePos = tiles[i_][j_].GetPos();
// 		if(tilePos.x>s_.max.x || tilePos.z>s_.max.z) s_.max = tilePos;
		
// 		int[] directions = {1,0, 0,1, -1,0, 0,-1};
// 		int idx, i, j;
// 		Tile t = null;
		
// 		for(idx = 0; idx<directions.Length; idx=idx+2)
// 		{
// 			i = i_+directions[idx];
// 			j = j_+directions[idx+1];
			
// 			if(i>=0 && j>=0 && i<width && j<height){
// 				t = tiles[i][j];
// 				if(t.GetSection()==null && !t.HasWall() && !t.HasDoor())
// 				{
// 					ExpandSectionSearch(i, j, s_);
// 				}
// 			}
// 		}
// 	}
	
// 	public void MakeDoorsConnections()
// 	{
// 		int i, j, i_, j_, idx;
// 		int[] directions = {1,0, 0,1, -1,0, 0,-1};
// 		Section s = null;
// 		Tile t = null;
		
// 		foreach(Door d in doors)
// 		{
// 			Vector3 doorPos = d.GetPos();
// 			i = (int)Mathf.Round(doorPos.x/tileWidth);
// 			j = (int)Mathf.Round(doorPos.z/tileWidth);
			
// 			for(idx = 0; idx<directions.Length; idx=idx+2)
// 			{
// 				i_ = i+directions[idx];
// 				j_ = j+directions[idx+1];
				
// 				if(i_>=0 && j_>=0 && i_<width && j_<height){
// 					t = tiles[i_][j_];
// 					s = t.GetSection();
// 					if(s!=null && !t.HasWall() && !t.HasDoor())
// 					{
// 						d.AddConnection(s);
// 						s.AddDoor(d);
// 					}
// 				}
// 			}
// 		}
		
// 		Section[] connections;
// 		foreach(Door d in doors)
// 		{
// 			connections = d.GetConnections();
// 			Node nA = graph.ContainsNode(connections[0]);
// 			Node nB = graph.ContainsNode(connections[1]);
// 			Edge newEdge = graph.InsertEdge(d, nA, nB);
// 			newEdge.CalculateDistance();
// 		}
// 	}
	
	
	
// 	public void AddCP(Section cp_){ cpSections.Add(cp_);}
// 	public void RemoveCP(Section cp_){ cpSections.Remove(cp_);}
	
	
	
// 	public void DeleteWalls()
// 	{
// 		for(int i=0; i<width; i++)
// 		{
// 			for(int j=0; j<height; j++)
// 			{
// 				tiles[i][j].DestroyWall();
// 			}
// 		}
// 	}
	
// 	public void DeleteDoors()
// 	{
// 		foreach(Section s in sections)
// 		{
// 			s.GetDoors().Clear();
// 		}
// 		doors.Clear();
// 		for(int i=0; i<width; i++)
// 		{
// 			for(int j=0; j<height; j++)
// 			{
// 				tiles[i][j].DestroyDoor();
// 			}
// 		}
// 	}
	
// 	public void DeleteSections()
// 	{
// 		for(int i=0; i<width; i++)
// 		{
// 			for(int j=0; j<height; j++)
// 			{
// 				tiles[i][j].SetSection(null);
// 				tiles[i][j].DeHighlight();
// 			}
// 		}
// 		foreach(Door d in doors)
// 		{
// 			d.ClearConnections();
// 		}
		
// 		cpSections.Clear();
// 	}
	
// 	public void DeleteAll()
// 	{
// 		DeleteSections();
// 		DeleteDoors();
// 		DeleteWalls();
// 	}
	
// 	public void CreateFloorFromData(MapData md)
// 	{
// 		Door d;
// 		Tile newTile;
// 		TileData t;
		
// 		for(int i=0; i<width; i++)
// 		{
// 			tiles.Add(new List<Tile>());
// 			for(int j=0; j<height; j++)
// 			{
// 				tileGO = GameObject.Instantiate(tilePrefab);
// 				tileGO.name = "Tile";
// 				newTile = tileGO.GetComponent<Tile>();
// 				t = md.mapTiles[i*height + j];
				
// 				newTile.gameObject.transform.parent = floor.transform;
				
// 				newTile.SetPos(t.pos);
// 				newTile.SetMap(this);
// 				if(t.hasWall)
// 				{
// 					newTile.AddWall();
// 				}
// 				else if(t.hasDoor)
// 				{
// 					d = newTile.AddDoor();
// 					if(d!=null) doors.Add(d);
// 				}
				
// 				tiles[i].Add(newTile);
// 			}
// 		}
// 	}
	
	
	
// 	#region GETTERS & SETTERS
// 	public SceneController GetSceneController(){return sc;}
// 	public Graph GetGraph() {return graph;}
// 	#endregion
	
// 	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" MAP:  "+msg);}
	
// }