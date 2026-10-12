using UnityEngine;
using UnityEngine.InputSystem;

// Press V to switch between the stereo wall view (two side-by-side images)
// and a normal single-camera view for recording your video.
// Put this on CC_HUB.
public class RecordingMode : MonoBehaviour {
    public GameObject stereoCanvas;      // CC_CANVAS
    public Camera centerCamera;          // CenterCamera
    public Camera leftEye;               // CC_LEFT_CAMERA
    public Camera rightEye;              // CC_RIGHT_CAMERA
    public float recordingFieldOfView = 65f;

    private bool recording;
    private Behaviour centerOffset;      // CC_CAMERAOFFSET on the center camera (wall projection)

    void Start() {
        if (centerCamera != null) {
            foreach (var mb in centerCamera.GetComponents<MonoBehaviour>()) {
                if (mb.GetType().Name == "CC_CAMERAOFFSET") centerOffset = mb;
            }
        }
    }

    void Update() {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.vKey.wasPressedThisFrame) SetRecording(!recording);
    }

    public void SetRecording(bool on) {
        recording = on;
        if (stereoCanvas != null) stereoCanvas.SetActive(!on);
        if (leftEye != null) leftEye.enabled = !on;     // also saves performance while recording
        if (rightEye != null) rightEye.enabled = !on;
        if (centerCamera != null) {
            if (centerOffset != null) centerOffset.enabled = !on;
            centerCamera.gameObject.SetActive(true);
            centerCamera.enabled = true;
            if (on) {
                centerCamera.ResetProjectionMatrix();       // normal perspective instead of the wall's off-axis one
                centerCamera.ResetWorldToCameraMatrix();    // unlock the view so it follows the camera again
                centerCamera.fieldOfView = recordingFieldOfView;
            }
        }
    }
}
