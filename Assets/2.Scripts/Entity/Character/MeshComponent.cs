using UnityEngine;
using UnityEngine.Animations.Rigging;

public class MeshComponent : MonoBehaviour
{
    [HideInInspector] public Transform HeadBone; // 머리 본 (Awake에서 자동 할당)

    [Header("무기 소켓")]
    public Transform WeaponSocket_1; // 무기 슬롯 1 홀스터 위치
    public Transform WeaponSocket_2; // 무기 슬롯 2 홀스터 위치

    [Header("손 소켓")]
    public Transform HandSocket_R; // 오른손 무기 장착 위치

    [Header("IK")]
    public TwoBoneIKConstraint IK_Hand_L;       // 왼손 IK 컨스트레인트
    public Transform           StartTarget_Hand_L; // 왼손 IK 타겟
    public Transform           Target_Hand_L;    // 왼손 IK 타겟

    private void Awake()
    {
        HeadBone = GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
    }
}
