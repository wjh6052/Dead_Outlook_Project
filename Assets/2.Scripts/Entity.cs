using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("카메라가 붙을 위치")]
    public Vector3 CameraTargetOffset = new Vector3(0f, 1.5f, 0f);

    [Header("스텟")]
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
}
