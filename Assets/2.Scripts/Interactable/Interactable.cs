using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] public Vector3 InteractUIOffset = new Vector3(0f, 0f, 0f); // Interact UI가 표시될 오프셋
    public static List<Interactable> All = new List<Interactable>(); // 씬의 모든 Interactable 목록


    Outline OutlineComp;  // 외각선 컴포넌트
    public bool IsInView { get; private set; } // 현재 카메라 시야에 들어와 있는지 여부

    public Vector3 InteractUIPosition => transform.position + InteractUIOffset;

    private void Awake()
    {
        All.Add(this);

        // 외각선
        OutlineComp             = gameObject.AddComponent<Outline>();
        OutlineComp.OutlineMode  = Outline.Mode.OutlineVisible;
        OutlineComp.OutlineColor = Color.white;
        OutlineComp.OutlineWidth = 3f;
        OutlineComp.enabled      = false;
        gameObject.tag = "Interactable";
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
    public virtual void OnEnterView() { }

    // 카메라 시야에서 벗어났을 때 호출
    public virtual void OnExitView() { }

    // 레이가 이 오브젝트에 맞기 시작할 때 호출
    public virtual void OnFocus()
    {
        OutlineComp.enabled = true;
        Mgr_UI.Inst.ShowInteractUI(true, this);
    }

    // 레이가 이 오브젝트에서 벗어날 때 호출
    public virtual void OnLoseFocus()
    {
        OutlineComp.enabled = false;
        Mgr_UI.Inst.ShowInteractUI(false);
    }

    // 플레이어가 F키를 눌렀을 때 호출 — 서브클래스에서 override
    public virtual void Interact() { }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // InteractUI가 표시될 위치를 씬 뷰에서 시각화
        Vector3 uiPos = InteractUIPosition;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(uiPos, 0.06f);
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawDottedLine(transform.position, uiPos, 4f);
    }
#endif
}
