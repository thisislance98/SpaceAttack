using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missiles : MonoBehaviour {


	ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[1000];

	public float MinSpeed = 50;
	public float LastStrechSpeed = 50;
	public float LastStretchTime = .5f;
	public float TargetRadius = 5;
	public float EnergyToNumMissiles = 2;
	public float ClingToPathFactor = 10;

	float _totalEnergy;
	List<ParticleSystem.Particle> _particleList = new List<ParticleSystem.Particle>();
	SplineTrailRenderer _path;
	Planet _targetPlanet;
	Planet _firingPlanet;
	PlayerType _firingPlanetType;
	

	public List<ParticleSystem.Particle> GetParticles()
	{
		List <ParticleSystem.Particle> particleList = new List<ParticleSystem.Particle>();

		for (int i=0; i < particleSystem.GetParticles(_particles); i++)
		{
			particleList.Add(_particles[i]);

		}
		return particleList;
	}

	void UpdateParticleList(int size)
	{
		if (size > _particleList.Count)
		{
			_particleList.Clear();

			for (int i=0; i < size; i++)
			{
				_particleList.Add(_particles[i]);
			}
		}
		else if (size < _particleList.Count)
		{
			for (int i=0; i < (_particleList.Count-size); i++)
			{
				_particleList.RemoveAt(0);
				_targetPlanet.SendMessage("OnMissileHit",this);
			}

			if (_particleList.Count == 0)
				OnAllMisslesExploded();
		}
	}

	public float GetEnergyPerMissile()
	{
		return _totalEnergy / particleSystem.emissionRate;
	}

	void OnAllMisslesExploded()
	{
		Destroy(_path);
		Destroy(gameObject);
	}

	public void Fire(SplineTrailRenderer path, GameObject targetPlanet, GameObject firingPlanet)
	{
		_path = path;
		_targetPlanet = targetPlanet.GetComponent<Planet>();
		_firingPlanet = firingPlanet.GetComponent<Planet>();
		_firingPlanetType = _firingPlanet.GetPlayerType();

		_totalEnergy = firingPlanet.GetComponent<Planet>().OnFiredMissiles();
		particleSystem.emissionRate = _totalEnergy * EnergyToNumMissiles;

	}

	public Planet GetTargetPlanet()
	{
		return _targetPlanet;
	}

	public Planet GetFiringPlanet()
	{
		return _firingPlanet;
	}

	public PlayerType GetFiringPlanetType()
	{
		return _firingPlanetType;
	}

	// Update is called once per frame
	void Update () {

		if (_path == null || _path.spline.Length() == 0)
		{
			return;
		}


		float length = _path.spline.Length(); 
		Vector3 targetPos = _path.spline.FindPositionFromDistance(length) + (Random.insideUnitSphere * TargetRadius);

		int size = particleSystem.GetParticles(_particles);
		UpdateParticleList(size);


		// cycle through each particle and have the velocity flow in the direction of the path.
		for  (int i=0; i < size; i++)
		{
			float percent = 1 - (_particles[i].lifetime/_particles[i].startLifetime);

			float distance = Mathf.Clamp(percent * length, 0, length-0.1f);

			Vector3 forward = _path.spline.FindTangentFromDistance(distance);
			Vector3 position = _path.spline.FindPositionFromDistance(distance);


			if (_particles[i].lifetime > LastStretchTime)
			{

				if (_particles[i].velocity.magnitude > 0)
				{
					float minSpeed = Mathf.Max(_particles[i].velocity.magnitude, MinSpeed);
					Vector3 pathVect = (position - _particles[i].position).normalized * minSpeed;
					_particles[i].velocity = Vector3.RotateTowards(_particles[i].velocity, pathVect, Time.deltaTime * ClingToPathFactor, float.MaxValue);

				}
			}
			else // home into target in last .5 seconds
			{
				_particles[i].velocity = (targetPos - _particles[i].position) * LastStrechSpeed;
			}


			if (_particles[i].lifetime < .3f && Vector3.Distance(_particles[i].position,targetPos) > 1.5f)
			{
				_particles[i].lifetime = .3f;
			}
		}
		
		particleSystem.SetParticles (_particles,size);
	}
	

	public static void FireMissiles (SplineTrailRenderer missilePath, GameObject firingPlanet, GameObject targetPlanet)
	{
		if (missilePath.spline.Length() == 0)
		{
			PathManager.Instance.StartCoroutine(PathManager.Instance.CreatePath(missilePath,firingPlanet,targetPlanet, success =>
			{
				if (success)
					FireHelper(missilePath,firingPlanet,targetPlanet);
			}));
		}
		else
			FireHelper(missilePath,firingPlanet,targetPlanet);

	}

	static void FireHelper(SplineTrailRenderer missilePath, GameObject firingPlanet, GameObject targetPlanet)
	{
		GameObject missiles;
		missiles = (GameObject)Instantiate (Resources.Load<GameObject>("Missiles"));
		missiles.GetComponent<ParticleSystem>().startLifetime = missilePath.spline.Length () / 5.0f;
		missiles.transform.position = missilePath.spline.FindPositionFromDistance (0);
		missiles.GetComponent<Missiles>().Fire (missilePath, targetPlanet, firingPlanet);
	}

}
