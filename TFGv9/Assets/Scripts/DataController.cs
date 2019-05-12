using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class DataController
{
	private static string dataProjectFilePath = "/SavedData/", dataFormat = ".json";
	private static string projectName;
	private static string dataMapFilename = "_topology", dataGraphFilename = "_graph", 
	dataPeopleFilename = "_people", dataPathsFilename = "_paths", dataInfoFilename = "_info";

	private static MapData mapData;
	private static PeopleData peopleData;
	private static GraphData graphData;
	private static PathsData pathsData;
	private static SceneInfo infoData;

	private static T LoadData<T>(string file_)
	{
		string filePath = Application.dataPath + dataProjectFilePath + projectName + file_ + dataFormat;
		
		if(File.Exists(filePath))
		{
			string dataAsJson = File.ReadAllText(filePath);
			return JsonUtility.FromJson<T>(dataAsJson);
		}
		else{ Debug.Log("File does not exist"); return default(T);}
	}

	public static void SaveData<T>(string file_, T data_)
	{
		string dataAsJson = JsonUtility.ToJson(data_);
		Debug.Log(dataAsJson);
		string filePath = Application.dataPath + dataProjectFilePath + projectName + file_ + dataFormat;
		File.WriteAllText(filePath, dataAsJson);
	}


	#region all save-load
	public static void LoadMap()
	{
		mapData = LoadData<MapData>(dataMapFilename);
	}

	public static void SaveMap()
	{
		SaveData<MapData>(dataMapFilename, mapData);
	}

	public static void LoadGraph()
	{
		graphData = LoadData<GraphData>(dataGraphFilename);
	}

	public static void SaveGraph()
	{
		SaveData<GraphData>(dataGraphFilename, graphData);
	}

	public static void LoadPeople()
	{
		peopleData = LoadData<PeopleData>(dataPeopleFilename);
	}

	public static void SavePeople()
	{
		SaveData<PeopleData>(dataPeopleFilename, peopleData);
	}

	public static void LoadPaths()
	{
		pathsData = LoadData<PathsData>(dataPathsFilename);
	}

	public static void SavePaths()
	{
		SaveData<PathsData>(dataPathsFilename, pathsData);
	}

	public static void LoadInfo()
	{
		infoData = LoadData<SceneInfo>(dataInfoFilename);
	}

	public static void SaveInfo()
	{
		SaveData<SceneInfo>(dataInfoFilename, infoData);
	}

	#endregion

	public static MapData GetMapData(){return mapData;}
	public static GraphData GetGraphData() {return graphData;}
	public static PeopleData GetPeopleData(){return peopleData;}
	public static PathsData GetPathsData(){return pathsData;}
	public static SceneInfo GetInfoData(){return infoData;}

	public static void SetMapData(MapData md_){ mapData = md_;}
	public static void SetGraphData(GraphData gd_){ graphData = gd_;}
	public static void SetPeopleData(PeopleData pd_){ peopleData = pd_;}
	public static void SetPathsData(PathsData pd_){ pathsData = pd_;}
	public static void SetInfoData(SceneInfo id_){ infoData = id_;}

	public static void SetProjectName(string n_){ projectName = n_;}
}


[System.Serializable]
public class MapData
{
	public int width, height;
	public List<Vector3> walls;
	public List<SectionData> sections;
	public List<DoorData> doors;
	
	public MapData(int width_, int height_, List<Vector3> w_, List<SectionData> s_, List<DoorData> d_)
	{
		width = width_; height = height_; walls = w_; sections = s_; doors = d_;
	}
}

[System.Serializable]
public class SectionData
{
	public int ID, maxCapacity, currentCapacity;
	public Vector3 pos;
	public bool isCP;
	public List<Vector3> tiles;
	public List<Vector3> corners;

	public SectionData(int ID_, int maxCapacity_, int currentCapacity_, Vector3 pos_, bool isCP_, List<Vector3> tiles_, List<Vector3> corners_)
	{
		ID = ID_;
		maxCapacity = maxCapacity_;
		currentCapacity = currentCapacity_;
		pos = pos_;
		isCP = isCP_;
		tiles = tiles_;
		corners = corners_;
	}
}

[System.Serializable]
public class DoorData
{
	public int ID, IDconnA, IDconnB;
	public Vector3 pos;

	public DoorData(int ID_, int IDconnA_, int IDconnB_, Vector3 pos_)
	{
		ID = ID_;
		IDconnA = IDconnA_;
		IDconnB = IDconnB_;
		pos = pos_;
	}
}

[System.Serializable]
public class PeopleData
{
	public List<PersonData> people;
	public PeopleData(List<PersonData> people_)
	{
		people = people_;
	}
}

[System.Serializable]
public class PersonData
{
	public int initNodeID;
	public int initSectionID;
	public Vector3 initPos;

	public int ID, priority, age, type, speedType;
	public string firstName;
	
	public PersonData(int ID_, int p_, int a_, int t_, int st_, string n_, int INID_, int ISID_, Vector3 ip_)
	{
		ID = ID_; priority = p_; age = a_; type = t_; speedType = st_; firstName = n_; 
		initNodeID = INID_; initSectionID = ISID_; initPos = ip_;
	}
}

[System.Serializable]
public class GraphData
{
	public List<NodeData> nodes; 
	public List<EdgeData> edges;

	public GraphData(List<NodeData> nodes_, List<EdgeData> edges_)
	{
		nodes = nodes_;
		edges = edges_;
	}
}

[System.Serializable]
public class NodeData
{
	public int sectionID, ID, capacity;
	public List<int> adjacentEdgesIDs;
	public Vector3 pos;
	public bool isCP;

	public NodeData(int ID_, int sID_, int c_, List<int> aeIDs_, Vector3 pos_, bool isCP_)
	{
		ID = ID_; sectionID = sID_; capacity = c_; adjacentEdgesIDs = aeIDs_; pos = pos_; isCP = isCP_;
	}
}

[System.Serializable]
public class EdgeData
{
	public int ID, doorID;
	public float distance;
	public int[] nodes;
	
	public EdgeData(int ID_, int dID_, float d_, int[] nodes_)
	{
		ID = ID_; doorID = dID_; distance = d_; nodes = nodes_;
	}
}

[System.Serializable]
public class PathsData
{
	public List<PathData> paths;

	public PathsData(List<PathData> p_)
	{
		paths = p_;
	}
}

[System.Serializable]
public class PathData
{
	public int personID;
	public List<int> nodes;

	public PathData(int pid_, List<int> n_)
	{
		personID = pid_;
		nodes = n_;
	}
}