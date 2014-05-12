using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ComputerPlayer : MonoBehaviour {

	public GameObject MissilePathPrefab;
	public float MinFireTime = 1;
	public float MaxFireTime = 5;

	float _nextFireTime;
	float _currentTime;
	GameObject _lastTarget;

	void Start()
	{
		UpdateNextFireTime();
	}

	void UpdateNextFireTime()
	{
		_nextFireTime = Random.Range(MinFireTime,MaxFireTime);
	}

	// Update is called once per frame
	void Update () {

		_currentTime += Time.deltaTime;

		if (_currentTime > _nextFireTime)
		{
			Fire ();

			_currentTime = 0;
			UpdateNextFireTime();
		}
	
	}

	GameObject FindGoodTarget()
	{
		List<Planet> possileTargets = new List<Planet>();

		foreach (GameObject target in GameObject.FindGameObjectsWithTag("PlayerPlanet"))
			possileTargets.Add(target.GetComponent<Planet>());

		foreach (GameObject target in GameObject.FindGameObjectsWithTag("NeutralPlanet"))
			possileTargets.Add(target.GetComponent<Planet>());

	//	float min
		foreach (Planet target in possileTargets)
		{


		}
		return null;
	}

	void Fire()
	{
		GameObject[] computerPlanets = GameObject.FindGameObjectsWithTag("ComputerPlanet");
		GameObject[] playerPlanets = GameObject.FindGameObjectsWithTag("PlayerPlanet");
		GameObject[] neutralPlanets = GameObject.FindGameObjectsWithTag("NeutralPlanet");

		
		if (computerPlanets.Length == 0 || (playerPlanets.Length == 0 && neutralPlanets.Length == 0))
			return;

		int numPlanets = Random.Range(1,computerPlanets.Length);

		List<GameObject> computerPlanetList = new List<GameObject>(computerPlanets);

		for (int i=0; i < numPlanets; i++)
		{
			GameObject firingPlanet = computerPlanetList[Random.Range(0,computerPlanetList.Count)];
			GameObject targetPlanet =  ( (Random.Range(0,3) == 1 && playerPlanets.Length > 0) || neutralPlanets.Length == 0) ?  playerPlanets[Random.Range(0,playerPlanets.Length)] : 
				neutralPlanets[Random.Range(0,neutralPlanets.Length)];

			// just shoot the last planet they targeted if they have not taken it over yet
			if (_lastTarget != null && _lastTarget.tag != "ComputerPlanet")
				targetPlanet = _lastTarget;

			computerPlanetList.Remove(firingPlanet);

			SplineTrailRenderer path = ((GameObject)Instantiate(MissilePathPrefab,firingPlanet.transform.position,Quaternion.identity)).GetComponent<SplineTrailRenderer>();
			path.renderer.enabled = false;
			Missiles.FireMissiles(path,firingPlanet,targetPlanet);
			_lastTarget = targetPlanet;
		}

	}
}
