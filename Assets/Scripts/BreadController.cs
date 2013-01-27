using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum State
{
    Falling,
    Jumping,
    Hanging,
    Walking,
    Climbing,
    Limbo,
    Dead
}

public enum Command
{
    Jumped,
    ApexReached,
    GrabbedLedge,
    LetGo,
    TouchedCeiling,
    LeftGround,
    TouchedGround,
    EnterLimbo,
    ExitLimbo,
    Die
}

public class Process
{
    class StateTransition
    {
        readonly State CurrentState;
        readonly Command Command;

        public StateTransition(State currentState, Command command)
        {
            CurrentState = currentState;
            Command = command;
        }

        public override int GetHashCode()
        {
            return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            StateTransition other = obj as StateTransition;
            return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
        }
    }

    Dictionary<StateTransition, State> transitions;
    public State CurrentState { get; private set; }

    public Process()
    {
        CurrentState = State.Falling;
        transitions = new Dictionary<StateTransition, State>
        {
            { new StateTransition(State.Walking, Command.Jumped), State.Jumping },
            { new StateTransition(State.Walking, Command.LeftGround), State.Falling },
            { new StateTransition(State.Jumping, Command.TouchedCeiling), State.Falling },
            { new StateTransition(State.Jumping, Command.ApexReached), State.Falling },
            { new StateTransition(State.Jumping, Command.GrabbedLedge), State.Hanging },
            { new StateTransition(State.Falling, Command.TouchedGround), State.Walking },
            { new StateTransition(State.Falling, Command.GrabbedLedge), State.Hanging },
            { new StateTransition(State.Falling, Command.EnterLimbo), State.Limbo },
            { new StateTransition(State.Jumping, Command.EnterLimbo), State.Limbo },
            { new StateTransition(State.Walking, Command.EnterLimbo), State.Limbo },
            { new StateTransition(State.Limbo, Command.ExitLimbo), State.Falling },
            { new StateTransition(State.Walking, Command.Die), State.Dead },
            { new StateTransition(State.Falling, Command.Die), State.Dead },
            { new StateTransition(State.Jumping, Command.Die), State.Dead },

        };
    }

    public State GetNext(Command command)
    {
        StateTransition transition = new StateTransition(CurrentState, command);
        State nextState;
        if (!transitions.TryGetValue(transition, out nextState))
            throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
        return nextState;
    }

    public State MoveNext(Command command)
    {
        
        CurrentState = GetNext(command);
        
        return CurrentState;
    }
}


public class BreadController : MonoBehaviour {

    OTSprite m_Sprite;

    private Process m_StateMachine = new Process();

    private CharacterController controller;

    [SerializeField]
    private float m_Gravity = 15f;
    [SerializeField]
    private float m_MaxFallSpeed = -20.0f;
    [SerializeField]
    private float m_WalkSpeed = 6f;
    [SerializeField]
    private float m_InAirSpeed = 3f;
    [SerializeField]
    private float m_JumpTime = 0.2f;
    [SerializeField]
    private float m_JumpSpeed = 100f;

    private float m_LastJumpTime = 0f;

    private Vector3 m_Velocity = Vector3.zero;

    private int m_ToastTimer = 0;

    private ToasterCounter m_CounterRef;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        m_Sprite = GetComponent<OTSprite>();
        
    }

	void Start () {
        m_Sprite.rotation = 0;
        m_Sprite.depth = 0;
        m_Sprite.frameIndex = 3;
        m_CounterRef = (ToasterCounter)GameObject.Find("GUIContainer").GetComponent(typeof(ToasterCounter));
	}

	void Update () {
        //Update the sprites position based on the gameobjects position.
        m_Sprite.position = new Vector2(transform.position.x, transform.position.y);
        SetSprite();

        if(m_StateMachine.CurrentState == State.Jumping || m_StateMachine.CurrentState == State.Falling)
        {
            MoveInAir();
        }
        else if (m_StateMachine.CurrentState == State.Walking)
        {
            Move();
        }

        if (Input.GetButtonDown("Jump") && m_StateMachine.CurrentState != State.Falling && m_StateMachine.CurrentState != State.Jumping && m_StateMachine.CurrentState != State.Limbo)
        {
            m_LastJumpTime = Time.time;
            m_StateMachine.MoveNext(Command.Jumped);
        }
        else if (Input.GetButtonDown("Jump") && m_StateMachine.CurrentState == State.Limbo)
        {
            ToasterPop();
        }

        if (m_StateMachine.CurrentState == State.Limbo)
        {
            //pass
        }
        
        if (m_StateMachine.CurrentState == State.Dead)
        {
            //Spin while falling;
            m_Sprite.rotation += 2;
        }

        ApplyJump();

        ApplyGravity();

        //Debug.Log(m_StateMachine.CurrentState);
        //Debug.Log(m_StateMachine.CurrentState.ToString());
        //Debug.Log(m_Velocity.x);
        
        controller.Move(m_Velocity * Time.smoothDeltaTime);
	}

    void Move()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        //MOVING LEFT
        if (horizontal != 0)
        {
            m_Velocity.x = m_WalkSpeed * horizontal;
        }
        else
        {
            if (m_StateMachine.CurrentState == State.Walking)
            {
                m_Velocity.x = 0;
            }
        }
    }

    void MoveInAir()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (m_InAirSpeed > 0)
        {
            //Moving left, but want to go right
            if (horizontal == 1 && m_Velocity.x < 0)
            {
                m_Velocity.x = m_InAirSpeed * horizontal;
            }
            //Moving right, but want to go left
            else if (horizontal == -1 && m_Velocity.x > 0)
            {
                m_Velocity.x = m_InAirSpeed * horizontal;
            }
        }
    }

    void OnTriggerEnter(Collider c)
    {
        
        if (c.tag == "BasicToaster")
        {
            m_StateMachine.MoveNext(Command.EnterLimbo);
            m_Velocity.x = 0;
            m_Velocity.y = 0;
            controller.transform.position = c.transform.position;
            m_CounterRef.Increment();
        }

        else if (c.tag == "Death")
        {
            m_StateMachine.MoveNext(Command.Die);
            m_Velocity.x = 0;
            m_Sprite.frameIndex = 1;
            m_Sprite.depth = -50;
            
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c.tag == "Boundary")
        {
            Application.LoadLevel(Application.loadedLevel);
        }
    }


    void OnControllerColliderHit()
    {
        if (m_StateMachine.CurrentState == State.Falling)
        {
            m_StateMachine.MoveNext(Command.TouchedGround);
        }
        else if (m_StateMachine.CurrentState == State.Jumping)
        {
            m_StateMachine.MoveNext(Command.TouchedCeiling);
        }
    }

    void ApplyJump()
    {

        if (m_StateMachine.CurrentState == State.Jumping)
        {
            m_Velocity.y = m_JumpSpeed * Time.smoothDeltaTime;
            if (m_LastJumpTime + m_JumpTime <= Time.time)
            {
                m_StateMachine.MoveNext(Command.ApexReached);
                
            }
        }
        
    }

    void ApplyGravity()
    {
        if (m_StateMachine.CurrentState != State.Jumping && m_StateMachine.CurrentState != State.Limbo)
        {
            if (m_Velocity.y > m_MaxFallSpeed)
            {
                m_Velocity.y -= m_Gravity * Time.smoothDeltaTime;
            }
        }
        else if (m_StateMachine.CurrentState == State.Dead)
        {
            if (m_Velocity.y > m_MaxFallSpeed)
            {
                m_Velocity.y -= m_Gravity * Time.smoothDeltaTime;
            }
        }
    }

    void ToasterPop()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        m_StateMachine.MoveNext(Command.ExitLimbo);
        
        float time = 800f;
        m_Velocity = new Vector3(horizontal * time, time);
        
    }

    void SetSprite()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (m_StateMachine.CurrentState == State.Walking && horizontal == 1)
        {
            m_Sprite.frameIndex = 5;
        }
        else if (m_StateMachine.CurrentState == State.Walking && horizontal == -1)
        {
            m_Sprite.frameIndex = 4;
        }
        else
        {
            switch (m_StateMachine.CurrentState)
            {
                case State.Falling:
                    if (m_Sprite.frameIndex != 2)
                    {
                        m_Sprite.frameIndex = 2;
                    }
                    break;
                case State.Jumping:
                    if (m_Sprite.frameIndex != 6)
                    {
                        m_Sprite.frameIndex = 6;
                    }
                    break;
                case State.Walking:
                    if (m_Sprite.frameIndex != 3)
                    {
                        m_Sprite.frameIndex = 3;
                    }
                    break;

            }
        }
    }

}
