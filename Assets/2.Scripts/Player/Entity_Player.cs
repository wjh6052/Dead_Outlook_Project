using UnityEngine;

public class Entity_Player : Entity
{
    [Header("½ºÅÝ")]
    [SerializeField] private float MoveSpeed = 5f;
    [SerializeField] private float RunSpeed = 8f;

    public PlayerMovement Movement { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        Mgr_PlayerController.Inst.SetPlayer(this);
    }

    public float GetMoveSpeed() => MoveSpeed;
    public float GetRunSpeed() => RunSpeed;

}
