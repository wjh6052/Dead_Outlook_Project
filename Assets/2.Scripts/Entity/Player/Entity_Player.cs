using UnityEngine;

public class Entity_Player : Entity
{
    public PlayerMovement     Movement  { get; private set; }
    public InventoryComponent InventoryComp;
    public static Entity_Player Inst { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Inst      = this;
        Movement  = GetComponent<PlayerMovement>();
        InventoryComp = GetComponent<InventoryComponent>();
    }

    private void Start()
    {
        Mgr_PlayerController.Inst.SetPlayer(this);
    }
}
