using UnityEngine;
using System.Collections;

public class PathManager : MonoBehaviour {


	public static PathManager Instance;

	// Use this for initialization
	void Awake () {
		Instance = this;
	}
	
	public IEnumerator CreatePath(SplineTrailRenderer missilePath, GameObject firingPlanet, GameObject targetPlanet, System.Action<bool> OnComplete)
	{
		
		float time = .3f;
		
		Vector3 moveDelta = targetPlanet.transform.position - firingPlanet.transform.position;
		float shieldRadius = targetPlanet.GetComponent<Planet>().collider.bounds.extents.x;
		
		moveDelta = moveDelta - (moveDelta.normalized * shieldRadius);
		
		if (missilePath != null)
			iTween.MoveBy(missilePath.gameObject,moveDelta,time);
		
		yield return new WaitForSeconds(time);
		
		OnComplete(missilePath != null);
		
	}
}
