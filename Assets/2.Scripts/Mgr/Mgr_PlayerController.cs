using UnityEngine;


public class Mgr_PlayerController : MonoBehaviour
{

    [SerializeField] private float InteractRange = 5f; // 상호작용 최대 거리
    private Interactable FocusedInteractable;          // 현재 레이에 감지된 상호작용 오브젝트
    
    
    public Entity_Player Player { get; private set; }
    public void SetPlayer(Entity_Player player) { Player = player; }
    public static Mgr_PlayerController Inst { get; private set; }

    private void Awake()
    {
        Inst = this;
    }

    

    // 인벤토리 창 열기/닫기 Mgr_Input에서 Tab키를 눌렀을 경우 호출
    public void OnInventory()
    {
        switch (Mgr_Game.Inst.GameState)
        {
            case EGameState.Play:
                {
                    Mgr_UI.Inst.OnInventory(true);
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
        if (FocusedInteractable != null)
        {
            Debug.Log(FocusedInteractable.gameObject.name);

            FocusedInteractable.Interact();
        }
    }


    // 아이템 회전 / Mgr_Input에서 R키를 통해 
    public void OnRotateItem()
    {
        if(Mgr_UI.Inst.IsOnUI)
        {
            Mgr_UI.Inst.OnRotateItem();
        }
    }


    private void Update()
    {
        if (Mgr_Game.Inst.GameState == EGameState.Play)
        {
            // 카메라 움직임
            Mgr_Camera.Inst.CameraController.Rotate(Mgr_Input.Inst.Look);

            Player.Movement.MoveInput    = Mgr_Input.Inst.Move;
            Player.Movement.bJump        = Mgr_Input.Inst.Jump;
            Player.Movement.bAim         = Mgr_Input.Inst.Aim;
            Player.Movement.SpeedType    = Mgr_Input.Inst.Run ? ESpeedType.Run : ESpeedType.Walk;
            Player.Movement.bCrouchInput = Mgr_Input.Inst.Crouch;

            UpdateInteractables();
        }
    }

    // 시야 상태 갱신 + 레이캐스트 포커스 탐색
    private void UpdateInteractables()
    {
        Camera  cam           = Mgr_Camera.Inst.CameraController.Cam;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (Interactable interactable in Interactable.All)
            interactable.UpdateViewState(cam, frustumPlanes);

        Ray          ray     = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Interactable hit     = null;
        float        minDist = float.MaxValue;

        foreach (RaycastHit hitInfo in Physics.RaycastAll(ray, InteractRange))
        {
            if (!hitInfo.collider.CompareTag("Interactable")) continue;
            if (hitInfo.distance >= minDist) continue;

            minDist = hitInfo.distance;
            hit     = hitInfo.collider.GetComponent<Interactable>();
        }

        if (hit != FocusedInteractable)
        {
            if (FocusedInteractable != null) FocusedInteractable.OnLoseFocus();
            FocusedInteractable = hit;
            if (FocusedInteractable != null) FocusedInteractable.OnFocus();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (Mgr_Camera.Inst?.CameraController?.Cam == null) return;

        Camera cam = Mgr_Camera.Inst.CameraController.Cam;
        Ray    ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 감지됐으면 초록, 아니면 빨강
        Gizmos.color = FocusedInteractable != null ? Color.green : Color.red;
        Gizmos.DrawRay(ray.origin, ray.direction * InteractRange);
    }
#endif
}
