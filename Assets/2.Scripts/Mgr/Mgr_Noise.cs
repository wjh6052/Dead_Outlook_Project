using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.AI;

public class Mgr_Noise : MonoBehaviour
{
    private NoiseWave WavePrefab;   // 파동 프리팹

    public static Mgr_Noise Inst;

    private void Awake()
    {
        Inst = this;

        WavePrefab = Resources.Load<NoiseWave>("Prefab/PF_NoiseWave");
        if (WavePrefab == null) Debug.LogError("[Mgr_Noise] 파동 프리팹을 찾지 못함 — Resources/Prefab/PF_NoiseWave");
    }

    // ============================
    // 소음
    // ============================

    // source    : 소음을 발생시킨 오브젝트
    // intensity : 소음 강도 (0 ~ 1)
    // radius    : 소음이 전달되는 거리 (m)
    public void MakeNoise(GameObject source, float intensity, float radius)
    {
        // 파동 생성
        NoiseWave wave = Instantiate(WavePrefab, source.transform.position, Quaternion.identity);
        wave.Init(radius);

        // 반경 내 콜라이더 검색
        Collider[] colliders = Physics.OverlapSphere(source.transform.position, radius);

        NavMeshPath path = new NavMeshPath();

        foreach (Collider col in colliders)
        {
            // 좀비 태그가 아니면 즉시 건너뜀
            if (!col.CompareTag("Zombie")) continue;

            // AIZombieController 없으면 즉시 건너뜀 (NavMesh 계산 생략)
            AIZombieController zombie = col.GetComponent<AIZombieController>();
            if (zombie == null) continue;

            // NavMesh 경로 계산 (벽·장애물 우회 경로)
            bool found = NavMesh.CalculatePath(source.transform.position, col.transform.position, NavMesh.AllAreas, path);

            // 경로가 없거나 완전히 도달 불가능하면 무시
            if (!found || path.status != NavMeshPathStatus.PathComplete) continue;

            // 실제 경로 거리가 radius를 초과하면 무시
            if (GetPathLength(path) > radius) continue;

            // 소음 전달
            zombie.OnNoiseDetected(source.transform.position, intensity);
        }
    }

    // NavMesh 경로의 총 거리 계산
    private float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        return length;
    }
}
