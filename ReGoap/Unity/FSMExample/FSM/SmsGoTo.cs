using System;
using ReGoap.Unity.FSM;
using ReGoap.Utilities;
using UnityEngine;

// generic goto state, can be used in most games, override Tick and Enter if you are using 
//  a navmesh / pathfinding library 
//  (ex. tell the library to search a path in Enter, when done move to the next waypoint in Tick)
namespace ReGoap.Unity.FSMExample.FSM
{
    [RequireComponent(typeof(StateMachine))]
    [RequireComponent(typeof(SmsIdle))]
    public class SmsGoTo : SmState
    {
        private Vector3? objective;
        private Transform objectiveTransform;
        private Action onDoneMovementCallback;
        private Action onFailureMovementCallback;

        private enum GoToState
        {
            Disabled, Pulsed, Active, Success, Failure
        }
        private GoToState currentState;
        private Rigidbody body;

        public bool WorkInFixedUpdate;
        public bool UseRigidBody;
        public bool UseRigidbodyVelocity;
        public float Speed;
        // when the magnitude of the difference between the objective and self is <= of this then we're done
        public float MinPowDistanceToObjective = 0.5f;

        // additional feature, check for stuck, userful when using rigidbody or raycasts for movements
        private Vector3 lastStuckCheckUpdatePosition;
        private float stuckCheckCooldown;
        public bool CheckForStuck;
        public float StuckCheckDelay = 1f;
        public float MaxStuckDistance = 0.1f;

        protected override void Awake()
        {
            base.Awake();
            if (UseRigidBody)
            {
                body = GetComponent<Rigidbody>();
            }
        }

        // if your games handle the speed from something else (ex. stats class) you can override this function
        protected virtual float GetSpeed()
        {
            return Speed;
        }

        #region Work
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!WorkInFixedUpdate) return;
            Tick();
        }

        protected override void Update()
        {
            base.Update();
            if (WorkInFixedUpdate) return;
            Tick();
        }

        // if you're using an animation just override this, call base function (base.Tick()) and then 
        //  set the animator variables (if you want to use root motion then also override MoveTo)
        protected virtual void Tick()
        {
            var objectivePosition = objectiveTransform != null ? objectiveTransform.position : objective.GetValueOrDefault();
            MoveTo(objectivePosition);
        }

        protected virtual void MoveTo(Vector3 position)
        {
            var delta = position - transform.position;
            var movement = delta.normalized * GetSpeed();
            if (UseRigidBody)
            {
                if (UseRigidbodyVelocity)
                {
                    body.velocity = movement;
                }
                else
                {
                    body.MovePosition(transform.position + movement * Time.deltaTime);
                }
            }
            else
            {
                transform.position += movement * Time.deltaTime;
            }
            if (delta.sqrMagnitude <= MinPowDistanceToObjective)
            {
                currentState = GoToState.Success;
            }
            if (CheckForStuck && CheckIfStuck())
            {
                currentState = GoToState.Failure;
            }
        }

        private bool CheckIfStuck()
        {
            if (Time.time > stuckCheckCooldown)
            {
                stuckCheckCooldown = Time.time + StuckCheckDelay;
                if ((lastStuckCheckUpdatePosition - transform.position).magnitude < MaxStuckDistance)
                {
                    ReGoapLogger.Log("[SmsGoTo] '" + name + "' is stuck.");
                    return true;
                }
                lastStuckCheckUpdatePosition = transform.position;
            }
            return false;
        }

        #endregion

        #region StateHandler
        public override void Init(StateMachine stateMachine)
        {
            base.Init(stateMachine);
            var transition = new SmTransition(GetPriority(), Transition);
            var doneTransition = new SmTransition(GetPriority(), DoneTransition);
            stateMachine.GetComponent<SmsIdle>().Transitions.Add(transition);
            Transitions.Add(doneTransition);
        }

        private Type DoneTransition(ISmState state)
        {
            if (currentState != GoToState.Active)
                return typeof(SmsIdle);
            return null;
        }

        private Type Transition(ISmState state)
        {
            if (currentState == GoToState.Pulsed)
                return GetType();
            return null;
        }

        public void GoTo(Vector3? position, Action onDoneMovement, Action onFailureMovement)
        {
            objective = position;
            GoTo(onDoneMovement, onFailureMovement);
        }

        public void GoTo(Transform transform, Action onDoneMovement, Action onFailureMovement)
        {
            objectiveTransform = transform;
            GoTo(onDoneMovement, onFailureMovement);
        }

        void GoTo(Action onDoneMovement, Action onFailureMovement)
        {
            currentState = GoToState.Pulsed;
            onDoneMovementCallback = onDoneMovement;
            onFailureMovementCallback = onFailureMovement;
        }

        public override void Enter()
        {
            base.Enter();
            currentState = GoToState.Active;
        }

        public override void Exit()
        {
            base.Exit();
            if (currentState == GoToState.Success)
                onDoneMovementCallback();
            else
                onFailureMovementCallback();
        }
        #endregion
    }
}
