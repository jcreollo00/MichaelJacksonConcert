using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
(C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications

This script is used in the CenterCamera to hide it whenever
the app is running as a built app.

This camera is not needed during running of the built app as it will
only reduce performance.
It is normally enabled when running in the editor to get rid of the
nagging message that says you have no camera assigned to a display.

*/

public class CC_HIDE_CAMERA_ON_APP_RUN : MonoBehaviour
{
    public bool hideOnAppRun = true;
    // Start is called before the first frame update
    void Start()
    {
        if ((!Application.isEditor) && (hideOnAppRun)){
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
