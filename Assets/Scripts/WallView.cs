using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Wider view + looking up and down on the CyberCANOE wall.
// Put this on CC_HUB.
//
// WIDER VIEW: the wall is wide but short, so the default view is narrow
// (about 23 degrees tall). This enlarges the virtual screen around its center,
// which widens the field of view like a wide-angle lens. It runs after the
// kit loads the lab's CCUnityConfig.xml, so it works on the real wall too.
//   D-pad Up / Down  (or + / - keys)  = wider / narrower, live
//
// LOOK UP / DOWN: the head and screen are pitched together around the eye,
// so the stereo stays correct.
//   Right stick up / down  (or Page Up / Page Down)  = look up / down
//   Click the right stick  (or Home)                  = look straight ahead again
public class WallView : MonoBehaviour {
    [Header("Wider view")]
    [Range(1f, 3f)] public float viewWidth = 1.8f;      // 1 = kit default (narrow)
    public float minWidth = 1f, maxWidth = 3f;
    public float widthStep = 0.15f;

    [Header("Look up / down")]
    public float lookSpeed = 55f;          // degrees per second
    public float maxLookUp = 55f;
    public float maxLookDown = 45f;
    public float stickDeadzone = 0.2f;

    private Transform head, screens, front, pivot;
    private Vector3 frontBaseScale;
    private Vector3 headLocal;
    private float pitch;
    private bool ready;

    IEnumerator Start() {
        head = FindDeep(transform, "CC_HEAD");
        screens = FindDeep(transform, "CC_WALL_SCREENS");
        front = FindDeep(transform, "CC_FRONT");
        if (head == null || screens == null || front == null) {
            Debug.LogWarning("WallView: could not find CC_HEAD / CC_WALL_SCREENS / CC_FRONT.");
            yield break;
        }

        // Wait for CC_CONFIG_WALLS to apply the wall size from the lab's config file
        yield return null;
        yield return null;
        frontBaseScale = front.localScale;

        // Pivot that carries the head and the screen together for looking up/down
        pivot = new GameObject("CC_LOOK_PIVOT").transform;
        pivot.SetParent(transform, false);
        pivot.localPosition = Vector3.zero;
        pivot.localRotation = Quaternion.identity;
        head.SetParent(pivot, false);
        screens.SetParent(pivot, false);
        headLocal = head.localPosition;

        ready = true;
        ApplyWidth();
    }

    void Update() {
        if (!ready) return;

        float look = 0f;
        bool widen = false, narrow = false, reset = false;

        Gamepad gp = Gamepad.current;
        if (gp != null) {
            float y = gp.rightStick.ReadValue().y;
            if (Mathf.Abs(y) > stickDeadzone) look += y;
            widen |= gp.dpad.up.wasPressedThisFrame;
            narrow |= gp.dpad.down.wasPressedThisFrame;
            reset |= gp.rightStickButton.wasPressedThisFrame;
        }
        Keyboard kb = Keyboard.current;
        if (kb != null) {
            if (kb.pageUpKey.isPressed) look += 1f;
            if (kb.pageDownKey.isPressed) look -= 1f;
            widen |= kb.equalsKey.wasPressedThisFrame || kb.numpadPlusKey.wasPressedThisFrame;
            narrow |= kb.minusKey.wasPressedThisFrame || kb.numpadMinusKey.wasPressedThisFrame;
            reset |= kb.homeKey.wasPressedThisFrame;
        }

        if (widen) { viewWidth = Mathf.Min(maxWidth, viewWidth + widthStep); ApplyWidth(); }
        if (narrow) { viewWidth = Mathf.Max(minWidth, viewWidth - widthStep); ApplyWidth(); }

        if (reset) pitch = 0f;
        // Stick up = look up (negative X rotation in Unity)
        pitch = Mathf.Clamp(pitch - Mathf.Clamp(look, -1f, 1f) * lookSpeed * Time.deltaTime, -maxLookUp, maxLookDown);

        // Rotate around the eye so the head stays put and the view tilts
        Quaternion r = Quaternion.Euler(pitch, 0f, 0f);
        pivot.localRotation = r;
        pivot.localPosition = headLocal - r * headLocal;
    }

    void ApplyWidth() {
        if (front == null) return;
        front.localScale = new Vector3(frontBaseScale.x * viewWidth, frontBaseScale.y * viewWidth, frontBaseScale.z);
    }

    static Transform FindDeep(Transform parent, string name) {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++) {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
