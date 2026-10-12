using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Runs the show: plays the setlist through every stage speaker in sync,
// rotates Michael through each song's dances, and triggers crowd reactions.
// Put this on an empty GameObject named "ConcertManager".

[System.Serializable]
public class SetlistSong {
    public string title = "Song Title";
    public AudioClip clip;
    public string danceState = "Dance";   // first dance; must match a state name in MJ's Animator Controller
    public string[] moreDances;            // optional: extra dances to rotate through during the song
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
    public float danceSwitchSeconds = 20f;  // how long each dance lasts before rotating to the next

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
        if (performer != null && !string.IsNullOrEmpty(idleState)) performer.CrossFade(idleState, 0f);
        if (autoStart && setlist.Count > 0) {
            setRoutine = StartCoroutine(RunSet(0, startDelay));
        }
    }

    // Called from ConcertPlayerInput (RB / M key)
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

            // Rotate through this song's dances while it plays
            int danceIndex = 0;
            float nextSwitch = Time.time + danceSwitchSeconds;
            while (AudioSettings.dspTime < endTime) {
                if (Time.time >= nextSwitch) {
                    danceIndex++;
                    Dance(DanceFor(setlist[i], danceIndex));
                    nextSwitch = Time.time + danceSwitchSeconds;
                }
                yield return null;
            }

            BetweenSongs();
            yield return new WaitForSeconds(gapBetweenSongs);

            i++;
            if (i >= setlist.Count && loopSetlist) i = 0;
        }

        SetSign("Thank you, goodnight!");
        Dance(idleState);
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

        Dance(DanceFor(song, 0));
        SetSign(song.title);
        CrowdMember.CheerAll(1.5f);

        if (song.clip == null) {
            Debug.LogWarning("ConcertManager: song '" + song.title + "' has no AudioClip assigned.");
            return startTime + 5.0;
        }
        return startTime + song.clip.length;
    }

    // Dance list for a song = danceState followed by moreDances, looping around
    string DanceFor(SetlistSong song, int index) {
        List<string> list = new List<string>();
        if (!string.IsNullOrEmpty(song.danceState)) list.Add(song.danceState);
        if (song.moreDances != null) {
            for (int d = 0; d < song.moreDances.Length; d++) {
                if (!string.IsNullOrEmpty(song.moreDances[d])) list.Add(song.moreDances[d]);
            }
        }
        if (list.Count == 0) return idleState;
        return list[index % list.Count];
    }

    void Dance(string state) {
        if (performer != null && !string.IsNullOrEmpty(state)) performer.CrossFade(state, danceCrossfade);
    }

    void BetweenSongs() {
        Dance(idleState);
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
