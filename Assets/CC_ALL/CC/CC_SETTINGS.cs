using UnityEngine;

[CreateAssetMenu(fileName = "CC_SETTINGS", menuName = "Scriptable Objects/CC_SETTINGS")]
public class CC_SETTINGS : ScriptableObject
{
    public bool invertStereo = false;
    public float eyeSeparation = 0.061f;
    public CC_StereoMode stereoMode = CC_StereoMode.sidebyside;
    public string ccConfigurationFile = "C:\\CCUnityConfig.xml";

}
