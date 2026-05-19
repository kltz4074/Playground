using UnityEngine;

public class IdleState : IState
{
    private readonly PlayerController player;

    public IdleState(PlayerController player)
    {
        this.player = player;
    }

    public void OnEnter()
    {
    }

    public void Update()
    {
    
    }

    public void FixedUpdate()
    {
    
    }

    public void OnExit() { }
}