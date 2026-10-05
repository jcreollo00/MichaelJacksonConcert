using UnityEngine;
using SpaceNavigatorDriver;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class SpaceMouseFlyAround : MonoBehaviour {
	public bool HorizonLock = true;
	public float moveSpeed = 10f;
	public float rotateSpeed = 10f;
	[Tooltip("Reset navigation speed.")]
	public float resetSpeed = 0.7f;
	[Tooltip("Key to reset to beginning")]
	public Key resetKey = Key.A;
	Quaternion resetAngle;
    Vector3 resetPosition;
	private bool resetEnabled = false;

		public void Start(){
			resetAngle = transform.rotation;
        	resetPosition = transform.position;
		}
		public void Update () {
		transform.Translate(SpaceNavigator.Translation* moveSpeed * Time.deltaTime , Space.Self);

		if (HorizonLock) {
			// This method keeps the horizon horizontal at all times.
			// Perform azimuth in world coordinates.
			transform.Rotate(Vector3.up, SpaceNavigator.Rotation.Yaw() * rotateSpeed * Time.deltaTime * Mathf.Rad2Deg, Space.World);
			// Perform pitch in local coordinates.
			transform.Rotate(Vector3.right, SpaceNavigator.Rotation.Pitch() * rotateSpeed * Time.deltaTime *Mathf.Rad2Deg, Space.Self);
		}
		else {
			transform.Rotate(Vector3.up, SpaceNavigator.Rotation.Yaw() * rotateSpeed * Time.deltaTime * Mathf.Rad2Deg, Space.World);
			transform.Rotate(Vector3.right, SpaceNavigator.Rotation.Pitch() * rotateSpeed * Time.deltaTime *Mathf.Rad2Deg, Space.Self);
			transform.Rotate(Vector3.forward, SpaceNavigator.Rotation.Roll() * rotateSpeed * Time.deltaTime *Mathf.Rad2Deg, Space.Self);
		}

		//if (gamepad[homeButton].isPressed){
		if ((resetEnabled == false) && (Keyboard.current[resetKey].isPressed)){
			resetEnabled = true;
		}
		if (resetEnabled){
			transform.position = Vector3.Slerp (transform.position, resetPosition, resetSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Slerp (transform.rotation, resetAngle, resetSpeed * Time.deltaTime);
			if ((Vector3.Distance(transform.position, resetPosition) <=0.1) && (Quaternion.Angle(transform.rotation, resetAngle) < 0.1))
				resetEnabled = false;
        }
	}

}