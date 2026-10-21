using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MeshComponent : MonoBehaviour
{
    [HideInInspector] public Transform HeadBone; // 머리 본 (Awake에서 자동 할당)

    [Header("무기 소켓")]
    [Tooltip("무기 슬롯 1을 등에 멜 위치")]
    public Transform WeaponSocket_1; // 무기 슬롯 1 홀스터 위치
    [Tooltip("무기 슬롯 2를 등에 멜 위치")]
    public Transform WeaponSocket_2; // 무기 슬롯 2 홀스터 위치

    [Header("손 소켓")]
    [Tooltip("손에 든 무기가 붙는 오른손 위치")]
    public Transform HandSocket_R; // 오른손 무기 장착 위치

    [Header("가방 소켓")]
    [Tooltip("가방을 멜 등 위치 — 가방 프리팹 원점이 여기에 맞춰짐")]
    public Transform BagSocket; // 가방 장착 위치 (척추 뼈 아래)

    [Header("IK")]
    [Tooltip("왼손 IK 컨스트레인트")]
    public TwoBoneIKConstraint IK_Hand_L;       // 왼손 IK 컨스트레인트
    [Tooltip("맨손일 때 왼손 IK 타겟이 돌아갈 기준 위치")]
    public Transform           StartTarget_Hand_L; // 왼손 IK 타겟
    [Tooltip("왼손이 실제로 따라가는 IK 타겟 — 무기를 들면 무기의 GripPoint_L로 이동")]
    public Transform           Target_Hand_L;    // 왼손 IK 타겟

    private void Awake()
    {
        HeadBone = GetComponentInChildren<Animator>().GetBoneTransform(HumanBodyBones.Head); // Animator가 자신 또는 자식 모델에 있음
    }
}
