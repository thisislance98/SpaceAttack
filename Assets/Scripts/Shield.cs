using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shield : MonoBehaviour {
	
	Planet _parentPlanet;

	public static float EnergyToShieldRadius = .1f;

	Color _startColor;

	// Use this for initialization
	void Awake () {
		_startColor = renderer.material.color;

	}

	void Start()
	{
		_parentPlanet = transform.parent.GetComponent<Planet>();
	}

	public void OnNewOwner()
	{
		if (!IsMine())
			renderer.material.color = new Color(0,0,0,renderer.material.color.a);
		else
			renderer.material.color = new Color(_startColor.r,_startColor.g,_startColor.b,renderer.material.color.a);
	}

	public bool IsMine()
	{
		return transform.parent.GetComponent<Planet>().IsMine();
	}

	void Update()
	{
		UpdateShieldRadius();

	}

	void UpdateShieldRadius()
	{
		float energy = _parentPlanet.GetEnergy();

		float planetRadius = _parentPlanet.transform.lossyScale.x;

		float shieldScale = (planetRadius + energy * EnergyToShieldRadius) / planetRadius;

		transform.localScale = new Vector3(shieldScale,shieldScale,shieldScale);
	}


}
