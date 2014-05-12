using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour {

	public Transform ShieldPrefab;
	public Material[] Materials;
	public float MaxRotationSpeed;
	public float MinRotationSpeed;
	public float MinStartShieldScale = 1.01f;
	public float MaxStartShieldScale = 1.5f;
	public float EnergyRegenRate = 10;
	public float DefaultEnergyUsedPerFire = .5f;

	Shield _shield;
	float _energy;
	float _rotationSpeed;
	Renderer _selectionRenderer;
	PlayerType _playerType;
	bool _isMine;
	float _maxEnergy;
	
	string[] _playerTypeToTag = { "NeutralPlanet","ComputerPlanet", "PlayerPlanet" };

	// Use this for initialization
	public void Initialize (PlayerType playerType) {

		_maxEnergy = (transform.localScale.x *2) / Shield.EnergyToShieldRadius;
		_rotationSpeed = Random.Range(MinRotationSpeed,MaxRotationSpeed);
		_shield = ((Transform)Instantiate(ShieldPrefab,transform.position,Quaternion.identity)).GetComponent<Shield>();

		_shield.transform.parent = transform;
		float shieldScale = Random.Range(MinStartShieldScale,MaxStartShieldScale);
		if (playerType == PlayerType.Neutral)
			_shield.transform.localScale = new Vector3(1.1f,1.1f,1.1f);
		else
			_shield.transform.localScale = new Vector3(shieldScale,shieldScale,shieldScale);

		_energy = GetStartEnergy();

		OnNewOwner(playerType);
	
		_selectionRenderer = ((Transform)Instantiate(ShieldPrefab,transform.position,Quaternion.identity)).GetComponent<Renderer>();
		_selectionRenderer.transform.parent = transform;
		_selectionRenderer.transform.localScale = new Vector3(1.5f,1.5f,1.5f);
		_selectionRenderer.material.color = new Color(0,1,0,.3f);
		_selectionRenderer.enabled = false;
	}

	void Update()
	{
		if (_playerType != PlayerType.Neutral && _energy < _maxEnergy)
			_energy += EnergyRegenRate * transform.localScale.x * Time.deltaTime;

		transform.Rotate(Vector3.up,_rotationSpeed*Time.deltaTime);
	}

	float GetStartEnergy()
	{
		float radiusDiff = _shield.transform.lossyScale.x - transform.lossyScale.x;
		return radiusDiff / Shield.EnergyToShieldRadius;

	}
	

	public float OnFiredMissiles()
	{
		float energyUsed = _energy * DefaultEnergyUsedPerFire;
		_energy -= energyUsed;

		return energyUsed;
	}

	public void OnMissileHit(Missiles missiles)
	{


		if (missiles.GetFiringPlanetType() == _playerType)
			_energy += missiles.GetEnergyPerMissile();
		else
			_energy -= missiles.GetEnergyPerMissile();


		if (_energy <= 0)
		{
			PlayerType newPlayerType = missiles.GetFiringPlanetType();
			OnNewOwner(newPlayerType);
			_energy = 0;
		}
	}

	public void SetSelected(bool selected)
	{
		_selectionRenderer.enabled = selected;
	}

	
	void OnNewOwner(PlayerType newType)
	{
		InputManager.Instance.OnNewOwner(gameObject);

		_playerType = newType;
		_isMine = (_playerType == PlayerType.Player1);

		gameObject.tag = _playerTypeToTag[(int)newType];

		renderer.material = Materials[(int)_playerType];


	//	_shield.renderer.enabled = !(!_isMine && newType != PlayerType.Neutral);
		_shield.OnNewOwner();

	}

#region Helper Functions

	public PlayerType GetPlayerType()
	{
		return _playerType;
	}
	
	public bool IsMine()
	{
		return _isMine;
	}

	public Shield GetShield()
	{
		return _shield;
	}

	public float GetEnergy()
	{
		return _energy;
	}

	float SphereVolume(float radius)
	{
		return (4.0f/3.0f) * Mathf.PI * (Mathf.Pow(radius, 3.0f));
	}

//	public float GetVolume()
//	{
//		return SphereVolume(transform.lossyScale.x);
//	}
	

#endregion
}
