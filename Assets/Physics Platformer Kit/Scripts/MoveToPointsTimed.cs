using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//moves object along a series of waypoints, useful for moving platforms or hazards
//this class adds a kinematic rigidbody so the moving object will push other rigidbodies whilst moving
[RequireComponent(typeof(Rigidbody))]
public class MoveToPointsTimed : MonoBehaviour 
{
	public float timeToNext;								//how long to move
	public float delay;                                     //how long to wait at each waypoint
	[Range(0, 1)]
	public float offset;
	public CycleType cycleType;                             //stop at final waypoint, loop through waypoints or move back n forth along waypoints
	public MoveType movementType;

	
	public enum CycleType { PlayOnce, Loop, PingPong, StartToEnd }
	public enum MoveType { Lerp, Ease }
	private int lastWp;
	private int currentWp;
	private float arrivalTime;
	private bool forward = true, arrived = false;
	private List<Transform> waypoints = new List<Transform>();
	private Rigidbody rigid;

	private AnimationCurve easeCurve;

    private Vector3 lastVelocity;
    private Vector3 currentVelocity;

	private float timer = 0;

    //setup
    void Awake()
	{
		if(transform.tag != "Enemy")
		{
			//add kinematic rigidbody
			rigid = GetComponent<Rigidbody>();
			if(!rigid)
                rigid = gameObject.AddComponent<Rigidbody>();
            rigid.isKinematic = true;
            rigid.useGravity = false;
            rigid.interpolation = RigidbodyInterpolation.Interpolate;	
		}
		else
		{
			Debug.LogWarning($"Tried to use MoveToPointsTimed on enemy {gameObject.name}. Only use this script on moving platforms!");	
		}

        if (!rigid)
            rigid = gameObject.AddComponent<Rigidbody>();
        //get child waypoints, then detach them (so object can move without moving waypoints)
        foreach (Transform child in transform)
			if(child.tag == "Waypoint")
				waypoints.Add (child);

		foreach(Transform waypoint in waypoints)
			waypoint.parent = null;
		
		if(waypoints.Count == 0)
			Debug.LogError("No waypoints found for 'MoveToPoints' script. To add waypoints: add child gameObjects with the tag 'Waypoint'", transform);

		easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        easeCurve.postWrapMode = WrapMode.ClampForever;
        easeCurve.preWrapMode = WrapMode.ClampForever;
        lastVelocity = currentVelocity = Vector3.zero;
		GetNextWP();
		timer = (timeToNext + delay) * offset;
	}
	
	//if this is a platform move platforms toward waypoint
	void FixedUpdate()
    {
		timer += Time.fixedDeltaTime;
        //if we've arrived at waypoint, get the next one
        if (waypoints.Count > 1)
        {
            if (timer > timeToNext + delay)
            {
                GetNextWP();
                timer -= timeToNext + delay;
            }
        }
        if (transform.tag != "Enemy")
		{
			if(!arrived && waypoints.Count > 0)
			{
				Vector3 lastPosition = transform.position;

				float timePosition = Mathf.Clamp01(timer / timeToNext);
				Vector3 newPos;
				if (movementType == MoveType.Lerp)
				{
					newPos = Vector3.Lerp(waypoints[lastWp].position, waypoints[currentWp].position, timePosition);
				} else if (movementType == MoveType.Ease)
                {
					timePosition = easeCurve.Evaluate(timePosition);
                    newPos = Vector3.Lerp(waypoints[lastWp].position, waypoints[currentWp].position, timePosition);
                } else
				{
					newPos = Vector3.zero;
				}

				rigid.MovePosition(newPos);

				lastVelocity = currentVelocity;
				currentVelocity = newPos - lastPosition;
			}
		}
	}
	
	//get the next waypoint
	private void GetNextWP()
    {
        lastWp = currentWp;
        if (cycleType == CycleType.PlayOnce)
		{
			currentWp++;
			if(currentWp == waypoints.Count)
					enabled = false;
		}
		
		if (cycleType == CycleType.Loop)
			currentWp = (currentWp == waypoints.Count-1) ? 0 : currentWp += 1;
		
		if (cycleType == CycleType.PingPong)
		{
			if(currentWp == waypoints.Count-1)
				forward = false;
			else if(currentWp == 0)
				forward = true;
			currentWp = (forward) ? currentWp += 1 : currentWp -= 1;
		}

		if (cycleType == CycleType.StartToEnd)
		{
			currentWp++;
            if (currentWp == waypoints.Count)
			{
				lastWp = 0;
				transform.position = waypoints[0].position;
				currentWp = 1;

			}

        }
	}

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Enemy")
        {
            float timePosition = Mathf.Clamp01((Time.fixedDeltaTime + timer) / timeToNext);
            Vector3 nextPos = Vector3.zero;
            if (movementType == MoveType.Lerp)
            {
                nextPos = Vector3.Lerp(waypoints[lastWp].position, waypoints[currentWp].position, timePosition);
            }
            else if (movementType == MoveType.Ease)
            {
                timePosition = easeCurve.Evaluate(timePosition);
                nextPos = Vector3.Lerp(waypoints[lastWp].position, waypoints[currentWp].position, timePosition);
            }

			Vector3 deltaVelocity = ((nextPos - transform.position) - currentVelocity) / Time.fixedDeltaTime;
			deltaVelocity.y = 0;

			if (deltaVelocity.sqrMagnitude > 1)
			{
				Debug.Log(
                    $"timer; {timer}\n" +
                    $"timePosition; {timePosition}\n" +
                    $"nextPos; {nextPos}\n" +
                    $"transform.position; {transform.position}\n" +
                    $"currentVelocity; {currentVelocity}\n" +
                    $"deltaVelocity; {deltaVelocity}\n" +

                    $"waypoints[lastWp].position; {waypoints[lastWp].position}\n" +
                    $"waypoints[currentWp].position; {waypoints[currentWp].position}\n" +

                    $"Time.time - arrivalTime; {Time.time - arrivalTime}\n"
                    );
			}


            Rigidbody otherRigid = collision.gameObject.GetComponent<Rigidbody>();
			otherRigid.linearVelocity += deltaVelocity;
		}
    }

    //draw gizmo spheres for waypoints
    void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		foreach (Transform child in transform)
		{
			if(child.tag == "Waypoint")
				Gizmos.DrawSphere(child.position, .7f);
		}
	}
}

/* NOTE: remember to tag object as "Moving Platform" if you want the player to be able to stand and move on it
 * for waypoints, simple use an empty gameObject parented the the object. Tag it "Waypoint", and number them in order */