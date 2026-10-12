using UnityEngine;
using UnityEngine.InputSystem;

// Fires confetti from the truss over the crowd. Pieces float down toward the
// viewer, which reads strongly in stereo 3D.
// Controls: Left Bumper (LB) on the game controller, or F on the keyboard.
public class ConfettiCannon : MonoBehaviour {
    public static ConfettiCannon Instance;

    public ParticleSystem[] cannons;
    public int piecesPerCannon = 450;
    public float cooldown = 3f;
    public bool crowdCheersOnFire = true;

    [Header("Sound (a cannon 'thump' is generated if no clip is set)")]
    public AudioSource popSource;
    public AudioClip popClip;

    private float lastFire = -99f;

    void Awake() {
        Instance = this;
        if (popClip == null) popClip = MakeThump();
    }

    void Update() {
        bool pressed = false;
        Gamepad gp = Gamepad.current;
        if (gp != null && gp.leftShoulder.wasPressedThisFrame) pressed = true;
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.fKey.wasPressedThisFrame) pressed = true;
        if (pressed) Fire();
    }

    public void Fire() {
        if (Time.time - lastFire < cooldown) return;
        lastFire = Time.time;
        for (int i = 0; i < cannons.Length; i++) {
            if (cannons[i] != null) cannons[i].Emit(piecesPerCannon);
        }
        if (popSource != null && popClip != null) popSource.PlayOneShot(popClip);
        if (crowdCheersOnFire) CrowdMember.CheerAll(0.6f);
    }

    // Short low "boom + air burst" so the cannon has a sound without needing a file
    static AudioClip MakeThump() {
        int rate = 44100;
        int samples = (int)(rate * 0.6f);
        float[] data = new float[samples];
        System.Random rng = new System.Random(7);
        for (int i = 0; i < samples; i++) {
            float t = (float)i / rate;
            float boom = Mathf.Sin(2f * Mathf.PI * (70f - 40f * t) * t) * Mathf.Exp(-t * 9f);
            float air = ((float)rng.NextDouble() * 2f - 1f) * Mathf.Exp(-t * 14f) * 0.5f;
            data[i] = Mathf.Clamp((boom + air) * 0.9f, -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("ConfettiThump", samples, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
