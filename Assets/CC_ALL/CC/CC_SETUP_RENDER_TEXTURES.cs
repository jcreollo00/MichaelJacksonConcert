using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
(C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications

Raw Image objects placed on UI canvas are what is used to
display the left and right images.

This code simply assigns the respective render textures to each Raw Image object.
*/

public class CC_SETUP_RENDER_TEXTURES : MonoBehaviour
{
    public RawImage leftCameraImage;
    public RawImage rightCameraImage;
    public RenderTexture cameraLeftRT;
    public RenderTexture cameraRightRT;


    // Start is called before the first frame update
    void Start()
    {
        leftCameraImage.texture = cameraLeftRT;
        rightCameraImage.texture = cameraRightRT;
        
    }

}
