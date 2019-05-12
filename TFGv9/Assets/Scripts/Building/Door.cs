using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class Door : MonoBehaviour
{
	private int ID;
	private List<Tile> tiles;
	private Section connectionA, connectionB;

	public void Setup(int id_, Tile t_)
	{
		tiles = new List<Tile>();
		ID = id_;
		tiles.Add(t_);
	}

	public void AddConnection(Section s_)
	{
		if(connectionA==null) connectionA = s_;
		else if(connectionB==null) connectionB = s_;
		else Utils.Print("ERROR");
	}
	
	public void ClearConnections()
	{
		connectionA = null; connectionB = null;
	}


	public int GetID(){ return ID;}
	public Vector3 GetPos(){return tiles[0].GetPos();}
	public Section[] GetConnections(){Section[] aux = new Section[2]; aux[0] = connectionA; aux[1] = connectionB; return aux;}

	public void SetID(int ID_){ID = ID_;}
}











// public class Door : MonoBehaviour
// {
// 	private int ID;
// 	private Tile tileParent;
// 	private Map map;
// 	//public List<Section> connections;
// 	private Section connectionA, connectionB;
	
//     void Awake()
//     {
//         //connections = new List<Section>();
//     }
	
// 	public void Init(int id_, Tile t_, Map map_){ID = id_;tileParent = t_;map = map_;}
	
// 	public void AddConnection(Section s_)
// 	{
// 		//connections.Add(s_);
// 		if(connectionA==null) connectionA = s_;
// 		else if(connectionB==null) connectionB = s_;
// 		else Debug.Log("ERROR");
// 	}
	
// 	public void ClearConnections()
// 	{
// 		connectionA = null; connectionB = null;
// 	}
	
// 	void OnMouseOver()
// 	{
// 		if(EventSystem.current.IsPointerOverGameObject()){
// 			return;
// 		}
// 		tileParent.GetMap().GetSceneController().ToggleText("Door"+ID);
// 	}
	
// 	void OnMouseExit()
// 	{
// 		tileParent.GetMap().GetSceneController().ToggleText("");
// 	}
	
// 	#region GETTERS & SETTERS
	
// 	public int GetID(){return ID;}
// 	public Map GetMap(){return map;}
// 	public Tile GetTileParent(){ return tileParent;}
// 	public Vector3 GetPos(){return tileParent.GetPos();}
// 	public Section[] GetConnections(){Section[] aux = new Section[2]; aux[0] = connectionA; aux[1] = connectionB; return aux;}
	
// 	public void SetTileParent(Tile t_){ tileParent = t_;}
	
// 	#endregion
	
// 	public override string ToString(){return "Door"+ID+" connects "+connectionA.ToString()+" and "+connectionB.ToString();}

// }
