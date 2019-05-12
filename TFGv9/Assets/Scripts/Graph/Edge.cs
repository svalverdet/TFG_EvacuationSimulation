using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge: MonoBehaviour
{
	[SerializeField] private int ID;
	[SerializeField] private Door data;
	[SerializeField] private Node[] nodes = new Node[2];
	[SerializeField] private float distance;

	public void Setup(int ID_, Door data_, Node nodeA_, Node nodeB_)
	{
		name = "Edge";
		ID = ID_;
		data = data_;
		nodes[0] = nodeA_;
		nodes[1] = nodeB_;
		
	}
	
	public void CalculateDistance()
	{
		distance = Utils.CalculateDistance(nodes[0].GetPos(), nodes[1].GetPos());

	}
	
	public Door GetData() {return data;}
	public float GetDistance(){return distance;}
	public void SetData(Door data_) {data = data_;}
	
	
	public Node[] GetNodes()
	{
		return nodes;
	}
	
	public Node GetOtherNode(Node node_)
	{
		if(node_.Equals(nodes[0]))	return nodes[1];
		else if(node_.Equals(nodes[1])) return nodes[0];
		else return null;
	}

	public void PaintEdge()
	{
		Vector3 dir = (nodes[0].transform.position - nodes[1].transform.position).normalized;
		if(dir.x+dir.y+dir.z != 0f)
		{
			float dist = Vector3.Distance(nodes[0].transform.position, nodes[1].transform.position);
			Vector3 newScale = new Vector3 (dist*7, transform.localScale.y, transform.localScale.z);
			transform.localScale = newScale;
			transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0, Utils.GetAngleFromVectorFloat(dir));
			transform.position = nodes[1].transform.position + dir * dist * 0.5f;
		}
	}

	public EdgeData TransformData()
	{
		int[] myNodes = new int[2];
		myNodes[0] = nodes[0].GetID();
		myNodes[1] = nodes[1].GetID();
		if(data == null){return new EdgeData(ID, -1, distance, myNodes);}
		else return new EdgeData(ID, data.GetID(), distance, myNodes);
		
	}
	
	public void SetNodes(Node nodeA_, Node nodeB_) { nodes[0] = nodeA_; nodes[1] = nodeB_;}
	public int GetID(){ return ID;}

	public void SetID(int id_){ID = id_;}
	public void SetDistance(float d_){distance = d_;}

	public override string ToString(){return "Edge"+ID+" with nodes "+nodes[0].GetID()+" and "+nodes[1].GetID();}


}
