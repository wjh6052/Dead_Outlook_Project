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

    private void Awake()
    {
        Inst = this;
        SetCursorLock(true);
    }

    public void RegisterCamera(CameraController cameraController)
    {
        CameraController = cameraController;
    }

    public void SetTarget(Transform target)
    {
        Target       = target;
    }

    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;

        IsCursorLocked = isLocked;
    }
}
