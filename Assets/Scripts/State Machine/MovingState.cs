using UnityEngine;
using UnityEngine.InputSystem;

public class MovingState : State
{
    private bool isJumping;
    private bool grounded;
    public MovingState(PlayerController playerController, StateMachine stateMachine) : base(playerController, stateMachine)
    {
        this.playerController = playerController;
        this.stateMachine = stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        isMoving = true;
        isJumping = false;
        grounded = playerController.characterController.isGrounded;
        moveInput = moveAction.ReadValue<Vector2>();
    }

    public override void HandleInput()
    {
        base.HandleInput();
        moveInput = moveAction.ReadValue<Vector2>();
        if (jumpKeyDown && grounded)
        {
            isJumping = true;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!isMoving)
        {
            stateMachine.ChangeState(playerController.standing);
        }
        if (isJumping)
        {
            stateMachine.ChangeState(playerController.jumping);
        } else if(!grounded)
        {
            stateMachine.ChangeState(playerController.falling);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (moveInput != Vector2.zero)
        {
            Vector3 move = playerController.transform.forward * moveInput.y + playerController.transform.right * moveInput.x;
            move *= playerController.movementSpeed * Time.deltaTime;

            playerController.characterVelocity = move;
        }
        else
        {
            playerController.characterVelocity -= playerController.characterVelocity * playerController.frictionConstant;
            //Debug.Log("friction applied");
        }

        playerController.characterVelocity.y = playerController.verticalVelocity * Time.deltaTime;
        playerController.characterController.Move(playerController.characterVelocity);

        grounded = playerController.characterController.isGrounded;
    }

    public override void Exit()
    {
        base.Exit();
    }


}
