using UnityEngine;

public class Mgr_Camera : MonoBehaviour
{
    [Header("감도")]
    public float SensitivityX = 1f;
    public float SensitivityY = 1f;

    

    public CameraController CameraController { get; private set; }
    public static Mgr_Camera Inst { get; private set; }

    public Transform Target       { get; private set; }
    public Vector3   TargetOffset { get; private set; }

    private void Awake()
    {
        Inst = this;
        SetCursorLock(true);
    }

    public void RegisterCamera(CameraController cameraController)
    {
        CameraController = cameraController;
    }

    public void SetTarget(Transform target, Vector3 targetOffset)
    {
        Target       = target;
        TargetOffset = targetOffset;
    }

    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;
    }
}
