using UnityEngine;
using System.Collections;

public class Planet : MonoBehaviour {

	public Transform ShieldPrefab;
	public Transform SelectionPrefab;
	public Material[] Materials;
	public float MaxRotationSpeed;
	public float MinRotationSpeed;
	public float MinStartShieldScale = 1.01f;
	public float MaxStartShieldScale = 1.5f;
	public float EnergyRegenRate = 10;

	float _selectionPercent = .5f;
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


		_selectionRenderer = ((Transform)Instantiate(SelectionPrefab,transform.position,Quaternion.identity)).GetComponent<Renderer>();
		_selectionRenderer.transform.parent = transform;

		UpdateSelectionScale();
		//_selectionRenderer.transform.parent = _shield.transform;
		_selectionRenderer.material.color = new Color(0,1,0,.3f);
		_selectionRenderer.enabled = false;
	}

	void Update()
	{
		if (_playerType != PlayerType.Neutral && _energy < _maxEnergy)
			_energy += EnergyRegenRate * transform.localScale.x * Time.deltaTime;

		transform.Rotate(Vector3.up,_rotationSpeed*Time.deltaTime);

		UpdateSelectionScale();
	}

	void UpdateSelectionScale()
	{
		if (_selectionRenderer.enabled == false)
			return;

		float scale = 1 + ((_shield.transform.localScale.x - 1) * _selectionPercent);

		_selectionRenderer.transform.localScale = Vector3.one * scale;

	}

	float GetStartEnergy()
	{
		float radiusDiff = _shield.transform.lossyScale.x - transform.lossyScale.x;
		return radiusDiff / Shield.EnergyToShieldRadius;

	}


	public void OnMissileHit(Missiles missiles)
	{


		if (missiles.GetFiringPlanetType() == _playerType)
			_energy += missiles.TakeMissileEnergy();
		else
			_energy -= missiles.TakeMissileEnergy();


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

	public void SetSelectionPercent(float percent)
	{
		_selectionPercent = percent;
	}

	public void IncreaseSelectionPercent(float percent)
	{
		_selectionPercent += percent;

		if (_selectionPercent > 1)
			_selectionPercent = 1;
	}
	public float TakeSelectionEnergy()
	{
		float amount = _energy * _selectionPercent;
		_energy -= amount;

		return amount;
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
