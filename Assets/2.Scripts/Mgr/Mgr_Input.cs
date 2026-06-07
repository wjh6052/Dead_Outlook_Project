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


    void Awake()
    {
        Inst = this;
        Input = GetComponent<PlayerInput>();

        if (Input.actions == null) Input.actions = Resources.Load<InputActionAsset>("Input/InputSystem");

    }
    #region InputSystem
    void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        Look = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        Jump = value.isPressed;
    }

    void OnAim(InputValue value)
    {
        Aim = value.isPressed;
    }

    void OnRun(InputValue value)
    {
        Run = value.isPressed;
    }

    void OnCrouch(InputValue value)
    {
        Crouch = value.isPressed;
    }

    void OnInventory(InputValue value)
    {
        Mgr_PlayerController.Inst.OnInventory();
    }

    void OnInteractable(InputValue value)
    {
        Mgr_PlayerController.Inst.OnInteractable();
    }

    void OnRotateItem(InputValue value)
    {
        Mgr_PlayerController.Inst.OnRotateItem();
    }
    #endregion
}
