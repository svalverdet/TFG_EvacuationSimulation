using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextHolder : MonoBehaviour
{
	
	public TMP_Text label;
	private Quaternion initialRot;
	
    // Start is called before the first frame update
    void Start()
    {
        initialRot = label.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        label.transform.rotation = initialRot;
    }
}
