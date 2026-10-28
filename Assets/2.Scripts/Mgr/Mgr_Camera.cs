using UnityEngine;

public class Mgr_Camera : MonoBehaviour
{
    [Header("감도")]
    public float SensitivityX = 1f;
    public float SensitivityY = 1f;

    [Header("커서 잠금")]
    [field: SerializeField] public bool IsCursorLocked { get; private set; }

    public CameraController CameraController { get; private set; }

    public Transform Target       { get; private set; }


    public static Mgr_Camera Inst { get; private set; }

    // 싱글톤 등록 + 커서 잠금
    private void Awake()
    {
        Inst = this;
        SetCursorLock(true);
    }

    // 카메라 등록 — CameraController.Start에서 호출
    public void RegisterCamera(CameraController cameraController)
    {
        CameraController = cameraController;
    }

    // 카메라가 따라갈 대상 설정
    public void SetTarget(Transform target)
    {
        Target       = target;
    }

    // 커서 잠금 (true: 숨김 / false: 표시)
    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;

        IsCursorLocked = isLocked;
    }
}
