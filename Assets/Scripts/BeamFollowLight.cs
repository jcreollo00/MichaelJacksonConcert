using UnityEngine;

// Visible light beam (a cone through the fog) that matches its spotlight's
// color and brightness every frame. Put this on the beam mesh, as a child of the Light.
[RequireComponent(typeof(Renderer))]
public class BeamFollowLight : MonoBehaviour {
    public Light source;
    [Range(0f, 1f)] public float opacity = 0.18f;
    public float intensityForFullBeam = 8f;

    private Renderer rend;
    private MaterialPropertyBlock block;
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    void Awake() {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        if (source == null) source = GetComponentInParent<Light>();
    }

    void LateUpdate() {
        if (source == null) return;
        float strength = Mathf.Clamp01(source.intensity / intensityForFullBeam);
        Color c = source.color;
        c.a = opacity * (0.35f + 0.65f * strength);
        rend.GetPropertyBlock(block);
        block.SetColor(BaseColor, c);
        rend.SetPropertyBlock(block);
    }
}
