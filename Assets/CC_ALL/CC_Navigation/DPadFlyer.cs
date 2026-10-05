/* DPAD navigation controller for VR.
(C) 2022 - Jason Leigh, Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa
    Version 09/17/2022 - added rotation speed variable
    Version 08/13/2022 - updated to use Unityʻs new input manager.

    Add this to the XRRig and give it the main camera.
    Controls need to be appropriately configured in Project Settings Input Manager as follows:

    Left Joystick left and right (for strafe left/right)
    Left Joystick up and down (for forward/back)
    Right Joystick up and down (pitch)
    Right Joystick left and right(yaw)
    LeftTrigger (roll left)
    RightTrigger  (roll right)
    Left Shoulder (move down)
    Right Shoulder (move up)
    AButton  (hold to reset navigtion to origin) 

*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class DPadFlyer : MonoBehaviour
{
	[Tooltip("Main Camera")]
    public GameObject mainCam;

    [Tooltip("Button to return navigation to home position")]
    public GamepadButton homeButton = GamepadButton.A;

	[Tooltip("Reset navigation speed.")]
	public float resetSpeed = 0.5f;

	[Tooltip("Move speed.")]
    public float moveSpeed = 20;

    [Tooltip("Rotation speed.")]
    public float rotateSpeed = 20;

    [Tooltip("Set to true if desire to flip pitch direction")]
    public bool flipPitch = false;

    Quaternion resetAngle;
    Vector3 resetPosition;
    // Start is called before the first frame update
    void Start()
    {
        resetAngle = transform.rotation;
        resetPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        Gamepad gamepad = Gamepad.current;
        if (gamepad == null) return;
        
        // Left DPAD joystick is for pitch and yaw
        float horizontalInput= gamepad.rightStick.ReadValue().x; // = Input.GetAxis("Horizontal2");
        float verticalInput= gamepad.rightStick.ReadValue().y; // = Input.GetAxis("Vertical2");

        // Right DPAD joystick is for forwards/backwards strafe left/right
        //float horizontalInput2 = Input.GetAxis("Horizontal");
        float horizontalInput2 = gamepad.leftStick.ReadValue().x;
        //float verticalInput2 = Input.GetAxis("Vertical");
        float verticalInput2 = -gamepad.leftStick.ReadValue().y;

        // Left and right trigger is for roll
        float leftInput=gamepad.leftTrigger.ReadValue() ; //= Input.GetAxis("LeftTrigger");
        float rightInput=gamepad.rightTrigger.ReadValue(); // = Input.GetAxis("RightTrigger");

        // Rotate the XRRig about the main cameras direction of view
        if (!flipPitch) verticalInput = -verticalInput;

        transform.Rotate(mainCam.transform.right, verticalInput* rotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(mainCam.transform.up, horizontalInput* rotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(mainCam.transform.forward, leftInput* rotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(mainCam.transform.forward, -rightInput* rotateSpeed * Time.deltaTime, Space.World);

        // Translate the XRRig about the main cameras axis
        transform.Translate(new Vector3(horizontalInput2,0 , -verticalInput2) * moveSpeed/2 * Time.deltaTime,mainCam.transform);

        if (gamepad[homeButton].isPressed){
        //if (Input.GetButton("AButton")){
            transform.position = Vector3.Slerp (transform.position, resetPosition, resetSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Slerp (transform.rotation, resetAngle, resetSpeed * Time.deltaTime);


        }
        if (gamepad.leftShoulder.isPressed){
        //if (Input.GetKey(KeyCode.JoystickButton4)){
            transform.Translate(new Vector3(0,-1,0) * moveSpeed/2 * Time.deltaTime,mainCam.transform);

        }
        if (gamepad.rightShoulder.isPressed){
        //if (Input.GetKey(KeyCode.JoystickButton5)){
            transform.Translate(new Vector3(0,1,0) * moveSpeed/2 * Time.deltaTime,mainCam.transform);
        }
    }
}
