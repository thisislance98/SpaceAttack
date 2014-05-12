using UnityEngine;
using System.Collections;

public class DestroyInSeconds : MonoBehaviour {

	public float Seconds = 10;

	// Use this for initialization
	IEnumerator Start () {
		yield return new WaitForSeconds(Seconds);

		Destroy(gameObject);
	}
	

}
