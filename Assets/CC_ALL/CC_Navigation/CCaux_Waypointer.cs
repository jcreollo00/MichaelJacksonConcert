/**
 * (C) 2016 Jason Leigh- Laboratory for Advanced Visualization & Applications
 * University of Hawaii at Manoa
 * 
 * CCaux_Waypointer script 
 * Version 09/10/2025 - Added Catmull-Rom Spline as well as linear interpolation 
 * Version 09/28/2022 - returned FixedUpdate to Update - for some reason keyboard now no longer responsive
 * Version 09/17/2022 - moved code from Update to FixedUpdate to fix some keyboard bounce issues
 * Version 08/13/2022 - updated to use Unityʻs new input manager
 * Version 11/17/2016 - first version
 
 * To use this script add it to the CC_CANOE game object.
 * 
 * The script enables a user to set up navigational waypoints that can later be played back to create a custom
 * tour of the VR space.
 * 
 * It is also possible to establish multiple different tours by adding multiple copies of this script
 * into CC_CANOE and enabling only one of the scripts at a time.
 * 
 * To create a waypoint one has to navigate in the space to a desired position (and orientation) and then record a waypoint.
 * To establish another waypoint, navigate to another part of the space and record another waypoint and so on.
 * For navigation consider using either CC_auxOmniNavigator or CC_auxNavigator.
 * 
 * The wand controls can be set up to bind to Waypointer's various functions such as,
 * record a waypoint, play entire trip, go to next waypoint,
 * go to previous waypoint, delete a waypoint, save waypoint file.
 * 
 * Created tours can be saved and read from a file. When the script launches it will attempt to load the waypoint file
 * specified by the Waypoint File parameter.
 * 
 * The "Journey Time" parameter lets you establish how long you would like the entire journey through all the waypoints to take.
 * 
 * The FixedUpdate function provides a good example of how to use the member functions like: play, stop, delete, next frame, previous frame, etc.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class CCaux_Waypointer : MonoBehaviour {

    [Header("Waypointer Settings")]
    [Tooltip("Loop")]
    public bool loopPlayback = true;

    [Tooltip("Play Button")]
    public GamepadButton playButton = GamepadButton.North;

    [Tooltip("Key to play through waypoints")]
    public Key playKey = Key.Space;

    [Tooltip("Previous Waypoint Button")]
    public GamepadButton previousButton = GamepadButton.West;

    [Tooltip("Previous Waypoint Key")]
    public Key previousKey = Key.P;

    [Tooltip("Next Waypoint Button")]
    public GamepadButton nextButton = GamepadButton.East;

    [Tooltip("Next Waypoint Key")]
    public Key nextKey = Key.N;

    [Tooltip("Save Waypoints to File")]
    public Key saveKey = Key.S; 

    [Tooltip("Record Waypoint")]
    public Key recordKey = Key.R;

    [Tooltip("Delete Waypoint")]
    public Key deleteKey = Key.X;

    public bool enablePlayButton = true;

    public bool enableRecordButton = true;

    public bool enableNextButton = true;

    public bool enablePreviousButton = true;

    public bool enableDeleteButton = true;

    public bool enableSaveButton = true;
    [Tooltip("How much time for the entire trip (in seconds)")]
    public float journeyTime = 15.0f;
    [Tooltip("Always play from start of animation sequence")]
    public bool playFromStart = true;
    [Tooltip("Filename to load/save waypoints")]
    public string waypointFile = "WAYPOINT.txt";
    [Tooltip("Load wavepoint file on startup")]
    public bool loadWaypointFileOnStart = true;
    [Tooltip("Play sound to start animation")]
    public GameObject soundToPlayOnStart;
    [Tooltip("Play sound to announce save")]
    public GameObject soundToPlayOnSave;
    [Tooltip("Smooth out movement using splines rather than linear interpolation")]
    public bool useSplineSmoothMovement = true;

    public GameObject mainNavigationNode = null;
    private Transform charCont;
    private LinkedList<Vector3> navPositionList = new LinkedList<Vector3>();
    private LinkedList<Quaternion> navRotationList = new LinkedList<Quaternion>();

    private Vector3 startFramePosition;
    private Quaternion startFrameRotation;
    private Vector3 endFramePosition;
    private Quaternion endFrameRotation;

    private LinkedListNode<Vector3> currentFramePos = null;
    private LinkedListNode<Quaternion> currentFrameRot = null;
    private bool playMode = false;
    private float startTime = 0;
    private bool playingFrame = false;
    private bool nextPrevAnimation = false;
    private float timeBetweenFrames = 0.5f;


    // Use this for initialization
    void Start () {
        charCont = mainNavigationNode.transform;
        if (loadWaypointFileOnStart)
            ReadWayPoints();
    }

    void Play(){
        if (playFromStart) PlayFromStart();
        else PlayFromCurrent();
    }

    void PlayFromCurrent(){

        playingFrame = true;
        playMode = true;


        if (navPositionList.Count > 0) {
            startFramePosition = charCont.transform.position;
            startFrameRotation = charCont.transform.rotation;

            if (currentFramePos.Next == null) {
                currentFramePos = navPositionList.First;
                currentFrameRot = navRotationList.First;
            }
            endFramePosition = currentFramePos.Value;
            endFrameRotation = currentFrameRot.Value;
            startTime = Time.time;
    
        }

    }
    void PlayFromStart() 
    {
        if (navPositionList.Count >0) {
            currentFramePos = navPositionList.First;
            currentFrameRot = navRotationList.First;

            startFramePosition = charCont.transform.position;
            startFrameRotation = charCont.transform.rotation;

            endFramePosition = currentFramePos.Value;
            endFrameRotation = currentFrameRot.Value;

            playingFrame = true;
            playMode = true;

            startTime = Time.time;
        }
    }

    void SetLoop(bool loop) {
        loopPlayback = loop;
    }

    void Stop() {
        playMode = false;

    }

    void ReadWayPoints(){
        // Delete waypoints first if any
        char[] delimiterChars = { ' ', ',', ':', '\t' };

        navPositionList.Clear();
        navRotationList.Clear();
        System.IO.StreamReader file =  new System.IO.StreamReader(waypointFile);
        if (file == null) return;

        string line;
        while((line = file.ReadLine()) != null){
            string[] words = line.Split(delimiterChars);
            Vector3 pos = new Vector3();
            Quaternion rot;
            pos.x = float.Parse(words[0]);
            pos.y = float.Parse(words[1]);
            pos.z = float.Parse(words[2]);
            rot = new Quaternion(
                 float.Parse(words[3]),
                 float.Parse(words[4]),
                float.Parse(words[5]),
                float.Parse(words[6]));
        
            navPositionList.AddLast(pos);
            navRotationList.AddLast(rot);
        }
        if (navPositionList.Count > 0) {
            currentFramePos = navPositionList.First;
            currentFrameRot = navRotationList.First;
        }
        file.Close();
    }

    void SaveWayPoints() {

        if (navPositionList.Count > 0 ) {
            System.IO.StreamWriter filew = new System.IO.StreamWriter(waypointFile);

            LinkedListNode<Vector3> fPos = navPositionList.First;
            LinkedListNode<Quaternion> fRot = navRotationList.First;

            while(fPos != null) {
                string line;
                line = fPos.Value.x + " " + fPos.Value.y + " " + fPos.Value.z + " " + fRot.Value.x + " " + fRot.Value.y + " " + fRot.Value.z + " " + fRot.Value.w;
                filew.WriteLine(line);
                fPos=fPos.Next;
                fRot=fRot.Next;
            }
            filew.Close();
        }
        else {
            System.IO.StreamWriter filew = new System.IO.StreamWriter(waypointFile);
            filew.Close();
        }


    }


    void Insert(){

        if (currentFramePos == null) {
            navPositionList.AddLast(charCont.transform.position);
            navRotationList.AddLast(charCont.transform.rotation);
            currentFramePos = navPositionList.First;
            currentFrameRot = navRotationList.First;
        } else {
            navPositionList.AddBefore(currentFramePos,charCont.transform.position);
            navRotationList.AddBefore(currentFrameRot,charCont.transform.rotation);
            currentFramePos = currentFramePos.Previous;
            currentFrameRot = currentFrameRot.Previous;
        }
        print("CC_aux_Waypointer number of key frames: "+navPositionList.Count);
    }

    void InsertAfter(){

        if (currentFramePos == null) {
            navPositionList.AddLast(charCont.transform.position);
            navRotationList.AddLast(charCont.transform.rotation);
            currentFramePos = navPositionList.First;
            currentFrameRot = navRotationList.First;
        } else {
            navPositionList.AddAfter(currentFramePos,charCont.transform.position);
            navRotationList.AddAfter(currentFrameRot,charCont.transform.rotation);
            currentFramePos = currentFramePos.Next;
            currentFrameRot = currentFrameRot.Next;
        }
        print("CC_aux_Waypointer number of key frames: "+navPositionList.Count);

    }

    void Delete() {

        if (navPositionList.Count > 0) {
            LinkedListNode <Vector3> saveNextPos = currentFramePos.Next;
            LinkedListNode <Quaternion> saveNextRot = currentFrameRot.Next;

            LinkedListNode <Vector3> savePos = currentFramePos.Previous;
            LinkedListNode <Quaternion> saveRot = currentFrameRot.Previous;

            navPositionList.Remove(currentFramePos);
            navRotationList.Remove(currentFrameRot);
            if (savePos != null) {
                currentFramePos = savePos;
                currentFrameRot = saveRot;
            } else {
                currentFramePos = saveNextPos;
                currentFrameRot = saveNextRot;
            }
            if (currentFramePos != null) {
                charCont.transform.position = currentFramePos.Value;
                charCont.transform.rotation = currentFrameRot.Value;
            }
        }
    }


    bool IsPlaying() {
        return playMode;
    }

    void NextFrame() {
        if (navPositionList.Count > 0) {
            currentFramePos = currentFramePos.Next;
            currentFrameRot = currentFrameRot.Next;

            if (currentFramePos == null) {
                currentFramePos = navPositionList.First;
                currentFrameRot = navRotationList.First;
            }

            playingFrame = true;

            startFramePosition = charCont.transform.position;
            startFrameRotation = charCont.transform.rotation;
            
            endFramePosition = currentFramePos.Value;
            endFrameRotation = currentFrameRot.Value;
            startTime = Time.time;
            playMode = true;
            nextPrevAnimation = true;

        }
    }

    void PrevFrame() {

        if (navPositionList.Count > 0) {
            currentFramePos = currentFramePos.Previous;
            currentFrameRot = currentFrameRot.Previous;

            if (currentFramePos == null) {
                currentFramePos = navPositionList.Last;
                currentFrameRot = navRotationList.Last;
            }

            playingFrame = true;

            startFramePosition = charCont.transform.position;
            startFrameRotation = charCont.transform.rotation;

            endFramePosition = currentFramePos.Value;
            endFrameRotation = currentFrameRot.Value;
            startTime = Time.time;
            playMode = true;
            nextPrevAnimation = true;

        }
    }

// Catmull–Rom interpolation (centripetal variant factor = 0.5)
private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t) {
    // Standard uniform Catmull–Rom
    float t2 = t * t;
    float t3 = t2 * t;
    return 0.5f * (
        (2f * p1) +
        (-p0 + p2) * t +
        (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
        (-p0 + 3f * p1 - 3f * p2 + p3) * t3
    );
}

    void DoAnimation() {

        if (!playMode) return;
        if (!playingFrame) {
            if (currentFramePos.Next != null) {
                startFramePosition = currentFramePos.Value;
                startFrameRotation = currentFrameRot.Value;
                endFramePosition = currentFramePos.Next.Value;
                endFrameRotation = currentFrameRot.Next.Value;
                startTime = Time.time;
                playingFrame =true;
                currentFramePos = currentFramePos.Next;
                currentFrameRot = currentFrameRot.Next;
                if (currentFramePos == null) {
                    currentFramePos = navPositionList.First;
                    currentFrameRot = navRotationList.First;
                }
            } else {
                if (loopPlayback) {
                    startFramePosition = currentFramePos.Value;
                    startFrameRotation = currentFrameRot.Value;

                    currentFramePos = navPositionList.First;
                    currentFrameRot = navRotationList.First;

                    endFramePosition = currentFramePos.Value;
                    endFrameRotation = currentFrameRot.Value;
                    startTime = Time.time;
                    playingFrame = true;
                } else {
                    playMode = false;
                    playingFrame = false;
                }
            }
        }

        float fracComplete;
    
                if (nextPrevAnimation == false) 
                    fracComplete = (Time.time - startTime) / (journeyTime/navPositionList.Count);
                else 
                    fracComplete = (Time.time - startTime) / timeBetweenFrames;


        //charCont.transform.position = Vector3.Slerp (startFramePosition, endFramePosition, fracComplete);

        // Catmull–Rom between current segment (p1 -> p2)
        Vector3 p1 = startFramePosition;
        Vector3 p2 = endFramePosition;

        // If we don't have enough points, fall back to linear
        if ((navPositionList.Count < 4) || (!useSplineSmoothMovement)) {
            charCont.transform.position = Vector3.Lerp(p1, p2, Mathf.Clamp01(fracComplete));
        } else {
            // p0 = previous of p1, p3 = next of p2 (handle ends with wrap if looping, else clamp)
            LinkedListNode<Vector3> node1 = currentFramePos;          // currentFramePos is the "end" node for the segment (the code advances it earlier)
            LinkedListNode<Vector3> node2 = currentFramePos.Next;     // may be null at tail

            // Reconstruct node pointers for p1/p2 that match start/end values
            // p2 should be the node whose value == endFramePosition; p1 == startFramePosition.
            // Walk backward/forward safely:
            LinkedListNode<Vector3> segEnd = currentFramePos; // holds endFramePosition
            LinkedListNode<Vector3> segStart = segEnd.Previous ?? (loopPlayback ? navPositionList.Last : null);

            // Guard: if segStart is null (no previous and not looping), just lerp
            if (segStart == null) {
                charCont.transform.position = Vector3.Lerp(p1, p2, Mathf.Clamp01(fracComplete));
            } else {
                LinkedListNode<Vector3> p0n = segStart.Previous ?? (loopPlayback ? navPositionList.Last : segStart);
                LinkedListNode<Vector3> p3n = segEnd.Next ?? (loopPlayback ? navPositionList.First : segEnd);

                Vector3 p0 = p0n.Value;
                Vector3 p3 = p3n.Value;

                charCont.transform.position = CatmullRom(p0, p1, p2, p3, Mathf.Clamp01(fracComplete));
            }
        }

        ////////
        charCont.transform.rotation = Quaternion.Slerp (startFrameRotation, endFrameRotation, fracComplete);

        if (fracComplete >= 1.0f) playingFrame = false;
        if ((playingFrame == false) && (nextPrevAnimation == true)) {
            playMode = false;
            nextPrevAnimation = false;
        }
    }


    void Update () {

        if (Keyboard.current[recordKey].wasPressedThisFrame && enableRecordButton){
        //if((Input.GetKeyDown(recordKey)) && enableRecordButton) {
            InsertAfter();

        }

        bool activatedGP = false;

        if (Gamepad.current != null) activatedGP = Gamepad.current[playButton].wasPressedThisFrame;

        if ((activatedGP || Keyboard.current[playKey].wasPressedThisFrame) && enablePlayButton){
        //if((Input.GetKeyDown(playKey)||(Input.GetKeyDown(KeyCode.Space))) && enablePlayButton) {
            if (navPositionList.Count > 0) {
                if (playMode) {
                    if (soundToPlayOnStart)  soundToPlayOnStart.GetComponent<AudioSource>().Stop();
                    Stop();
                }
                else {
                    Play();
                    if (soundToPlayOnStart) {
                        soundToPlayOnStart.GetComponent<AudioSource>().Stop();
                        soundToPlayOnStart.GetComponent<AudioSource>().Play();
                    }
                }
            
            }
        }
        if (Gamepad.current != null) activatedGP = Gamepad.current[nextButton].wasPressedThisFrame;

        if ((activatedGP || Keyboard.current[nextKey].wasPressedThisFrame) && enableNextButton){
        //if((Input.GetKeyDown(nextKey)) && enableNextButton) {
            NextFrame();
        }

        if (Gamepad.current != null) activatedGP = Gamepad.current[previousButton].wasPressedThisFrame;

        if ((activatedGP || Keyboard.current[previousKey].wasPressedThisFrame) && enablePreviousButton){
        //if ((Input.GetKeyDown(previousKey)) && enablePreviousButton) {
            PrevFrame();

        }

    
        if (Keyboard.current[deleteKey].wasPressedThisFrame && enableDeleteButton){
        //if ((Input.GetKeyDown(removeKey)) && enableDeleteButton) {
            Delete();
        }

        if (Keyboard.current[saveKey].wasPressedThisFrame && enableSaveButton){
        //if ((Input.GetKeyDown(saveKey)) && enableSaveButton) {
            SaveWayPoints();
            if (soundToPlayOnSave) {
                soundToPlayOnSave.GetComponent<AudioSource>().Play();
            }

        }

        DoAnimation();
    }
}

