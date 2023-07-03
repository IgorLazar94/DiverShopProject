using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

namespace Player
{
    public class PlayerStateController : MonoBehaviour
    {

        private PlayerStateMachine playerSM;
        private OnTheGroundState onTheGroundState;
        private OnTheWaterState onTheWaterState;

        private void Start()
        {
            playerSM = new PlayerStateMachine();
            onTheGroundState = new OnTheGroundState(this);
            onTheWaterState = new OnTheWaterState(this);
            playerSM.Initialize(new OnTheGroundState(this));
        }

        private void OnEnable()
        {
            PlayerTrigger.onTriggerGround += ChangeStateToGround;
            PlayerTrigger.onTriggerWater += ChangeStateToWater;
        }

        private void OnDisable()
        {
            PlayerTrigger.onTriggerGround -= ChangeStateToGround;
            PlayerTrigger.onTriggerWater -= ChangeStateToWater;
        }

        private void Update()
        {
            playerSM.CurrentState.Update();
        }

        private void ChangeStateToGround()
        {
            playerSM.ChangeState(onTheGroundState);
        }

        private void ChangeStateToWater()
        {
            playerSM.ChangeState(onTheWaterState);
        }

        //public bool GetIsInWaterState()
        //{
        //    return onTheWaterState.isInWater;
        //}
    }
}
