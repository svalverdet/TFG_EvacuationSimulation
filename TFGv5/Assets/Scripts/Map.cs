using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Map
{
	private GameObject tilePrefab, sectionPrefab, floor;
	private GameObject wallGO, tileGO, sectionGO;
	
	private SceneController sc;
	
	public int width, height;
	public float tileWidth = 1.0f;
	
	public List<List<Tile>> tiles;
	public List<Section> sections, cpSections;
	
	public Vector3 initClickedPos, endClickedPos, actualClickedPos;
	
    public Map(SceneController sc_, int width_, int height_, GameObject tilePrefab_, GameObject sectionPrefab_, GameObject floor_)
	{
		tiles = new List<List<Tile>>();
		sections = new List<Section>();
		cpSections = new List<Section>();
		
		sc = sc_;
		width = width_;
		height = height_;
		tilePrefab = tilePrefab_;
		sectionPrefab = sectionPrefab_;
		floor = floor_;
		
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
				Tile newTile = tileGO.GetComponent<Tile>();
				Vector3 devPos = new Vector3(i*tileWidth, 0, j*tileWidth);
				
				newTile.gameObject.transform.parent = floor.transform;
				newTile.SetPos(devPos);
				newTile.SetMap(this);
				
				tiles[i].Add(newTile);
			}
		}
	}
	
	public void CreateFloorFromData(MapData md)
	{
		for(int i=0; i<width; i++)
		{
			tiles.Add(new List<Tile>());
			for(int j=0; j<height; j++)
			{
				tileGO = GameObject.Instantiate(tilePrefab);
				Tile newTile = tileGO.GetComponent<Tile>();
				TileData t = md.mapTiles[i*height + j];
				
				newTile.gameObject.transform.parent = floor.transform;
				
				newTile.SetPos(t.pos);
				newTile.SetMap(this);
				if(t.used)
				{
					newTile.AddWall();
				}
				
				if(t.section.ID != -1)
				{
					bool found = false;
					
					// Compruebo si la sección ya está creada
					foreach(Section s in sections)
					{
						if(!found && s.GetSectionID()==t.section.ID)
						{
							found = true;
							s.AddTile(newTile);
							newTile.SetSection(s);
						}
					}
					
					if(!found)
					{
						// Creo una nueva sección
						sectionGO = GameObject.Instantiate(sectionPrefab);
						Section newSection = sectionGO.GetComponent<Section>();
						newSection.InitSection(this, sc, t.section.ID);
						// Añado la tile a la lista de tiles de la sección
						newSection.AddTile(newTile);
						// Asigno la nueva sección a la nueva tile
						newTile.SetSection(newSection);
						
						if(t.section.isCP)
						{
							newSection.SetIsCP(true);
							cpSections.Add(newSection);
						}
						sections.Add(newSection);
					}
					
				}
				
				tiles[i].Add(newTile);
			}
		}
	}
	
	public void ReColorCPs()
	{
		foreach(Section cp in cpSections)
		{
			cp.PaintCP();
		}
	}
	
	public GameObject GetMapElement()
	{
		RaycastHit hit;
		Ray ray = sc.GetCam().ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray.origin, ray.direction, out hit, 300f);
		
		actualClickedPos = new Vector3(Mathf.Floor(hit.point.x+0.5f),0.5f,Mathf.Floor(hit.point.z+0.5f));
		
		// Si hay una pared la destruyo
		if (hit.transform.gameObject.name == "Wall(Clone)") {
			hit.transform.gameObject.transform.parent.GetComponent<Tile>().DestroyWall();
		}
		
		return hit.transform.gameObject;
	}
	
	public void BuildNavMesh()
	{ 
		floor.GetComponent<NavMeshSurface>().BuildNavMesh();
		
		// Set sections positions
		foreach(Section cp in cpSections)
		{
			cp.SetPos(cp.GetTiles()[cp.GetTiles().Count*3/5].gameObject.transform);
		}
	}
	
	public void CreateRoom()
	{
		int minX, minY, maxX, maxY;
		minX = Mathf.RoundToInt(Mathf.Min(initClickedPos.x, endClickedPos.x));
		maxX = Mathf.RoundToInt(Mathf.Max(initClickedPos.x, endClickedPos.x));
		minY = Mathf.RoundToInt(Mathf.Min(initClickedPos.z, endClickedPos.z));
		maxY = Mathf.RoundToInt(Mathf.Max(initClickedPos.z, endClickedPos.z));
		
		int currentSectionID = sections.Count;
		Section currentSection = null;
		Section lastSection = null;
		
		if(maxX-minX>1 && maxY-minY>1)
		{
			// Crear seccion
			Print("Se ha creado una nueva sección!");
			sectionGO = GameObject.Instantiate(sectionPrefab);
			currentSection = sectionGO.GetComponent<Section>();
			currentSection.InitSection(this, sc, currentSectionID);
			sections.Add(currentSection);
			
		}
		
		// Para y=0, y=m
		for(int i=minX; i<=maxX; i++)
		{
			// Rellenar minY
			tiles[i][minY].AddWall(new Vector3(i, initClickedPos.y, minY));
			
			if(minY!=maxY)
			{
				// Rellenar maxY
				tiles[i][maxY].AddWall(new Vector3(i, initClickedPos.y, maxY));
			}
		}
		
		// Para el interior
		for(int j=minY+1; j<maxY; j++)
		{				
			// Rellenar minX
			tiles[minX][j].AddWall(new Vector3(minX, initClickedPos.y, j));
			
			if(minX!=maxX)
			{
				// Rellenar maxX
				tiles[maxX][j].AddWall(new Vector3(maxX, initClickedPos.y, j));
			}
			
			for(int k=minX+1; k<maxX; k++)
			{
				// Asignar a seccion
				lastSection = tiles[k][j].GetSection();
				if(lastSection != null)
				{
					lastSection.RemoveTile(tiles[k][j]);
					if(lastSection.GetTiles().Count<1)
					{ 
						Print("Se ha eliminado una sección");
						if(lastSection.GetIsCP()) cpSections.Remove(lastSection);
						sections.Remove(lastSection);
						
					}
				}
				
				tiles[k][j].SetSection(currentSection);
				tiles[k][j].DestroyWall();
				currentSection.AddTile(tiles[k][j]);
			}
		}
	}
	
	// Calcula el cp más cercano para la persona
	public Vector3 GetClosestCP(PersonBehavior p)
	{
		int tileIndex;
		Transform auxTransform;
		NavMeshHit hit;
		
		NavMeshAgent agent = p.GetComponent<NavMeshAgent>();
		NavMeshPath path = new NavMeshPath();
		float minDistance = Mathf.Infinity;
		Vector3 finalPos = Vector3.zero;
		Section finalSection = null;
		
		foreach(Section s in cpSections)
		{
			tileIndex = s.GetTiles().Count*3/5;
			auxTransform = s.GetTiles()[tileIndex].gameObject.transform;
			
			if (!s.IsFull() && NavMesh.SamplePosition(auxTransform.position, out hit, 3.0f, NavMesh.AllAreas)) 
			{
				agent.CalculatePath(hit.position, path);
				agent.SetPath(path);
				agent.isStopped = true;
				
				if(agent.pathStatus == NavMeshPathStatus.PathComplete)
				{
					float auxMinDist = PathDistance(agent);
					if(auxMinDist<minDistance)
					{
						minDistance = auxMinDist;
						finalPos = hit.position;
						finalSection = s;
					}
				}
			}
		}
		
		if(!finalSection.IsFull()) finalSection.AddPerson(p);
		return finalPos;
	}
	
	public float PathDistance(NavMeshAgent agent)
	{
		
		float distance = 0.0f;
		Vector3[] corners = agent.path.corners;
		for (int c = 0; c < corners.Length - 1; ++c) {
			distance += Mathf.Abs((corners[c] - corners[c + 1]).magnitude);
		}
		
		//Debug.Log("here it goes the path distance "+distance);
		
		return distance;
		
	}
	
	public void CreateCP()
	{
		Tile selectedTile = GetMapElement().GetComponent<Tile>(); 
		if(selectedTile != null)
		{
			Section selectedSection = selectedTile.GetSection();
			if(selectedSection != null)
			{
				selectedSection.Clic();
			}
		}
	}
	
	// Devuelve la posicion en el mapa
	public Vector3 MouseOnGridPosition()
	{
		RaycastHit hit;
		Ray ray = sc.GetCam().ScreenPointToRay(Input.mousePosition);
		Physics.Raycast(ray.origin, ray.direction, out hit, 300f);
		
		Vector3 aux = new Vector3(Mathf.Floor(hit.point.x+0.5f),0.5f,Mathf.Floor(hit.point.z+0.5f));
		return aux + new Vector3(width/2, 0, height/2);
	}
	
	public void DeleteAll()
	{
		tiles.Clear();
		sections.Clear();
		cpSections.Clear();
	}
	
		
	#region GETTERS & SETTERS
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" MAP:  "+msg);}
	
}
