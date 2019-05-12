using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneInfo
{
	public float totalTime, maximumCPTime; // SC, MAP
	public List<PersonTime> timesPerPerson; // PEOPLE

	public int numberOfSections, numberOfCPs, numberOfNodes, numberOfEdges, numberOfPeople; // MAP, GRAPH, PEOPLE
	public float percentGL, percentMI, percentFM, percentBasic; // PEOPLE

    public SceneInfo(){}
	// public SceneInfo(float a, float b, List<PersonTime> c, int d, int e, int f, int g, int h, float i, float j, float k, float l)
	// {
	// 	totalTime = a; maximumCPTime = b; timesPerPerson = c;
	// 	numberOfSections = d; numberOfCPs = e; numberOfNodes = f; numberOfEdges = g; numberOfPeople = h;
	// 	percentGL = i; percentMI = j; percentFM = k; percentBasic = l;
	// }

    public void RecalculatePercents(int nGL, int nMI, int nFM, int nB)
    {
        percentGL = Round(nGL);
        percentMI = Round(nMI);
        percentFM = Round(nFM);
        percentBasic = Round(nB);
    }

    private float Round(float n)
    {
        return (float) System.Math.Round(n / numberOfPeople * 100f, 2);
    }
}

[System.Serializable]
public class PersonTime
{
	public int personID, CPID;
	public float time;

	public PersonTime(int pID_, int CPID_, float t_)
	{
		personID = pID_; CPID = CPID_; time = t_;
	}
}


