using UnityEngine;

// Fills a rectangular floor area with random audience members facing the stage.
// Put this on an empty GameObject placed at the center of the audience floor.
// The yellow box in the Scene view shows the area.

public class CrowdSpawner : MonoBehaviour {
    public GameObject[] crowdPrefabs;
    public Transform stageFocus;                  // point on stage people look toward (e.g. MJ)
    public Vector2 areaSize = new Vector2(20f, 14f);
    public float spacing = 1.1f;
    public float positionJitter = 0.35f;
    [Range(0f, 1f)] public float density = 0.75f;
    public float maxYawOffset = 20f;              // people don't all face exactly forward
    public Vector2 scaleRange = new Vector2(0.92f, 1.08f);
    public int randomSeed = 0;                    // 0 = different crowd every run

    [Header("Keep these spots empty (entry aisle, player start, vantage points)")]
    public Transform[] keepClear;
    public float keepClearRadius = 1.8f;

    void Start() {
        Spawn();
    }

    public void Spawn() {
        if (crowdPrefabs == null || crowdPrefabs.Length == 0) {
            Debug.LogWarning("CrowdSpawner: no crowd prefabs assigned.");
            return;
        }
        if (randomSeed != 0) Random.InitState(randomSeed);

        int count = 0;
        for (float x = -areaSize.x / 2f; x <= areaSize.x / 2f; x += spacing) {
            for (float z = -areaSize.y / 2f; z <= areaSize.y / 2f; z += spacing) {
                if (Random.value > density) continue;

                Vector3 local = new Vector3(
                    x + Random.Range(-positionJitter, positionJitter), 0f,
                    z + Random.Range(-positionJitter, positionJitter));
                Vector3 pos = transform.TransformPoint(local);
                if (IsBlocked(pos)) continue;

                Quaternion rot = transform.rotation;
                if (stageFocus != null) {
                    Vector3 dir = stageFocus.position - pos;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.01f) rot = Quaternion.LookRotation(dir);
                }
                rot *= Quaternion.Euler(0f, Random.Range(-maxYawOffset, maxYawOffset), 0f);

                GameObject prefab = crowdPrefabs[Random.Range(0, crowdPrefabs.Length)];
                GameObject person = (GameObject)Instantiate(prefab, pos, rot, transform);
                person.transform.localScale *= Random.Range(scaleRange.x, scaleRange.y);
                if (person.GetComponent<CrowdMember>() == null) person.AddComponent<CrowdMember>();
                count++;
            }
        }
        Debug.Log("CrowdSpawner: spawned " + count + " audience members.");
    }

    bool IsBlocked(Vector3 pos) {
        if (keepClear == null) return false;
        for (int i = 0; i < keepClear.Length; i++) {
            if (keepClear[i] == null) continue;
            Vector3 a = pos; a.y = 0f;
            Vector3 b = keepClear[i].position; b.y = 0f;
            if (Vector3.Distance(a, b) < keepClearRadius) return true;
        }
        return false;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(new Vector3(0f, 1f, 0f), new Vector3(areaSize.x, 2f, areaSize.y));
    }
}
