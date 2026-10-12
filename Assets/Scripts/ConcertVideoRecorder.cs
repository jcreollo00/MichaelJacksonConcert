using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder.Encoder;
#endif

// Press K in Play mode to start recording a 1920x1080 MP4 with game audio.
// Press K again to stop. Videos are saved in the "Recordings" folder next to Assets.
// It switches to the single-camera recording view automatically.
// (Editor only: this does nothing in the built app for the wall.)
public class ConcertVideoRecorder : MonoBehaviour {
    public bool switchToRecordingView = true;
    public int width = 1920;
    public int height = 1080;
    public float frameRate = 30f;
    public string folder = "Recordings";

#if UNITY_EDITOR
    private RecorderController controller;
    private bool recording;
    private string lastFile;

    void Update() {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.kKey.wasPressedThisFrame) {
            if (recording) StopRec(); else StartRec();
        }
    }

    void StartRec() {
        RecordingMode view = GetComponent<RecordingMode>();
        if (switchToRecordingView && view != null) view.SetRecording(true);

        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "Concert Video";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
        };
        // Record straight from the player's eye camera (follows you), not from the Game view
        if (view != null && view.centerCamera != null) {
            GameObject oldMain = GameObject.FindWithTag("MainCamera");
            if (oldMain != null && oldMain != view.centerCamera.gameObject) oldMain.tag = "Untagged";
            view.centerCamera.gameObject.tag = "MainCamera";
        }
        movie.ImageInputSettings = new CameraInputSettings {
            Source = ImageSource.MainCamera,
            OutputWidth = width,
            OutputHeight = height,
            CaptureUI = false
        };
        movie.AudioInputSettings.PreserveAudio = true;

        string dir = System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, folder);
        System.IO.Directory.CreateDirectory(dir);
        lastFile = System.IO.Path.Combine(dir, "ConcertVideo_" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        movie.OutputFile = lastFile;

        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        settings.FrameRate = frameRate;
        // Variable = record in real time, so the game never slows down and audio stays in sync
        settings.FrameRatePlayback = FrameRatePlayback.Variable;
        settings.CapFrameRate = true;

        controller = new RecorderController(settings);
        controller.PrepareRecording();
        controller.StartRecording();
        recording = true;
        Debug.Log("<color=red>● RECORDING</color> to " + lastFile + ".mp4  (press K to stop)");
    }

    void StopRec() {
        if (controller != null) controller.StopRecording();
        recording = false;
        Debug.Log("Recording saved: " + lastFile + ".mp4");
    }

    void OnDisable() {
        if (recording) StopRec();
    }
#endif
}
