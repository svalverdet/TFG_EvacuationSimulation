using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
	private SceneController sc;
	
	// Zoom
	private float minSize = 5f;
	private float maxSize = 70f;
	private float sensitivity = 10f;
	
	// Panning
	private Vector3 mouseOrigin;
	private bool isPanning;
	private float panSpeed = 0.75f;
	
	void Update()
	{
		float size = Camera.main.orthographicSize;
		size -= Input.GetAxis("Mouse ScrollWheel") * sensitivity;
		size = Mathf.Clamp(size, minSize, maxSize);
		Camera.main.orthographicSize = size;
		
		if(sc.GetMode() == SceneController.Modes.CAM)
		{
			if(Input.GetMouseButtonDown(0))
			{
				mouseOrigin = Input.mousePosition;
				isPanning = true;
			}
			if (!Input.GetMouseButton(0)) isPanning=false;
			if (isPanning)
			{
				Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);
				Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
				transform.Translate(move, Space.Self);
			}
		}
	}
	
	public void Initialize(SceneController sc_)
	{
		sc = sc_;
	}
     
   
}
