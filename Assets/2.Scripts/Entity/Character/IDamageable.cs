public interface IDamageable
{
    float HP    { get; }
    float MaxHP { get; }
    void  TakeDamage(float damage);
}
