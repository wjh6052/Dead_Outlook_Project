using UnityEngine;

// 플레이어 애니메이션 이벤트 전달 — Animator가 있는 모델 오브젝트에 부착
// - 애니메이션 이벤트와 OnAnimatorMove는 Animator가 붙은 오브젝트로만 오므로, 부모의 Character_Player · PlayerMovement로 넘김
public class PlayerAnimationEvents : MonoBehaviour
{
    private Character_Player Player;   // 부모의 플레이어 엔티티
    private PlayerMovement   Movement; // 부모의 이동 처리

    private void Awake()
    {
        Player   = GetComponentInParent<Character_Player>();
        Movement = GetComponentInParent<PlayerMovement>();
    }

    // Apply Root Motion 대신 Unity가 호출 — 루트 모션 처리를 PlayerMovement로 넘김
    private void OnAnimatorMove() => Movement.UpdateRootMotion();


    #region 애니메이션 이벤트 — 클립에 입력된 함수 이름과 같아야 함
    public void OnAttackCombo()           => Player.OnAttackCombo();
    public void OnCollider(string value)  => Player.OnCollider(value);
    public void TakeWeapon(string attach) => Player.TakeWeapon(attach);
    public void OnHolsterEnd()            => Player.OnHolsterEnd();
    public void OnGrabEnd()               => Player.OnGrabEnd();
    public void OnPush()                  => Player.OnPush();
    #endregion
}
