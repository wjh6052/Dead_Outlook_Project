using UnityEngine;

[SelectionBase]
public class CameraController : MonoBehaviour
{
    [Header("카메라 위치")]
    [SerializeField] private Vector3 CameraOffset = new Vector3(0f, 0f, -5f);

    [Header("수직 각도 제한")]
    public float MinPitch = -30f;
    public float MaxPitch = 60f;

    public Camera Cam { get; private set; }

    private float Yaw;
    private float Pitch;

    private void Start()
    {
        Cam = GetComponentInChildren<Camera>();
        Cam.transform.localPosition = CameraOffset;
        Mgr_Camera.Inst.RegisterCamera(this);
    }

    private void OnValidate()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;
        cam.transform.localPosition = CameraOffset;
    }

    private void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (Mgr_Camera.Inst.Target == null) return;

        transform.position = Mgr_Camera.Inst.Target.position + Mgr_Camera.Inst.TargetOffset;
    }

    public void Rotate(Vector2 look)
    {
        Yaw   += look.x * Mgr_Camera.Inst.SensitivityX;
        Pitch -= look.y * Mgr_Camera.Inst.SensitivityY;
        Pitch  = Mathf.Clamp(Pitch, MinPitch, MaxPitch);

        transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam == null) return;

        Vector3 pivotPos  = transform.position;
        Vector3 localCamPos = cam.transform.localPosition;
        float   radius    = localCamPos.magnitude;
        float   yaw       = transform.eulerAngles.y;

        // MinPitch / MaxPitch 각도에서의 카메라 실제 위치
        Vector3 minCamPos = pivotPos + Quaternion.Euler(MinPitch, yaw, 0f) * localCamPos;
        Vector3 maxCamPos = pivotPos + Quaternion.Euler(MaxPitch, yaw, 0f) * localCamPos;

        Vector3 fromDir   = (minCamPos - pivotPos).normalized;
        Vector3 arcNormal = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;

        // 카메라 이동 범위 3D 구
        Gizmos.color = new Color(1f, 1f, 1f, 0.1f);
        Gizmos.DrawWireSphere(pivotPos, radius);

        // 카메라가 이동하는 범위 호(arc)
        UnityEditor.Handles.color = new Color(1f, 1f, 1f, 0.6f);
        UnityEditor.Handles.DrawWireArc(pivotPos, arcNormal, fromDir, MaxPitch - MinPitch, radius);

        // 상단 한계 수평 원 (MinPitch 위도선)
        Vector3 minCircleCenter = new Vector3(pivotPos.x, minCamPos.y, pivotPos.z);
        float   minCircleRadius = Vector3.ProjectOnPlane(minCamPos - pivotPos, Vector3.up).magnitude;
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(minCircleCenter, Vector3.up, minCircleRadius);

        // 하단 한계 수평 원 (MaxPitch 위도선)
        Vector3 maxCircleCenter = new Vector3(pivotPos.x, maxCamPos.y, pivotPos.z);
        float   maxCircleRadius = Vector3.ProjectOnPlane(maxCamPos - pivotPos, Vector3.up).magnitude;
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireDisc(maxCircleCenter, Vector3.up, maxCircleRadius);
    }
#endif
}
