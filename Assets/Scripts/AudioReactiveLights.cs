using UnityEngine;

// Makes stage lights pulse with the bass, cycle colors, and sweep like moving-head lights.
// Put this on an empty "StageLights" object and drag in one of the stage speakers.

public class AudioReactiveLights : MonoBehaviour {
    public AudioSource source;
    public Light[] lights;
    public float minIntensity = 0.5f;
    public float maxIntensity = 6f;

    [Header("Beat detection (low bins = bass/kick drum)")]
    public int bandStart = 0;
    public int bandEnd = 6;
    public float sensitivity = 40f;
    public float smoothing = 10f;

    [Header("Color")]
    public Gradient colorCycle;
    public float colorCycleSpeed = 0.05f;

    [Header("Moving heads (spot light transforms that sweep)")]
    public Transform[] movingHeads;
    public float sweepAngle = 30f;
    public float sweepSpeed = 0.7f;

    [Header("Optional glowing surfaces (screens, truss strips)")]
    public Renderer[] emissiveRenderers;

    private float[] spectrum = new float[256];
    private float level;
    private Quaternion[] baseRotations;

    public float Level { get { return level; } }

    void Reset() {
        colorCycle = new Gradient();
        colorCycle.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(1f, 0.2f, 0.6f), 0f),
                new GradientColorKey(new Color(0.2f, 0.4f, 1f), 0.33f),
                new GradientColorKey(new Color(1f, 0.85f, 0.3f), 0.66f),
                new GradientColorKey(new Color(1f, 0.2f, 0.6f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
    }

    void Start() {
        baseRotations = new Quaternion[movingHeads.Length];
        for (int i = 0; i < movingHeads.Length; i++) {
            if (movingHeads[i] != null) baseRotations[i] = movingHeads[i].localRotation;
        }
        for (int i = 0; i < emissiveRenderers.Length; i++) {
            if (emissiveRenderers[i] != null) emissiveRenderers[i].material.EnableKeyword("_EMISSION");
        }
    }

    void Update() {
        float target = 0f;
        if (source != null && source.isPlaying) {
            source.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
            float sum = 0f;
            int end = Mathf.Min(bandEnd, spectrum.Length - 1);
            for (int b = bandStart; b <= end; b++) sum += spectrum[b];
            target = Mathf.Clamp01(sum * sensitivity);
        }
        level = Mathf.Lerp(level, target, Time.deltaTime * smoothing);

        for (int i = 0; i < lights.Length; i++) {
            if (lights[i] == null) continue;
            lights[i].intensity = Mathf.Lerp(minIntensity, maxIntensity, level);
            if (colorCycle != null) {
                float t = Mathf.Repeat(Time.time * colorCycleSpeed + i * 0.15f, 1f);
                lights[i].color = colorCycle.Evaluate(t);
            }
        }

        for (int i = 0; i < movingHeads.Length; i++) {
            if (movingHeads[i] == null) continue;
            float t = Time.time * sweepSpeed + i * 1.3f;
            movingHeads[i].localRotation = baseRotations[i] *
                Quaternion.Euler(Mathf.Sin(t) * sweepAngle, Mathf.Cos(t * 0.7f) * sweepAngle * 0.6f, 0f);
        }

        for (int i = 0; i < emissiveRenderers.Length; i++) {
            if (emissiveRenderers[i] == null || colorCycle == null) continue;
            Color c = colorCycle.Evaluate(Mathf.Repeat(Time.time * colorCycleSpeed, 1f));
            emissiveRenderers[i].material.SetColor("_EmissionColor", c * (0.3f + level * 2f));
        }
    }
}
