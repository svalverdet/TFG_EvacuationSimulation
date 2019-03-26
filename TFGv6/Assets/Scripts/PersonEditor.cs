using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PersonEditor : MonoBehaviour
{
    public TMP_Text selectedPersonText, ageText;
	public InputField pName;
	public Slider ageSlider;
	public Dropdown typeDropdown;
	
	private PersonBehavior selectedPerson;
	private int lastPersonID = -1;
	
	public void SetPerson(PersonBehavior p_)
	{
		if(!this.gameObject.activeSelf) this.gameObject.SetActive(true);
		
		selectedPerson = p_;
		selectedPersonText.text = "Selected person: "+p_.GetID();
		pName.text = p_.firstName;
		ageText.text = "Age: "+p_.age;
		ageSlider.value = p_.age;
		typeDropdown.value = (int)p_.type;
		
		lastPersonID = p_.GetID();
	}
	
	public void ChangedName()
	{
		if(selectedPerson != null && (lastPersonID==selectedPerson.GetID()))
		{
			selectedPerson.SetName(pName.text);
		}
	}
	
	public void ChangedAge()
	{
		if(selectedPerson != null && (lastPersonID==selectedPerson.GetID()))
		{
			ageText.text = "Age: "+ageSlider.value;
			selectedPerson.age = (int)ageSlider.value;
		}
	}
	
	public void ChangedType()
	{
		if(selectedPerson != null && (lastPersonID==selectedPerson.GetID()))
		{
			selectedPerson.type = (PersonBehavior.PersonType)typeDropdown.value;
		}
	}
	
}
