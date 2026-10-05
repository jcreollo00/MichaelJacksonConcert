using UnityEngine;
using UnityEngine.InputSystem;

// Player controls for the class CyberCANOE kit (CC_HUB + Xbox-style game controller).
// Put this on the CC_HUB object.
// IMPORTANT: disable the DPadFlyer script on CC_HUB (it lets you fly and roll, which we don't want).
// You can keep CCaux_Waypointer enabled: its X/B/Y buttons don't conflict with anything here,
// and its waypoints make great "viewing spots" and a recorded flythrough for your video.
//
// Game controller:
//   Left stick        = walk forward/back and step sideways (stays on the floor)
//   Right stick L/R   = turn
//   Right trigger     = hold to raise your glow stick; the people around you raise theirs too
//   A button          = cheer; the cheer ripples outward through the crowd
//   Right bumper (RB) = skip to the next song
//
// Keyboard (for testing on your laptop without a controller):
//   Up/Down arrows = walk, J/L = step sideways, Left/Right arrows = turn
//   G (hold) = glow stick, C = cheer, M = next song
//   (WASD is avoided on purpose: the kit's waypoint script uses A, S, X, R, N and P.)

[RequireComponent(typeof(CharacterController))]
public class ConcertPlayerInput : MonoBehaviour {
    [Header("References")]
    public Transform head;                         // CC_HEAD, found automatically if left empty

    [Header("Walking")]
    public float walkSpeed = 1.4f;                 // normal walking pace, comfortable on a stereo wall
    public float turnSpeed = 60f;                  // degrees per second
    public float stickDeadzone = 0.15f;
    public float gravity = -9.81f;

    [Header("Crowd interaction")]
    public float lightsRadius = 6f;
    public float cheerRadius = 8f;
    public GameObject playerGlowStick;             // optional, starts disabled
    public AudioSource playerCheerSource;          // AudioSource on CC_HUB or CC_HEAD
    public AudioClip[] cheerClips;

    private CharacterController cc;
    private float verticalVelocity;
    private bool lightsUp;
    private float lightsRefreshTimer;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float externalMoveTimer;

    void Start() {
        cc = GetComponent<CharacterController>();
        if (head == null) head = FindDeep(transform, "CC_HEAD");

        // CC_HEAD sits about 2 m behind the hub's origin, so move the collision
        // capsule under the viewer's head instead of the hub's origin.
        if (head != null) {
            Vector3 local = transform.InverseTransformPoint(head.position);
            cc.center = new Vector3(local.x, cc.height / 2f + cc.skinWidth, local.z);
        }

        if (playerGlowStick != null) playerGlowStick.SetActive(false);
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update() {
        // If something else moved the hub (e.g. the waypoint script), resync physics
        // and pause gravity briefly so we don't fight it.
        if (transform.position != lastPosition || transform.rotation != lastRotation) {
            Physics.SyncTransforms();
            externalMoveTimer = 0.5f;
            verticalVelocity = 0f;
        }
        if (externalMoveTimer > 0f) externalMoveTimer -= Time.deltaTime;

        Vector2 move; float turn; bool glowHeld; bool cheerPressed; bool nextSongPressed;
        ReadInput(out move, out turn, out glowHeld, out cheerPressed, out nextSongPressed);

        HandleTurning(turn);
        HandleWalking(move);
        HandleGlowStick(glowHeld);
        if (cheerPressed) Cheer();
        if (nextSongPressed && ConcertManager.Instance != null) ConcertManager.Instance.NextSong();

        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void ReadInput(out Vector2 move, out float turn, out bool glowHeld, out bool cheerPressed, out bool nextSongPressed) {
        move = Vector2.zero; turn = 0f;
        glowHeld = false; cheerPressed = false; nextSongPressed = false;

        Gamepad gp = Gamepad.current;
        if (gp != null) {
            move = gp.leftStick.ReadValue();
            turn = gp.rightStick.ReadValue().x;
            glowHeld = gp.rightTrigger.ReadValue() > 0.5f;
            cheerPressed = gp.aButton.wasPressedThisFrame;
            nextSongPressed = gp.rightShoulder.wasPressedThisFrame;
        }

        Keyboard kb = Keyboard.current;
        if (kb != null) {
            if (kb.upArrowKey.isPressed) move.y += 1f;
            if (kb.downArrowKey.isPressed) move.y -= 1f;
            if (kb.jKey.isPressed) move.x -= 1f;
            if (kb.lKey.isPressed) move.x += 1f;
            if (kb.leftArrowKey.isPressed) turn -= 1f;
            if (kb.rightArrowKey.isPressed) turn += 1f;
            glowHeld |= kb.gKey.isPressed;
            cheerPressed |= kb.cKey.wasPressedThisFrame;
            nextSongPressed |= kb.mKey.wasPressedThisFrame;
        }

        if (move.magnitude < stickDeadzone) move = Vector2.zero;
        move = Vector2.ClampMagnitude(move, 1f);
        if (Mathf.Abs(turn) < stickDeadzone) turn = 0f;
        turn = Mathf.Clamp(turn, -1f, 1f);
    }

    void HandleTurning(float turn) {
        if (turn == 0f) return;
        // Turn around the viewer's head, not the hub origin, so the view pivots naturally
        transform.RotateAround(HeadPosition(), Vector3.up, turn * turnSpeed * Time.deltaTime);
        Physics.SyncTransforms();
    }

    void HandleWalking(Vector2 move) {
        Vector3 fwd = transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = transform.right; right.y = 0f; right.Normalize();
        Vector3 horizontal = (fwd * move.y + right * move.x) * walkSpeed;

        if (externalMoveTimer <= 0f) {
            if (cc.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
            verticalVelocity += gravity * Time.deltaTime;
        }

        if (horizontal.sqrMagnitude > 0f || externalMoveTimer <= 0f) {
            cc.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
        }
    }

    void HandleGlowStick(bool wantUp) {
        if (wantUp != lightsUp) {
            lightsUp = wantUp;
            if (playerGlowStick != null) playerGlowStick.SetActive(lightsUp);
            if (lightsUp) CrowdMember.RaisePropsNear(HeadPosition(), lightsRadius);
            else CrowdMember.LowerAllProps();
        }

        // While lights are up, keep lighting up new neighbors as you walk through the crowd
        if (lightsUp) {
            lightsRefreshTimer -= Time.deltaTime;
            if (lightsRefreshTimer <= 0f) {
                CrowdMember.RaisePropsNear(HeadPosition(), lightsRadius);
                lightsRefreshTimer = 0.75f;
            }
        }
    }

    void Cheer() {
        if (playerCheerSource != null && cheerClips != null && cheerClips.Length > 0) {
            playerCheerSource.PlayOneShot(cheerClips[Random.Range(0, cheerClips.Length)]);
        }
        CrowdMember.CheerNear(HeadPosition(), cheerRadius);
    }

    Vector3 HeadPosition() {
        return head != null ? head.position : transform.position;
    }

    static Transform FindDeep(Transform parent, string name) {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++) {
            Transform found = FindDeep(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
