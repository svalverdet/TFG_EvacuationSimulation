using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Node: MonoBehaviour
{
	[SerializeField] private Section data;
	[SerializeField] private List<Edge> adjacentEdges;

	[SerializeField] private int ID;
	[SerializeField] private Vector3 pos;
	[SerializeField] private int capacity;
	[SerializeField] private bool isCP;

	public TMP_Text myText;
	
	
	public void Setup(int ID_, Section data_, Vector3 pos_, bool isCP_)
	{
		name = "Node"+ID_;
		ID = ID_;
		data = data_;
		pos = pos_;
		isCP = isCP_;
		adjacentEdges = new List<Edge>();
		myText.text = "node "+ID;
	}

	public NodeData TransformData()
	{
		List<int> adjacentEdgesIDs = new List<int>();
		foreach(Edge e in adjacentEdges)
		{
			adjacentEdgesIDs.Add(e.GetID());
		}
		return new NodeData(ID, data.GetID(), capacity, adjacentEdgesIDs, pos, isCP);
	}
	
	public Edge ConnectedTo(Node n_){return adjacentEdges.Find(x => (x.GetNodes()[0] == n_ || x.GetNodes()[1] == n_) ); }


	public Section GetData() {return data;}
	public void SetData(Section data_) {data = data_;}
	public override string ToString(){ return data.ToString();}
	public Vector3 GetPos() { return pos;}
	public void SetPos(Vector3 pos_){ pos = pos_;}
	public int GetID(){return ID;}
	public bool GetIsCP(){return isCP;}
	
	public List<Edge> GetAdjacentEdges(){ return adjacentEdges;}
	public void SetAdjacentEdges(List<Edge> adjacentEdges_){ adjacentEdges = adjacentEdges_;}
	public void AddAdjacentEdge(Edge edge_){ adjacentEdges.Add(edge_);}
	public void RemoveAdjacentEdge(Edge e_){adjacentEdges.Remove(e_);}
	public void SetID(int id_){ ID = id_; myText.text = "node "+ID; name = "Node"+ID;}
	public void SetCapacity(int c_){capacity = c_;}
}
