using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public Transform CameraTarget { get; private set; } // 카메라가 따라다닐 트랜스폼 (플레이어 자식 오브젝트)

    [Header("스탯")]
    public float HP;
    public float MaxHP;
    

    protected virtual void Awake()
    {
        HP = MaxHP;

        CameraTarget = transform.Find("CameraTarget");
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

}
