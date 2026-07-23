using System.Collections;
using UnityEngine;

public class Entity_Player : Entity
{
    public PlayerMovement     Movement     { get; private set; }
    public InventoryComponent InventoryComp { get; private set; }
    public static Entity_Player Inst { get; private set; }

    [Header("카메라")]
    float CameraStandHeight;  // 기립 시 카메라 높이 — Awake에서 CameraTarget 초기 위치로 설정
    public float CameraCrouchHeight = 0.9f;                // 웅크림 시 카메라 높이 (m)
    public float CameraHeightSpeed  = 10f;                 // 카메라 높이 전환 속도

    protected override void Awake()
    {
        base.Awake();
        Inst              = this;
        Movement          = GetComponent<PlayerMovement>();
        InventoryComp     = GetComponent<InventoryComponent>();
        CameraStandHeight = CameraTarget.localPosition.y;
    }

    private void Start()
    {
        Mgr_PlayerController.Inst.SetPlayer(this);
    }

    // PlayerMovement에서 웅크림 상태 전달 — 카메라 높이 코루틴 시작
    public void SetCrouching(bool isCrouching)
    {
        float target = isCrouching ? CameraCrouchHeight : CameraStandHeight;
        StopAllCoroutines();
        StartCoroutine(LerpCameraHeight(target));
    }

    private IEnumerator LerpCameraHeight(float targetHeight)
    {
        while (Mathf.Abs(CameraTarget.localPosition.y - targetHeight) > 0.001f)
        {
            Vector3 pos = CameraTarget.localPosition;
            pos.y                      = Mathf.Lerp(pos.y, targetHeight, CameraHeightSpeed * Time.deltaTime);
            CameraTarget.localPosition = pos;
            yield return null;
        }

        Vector3 final = CameraTarget.localPosition;
        final.y                    = targetHeight;
        CameraTarget.localPosition = final;
    }
}
