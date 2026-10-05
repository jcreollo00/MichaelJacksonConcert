using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* (C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications
Set up eye separation for the two cameras.
*/

public class CC_CAMERA_SETUP : MonoBehaviour
{
    public GameObject leftCamera;
    public GameObject rightCamera;

    public CC_SETTINGS CCSettings;

    private Vector3 leftCamPosition, rightCamPosition;
    private bool currentStereo;
    private float currentEyeSeparation;

    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor){
            if (CC_CONFIG.LoadXMLConfig(CCSettings.ccConfigurationFile)) {
                CCSettings.eyeSeparation = CC_CONFIG.interaxial;
                CCSettings.invertStereo = CC_CONFIG.invertStereo;
                transform.localPosition = CC_CONFIG.headPosition;
            }
        }

        leftCamPosition = leftCamera.transform.position;
        rightCamPosition = rightCamera.transform.position;
        currentStereo = CCSettings.invertStereo;
        currentEyeSeparation = CCSettings.eyeSeparation;

        if (!CCSettings.invertStereo){
            leftCamera.transform.position = leftCamPosition +new Vector3(-CCSettings.eyeSeparation/2.0f,0,0);
            rightCamera.transform.position = rightCamPosition +new Vector3(CCSettings.eyeSeparation/2.0f,0,0);
        } else {
            leftCamera.transform.position = leftCamPosition + new Vector3(CCSettings.eyeSeparation/2.0f,0,0);
            rightCamera.transform.position = rightCamPosition + new Vector3(-CCSettings.eyeSeparation/2.0f,0,0);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if ((currentStereo == CCSettings.invertStereo) && (currentEyeSeparation == CCSettings.eyeSeparation)) return;
        currentStereo = CCSettings.invertStereo;
        if (!CCSettings.invertStereo){
            leftCamera.transform.position = leftCamPosition +new Vector3(-CCSettings.eyeSeparation/2.0f,0,0);
            rightCamera.transform.position = rightCamPosition +new Vector3(CCSettings.eyeSeparation/2.0f,0,0);
        } else {
            leftCamera.transform.position = leftCamPosition + new Vector3(CCSettings.eyeSeparation/2.0f,0,0);
            rightCamera.transform.position = rightCamPosition + new Vector3(-CCSettings.eyeSeparation/2.0f,0,0);
        }
    }
}
