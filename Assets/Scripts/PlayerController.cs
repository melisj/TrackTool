using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public float walkAcceleration = 2f;
    public float walkDeceleration = 0.98f;
    public float jumpForce = 30f;

    public float maxWalkSpeed = 2f;
    public float maxSprintSpeed = 3f;

    public float InteractingMouseSensitivity = 0.1f;

    public float maxSlopeAngleForJump = 0.2f;

    public Transform cameraLocation;

    private float forwardVelocity;

    private bool sprinting;
    private bool jumping;
    private bool prevGrounded;

    private bool grounded;

    private Vector3 velocity;
    private Rigidbody rigidBody;

    public CapsuleCollider mainCollider;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
            MovementBehaviour();
    }

    public float GetForwardMomentum()
    {
        return Mathf.Sqrt(Mathf.Pow(velocity.x, 2) + Mathf.Pow(velocity.z, 2));
    }

    private void FixedUpdate()
    {
        grounded = false;        
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            float dotProduct = Vector3.Dot(contact.normal, Vector3.up);
            if (dotProduct >= 1 - maxSlopeAngleForJump && dotProduct <= 1 + maxSlopeAngleForJump)
                grounded = true;
        }
    }

    private void MovementBehaviour()
    {
        CheckMovementInput();

        // Normalize
        velocity.Normalize();

        // Check if one of the input buttons was pressed
        if (velocity != Vector3.zero)
            velocity = rigidBody.velocity + (velocity * walkAcceleration);
        // Slow down when nothing is pressed
        else
            velocity = rigidBody.velocity * walkDeceleration;

        // Don't override the unity gravity
        velocity.y = rigidBody.velocity.y;

        CheckJumpInput();

        NormalizeForwardSpeed();

        // Assign velocity
        rigidBody.velocity = velocity;

        // Set the grounded from previous frame
        prevGrounded = grounded;
    }

    private void CheckMovementInput()
    {
        sprinting = false;
        velocity = Vector3.zero;

        // Check input
        if (Input.GetKey(KeyCode.W))
        {
            velocity += transform.forward;
            sprinting = Input.GetKey(KeyCode.LeftShift);
        }
        if (Input.GetKey(KeyCode.S))
            velocity -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            velocity -= transform.right;
        if (Input.GetKey(KeyCode.D))
            velocity += transform.right;
    }
    
    private void CheckJumpInput()
    {
        // Check if the player was jumping
        if (grounded && !prevGrounded)
            jumping = false;

        // Check for a jump input
        if (Input.GetKeyDown(KeyCode.Space) && !jumping)
        {
            jumping = true;
            velocity.y = jumpForce;
        }
    }

    private void NormalizeForwardSpeed()
    {
        // Normalize the speed when it exceeds the limit
        if (GetForwardMomentum() >= (sprinting ? maxSprintSpeed : maxWalkSpeed))
        {
            Vector2 norm = new Vector2(velocity.x, velocity.z);
            norm.Normalize();
            norm *= (sprinting ? maxSprintSpeed : maxWalkSpeed);
            velocity = new Vector3(norm.x, velocity.y, norm.y);
        }
    }
}
