using UnityEngine;
using System.Collections;

// The opening moment: once you walk out of the tunnel into the arena,
// the house lights fade to black, the crowd screams, "MICHAEL JACKSON"
// glows above the stage, then the lights slam back on with confetti
// and the first song starts.
// Set ConcertManager's Auto Start to OFF when using this.
public class ShowIntro : MonoBehaviour {
    [Header("When to start")]
    public VenueAudioZone venue;            // starts when the player is inside the arena
    public float minWaitSeconds = 4f;
    public float maxWaitSeconds = 30f;      // starts anyway after this long

    [Header("Lights to fade (washes, front-of-house, Michael's key/rim, uplights)")]
    public Light[] houseLights;
    [Range(0f, 1f)] public float blackoutLevel = 0.03f;
    public float fadeDownSeconds = 2.5f;
    public float darkHoldSeconds = 3.5f;

    [Header("Moment")]
    public AudioSource screamSource;
    public AudioClip screamClip;
    public GameObject titleSign;
    public ConfettiCannon confetti;

    private float[] baseIntensity;

    IEnumerator Start() {
        baseIntensity = new float[houseLights.Length];
        for (int i = 0; i < houseLights.Length; i++) {
            if (houseLights[i] != null) baseIntensity[i] = houseLights[i].intensity;
        }
        if (titleSign != null) titleSign.SetActive(false);

        // Wait for the player to enter the arena
        float t0 = Time.time;
        while (Time.time - t0 < maxWaitSeconds) {
            if (ShowAlreadyStarted()) yield break;   // player skipped ahead with Next Song
            bool inside = venue != null && venue.IsInside;
            if (Time.time - t0 > minWaitSeconds && inside) break;
            yield return null;
        }
        if (ShowAlreadyStarted()) yield break;

        // Fade to black
        float t = 0f;
        while (t < fadeDownSeconds) {
            t += Time.deltaTime;
            SetLevel(Mathf.Lerp(1f, blackoutLevel, Mathf.SmoothStep(0f, 1f, t / fadeDownSeconds)));
            yield return null;
        }
        SetLevel(blackoutLevel);

        // Darkness: crowd screams, title glows
        if (screamSource != null && screamClip != null) screamSource.PlayOneShot(screamClip, 1f);
        CrowdMember.CheerAll(0.5f);
        if (titleSign != null) titleSign.SetActive(true);
        yield return new WaitForSeconds(darkHoldSeconds);
        if (titleSign != null) titleSign.SetActive(false);

        // Lights slam on, confetti, first song
        SetLevel(1f);
        if (confetti != null) confetti.Fire();
        if (ConcertManager.Instance != null) ConcertManager.Instance.RestartFrom(0, 0.2f);
    }

    bool ShowAlreadyStarted() {
        return ConcertManager.Instance != null && ConcertManager.Instance.CurrentIndex >= 0;
    }

    void SetLevel(float k) {
        AudioReactiveLights.masterLevel = k;
        for (int i = 0; i < houseLights.Length; i++) {
            if (houseLights[i] != null) houseLights[i].intensity = baseIntensity[i] * k;
        }
    }
}
