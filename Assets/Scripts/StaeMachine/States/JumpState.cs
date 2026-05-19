using UnityEngine;

public class JumpState : IState
{
    private readonly PlayerController player;

    public JumpState(PlayerController player)
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