using UnityEngine;

public class Mgr_Camera : MonoBehaviour
{

    [Header("감도")]
    public float SensitivityX = 1f;
    public float SensitivityY = 1f;

    [Header("수직 각도 제한")]
    public float MinPitch = -30f;
    public float MaxPitch =  60f;

    public CameraController CameraController { get; private set; }
    public static Mgr_Camera Inst { get; private set; }

    void Awake()
    {
        Inst = this;
    }

    public void RegisterCamera(CameraController cameraController)
    {
        CameraController = cameraController;
    }

    public void SetCursorLock(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !isLocked;
    }
}
