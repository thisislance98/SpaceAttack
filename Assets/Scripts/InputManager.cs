using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class InputManager : MonoBehaviour 
{

	public string groundLayerName = "Water";
	public string playerLayerName = "Default";
	public string shieldLayerName = "Shields";
	public Vector3 trailOffset = new Vector3(0, 0.02f, 0);
	public float doubleTapTime = 1;
	public GameObject MissilePathPrefab; 
	public float DefaultEnergyPercent = .5f;
	public float SelectionPercentIncreaseSpeed = 1;



	List<SplineTrailRenderer> _missilePaths = new List<SplineTrailRenderer>();
	List<GameObject> _firingPlanets = new List<GameObject>();

	
	bool wasTouchingShield = false;
	float _lastTapTime;
	Vector3 _touchStartPos;
	bool _isTouching;
	GameObject _lastTappedPlanet;

	public static InputManager Instance;

	void Awake()
	{
		Application.targetFrameRate = 60;
		Instance = this;
	}


	void Update () 
	{
		if(Input.GetMouseButtonDown(0))
		{
			RaycastHit hit;

			// start a path if they are touching a planet and there isn't currently a path
			if(IsTouchingLayer(playerLayerName, out hit))
			{
				if (hit.transform.GetComponent<Planet>().IsMine())
				{
					Select(hit.transform.gameObject);
					_touchStartPos = Input.mousePosition;
					_isTouching = true;
				}
			}
		}
		else if(Input.GetMouseButtonUp(0)) 
		{

			RaycastHit hit;
			_isTouching = false;

			// on touch up.. is their a selection and a path if so then if they are touching a planet..fire
			if (IsTouchingLayer(playerLayerName, out hit) &&  _missilePaths.Count > 0 && (!IsTouchMyPlanet() || _missilePaths[0].spline.Length() > 0))
			{

				for (int i=_missilePaths.Count-1; i >= 0; i--)
				{
					SplineTrailRenderer missilePath = _missilePaths[i];
					GameObject firingPlanet = _firingPlanets[i];
					float energy = firingPlanet.GetComponent<Planet>().TakeSelectionEnergy();
					Missiles.FireMissiles (missilePath, firingPlanet, hit.transform.gameObject, energy);

					Unselect(missilePath,firingPlanet);
				}

			}
			else if (!IsTouchingLayer(playerLayerName)) // they lifted their finger not on a planet, so unselect all
			{
			
				for (int j=_missilePaths.Count-1; j >= 0; j--)
				{
					Destroy (_missilePaths[j].gameObject);
					Unselect (_missilePaths[j],_firingPlanets[j]);
				
				}

			}
			else // just tapped on own planet, if double tap.. select all
			{
				RaycastHit tapHit;
				// select all if was wa double tap
				if (IsTouchMyPlanet(out tapHit) && Time.time - _lastTapTime < doubleTapTime && tapHit.transform.gameObject == _lastTappedPlanet )
					SelectAll();

				_lastTapTime = Time.time;
				_lastTappedPlanet = tapHit.transform.gameObject;


			}
		}
		else if(Input.GetMouseButton(0)) // they are holding mouse button or touch down
		{

			if (_isTouching)
			{
				// they are just moving the path(s)
				if ( (Input.mousePosition - _touchStartPos).magnitude > 3)
				{
					RaycastHit hit;

					// don't do anything if they are touching within the firing planet
					if (IsTouchMyPlanet(out hit) && _firingPlanets.Count > 0 && hit.transform.gameObject == _firingPlanets[_firingPlanets.Count-1])
						return;

					OnSelectionMove();
					wasTouchingShield = IsTouchingLayer(shieldLayerName);
				}
				else if (_missilePaths[0].spline.Length() == 0) // they are holding down on their planet
				{
					foreach (GameObject firingPlanet in _firingPlanets)
						firingPlanet.GetComponent<Planet>().IncreaseSelectionPercent(Time.deltaTime * SelectionPercentIncreaseSpeed);

				}
			}


		}
	}

	bool IsPathDrawn()
	{
		return (_missilePaths.Count > 0 && _missilePaths[0].spline.Length() > 0);
	}

	bool IsTouchMyPlanet(out RaycastHit hit)
	{
		return IsTouchingLayer(playerLayerName, out hit) && hit.transform.GetComponent<Planet>().IsMine();
	}

	bool IsTouchMyPlanet()
	{
		RaycastHit hit;
		return IsTouchMyPlanet(out hit);
	}
//	IEnumerator d(System.Action<bool> a)
//	{
//
//		yield return null;
//	}
//
//	StartCoroutine ( d( success => { int i=0; });

	

	#region Selection

	void SelectAll()
	{
		GameObject[] playerPlanets = GameObject.FindGameObjectsWithTag("PlayerPlanet");

		foreach(GameObject planet in playerPlanets)
			Select(planet);
		
	}

	public void OnNewOwner(GameObject planet)
	{
		if (_firingPlanets.Contains(planet))
		{
			int index = _firingPlanets.IndexOf(planet);
			Unselect(_missilePaths[index],planet);

		}
	}

	void Select(GameObject firingPlanet)
	{
		if (_firingPlanets.Contains(firingPlanet) == true)
			return;

		SplineTrailRenderer path = ((GameObject)Instantiate(MissilePathPrefab,firingPlanet.transform.position,Quaternion.identity)).GetComponent<SplineTrailRenderer>();
		
		_missilePaths.Add(path);
		_firingPlanets.Add(firingPlanet);
		firingPlanet.GetComponent<Planet>().SetSelectionPercent(DefaultEnergyPercent);
		
		firingPlanet.SendMessage("SetSelected",true);
	}

	void Unselect(SplineTrailRenderer missilePath, GameObject firingPlanet)
	{
		if (_firingPlanets.Contains(firingPlanet) == false)
			return;
		// cleanup
		firingPlanet.SendMessage ("SetSelected", false);
		missilePath.renderer.enabled = false;
		_missilePaths.Remove(missilePath);
		_firingPlanets.Remove(firingPlanet);
	}

	void OnSelectionMove()
	{

		if(IsTouchingLayer(groundLayerName) && wasTouchingShield == false && _missilePaths.Count > 0)
		{
			RaycastHit hit;
			Vector3 newPos = (IsTouchingLayer(shieldLayerName,out hit)  && !hit.transform.GetComponent<Shield>().IsMine()) ? GetShieldHitPos() : GetGroundTouchPos();
		
			foreach (SplineTrailRenderer missilePath in _missilePaths)
			{
				missilePath.transform.position = newPos;
			}

		}

	}

#endregion

#region helper functions

	Vector3 GetShieldHitPos()
	{
		float length = _missilePaths[0].spline.Length();
		Vector3 lastPos = _missilePaths[0].transform.position;

		Vector3 dir = (GetGroundTouchPos() - lastPos).normalized;


		Ray ray = new Ray(lastPos,dir);
		RaycastHit shieldHit;

		if (Physics.Raycast(ray, out shieldHit, float.MaxValue, LayerNameToIntMask(shieldLayerName)))
		{
//			Debug.Log ("lastpos: " + lastPos + " shield: " + shieldHit.point + " dir: " + dir);
			return shieldHit.point + dir*.1f;
		}
		else
			return lastPos;
		
	}


	Vector3 GetGroundTouchPos()
	{
		RaycastHit hit;
		if(IsTouchingLayer(groundLayerName, out hit))
		{
			Vector3 pos = hit.point + trailOffset;

			return pos;
		}

		return Vector3.zero;
	}

	bool IsTouchingLayer(string layerName)
	{
		RaycastHit hit;
		return IsTouchingLayer(layerName, out hit);
	}

	bool IsTouchingLayer(string layerName, out RaycastHit hit)
	{
		return (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, 
		                                                                 Input.mousePosition.y, 0)), out hit, float.MaxValue, LayerNameToIntMask(layerName)));

	}

	static int LayerNameToIntMask(string layerName)
	{
		int layer = LayerMask.NameToLayer(layerName);

//		if(layer == 0)
//			return int.MaxValue;

		return 1 << layer;
	}

#endregion
}
