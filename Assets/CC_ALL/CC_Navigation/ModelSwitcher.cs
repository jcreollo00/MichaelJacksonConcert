/* DPAD navigation controller for VR.
(C) 2022 - Jason Leigh, Laboratory for Advanced Visualization & Applications, University of Hawaii at Manoa
    Version 09/17/2022 - first version

    This script accepts a bunch of 3D models, and with the press
    of a keyboard button toggles between showing each one of them.
    To use it, create a game object and add the script to it.
    Then set up the models variable with the number of models
    you want to cycle and drag and drop models in your scene into it.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class ModelSwitcher : MonoBehaviour
{


    [Tooltip("Models you want to switch between")]
    public GameObject[] models;

    [Tooltip("Key to press to switch the model")]
    public Key switchKey = Key.Tab;	

    [Tooltip("Game controller button to press to switch the model")]
    public GamepadButton switchButton = GamepadButton.Select;

    int currentModel = 0;

    // Start is called before the first frame update
    void Start()
    {
 
    }

    void HideAll()
    {
        for(int i =0; i< models.Length;i++){
            models[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        bool activatedGP = false;

		if (Gamepad.current != null) activatedGP = Gamepad.current[switchButton].wasPressedThisFrame;

		if ((activatedGP || Keyboard.current[switchKey].wasPressedThisFrame)){

            currentModel++;
            if (currentModel >= models.Length) currentModel=0;
            HideAll();
            models[currentModel].SetActive(true);
        }
		
    }
}
