using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SectionEditor : MonoBehaviour
{
	public TMP_Text selectedSectionText;
	public Toggle isCP; 
	public Slider capacity;
	public TMP_Text capacityText;
	
	private Section selectedSection;
	private int lastSectionID = -1;
	
    public void SetSection(Section s_)
	{
		if(!this.gameObject.activeSelf) this.gameObject.SetActive(true);
		
		selectedSection = s_;
		selectedSectionText.text = "Selected section: "+s_.GetSectionID();
		isCP.isOn = s_.GetIsCP();
		capacity.maxValue = s_.GetMaxCapacity();
		capacity.value = s_.currentCapacity;
		capacityText.text = "Capacity: "+s_.currentCapacity;
		
		lastSectionID = s_.GetSectionID();
	}
	
	public void ChangeIsCP()
	{
		if(selectedSection != null && (lastSectionID==selectedSection.GetSectionID()))
		{
			selectedSection.SetIsCP(isCP.isOn);
		}
	}
	
	public void ChangeCapacity()
	{
		if(selectedSection != null && (lastSectionID==selectedSection.GetSectionID()))
		{
			capacityText.text = "Capacity: "+capacity.value;
			selectedSection.currentCapacity = (int)capacity.value;
		}
	}
}
