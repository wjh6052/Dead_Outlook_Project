using System.Collections;
using UnityEngine;


public class Mgr_PlayerController : MonoBehaviour
{

    [SerializeField] private float InteractRange = 5f; // 상호작용 최대 거리
    private Interactable FocusedInteractable;          // 현재 레이에 감지된 상호작용 오브젝트
    
    

    public static Mgr_PlayerController Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    // 모든 Start() 완료 후 실행되도록 한 프레임 대기
    private IEnumerator Start()
    {
        yield return null;
        // 카메라를 플레이어에게 설정
        Mgr_Camera.Inst.SetTarget(Character_Player.Inst.CameraTarget);
    }

    private void Update()
    {
        if (Mgr_Game.Inst.GameState == EGameState.Play)
        {
            // 카메라 움직임
            Mgr_Camera.Inst.CameraController.Rotate(Mgr_Input.Inst.Look);

            Character_Player.Inst.Movement.MoveInput    = Mgr_Input.Inst.Move;
            Character_Player.Inst.Movement.bJump        = Mgr_Input.Inst.Jump;
            Character_Player.Inst.Movement.bAim         = Mgr_Input.Inst.Aim;
            Character_Player.Inst.Movement.bRunInput    = Mgr_Input.Inst.Run;
            Character_Player.Inst.Movement.bCrouchInput = Mgr_Input.Inst.Crouch;

            UpdateInteractables();
        }
        else
        {
            // Play가 아닐 때(UI·Stop) 마지막 입력이 남아 계속 움직이지 않도록 입력 없음으로 전달
            Character_Player.Inst.Movement.MoveInput = Vector2.zero;
            Character_Player.Inst.Movement.bAim      = false;
            Character_Player.Inst.Movement.bRunInput = false;
        }
    }

    // 시야 상태 갱신 + 레이캐스트 포커스 탐색
    private void UpdateInteractables()
    {
        Camera cam = Mgr_Camera.Inst.CameraController.MainCamera;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (Interactable interactable in Interactable.All)
            interactable.UpdateViewState(cam, frustumPlanes);

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Interactable hit = null;
        float minDist = float.MaxValue;

        foreach (RaycastHit hitInfo in Physics.RaycastAll(ray, InteractRange))
        {
            Interactable interactable = hitInfo.collider.GetComponentInParent<Interactable>();
            if (interactable == null || interactable.IsDisabled) continue;
            if (hitInfo.distance >= minDist) continue;

            minDist = hitInfo.distance;
            hit = interactable;
        }

        if (hit != FocusedInteractable)
        {
            if (FocusedInteractable != null) FocusedInteractable.OnLoseFocus();
            FocusedInteractable = hit;
            if (FocusedInteractable != null) FocusedInteractable.OnFocus();
        }
    }


    // 인벤토리 창 열기/닫기 Mgr_Input에서 Tab키를 눌렀을 경우 호출
    public void OnInventory()
    {
        switch (Mgr_Game.Inst.GameState)
        {
            case EGameState.Play:
                {
                    Mgr_UI.Inst.OnInventory(true);
                    // 은신처에 있으면 보관함 자동 표시
                    //if (Mgr_Map.Inst.CurrentMap == EScene.Hideout)
                    //    Mgr_UI.Inst.UI_Inventory_Player.ShowBox(Mgr_Hideout.Inst.StorageData);
                    Mgr_Game.Inst.SetGameState(EGameState.UI);
                }
                break;
            case EGameState.UI:
                {
                    Mgr_UI.Inst.OnInventory(false);
                    Mgr_Game.Inst.SetGameState(EGameState.Play);
                }
                break;
            case EGameState.Stop:
                {

                }
                break;
        }

        
    }

    // 상호작용 / Mgr_Input에서 F를 눌렀을 경우 호출
    public void OnInteractable()
    {
        if (Mgr_Game.Inst.GameState != EGameState.Play) return; // UI · 잡힘 등 조작 불가 상태면 무시

        if (FocusedInteractable != null)
        {
            //Debug.Log(FocusedInteractable.gameObject.name);

            FocusedInteractable.Interact();
        }
    }


    // 무기 슬롯 선택 / Mgr_Input에서 1·2·3키를 눌렀을 경우 호출
    public void OnWeaponSlot(int slot)
    {
        if (Mgr_Game.Inst.GameState != EGameState.Play) return;
        Character_Player.Inst.OnWeaponSlot(slot);
    }

    // 공격 / Mgr_Input에서 마우스 좌클릭 시 호출
    public void OnAttack()
    {
        if (Mgr_Game.Inst.GameState != EGameState.Play) return;
        Character_Player.Inst.Attack();
    }

    // 아이템 회전 / Mgr_Input에서 R키를 통해
    public void OnRotateItem()
    {
        if(Mgr_UI.Inst.IsOnUI)
        {
            Mgr_UI.Inst.OnRotateItem();
        }
    }





#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (Mgr_Camera.Inst?.CameraController?.MainCamera == null) return;

        Camera cam = Mgr_Camera.Inst.CameraController.MainCamera;
        Ray    ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 감지됐으면 초록, 아니면 빨강
        Gizmos.color = FocusedInteractable != null ? Color.green : Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * InteractRange);
    }
#endif
}
