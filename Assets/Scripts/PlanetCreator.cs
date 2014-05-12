using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlanetCreator : MonoBehaviour {

	public GameObject PlanetPrefab;
	public Transform Parent;
	public int NumPlanets;
	public float MinRadius;
	public float MaxRadius;

	public int PlanetPosTries;

	List<Transform> _planets = new List<Transform>();

	// Use this for initialization
	void Awake () {

		Debug.Log("height: " + Screen.height);
		MinRadius *= Screen.height / 768.0f;
		MaxRadius *= Screen.height / 768.0f;
		CreatePlanets();
	
	}

	void CreatePlanets()
	{

		float z = 0;//-Camera.main.transform.position.z;
		Vector3 minPos = Camera.main.ScreenToWorldPoint(new Vector3(0,0,z));
		Vector3 maxPos = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width,Screen.height,z));
		minPos += new Vector3(MaxRadius/2.0f,MaxRadius/2.0f,0);
		maxPos -= new Vector3(MaxRadius/2.0f,MaxRadius/2.0f,0);
		
		for (int i=0; i < NumPlanets; i++)
		{
			GameObject planetObj = (GameObject)Instantiate(PlanetPrefab,GetBestPlanetPosition(minPos,maxPos,z),Quaternion.identity);
			PlayerType playerType = (i < 2) ? PlayerType.Player1 : (i < 4) ? PlayerType.Computer : PlayerType.Neutral;
			planetObj.GetComponent<Planet>().Initialize(playerType);

			float radius = Random.Range(MinRadius,MaxRadius);
			planetObj.transform.localScale = new Vector3(radius,radius,radius);
			_planets.Add(planetObj.transform);
			planetObj.transform.parent = Parent;
		}
	}



	Vector3 GetBestPlanetPosition(Vector3 minPos, Vector3 maxPos, float z)
	{
		Vector3 bestPos = new Vector3(Random.Range(minPos.x,maxPos.x),Random.Range(minPos.y,maxPos.y),z);
		float biggestMinDist = float.MinValue;


		for (int j=0; j < PlanetPosTries; j++)
		{
			Vector3 pos = new Vector3(Random.Range(minPos.x,maxPos.x),Random.Range(minPos.y,maxPos.y),z);

			float minDist = float.MaxValue;


			foreach (Transform planet in _planets)
			{
				float dist = Vector3.Distance(pos,planet.position);

				if (dist < minDist)
				{
					minDist = dist;
				}
			}

			if (minDist > biggestMinDist)
			{

				biggestMinDist = minDist;
				bestPos = pos;
			}

		}

		return bestPos;
	}



}
