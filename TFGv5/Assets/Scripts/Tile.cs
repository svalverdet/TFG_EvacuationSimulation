using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public GameObject wallPrefab;
	private GameObject wallGO;
	
	private Map map;
	private Section section = null;
	
	private Vector3 pos;
	private bool used = false;
	
	
    void Start()
    {
        transform.position = pos - new Vector3(map.width/2, 0, map.height/2);
    }
	
	public void AddWall(Vector3 destPos)
	{
		if(!used)
		{
			used = true;
			wallGO = GameObject.Instantiate(wallPrefab);
			wallGO.transform.parent = this.transform;
			wallGO.transform.position = destPos - new Vector3(map.width/2, -1, map.height/2);
		}
	}
	
	public void AddWall()
	{
		if(!used)
		{
			used = true;
			wallGO = GameObject.Instantiate(wallPrefab);
			wallGO.transform.parent = this.transform;
			wallGO.transform.position -= new Vector3(0, -.5f, 0);
		}
	}
	
	public void DestroyWall()
	{
		used = false;
		Destroy(wallGO);
	}
	
	
	
	#region GETTERS & SETTERS
	
	public bool 		GetUsed(){return used;}
	public Section 		GetSection(){return section;}
	public Vector3		GetPos(){return pos;}
	
	public void 		SetPos(Vector3 pos_){pos = pos_;}
	public void 		SetMap(Map map_){map = map_;}
	public void 		SetSection(Section newSection){section = newSection;}
	
	#endregion
	
	public void Print(string msg){Debug.Log(Random.Range(1000, 2000)+" TILE:  "+msg);}
}
