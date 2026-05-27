using UnityEngine;
using UnityEngine.InputSystem;

public class Mgr_Input : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public static Mgr_Input Inst { get; private set; }

    private PlayerInput Input;

    void Awake()
    {
        Inst = this;
        Input = GetComponent<PlayerInput>();

        if (Input.actions == null) Input.actions = Resources.Load<InputActionAsset>("Input/InputSystem");

    }

    void OnMove(InputValue value)
    {
        Move = value.Get<Vector2>();
        //Debug.Log($"Move: {Move}");
    }

    void OnLook(InputValue value)
    {
        Look = value.Get<Vector2>();
        //Debug.Log($"Look: {Look}");
    }
}
