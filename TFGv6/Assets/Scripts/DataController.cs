using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DataController
{
	public MapData mapData;
	private string dataProjectFilePath = "/SavedData/data.json";
	
	private SceneController sc;
	public bool loadedData, loadedMap;
	
	
	public DataController(SceneController sc_)
	{
		sc = sc_;
	}
	
    public void LoadData()
	{
		
		string filePath = Application.dataPath + dataProjectFilePath;
		
		if(File.Exists(filePath))
		{
			loadedData = true;
			string dataAsJson = File.ReadAllText(filePath);
			mapData = JsonUtility.FromJson<MapData>(dataAsJson);
		}
		else
		{
			Debug.Log("File does not exist");
		}
	}
	
	public void LoadMap()
	{
		if(!loadedData)
		{
			LoadData();
		}
		
		sc.DeleteAll();
		
		sc.GetMap().width = mapData.width;
		sc.GetMap().height = mapData.height;
		
		sc.GetMap().CreateFloorFromData(mapData);
		
		loadedMap = true;
	}
	
	public void LoadPeople()
	{
		if(loadedMap)
		{
			//sc.SetPeopleFromData(mapData.mapPeople);
		}
		else Debug.Log("No map has been loaded");
	}
	
	public void SaveData()
	{
		string dataAsJson = JsonUtility.ToJson(mapData);
		Debug.Log(dataAsJson);
		string filePath = Application.dataPath + dataProjectFilePath;
		File.WriteAllText(filePath, dataAsJson);
	}
}

[System.Serializable]
public class MapData
{
	public List<TileData> mapTiles;
	public int width, height;
	public List<PersonData> mapPeople;
	
	public MapData(List<TileData> mapTiles_, int width_, int height_, List<PersonData> mapPeople_)
	{
		mapTiles = mapTiles_;
		width = width_;
		height = height_;
		mapPeople = mapPeople_;
	}
}

[System.Serializable]
public class TileData
{
	public Vector3 pos;
	public bool hasWall;
	public bool hasDoor;
	//public SectionData section;
	
	public TileData(Tile t)
	{
		pos = t.GetPos();
		hasWall = t.HasWall();
		hasDoor = t.HasDoor();
		//if(t.GetSection() != null) section = new SectionData(t.GetSection());
		//else section = new SectionData();
	}
}

/*
[System.Serializable]
public class SectionData
{
	public int ID = -1;
	public int maxCapacity;
	public bool isCP;
	
	public SectionData(Section s_)
	{
		ID = s_.GetSectionID();
		maxCapacity = s_.GetMaxCapacity();
		isCP = s_.GetIsCP();
	}
	
	public SectionData()
	{
		ID = -1;
	}
}
*/

[System.Serializable]
public class PersonData
{
	public Vector3 initPos;
	public int ID;
	public string name;
	
	public PersonData(PersonBehavior p_)
	{
		initPos = p_.GetInitPos();
		ID = p_.GetID();
		name = p_.firstName;
	}
	
}