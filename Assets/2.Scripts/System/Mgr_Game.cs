using System.Collections;
using UnityEngine;

public class Mgr_Game : MonoBehaviour
{
    public static Mgr_Game Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    private IEnumerator Start()
    {
        // 모든 Start() 완료 후 실행되도록 한 프레임 대기
        yield return null;

        Entity_Player player = Mgr_PlayerController.Inst.Player;
        Mgr_Camera.Inst.SetTarget(player.transform, player.CameraTargetOffset);
    }
}
