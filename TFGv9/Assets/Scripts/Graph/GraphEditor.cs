using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphEditor : MonoBehaviour
{
    private Graph graph;
    
    private Node startNode;
    private enum GraphEditModes{move, create, edit}
    private GraphEditModes editMode = GraphEditModes.move;

    public void MouseEvent(Utils.MouseInputEvents mouseEvent)
    {
        KeyValuePair<Vector3,GameObject> info = Utils.ScreenToWorld();

        if(mouseEvent == Utils.MouseInputEvents.left_down)
        {
            if(info.Value != null && info.Value.tag == "node")
            {
                startNode = info.Value.GetComponent<Node>();
            }
        }

        switch(editMode)
        {
            case GraphEditModes.move: MoveEvent(mouseEvent); break;
            case GraphEditModes.create: CreateEvent(mouseEvent); break;
            case GraphEditModes.edit: EditEvent(mouseEvent); break;
            default: break;
        }
    }

    private void MoveEvent(Utils.MouseInputEvents mouseEvent)
    {
        KeyValuePair<Vector3,GameObject> info = Utils.ScreenToWorld();

        if(mouseEvent == Utils.MouseInputEvents.left_up && startNode != null) 
        {
            if(info.Key.x != Mathf.Infinity)
            {
                Vector3 mapPos = graph.GetSc().GetMap().ToMapPos(info.Key);
                Section s = graph.GetSc().GetMap().IsPosSection(mapPos);
                if(s != null)
                {
                    startNode.SetPos(new Vector3(mapPos.x, this.transform.position.y, mapPos.z));
                    startNode.SetData(s);
                }
                else startNode.transform.position = startNode.GetPos();
            }
            else
            {
                startNode.transform.position = startNode.GetPos();
            }

            foreach(Edge e in startNode.GetAdjacentEdges())
            {
                e.PaintEdge();
                e.CalculateDistance();
            }
            startNode = null;
        }
    }

    private void CreateEvent(Utils.MouseInputEvents mouseEvent)
    {
        KeyValuePair<Vector3,GameObject> info = Utils.ScreenToWorld();

        if(mouseEvent == Utils.MouseInputEvents.left_up && info.Value != null)
        {
            if(info.Value.name == "Tile")
            {
                Tile t = info.Value.GetComponent<Tile>();
                Vector3 newNodePos = new Vector3(t.GetPos().x, this.transform.position.y, t.GetPos().z);
                graph.InsertNode(t.GetSection(), newNodePos);
            }
            else if(info.Value.tag == "node" && startNode != null)
            {

                Node finishNode = info.Value.GetComponent<Node>();
                if(finishNode != startNode)
                {
                    Edge edge = graph.ContainsEdge(startNode, finishNode);
                    if(edge != null) graph.DeleteEdge(edge);
                    else edge = graph.InsertEdge(null, startNode, finishNode);
                }
            }
            startNode = null;
        } 
    }

    private void EditEvent(Utils.MouseInputEvents mouseEvent)
    {

    }

    public void NextState()
    {
        editMode = (GraphEditModes)(((int) editMode+1)%3);
        Utils.Print("Now "+editMode);
    }

// No va aqui??
    void Update()
    {
        if(editMode == GraphEditModes.move && startNode != null)
        {
            Vector3 destPos = Utils.ScreenToWorld().Key;
            if(destPos.x != Mathf.Infinity)
            {
                Vector3 mapPos = graph.GetSc().GetMap().ToMapPos(destPos);
                startNode.transform.position = new Vector3(mapPos.x, this.transform.position.y, mapPos.z);

                foreach(Edge e in startNode.GetAdjacentEdges())
                {
                    e.PaintEdge();
                }
            }
        }
    }


    public void SetGraph(Graph graph_){ graph = graph_;}
    
}
