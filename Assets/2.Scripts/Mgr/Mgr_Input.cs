using UnityEngine;
using UnityEngine.InputSystem;

public class Mgr_Input : MonoBehaviour
{
    public Vector2 Move;
    public Vector2 Look;
    public bool    Jump;
    public bool    Aim;
    public bool    Run;
    public bool    Crouch;

    private PlayerInput Input;
    public static Mgr_Input Inst { get; private set; }


    // PlayerInput과 입력 액션을 확인한 뒤, 둘 다 있을 때만 싱글톤 등록 (Mgr_Game.C_GameStart가 Inst로 준비 완료 확인)
    void Awake()
    {
        Input = GetComponent<PlayerInput>();

        if (Input != null && Input.actions == null) Input.actions = Resources.Load<InputActionAsset>("Input/InputSystem");

        if (Input != null && Input.actions != null) Inst = this;
        else Debug.LogWarning("[Mgr_Input] PlayerInput 또는 입력 액션이 없어 Inst를 등록하지 않음 — 게임 시작이 대기 상태로 멈춤");
    }
    #region InputSystem
    void OnMove(InputValue value)        => Move   = value.Get<Vector2>();
    void OnLook(InputValue value)        => Look   = value.Get<Vector2>();
    void OnJump(InputValue value)        => Jump   = value.isPressed;
    void OnAim(InputValue value)         => Aim    = value.isPressed;
    void OnRun(InputValue value)         => Run    = value.isPressed;
    void OnCrouch(InputValue value)      => Crouch = value.isPressed;

    void OnInventory(InputValue value)   => Mgr_PlayerController.Inst.OnInventory();
    void OnInteractable(InputValue value)=> Mgr_PlayerController.Inst.OnInteractable();
    void OnRotateItem(InputValue value)  => Mgr_PlayerController.Inst.OnRotateItem();

    void OnWeaponSlot1(InputValue value) => Mgr_PlayerController.Inst.OnWeaponSlot(1);
    void OnWeaponSlot2(InputValue value) => Mgr_PlayerController.Inst.OnWeaponSlot(2);
    void OnWeaponSlot3(InputValue value) => Mgr_PlayerController.Inst.OnWeaponSlot(3);

    void OnAttack(InputValue value)      => Mgr_PlayerController.Inst.OnAttack();
    #endregion
}
