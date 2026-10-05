using UnityEngine;

// The "walking in" moment: outside the arena the music sounds muffled and distant,
// and it opens up to full, clear sound as you step through the entrance.
// 1. Add a BoxCollider covering the whole arena interior, tick "Is Trigger".
// 2. Add an AudioLowPassFilter component to each stage speaker AudioSource.
// 3. Put this script on the same object as the BoxCollider and fill in the fields.

public class VenueAudioZone : MonoBehaviour {
    public BoxCollider venueBounds;
    public Transform listener;                     // leave empty to use the scene's AudioListener

    [Header("Muffled when outside")]
    public AudioLowPassFilter[] muffledFilters;
    public float outsideCutoff = 700f;
    public float insideCutoff = 22000f;

    [Header("Quieter when outside (stage speakers, crowd ambience)")]
    public AudioSource[] venueSources;
    [Range(0f, 1f)] public float outsideVolumeScale = 0.35f;

    public float fadeSpeed = 0.8f;

    private float blend;          // 0 = outside, 1 = inside
    private float[] baseVolumes;

    public bool IsInside { get { return blend > 0.5f; } }

    void Start() {
        if (venueBounds == null) venueBounds = GetComponent<BoxCollider>();
        if (listener == null) {
            AudioListener al = FindFirstObjectByType<AudioListener>();
            if (al != null) listener = al.transform;
        }
        baseVolumes = new float[venueSources.Length];
        for (int i = 0; i < venueSources.Length; i++) {
            baseVolumes[i] = venueSources[i] != null ? venueSources[i].volume : 1f;
        }
    }

    void Update() {
        if (venueBounds == null || listener == null) return;

        bool inside = venueBounds.bounds.Contains(listener.position);
        blend = Mathf.MoveTowards(blend, inside ? 1f : 0f, Time.deltaTime * fadeSpeed);

        // Interpolate cutoff on a log scale so the change sounds even to the ear
        float cutoff = Mathf.Exp(Mathf.Lerp(Mathf.Log(outsideCutoff), Mathf.Log(insideCutoff), blend));
        for (int i = 0; i < muffledFilters.Length; i++) {
            if (muffledFilters[i] != null) muffledFilters[i].cutoffFrequency = cutoff;
        }

        float volScale = Mathf.Lerp(outsideVolumeScale, 1f, blend);
        for (int i = 0; i < venueSources.Length; i++) {
            if (venueSources[i] != null) venueSources[i].volume = baseVolumes[i] * volScale;
        }
    }
}
