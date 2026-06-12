using UnityEngine;

public class Entity_Player : Entity
{
    [Header("스탯")]
    [Tooltip("걷기 이동 속도 (m/s)")]
    [SerializeField] private float WalkSpeed = 5f;
    [Tooltip("달리기 이동 속도 (m/s)")]
    [SerializeField] private float RunSpeed = 8f;
    [Tooltip("점프 시 적용되는 수직 힘")]
    [SerializeField] private float JumpForce = 3.5f;
    [Tooltip("이동 방향으로 회전하는 속도 (높을수록 즉각 회전)")]
    [SerializeField] private float RotationSpeed = 10f;
    [Tooltip("출발·방향전환 가속도 (m/s²) - 높을수록 빠르게 목표 속도에 도달")]
    [SerializeField] private float MoveAcceleration = 10f;
    [Tooltip("정지 감속도 (m/s²) - 높을수록 빠르게 멈춤")]
    [SerializeField] private float MoveDeceleration = 25f;
    [Tooltip("웅크리기 이동 속도 (m/s)")]
    [SerializeField] private float CrouchSpeed = 2f;
    [Tooltip("기립 시 캡슐 높이 (m)")]
    [SerializeField] private float StandHeight = 2f;
    [Tooltip("웅크리기 시 캡슐 높이 (m)")]
    [SerializeField] private float CrouchHeight = 1f;


    public PlayerMovement     Movement  { get; private set; }
    public InventoryComponent Inventory;
    public static Entity_Player Inst { get; private set; }

    protected override void Awake()
    {
        Inst      = this;
        base.Awake();
        Movement  = GetComponent<PlayerMovement>();
        Inventory = GetComponent<InventoryComponent>();
    }

    private void Start()
    {
        Mgr_PlayerController.Inst.SetPlayer(this);
        //Mgr_UI.Inst.PocketInventory.UpdateInventory(Inventory.PocketData);
        //Mgr_UI.Inst.BeltEquipSlot.Init(Inventory);
        //Mgr_UI.Inst.BagEquipSlot.Init(Inventory);
    }

    public float GetWalkSpeed()        => WalkSpeed;
    public float GetRunSpeed()         => RunSpeed;
    public float GetJumpForce()        => JumpForce;
    public float GetRotationSpeed()    => RotationSpeed;
    public float GetMoveAcceleration() => MoveAcceleration;
    public float GetMoveDeceleration() => MoveDeceleration;
    public float GetCrouchSpeed()      => CrouchSpeed;
    public float GetStandHeight()      => StandHeight;
    public float GetCrouchHeight()     => CrouchHeight;
}
