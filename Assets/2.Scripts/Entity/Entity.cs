using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("카메라 타겟 위치")]
    public Vector3 CameraTargetOffset = new Vector3(0f, 1.5f, 0f);
    public bool DebugCameraTarget = false;

    [Header("스탯")]
    public float HP;
    public float MaxHP;
    

    protected virtual void Awake()
    {
        HP = MaxHP;
    }

    public virtual void TakeDamage(float damage)
    {
        HP = Mathf.Max(HP - damage, 0f);

        if (HP <= 0f)
            OnDeath();
    }

    protected virtual void OnDeath()
    {

    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying || !DebugCameraTarget) return;
        UnityEditor.EditorApplication.delayCall += () =>
        {
            CameraController camCtrl = FindAnyObjectByType<CameraController>();
            if (camCtrl == null) return;
            UnityEditor.Undo.RecordObject(camCtrl.transform, "Debug Camera Target");
            camCtrl.transform.position = transform.position + CameraTargetOffset;
        };
    }
#endif
}
