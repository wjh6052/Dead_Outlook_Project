using UnityEngine;

public enum ControlType
{
    Player,
    Vehicle,
    Observer
}

public class Mgr_PlayerController : MonoBehaviour
{
    

    public static Mgr_PlayerController Inst { get; private set; }

    private ControlType CurrentControlType = ControlType.Player;
    public Entity_Player Player { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    public void SetPlayer(Entity_Player player)
    {
        Player = player;
    }

    private void Update()
    {
        switch (CurrentControlType)
        {
            case ControlType.Player:
                Player.Movement.SetMoveInput(Mgr_Input.Inst.Move);
                break;

            case ControlType.Vehicle:
                // 차량 입력 처리 (추후 구현)
                break;

            case ControlType.Observer:
                // 관전 카메라 입력 처리 (추후 구현)
                break;
        }
    }
}
