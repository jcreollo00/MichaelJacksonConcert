using UnityEngine;

// A slow "concert cameraman" for the big screens beside the stage.
// 1. Create a Camera, set its Target Texture to a new Render Texture (1024x576).
// 2. Make a Quad for each screen and drag the Render Texture onto it.
// 3. Put this script on the camera and set target to MJ's head or chest bone.

public class JumbotronCamera : MonoBehaviour {
    public Transform target;                             // MJ's head or chest bone (what to aim at)
    public Transform facingReference;                    // MJ's root object (which way is "front"); optional
    public Vector3 offset = new Vector3(0f, 0.2f, 4f);   // in front of MJ, slightly above
    public float swayDistance = 1.5f;
    public float swaySpeed = 0.15f;
    public float followSmoothing = 3f;

    void LateUpdate() {
        if (target == null) return;
        Transform facing = facingReference != null ? facingReference : target;
        Vector3 fwd = facing.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, fwd);
        float sway = Mathf.Sin(Time.time * swaySpeed * Mathf.PI * 2f) * swayDistance;
        Vector3 desired = target.position + fwd * offset.z + right * sway + Vector3.up * offset.y;
        transform.position = Vector3.Lerp(transform.position, desired, Time.deltaTime * followSmoothing);
        transform.LookAt(target.position);
    }
}
