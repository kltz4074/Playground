using UnityEngine;

public class SprintState : IState
{
    private readonly PlayerController player;

    public SprintState(PlayerController player)
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