using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EGameState
{
    Play,  // 캐릭터를 움직이는 상태
    UI,    // 인벤토리, 메뉴 등 UI가 열린 상태
    Stop   // 사망 등 게임을 멈춰야 할 때
}

public class Mgr_Game : MonoBehaviour
{

    [field: SerializeField] public EGameState GameState { get; private set; } = EGameState.Play;


    public static Mgr_Game Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    private void Update()
    {
        // 테스트
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                Character_Player.Inst.IsFocusing = false;
            }

            // 플레이어에게 데미지
            if (Keyboard.current.yKey.wasPressedThisFrame)
            {
                Character_Player.Inst?.TakeDamage(10f, this.gameObject, this.gameObject);
            }
        }

    }


    public void SetGameState(EGameState newGameState)
    {
        GameState = newGameState;

        switch (GameState)
        {
            case EGameState.Play:
                {
                    Mgr_Camera.Inst.SetCursorLock(true);
                }
                break;
            case EGameState.UI:
                {
                    Mgr_Camera.Inst.SetCursorLock(false);
                }
                break;
            case EGameState.Stop:
                {

                }
                break;
        }
    }

    
}
