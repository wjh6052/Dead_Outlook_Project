using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static List<Interactable> All = new List<Interactable>(); // 씬의 모든 Interactable 목록

    GameObject Interact_Dot;  // 근접 시 표시되는 점 이미지
    GameObject Interact_Icon; // 조준 시 표시되는 상호작용 이미지

    Outline OutlineComp;  // 외각선 컴포넌트
    public bool IsInView { get; private set; } // 현재 카메라 시야에 들어와 있는지 여부

    private void Awake()
    {
        All.Add(this);

        // InteractUI 하위에서 이름으로 자동 탐색
        Interact_Dot  = transform.Find("InteractUI/Interact_Dot")?.gameObject;
        Interact_Icon = transform.Find("InteractUI/Interact_Icon")?.gameObject;

        if (Interact_Dot)  Interact_Dot.SetActive(false);
        if (Interact_Icon) Interact_Icon.SetActive(false);

        // 외각선
        OutlineComp             = gameObject.AddComponent<Outline>();
        OutlineComp.OutlineMode  = Outline.Mode.OutlineVisible;
        OutlineComp.OutlineColor = Color.white;
        OutlineComp.OutlineWidth = 3f;
        OutlineComp.enabled      = false;
    }

    private void OnDestroy()
    {
        All.Remove(this);
    }

    // 카메라 시야 상태 갱신 — Mgr_PlayerController에서 호출
    public void UpdateViewState(Camera cam, Plane[] frustumPlanes)
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        // 카메라 프러스텀 안에 있는지 확인
        bool inFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, col.bounds);

        // 벽에 막히지 않는지 확인 (플레이어 레이어 제외)
        bool hasLOS = false;
        if (inFrustum)
        {
            int     playerLayer = Mgr_PlayerController.Inst.Player.gameObject.layer;
            int     layerMask   = ~(1 << playerLayer);
            Vector3 dir         = cam.transform.position - transform.position;
            hasLOS = !Physics.Raycast(transform.position, dir.normalized, dir.magnitude, layerMask);
        }

        bool visible = inFrustum && hasLOS;
        if (visible == IsInView) return;

        IsInView = visible;
        if (IsInView) OnEnterView();
        else          OnExitView();
    }

    // 카메라 시야에 들어왔을 때 호출
    public virtual void OnEnterView()
    {
        Interact_Dot.gameObject.SetActive(true);
    }

    // 카메라 시야에서 벗어났을 때 호출
    public virtual void OnExitView()
    {
        Interact_Dot.gameObject.SetActive(false);
    }

    // 레이가 이 오브젝트에 맞기 시작할 때 호출
    public virtual void OnFocus()
    {
        OutlineComp.enabled = true;
        if (Interact_Dot) Interact_Dot.SetActive(true);

        Interact_Dot.gameObject.SetActive(false);
        Interact_Icon.gameObject.SetActive(true);
    }

    // 레이가 이 오브젝트에서 벗어날 때 호출
    public virtual void OnLoseFocus()
    {
        OutlineComp.enabled = false;
        if (Interact_Dot) Interact_Dot.SetActive(false);

        Interact_Dot.gameObject.SetActive(true);
        Interact_Icon.gameObject.SetActive(false);
    }

    // 플레이어가 F키를 눌렀을 때 호출 — 서브클래스에서 override
    public virtual void Interact() { }
}
