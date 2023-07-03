using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTheGroundState : GenericState
{
    private PlayerStateController playerStateController;
    public OnTheGroundState(PlayerStateController playerStateController)
    {
        this.playerStateController = playerStateController;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }
}