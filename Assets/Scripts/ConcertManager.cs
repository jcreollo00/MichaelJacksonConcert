using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Runs the show: plays the setlist through every stage speaker in sync,
// switches Michael's dance for each song, and triggers crowd reactions.
// Put this on an empty GameObject named "ConcertManager".

[System.Serializable]
public class SetlistSong {
    public string title = "Song Title";
    public AudioClip clip;
    public string danceState = "Dance";   // must match a state name in MJ's Animator Controller
}

public class ConcertManager : MonoBehaviour {
    public static ConcertManager Instance;

    [Header("Setlist")]
    public List<SetlistSong> setlist = new List<SetlistSong>();
    public bool autoStart = true;
    public float startDelay = 10f;          // time to walk in before the first song
    public float gapBetweenSongs = 6f;
    public bool loopSetlist = false;

    [Header("Stage Audio (all speakers play the same song in sync)")]
    public AudioSource[] stageSpeakers;

    [Header("Performer")]
    public Animator performer;
    public string idleState = "Idle";
    public float danceCrossfade = 0.6f;

    [Header("Crowd Reactions")]
    public AudioSource crowdCheerSource;
    public AudioClip[] cheerClips;

    [Header("Optional 3D sign above the stage")]
    public TextMesh nowPlayingSign;

    private int currentIndex = -1;
    private Coroutine setRoutine;

    public int CurrentIndex { get { return currentIndex; } }

    void Awake() {
        Instance = this;
    }

    void Start() {
        SetSign("Doors open...");
        if (performer != null) performer.CrossFade(idleState, 0f);
        if (autoStart && setlist.Count > 0) {
            setRoutine = StartCoroutine(RunSet(0, startDelay));
        }
    }

    // Called from ConcertPlayerInput (right wand menu button)
    public void NextSong() {
        if (setlist.Count == 0) return;
        int next = currentIndex + 1;
        if (next >= setlist.Count) next = 0;
        RestartFrom(next, 1.5f);
    }

    public void RestartFrom(int index, float delay) {
        if (setRoutine != null) StopCoroutine(setRoutine);
        StopSpeakers();
        PlayCheer();
        CrowdMember.CheerAll(0.8f);
        setRoutine = StartCoroutine(RunSet(index, delay));
    }

    IEnumerator RunSet(int startIndex, float delay) {
        yield return new WaitForSeconds(delay);

        int i = startIndex;
        while (i < setlist.Count) {
            double endTime = PlaySong(i);
            while (AudioSettings.dspTime < endTime) yield return null;

            BetweenSongs();
            yield return new WaitForSeconds(gapBetweenSongs);

            i++;
            if (i >= setlist.Count && loopSetlist) i = 0;
        }

        SetSign("Thank you, goodnight!");
        if (performer != null) performer.CrossFade(idleState, danceCrossfade);
    }

    double PlaySong(int index) {
        currentIndex = index;
        SetlistSong song = setlist[index];

        // Schedule every speaker on the same audio clock tick so they never drift apart
        double startTime = AudioSettings.dspTime + 0.2;
        for (int s = 0; s < stageSpeakers.Length; s++) {
            if (stageSpeakers[s] == null) continue;
            stageSpeakers[s].Stop();
            stageSpeakers[s].clip = song.clip;
            stageSpeakers[s].PlayScheduled(startTime);
        }

        if (performer != null && !string.IsNullOrEmpty(song.danceState)) {
            performer.CrossFade(song.danceState, danceCrossfade);
        }

        SetSign(song.title);
        CrowdMember.CheerAll(1.5f);

        if (song.clip == null) {
            Debug.LogWarning("ConcertManager: song '" + song.title + "' has no AudioClip assigned.");
            return startTime + 5.0;
        }
        return startTime + song.clip.length;
    }

    void BetweenSongs() {
        if (performer != null) performer.CrossFade(idleState, danceCrossfade);
        SetSign("");
        PlayCheer();
        CrowdMember.CheerAll(0.8f);
    }

    void StopSpeakers() {
        for (int s = 0; s < stageSpeakers.Length; s++) {
            if (stageSpeakers[s] != null) stageSpeakers[s].Stop();
        }
    }

    void PlayCheer() {
        if (crowdCheerSource == null || cheerClips == null || cheerClips.Length == 0) return;
        crowdCheerSource.PlayOneShot(cheerClips[Random.Range(0, cheerClips.Length)]);
    }

    void SetSign(string text) {
        if (nowPlayingSign != null) nowPlayingSign.text = text;
    }
}
