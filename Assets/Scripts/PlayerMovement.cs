using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float moveSpeed = 5f;

    [Header("Gravity")]
    [SerializeField]
    private float gravity = -20f;

    [SerializeField, Min(0f)]
    private float groundedVelocity = 2f;

    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("玩家移動初始化失敗，找不到 CharacterController。", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Vector2 input = ReadMovementInput();
        Vector3 movement = ResolveMovement(input);

        ApplyGravity();

        movement.y = verticalVelocity;

        characterController.Move(movement * Time.deltaTime);
    }

    private static Vector2 ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;

        if (keyboard.wKey.isPressed)
        {
            input.y += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            input.y -= 1f;
        }

        if (keyboard.aKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            input.x += 1f;
        }

        return Vector2.ClampMagnitude(input, 1f);
    }

    private Vector3 ResolveMovement(Vector2 input)
    {
        return new Vector3(
            input.x * moveSpeed,
            0f,
            input.y * moveSpeed);
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -groundedVelocity;
            return;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }
}