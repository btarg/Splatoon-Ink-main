using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(KinematicBody))]
public class KinematicPlayer : MonoBehaviour
{
	public float speed = 12.0f;
	public float jumpSpeed = 15.0f;
	public float gravity = 35.0f;
    public float snapForce = 10.0f;

    float m_MouseX;
    float m_MouseY;
    public float m_LookSensitivity = 1.0f;
    Vector2 inputLook = Vector2.zero;
    Vector2 inputMove = Vector2.zero;

    Vector2 inputLookCurrent = Vector2.zero;
    Vector2 inputMoveCurrent = Vector2.zero;
    private bool jumping = false;

    private KinematicBody kinematicBody;

    Vector2 smoothInputVelocity = Vector2.zero;
    Vector2 smoothLookVelocity = Vector2.zero;
    public float smoothMovementInputSpeed = .1f;
    public float smoothLookInputSpeed = .2f;

    public Camera m_Camera;

    private void Start()
    {
        kinematicBody = GetComponent<KinematicBody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update() {
        Rotate();
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

    private void FixedUpdate()
    {
        //  Always deriv your motion from the kinematic body velocity
        //  otherwise your movement will be incorrect, like when you
        //  jump against a ceiling and don't fall since your motion 
        //  still being applied against its direction.
        Vector3 moveDirection = kinematicBody.velocity;

        if (kinematicBody.isGrounded)
        {
            // Smooth movement
            inputMoveCurrent = Vector2.SmoothDamp(inputMoveCurrent, inputMove, ref smoothInputVelocity, smoothMovementInputSpeed);

            // Receive user input for movement
            Vector3 forwardMovement = transform.forward * inputMoveCurrent.y;
            Vector3 strafeMovement = transform.right * inputMoveCurrent.x;

            // Convert Input into a Vector3
            moveDirection = (forwardMovement + strafeMovement) * speed;

            moveDirection.y = -snapForce;

            if (jumping)
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        // Call the Move method from Kinematic Body with the desired motion.
        kinematicBody.Move(moveDirection);
    }
}
