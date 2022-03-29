using UnityEngine;
using UnityEngine.InputSystem;

public class FirstPersonCharacterController : MonoBehaviour
{
    public Camera m_Camera;
    public CharacterController m_CharacterController;

    public float m_MoveSpeed = 5.0f;
    public float m_JumpForce = 5.0f;
    public float m_GravityForce = 10f;
    public float m_LookSensitivity = 1.0f;
    float m_MouseX;
    float m_MouseY;

    Vector3 m_MoveDirection;

    Vector2 inputLook = Vector2.zero;
    Vector2 inputMove = Vector2.zero;

    Vector2 inputLookCurrent = Vector2.zero;
    Vector2 inputMoveCurrent = Vector2.zero;
    private bool jumping = false;

    Vector2 smoothInputVelocity = Vector2.zero;
    Vector2 smoothLookVelocity = Vector2.zero;
    public float smoothMovementInputSpeed = .1f;
    public float smoothLookInputSpeed = .2f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Rotate();
        Movement();
    }

    public void OnLook(InputAction.CallbackContext value) {
        inputLook = value.ReadValue<Vector2>();
    }
    public void OnMove(InputAction.CallbackContext value) {
        inputMove = value.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext value) {
        jumping = value.ReadValueAsButton();
    }

    private void Rotate()
    {
        // Receive mouse input and modifies
        m_MouseX += inputLook.x * m_LookSensitivity;
        m_MouseY += inputLook.y * m_LookSensitivity;

        // Smooth camera movement
        inputLookCurrent = Vector2.SmoothDamp(inputLookCurrent, inputLook, ref smoothLookVelocity, smoothLookInputSpeed);

        // Keep mouseY between -90 and +90
        m_MouseY = Mathf.Clamp(m_MouseY, -90.0f, 90.0f);

        // Rotate the player object around the y-axis
        transform.localRotation = Quaternion.Euler(Vector3.up * m_MouseX);
        // Rotate the camera around the x-axis
        m_Camera.transform.localRotation = Quaternion.Euler(Vector3.left * m_MouseY);
    }
    
    private void Movement()
    {
        if (m_CharacterController.isGrounded)
        {
            // Smooth movement
            inputMoveCurrent = Vector2.SmoothDamp(inputMoveCurrent, inputMove, ref smoothInputVelocity, smoothMovementInputSpeed);

            // Receive user input for movement
            Vector3 forwardMovement = transform.forward * inputMoveCurrent.y;
            Vector3 strafeMovement = transform.right * inputMoveCurrent.x;
            // Convert Input into a Vector3
            m_MoveDirection = (forwardMovement + strafeMovement) * m_MoveSpeed;
            if (jumping)
                m_MoveDirection.y = m_JumpForce; // Jump
                jumping = false;
        }

        // Calculate gravity and modify movement vector as such
        m_MoveDirection.y -= m_GravityForce * Time.deltaTime;

        m_CharacterController.Move(m_MoveDirection * Time.deltaTime);
    }
}