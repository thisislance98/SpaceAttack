/// <summary>
/// Author: Cesar Rios (Pigtail Games) 2013
/// </summary>

using UnityEngine;
using System.Collections;
using Spline;

[ExecuteInEditMode]
[RequireComponent(typeof(ParticleSystem))]
public class SplineParticlesEmitterFollowPath : MonoBehaviour {

	/// <summary>
	/// The particle path to follow
	/// </summary>
	public SplineParticles 		particlePath;
	
	/// <summary>
	/// This will orient the z axis to the direction of the movement
	/// </summary>
	public bool					orientToPath;
	
	/// <summary>
	/// How much time is going to take to travel through the path. If 0 it will use the particle Duration 
	/// </summary>
	public float 				customTime;
	
	/// <summary>
	/// Set an offset from the origin
	/// </summary>
	public Vector3				offset;
	
	//Cache variables
	private Spline.BaseSpline.SplineIterator 	splineIterator;
	private Transform  					splineTansform;
	private Transform					myTransform;
	private ParticleSystem				myParticleSystem;
	
	
	void Start () {
		
		//Cache variables
		if (particlePath != null)
		{
			myTransform = transform;
			
			splineIterator = particlePath.Spline.GetIterator();
			splineTansform = particlePath.transform;
			myParticleSystem = particleSystem;
		}
		else
			Debug.LogWarning("You have to set a path to follow");
	}
	
	
	void Update () {
		
		if (splineIterator == null)
			Start(); //To avoid problems when we are in editmode
		
		else
		{
			float timeToUse =	particleSystem.duration;
			
			if (customTime > 0)  //Use custom time?
				timeToUse = customTime;
					
			splineIterator.SetOffsetPercent(myParticleSystem.time/timeToUse); //Get the position
			
			Vector3 offsetVector = myTransform.right*offset.x + myTransform.up*offset.y + myTransform.forward * offset.z;
		//	Vector3 pos = Camera.allCameras[0].ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,-Camera.main.transform.position.z));

			myTransform.position += Vector3.right *Time.deltaTime;//splineTansform.TransformPoint(splineIterator.GetPosition()) + offsetVector;  //Set the position
			
//			if (orientToPath) //Change rotation is needed
//				myTransform.rotation = Quaternion.LookRotation(splineIterator.GetTangent());
		}
	}
}
