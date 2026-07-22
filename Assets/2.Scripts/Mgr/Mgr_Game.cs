using System.Collections;
using UnityEngine;

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

    

    private IEnumerator Start()
    {
        // 모든 Start() 완료 후 실행되도록 한 프레임 대기
        yield return null;

        Entity_Player player = Mgr_PlayerController.Inst.Player;
        Mgr_Camera.Inst.SetTarget(player.transform, player.CameraTargetOffset);
    }
}
