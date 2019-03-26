using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableMenu : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	
	private bool dragging = false;
	
	public void ToggleActive()
	{
		gameObject.SetActive(!gameObject.activeSelf);
	}
	
	public void OnPointerDown(PointerEventData pointerEventData)
    {
        dragging = true;
    }
	
	void Update()
	{
		if(dragging)
		{
			gameObject.transform.parent.position = Input.mousePosition;
		}
	}

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        dragging = false;
    }
	
}
