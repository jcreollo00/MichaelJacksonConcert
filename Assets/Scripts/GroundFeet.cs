using UnityEngine;

// Keeps a dancing Humanoid's feet on top of the floor. Some Mixamo dances
// sit a little low after retargeting, which makes the ankles sink into the stage.
// Every frame this lifts the character just enough that the lowest foot is on
// the floor. It never pushes down, so jumps and kicks still leave the ground.
[RequireComponent(typeof(Animator))]
public class GroundFeet : MonoBehaviour {
    public float floorY = float.NaN;       // leave empty to use the character's starting height
    public float soleClearance = 0.025f;   // toe joint sits this far above the sole
    public float smoothing = 12f;

    private Animator anim;
    private Transform[] feet;
    private Vector3 basePosition;
    private float lift;

    void Start() {
        anim = GetComponent<Animator>();
        basePosition = transform.position;
        if (float.IsNaN(floorY)) floorY = basePosition.y;
        feet = new Transform[] {
            anim.GetBoneTransform(HumanBodyBones.LeftToes),
            anim.GetBoneTransform(HumanBodyBones.RightToes),
            anim.GetBoneTransform(HumanBodyBones.LeftFoot),
            anim.GetBoneTransform(HumanBodyBones.RightFoot)
        };
    }

    void LateUpdate() {
        float lowest = float.MaxValue;
        for (int i = 0; i < feet.Length; i++) {
            if (feet[i] != null) lowest = Mathf.Min(lowest, feet[i].position.y);
        }
        if (lowest == float.MaxValue) return;

        float rawLowest = lowest - lift;                       // where the foot would be with no lift
        float needed = Mathf.Max(0f, floorY + soleClearance - rawLowest);
        lift = Mathf.Lerp(lift, needed, 1f - Mathf.Exp(-smoothing * Time.deltaTime));

        Vector3 p = transform.position;
        transform.position = new Vector3(p.x, basePosition.y + lift, p.z);
    }
}
