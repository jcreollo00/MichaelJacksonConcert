using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*

(C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications

This code properly sizes and positions the raw images for interleave and side-by-side rendering.

For interleave, the images are rendered at full width of the screen, and then the mask is applied to the left
and right images, which are stacked on top of each other to form the final interleaved image.

For side-by-side, the mask is disabled, and the left and right images are shown side by side to each other.

Note: If the screen changes in size for any reason you have to dynamically change
the size of the raw images, render textures, and masks to fit the resolution
of the new screen size.

*/
public enum CC_StereoMode {
    interleave,
    sidebyside,
    topbottom
}

public class CC_CANVAS_SETUP : MonoBehaviour
{
    public GameObject leftMask;
    public GameObject rightMask;
    public GameObject leftRawImage;
    public GameObject rightRawImage;

    public RenderTexture leftRT;
    public RenderTexture rightRT;

    public CC_SETTINGS CCSettings;

    private int currentScreenWidth, currentScreenHeight;
    private CC_StereoMode currentStereoMode = CC_StereoMode.sidebyside;
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor){
            if (CC_CONFIG.LoadXMLConfig(CCSettings.ccConfigurationFile)) {
                if (CC_CONFIG.stereoMode == 0) CCSettings.stereoMode = CC_StereoMode.interleave;
                else if (CC_CONFIG.stereoMode == 1) CCSettings.stereoMode = CC_StereoMode.sidebyside;
                else CCSettings.stereoMode = CC_StereoMode.topbottom;
            }
        }
        currentScreenWidth = 0;
        currentScreenHeight = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Screen.width == currentScreenWidth && Screen.height == currentScreenHeight) && (currentStereoMode == CCSettings.stereoMode)) return;
        currentStereoMode = CCSettings.stereoMode;
        currentScreenWidth = Screen.width;
        currentScreenHeight = Screen.height;

        if (CCSettings.stereoMode == CC_StereoMode.sidebyside) SetupSideBySide();
        else if (CCSettings.stereoMode == CC_StereoMode.interleave) SetupInterleave();
        else SetupTopBottom();
    }

void SetupInterleave(){

        leftMask.GetComponent<Mask>().enabled = true;
        rightMask.GetComponent<Mask>().enabled = true;
        leftMask.GetComponent<RawImage>().enabled = true;
        rightMask.GetComponent<RawImage>().enabled = true;   

        RectTransform rectTransform;

        rectTransform = rightMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);

        rightRT.Release();

        rectTransform = rightRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);
        rightRT.width = Screen.width;
        rightRT.height = Screen.height;
        rightRT.Create();

        rectTransform = leftMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);
            
        leftRT.Release();
        rectTransform = leftRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);

        leftRT.width = Screen.width;
        leftRT.height = Screen.height;
        leftRT.Create();
}
void SetupSideBySide(){
        leftMask.GetComponent<Mask>().enabled = false;
        rightMask.GetComponent<Mask>().enabled = false;
        leftMask.GetComponent<RawImage>().enabled = false;
        rightMask.GetComponent<RawImage>().enabled = false;   

        rightRT.Release();
        RectTransform rectTransform;
        rectTransform = rightRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width/2,Screen.height);
        rectTransform.anchoredPosition = new Vector2(Screen.width/4, 0);
        rightRT.width = Screen.width/2;
        rightRT.height = Screen.height;
        rightRT.Create();

        rectTransform = rightMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width/2,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);

        leftRT.Release();
        rectTransform = leftRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width/2,Screen.height);
        rectTransform.anchoredPosition = new Vector2(-Screen.width/4, 0);

        leftRT.width = Screen.width/2;
        leftRT.height = Screen.height;
        leftRT.Create();

        rectTransform = leftMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width/2,Screen.height);
        rectTransform.anchoredPosition = new Vector2(0, 0);
}

void SetupTopBottom(){
        leftMask.GetComponent<Mask>().enabled = false;
        rightMask.GetComponent<Mask>().enabled = false;
        leftMask.GetComponent<RawImage>().enabled = false;
        rightMask.GetComponent<RawImage>().enabled = false;   

        rightRT.Release();
        RectTransform rectTransform;
        rectTransform = rightRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height/2);
        rectTransform.anchoredPosition = new Vector2(0,Screen.height/4);
        rightRT.width = Screen.width;
        rightRT.height = Screen.height/2;
        rightRT.Create();

        rectTransform = rightMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height/2);
        rectTransform.anchoredPosition = new Vector2(0, 0);

        leftRT.Release();
        rectTransform = leftRawImage.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height/2);
        rectTransform.anchoredPosition = new Vector2(0,-Screen.height/4);

        leftRT.width = Screen.width;
        leftRT.height = Screen.height/2;
        leftRT.Create();

        rectTransform = leftMask.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(Screen.width,Screen.height/2);
        rectTransform.anchoredPosition = new Vector2(0, 0);
}

}
