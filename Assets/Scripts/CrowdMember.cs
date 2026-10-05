using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Put this on every audience prefab (CrowdSpawner adds it automatically if missing).
// The prefab's Animator Controller needs states named to match danceStates and cheerState.

public class CrowdMember : MonoBehaviour {
    public static List<CrowdMember> All = new List<CrowdMember>();

    public string[] danceStates = new string[] { "Dance1", "Dance2" };
    public string cheerState = "Cheer";
    public float cheerDuration = 3f;
    public GameObject handProp;   // glow stick / lighter parented to the hand bone, starts disabled

    private Animator anim;
    private bool cheering;

    void OnEnable() { All.Add(this); }
    void OnDisable() { All.Remove(this); }

    void Start() {
        anim = GetComponentInChildren<Animator>();
        if (handProp != null) handProp.SetActive(false);
        if (anim == null) return;

        // Big performance win for stereo rendering: don't animate people nobody can see
        anim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        // Slightly different speed and start time so the crowd never moves in lockstep
        anim.speed = Random.Range(0.9f, 1.1f);
        string dance = PickDance();
        if (dance != null) anim.Play(dance, 0, Random.value);
    }

    string PickDance() {
        if (danceStates == null || danceStates.Length == 0) return null;
        return danceStates[Random.Range(0, danceStates.Length)];
    }

    public void Cheer(float delay) {
        if (cheering || anim == null || !gameObject.activeInHierarchy) return;
        StartCoroutine(CheerRoutine(delay));
    }

    IEnumerator CheerRoutine(float delay) {
        cheering = true;
        yield return new WaitForSeconds(delay);
        if (!string.IsNullOrEmpty(cheerState)) anim.CrossFade(cheerState, 0.25f);
        yield return new WaitForSeconds(cheerDuration + Random.Range(-0.5f, 0.5f));
        string dance = PickDance();
        if (dance != null) anim.CrossFade(dance, 0.4f);
        cheering = false;
    }

    public void SetPropRaised(bool raised, float delay) {
        if (handProp == null || !gameObject.activeInHierarchy) return;
        StartCoroutine(PropRoutine(raised, delay));
    }

    IEnumerator PropRoutine(bool raised, float delay) {
        yield return new WaitForSeconds(delay);
        handProp.SetActive(raised);
    }

    // ---------- Helpers other scripts call ----------

    public static void CheerAll(float maxDelay) {
        for (int i = 0; i < All.Count; i++) All[i].Cheer(Random.Range(0f, maxDelay));
    }

    // Cheer ripples outward from the player: closer people react first
    public static void CheerNear(Vector3 center, float radius) {
        for (int i = 0; i < All.Count; i++) {
            float d = FlatDistance(All[i].transform.position, center);
            if (d < radius) All[i].Cheer(d * 0.08f + Random.Range(0f, 0.2f));
        }
    }

    public static void RaisePropsNear(Vector3 center, float radius) {
        for (int i = 0; i < All.Count; i++) {
            float d = FlatDistance(All[i].transform.position, center);
            if (d < radius) All[i].SetPropRaised(true, d * 0.1f + Random.Range(0f, 0.3f));
        }
    }

    public static void LowerAllProps() {
        for (int i = 0; i < All.Count; i++) All[i].SetPropRaised(false, Random.Range(0f, 0.6f));
    }

    static float FlatDistance(Vector3 a, Vector3 b) {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
