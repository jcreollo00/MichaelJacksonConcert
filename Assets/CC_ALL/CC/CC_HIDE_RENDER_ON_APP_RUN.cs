using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
(C) 09/28/2024 Jason Leigh - Laboratory for Advanced Visualization & Applications
This simply hides the rendering of the semi-transparent wall when
you are actually running the app.
If you wish to keep the wall visible when run in the editor
you can override it by setting visibleInEditor to true.
By default you should leave it false.

*/

public class CC_HIDE_RENDER_WHEN_RUN_AS_APP : MonoBehaviour
{
    public bool visibleInEditor = false;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        if (Application.isEditor && visibleInEditor){gameObject.GetComponent<MeshRenderer>().enabled = true;}

    }

    // Update is called once per frame
    void Update()
    {
         if (Application.isEditor){
            if (visibleInEditor) gameObject.GetComponent<MeshRenderer>().enabled = true;
            else gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
}
