using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
	List<Button> menuButtons;
	List<GameObject> subMenus;
	public PersonEditor personEdit;
	public SectionEditor sectionEdit;
	
    void Start()
    {
		menuButtons = new List<Button>();
		subMenus = new List<GameObject>();
        for (int i = 0; i < gameObject.transform.childCount; i++) 
		{
			Button b = gameObject.transform.GetChild(i).GetComponent<Button>();
			menuButtons.Add(b);
			if(b.transform.childCount > 1)
			{
				subMenus.Add(b.transform.GetChild(1).gameObject);
			}
		}
    }

    public void ClickedButton(int subMenuIndex)
	{
		for(int i=0; i<subMenus.Count; i++)
		{
			if(i!=subMenuIndex)
			{
				subMenus[i].SetActive(false);
			}
			else{
				subMenus[i].SetActive(!subMenus[i].activeSelf);
			}
		}
		
		if(subMenuIndex==2 && subMenus[subMenuIndex].activeSelf) 
		{
			personEdit.gameObject.SetActive(true);
		}
	}
	
	public GameObject GetSubMenu(int subMenu_){ return subMenus[subMenu_];}
}
