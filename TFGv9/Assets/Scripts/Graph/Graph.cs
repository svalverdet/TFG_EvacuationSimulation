using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph: MonoBehaviour, IGraph, ISceneElement
{

	public GameObject nodePrefab, edgePrefab;

	private List<Node> nodes;
	private List<Edge> edges;
	private bool alreadyBaked = false;

	private int currentNodeID, currentEdgeID;

	private SceneController sc;
	private GraphEditor graphEditor;
	
	public void Setup(SceneController sc_)
	{
		sc = sc_;
		nodes = new List<Node>();
		edges = new List<Edge>();
		graphEditor = GetComponent<GraphEditor>();
		graphEditor.SetGraph(this);
	}
	
	#region IGraph

	public Node InsertNode(Section elem, Vector3 pos)
	{
		Node newNode = GameObject.Instantiate(nodePrefab, pos, Quaternion.Euler(0,0,0), this.transform).GetComponent<Node>();
		newNode.Setup(currentNodeID++,elem, pos, elem.GetIsCP());
		nodes.Add(newNode);
		
		sc.GetSceneInfo().numberOfNodes += 1;
		
		return newNode;
	}
	
	public Edge InsertEdge(Door elem, Node linkA, Node linkB)
	{
		Vector3 pos = this.transform.position;
		if(elem!=null){ pos = new Vector3 (elem.GetPos().x, this.transform.position.y, elem.GetPos().z);}

		Edge newEdge = GameObject.Instantiate(edgePrefab, pos, Quaternion.Euler(90f,0,0),this.transform).GetComponent<Edge>();
		newEdge.Setup(currentEdgeID++, elem, linkA, linkB);
		edges.Add(newEdge);
		linkA.AddAdjacentEdge(newEdge);
		linkB.AddAdjacentEdge(newEdge);

		newEdge.CalculateDistance();

		// Paint the edge
		newEdge.PaintEdge();

		sc.GetSceneInfo().numberOfEdges += 1;
		
		return newEdge;
	}
	
	public int NodesCount(){return nodes.Count;}
	public int EdgesCount(){return edges.Count;}
	public List<Node> GetNodes(){return nodes;}
	public List<Edge> GetEdges(){return edges;}
	
	public Node ContainsNode(Section elem)
	{
		foreach(Node node in nodes)
		{
			if(node.GetData().Equals(elem)) return node;
		}
		return null;
	}

	public List<Node> NodesInSection(Section elem)
	{
		List<Node> sectionNodes = new List<Node>();
		foreach(Node node in nodes)
		{
			if(node.GetData().Equals(elem)) sectionNodes.Add(node);
		}
		return sectionNodes;
	}
	
	public Edge ContainsEdge(Node linkA, Node linkB)
	{
		return linkA.ConnectedTo(linkB);
	}
	
	public bool DeleteNode(Section elem){return false;}

	public bool DeleteEdgeBtwNodes(Node linkA, Node linkB){return false;}

	public bool DeleteEdge(Edge e_)
	{
		e_.GetNodes()[0].RemoveAdjacentEdge(e_);
		e_.GetNodes()[1].RemoveAdjacentEdge(e_);

		edges.Remove(e_);
		GameObject.Destroy(e_.gameObject);

		sc.GetSceneInfo().numberOfEdges -= 1;

		return true;
	}
	
	
	public override string ToString()
	{
		string result = "";
		result += "There are "+nodes.Count+" nodes and "+edges.Count+" edges\nList of nodes:\n";
		
		foreach(Node node in nodes)
		{
			result+=node+", ";
		}
		
		result += "\n";
		
		foreach(Edge edge in edges)
		{
			result+=edge+"\n";
		}
		result += "\n";
		result += "\nShowing connections:\n";
		
		List<Node> adjacentNodes = null;
		
		foreach(Node node_ in nodes)
		{
			result+=node_+" is connected to ";
			adjacentNodes = GetAdjacentNodes(node_);
			foreach(Node n in adjacentNodes)
			{
				result += n+", ";
			}
			result+="\n";
		}
		
		return result;
	}
	
	
	public List<Node> GetAdjacentNodes(Node node_)
	{
		List<Node> adjacentNodes = new List<Node>();
		
		Node node = null;
		foreach(Edge edge in node_.GetAdjacentEdges())
		{
			node = edge.GetOtherNode(node_);
			if(node!=null) adjacentNodes.Add(node);
		}
		
		return adjacentNodes;
	}


	public Node GetClosestNode(Vector3 pos_, Section s_)
	{
		List<Node> sectionNodes = NodesInSection(s_);
		Node chosenNode = null;
		float maxDistance = Mathf.Infinity;
		foreach(Node n in sectionNodes)
		{
			float distance = (n.GetPos()-pos_).sqrMagnitude; 
			if(distance < maxDistance)
			{
				chosenNode = n;
				maxDistance = distance;
			}
		}

		return chosenNode;
	}
	
	public Node FindNodeByID(int id_)
	{
		return nodes.Find(x => x.GetID() == id_);
	}


	#endregion

	public void MouseEvent(Utils.MouseInputEvents mouseEvent)
	{
		graphEditor.MouseEvent(mouseEvent);
	}
	

	public void KeyboardEvent()
	{
		if(Input.GetKeyUp(KeyCode.Space) && !alreadyBaked)
		{
			// Bake
			CreateNodes();
			CreateEdges();
			alreadyBaked = true;
		}
		else if(Input.GetKeyUp(KeyCode.O))
		{
			graphEditor.NextState();
		}
	}
	public void DoAction(Utils.Actions action_)
	{
		if(action_ == Utils.Actions.bake && !alreadyBaked)
		{ 
			CreateNodes(); 
			CreateEdges(); 
			alreadyBaked = true;
		} 
	}
	private void CreateNodes()
	{
		foreach(Section s in sc.GetMap().GetSections())
		{
			InsertNode(s, new Vector3 (s.GetPos().x, this.transform.position.y, s.GetPos().z));
		}
	}
	private void CreateEdges()
	{
		Section[] connections;
		foreach(Door d in sc.GetMap().GetDoors())
		{
			connections = d.GetConnections();
			Node nA = ContainsNode(connections[0]);
			Node nB = ContainsNode(connections[1]);
			Edge newEdge = InsertEdge(d, nA, nB);
		}
	}

	

	public void DeleteAll()
	{
		foreach(Node n in nodes)
		{
			GameObject.Destroy(n.gameObject);
		}
		nodes.Clear();

		foreach(Edge e in edges)
		{
			GameObject.Destroy(e.gameObject);
		}
		edges.Clear();

		alreadyBaked = false;
	}

	public void Save()
	{
		List<NodeData> nodesDataList = new List<NodeData>();
		List<EdgeData> edgesDataList = new List<EdgeData>();

		foreach(Node n in nodes)
		{
			nodesDataList.Add(n.TransformData());
		}

		foreach(Edge e in edges)
		{
			edgesDataList.Add(e.TransformData());
		}

		DataController.SetGraphData(new GraphData(nodesDataList, edgesDataList));
	}

	public void Load()
	{
		GraphData gd = DataController.GetGraphData();
		Node newNode;
		Edge newEdge;

		// Nodes
		foreach(NodeData n in gd.nodes)
		{
			newNode = InsertNode(sc.GetMap().FindSectionByID(n.sectionID), n.pos);
			newNode.SetID(n.ID);
			newNode.SetCapacity(n.capacity);
		}

		// Edges
		foreach(EdgeData e in gd.edges)
		{
			newEdge = InsertEdge(sc.GetMap().FindDoorByID(e.doorID), FindNodeByID(e.nodes[0]), FindNodeByID(e.nodes[1]));
			newEdge.SetID(e.ID);
			newEdge.SetDistance(e.distance);
		}
		
		alreadyBaked = true;
	}


	public SceneController GetSc(){ return sc;}

}
