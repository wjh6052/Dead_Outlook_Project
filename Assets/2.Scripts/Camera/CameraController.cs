using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera Cam { get; private set; }

    private float _yaw;
    private float _pitch;

    void Start()
    {
        Cam = GetComponentInChildren<Camera>();
        Mgr_Camera.Inst.RegisterCamera(this);
    }

    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        Vector2 look = Mgr_Input.Inst.Look;

        _yaw   += look.x * Mgr_Camera.Inst.SensitivityX;
        _pitch -= look.y * Mgr_Camera.Inst.SensitivityY;
        _pitch  = Mathf.Clamp(_pitch, Mgr_Camera.Inst.MinPitch, Mgr_Camera.Inst.MaxPitch);

        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }
}
