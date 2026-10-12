using UnityEngine;
using System.Collections;

// Spark fountains ("gerbs") along the front of the stage that fire at the
// start of every song, with a flash of light and a whoosh.
public class PyroController : MonoBehaviour {
    public ParticleSystem[] fountains;
    public Light[] flashLights;
    public float flashIntensity = 8f;
    public float burstSeconds = 2.5f;
    public AudioSource whooshSource;

    private int lastSong = -1;
    private AudioClip whoosh;

    void Awake() {
        whoosh = MakeWhoosh();
        SetFlash(0f);
    }

    void Update() {
        if (ConcertManager.Instance == null) return;
        int song = ConcertManager.Instance.CurrentIndex;
        if (song >= 0 && song != lastSong) {
            lastSong = song;
            StartCoroutine(Fire());
        }
    }

    public IEnumerator Fire() {
        for (int i = 0; i < fountains.Length; i++) if (fountains[i] != null) fountains[i].Play();
        if (whooshSource != null) whooshSource.PlayOneShot(whoosh, 0.8f);
        float t = 0f;
        while (t < burstSeconds) {
            t += Time.deltaTime;
            float k = 1f - t / burstSeconds;
            SetFlash(flashIntensity * k * (0.8f + Random.value * 0.4f));   // flicker like fire
            yield return null;
        }
        for (int i = 0; i < fountains.Length; i++) if (fountains[i] != null) fountains[i].Stop();
        SetFlash(0f);
    }

    void SetFlash(float intensity) {
        for (int i = 0; i < flashLights.Length; i++) if (flashLights[i] != null) flashLights[i].intensity = intensity;
    }

    // Rushing-air sound made from filtered noise, so no audio file is needed
    static AudioClip MakeWhoosh() {
        int rate = 44100, samples = (int)(rate * 2.2f);
        float[] data = new float[samples];
        System.Random rng = new System.Random(11);
        float lp = 0f;
        for (int i = 0; i < samples; i++) {
            float t = (float)i / rate;
            float env = Mathf.Min(1f, t * 12f) * Mathf.Exp(-t * 1.6f);
            float n = (float)rng.NextDouble() * 2f - 1f;
            lp += (n - lp) * 0.08f;                     // soften the hiss
            data[i] = Mathf.Clamp(lp * env * 2.2f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("PyroWhoosh", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
