using UnityEngine;

// 좀비 애니메이션 이벤트 전달 — Animator가 있는 모델 오브젝트에 부착
// - 애니메이션 이벤트와 OnAnimatorMove는 Animator가 붙은 오브젝트로만 오므로, 부모의 Character_Zombie로 넘김
public class ZombieAnimationEvents : MonoBehaviour
{
    private Character_Zombie Zombie; // 부모의 좀비 엔티티
    private Animator         Anim;   // 이 모델의 Animator — 루트 모션 이동량 접근용

    private void Awake()
    {
        Zombie = GetComponentInParent<Character_Zombie>();
        Anim   = GetComponent<Animator>();
    }

    // Apply Root Motion 대신 Unity가 호출 — 모델이 아닌 부모(좀비)를 이동시킴
    private void OnAnimatorMove() => Zombie.ApplyRootMotion(Anim.deltaPosition, Anim.deltaRotation);


    #region 애니메이션 이벤트 — 클립에 입력된 함수 이름과 같아야 함
    public void OnBite()    => Zombie.OnBite();
    public void EndAttack() => Zombie.EndAttack();
    #endregion
}
