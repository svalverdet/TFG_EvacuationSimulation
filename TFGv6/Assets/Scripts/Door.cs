using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Door : MonoBehaviour
{
	private Tile tileParent;
	public List<Section> connections;
	
    void Awake()
    {
        connections = new List<Section>();
    }
	
	public void AddConnection(Section s_)
	{
		connections.Add(s_);
	}
	
	void OnMouseOver()
	{
		if(EventSystem.current.IsPointerOverGameObject()){
			return;
		}
		tileParent.GetMap().GetSceneController().ToggleText("Door");
	}
	
	void OnMouseExit()
	{
		tileParent.GetMap().GetSceneController().ToggleText("");
	}
	
	public void SetTileParent(Tile t_){ tileParent = t_;}
	public Tile GetTileParent(){ return tileParent;}
	public Vector3 GetPos(){return tileParent.GetPos();}

}
