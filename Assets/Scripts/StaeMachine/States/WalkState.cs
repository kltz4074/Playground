using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class WalkState : IState
{
    private readonly PlayerController player;

    public WalkState(PlayerController player)
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