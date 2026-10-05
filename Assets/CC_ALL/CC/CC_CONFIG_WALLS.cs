using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
(C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications

Use the CCUnityConfig/CCUnityConfig.xml file to position and scale
the front wall display.
*/
public class CC_CONFIG_WALLS : MonoBehaviour
{
    public GameObject frontWall;
    public CC_SETTINGS CCSettings;
    // Start is called before the first frame update
    void Start()
    {
        if (!Application.isEditor){
            if (CC_CONFIG.LoadXMLConfig(CCSettings.ccConfigurationFile)) {
                frontWall.transform.localPosition = CC_CONFIG.wallPosition;
                frontWall.transform.localScale = new Vector3(CC_CONFIG.wallWidth, CC_CONFIG.wallHeight,1);
            }
        }
    }

}
