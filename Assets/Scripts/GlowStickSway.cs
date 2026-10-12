using UnityEngine;

// Gently waves the player's glow stick side to side while it's held up.
public class GlowStickSway : MonoBehaviour {
    public float swayDegrees = 22f;
    public float swaySpeed = 1.6f;
    public float bobHeight = 0.04f;

    private Quaternion baseRotation;
    private Vector3 basePosition;

    void Awake() {
        baseRotation = transform.localRotation;
        basePosition = transform.localPosition;
    }

    void Update() {
        float t = Time.time * swaySpeed;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(t) * swayDegrees);
        transform.localPosition = basePosition + new Vector3(0f, Mathf.Abs(Mathf.Sin(t)) * bobHeight, 0f);
    }
}
