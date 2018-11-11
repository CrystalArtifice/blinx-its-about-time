using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // Horizontal walking movement speed of the player.
    public float walkSpeed;

    // The current vertical velocity of the player.
    private float deltaV = 0.0f;

    // The jump velocity of the player.
    public float jumpVelocity;

    // The jump break gravity multiplier.
    public float jumpBreakingMultiplier;

    // The falling gravity multiplier.
    public float jumpGravityMultiplier;

    // The maximum falling speed of the player.
    public float terminalVelocity;

    // Rigidbody of the player.
    private Rigidbody2D rb2d;

    // Collider of the player.
    private Collider2D collider2d;

    // The distance from the player's pivot to the bottom collision face.
    private float boundsOffset;

    // The error bounds for the ground collision checker.
    private float groundedMargin = 0.1f;

    // Use this for initialization
    void Start ()
    {
        rb2d = GetComponent<Rigidbody2D>();
        collider2d = GetComponent<Collider2D>();

        boundsOffset = collider2d.bounds.extents.y;
	}

    private void FixedUpdate()
    {
        // Calculate the horizontal speed of the player to offset position by.
        float deltaH = walkSpeed * Input.GetAxis("Horizontal");

        // Calculate gravity effect depending on jump stage.
        if (!IsGrounded() && deltaV > -terminalVelocity)
        {
            if (deltaV < 0)
                deltaV += Physics2D.gravity.y * jumpGravityMultiplier;
            else if (!Input.GetButton("Jump"))
                deltaV += Physics2D.gravity.y * jumpBreakingMultiplier;
            else
                deltaV += Physics2D.gravity.y;
        }
        else
        {
            deltaV = 0;
        }

        // Jump if on the ground.
        if(IsGrounded() && (Input.GetButtonDown("Jump")))
            deltaV = jumpVelocity;

        // Sum movement and add.
        Vector2 movement = new Vector2(deltaH, deltaV) * Time.deltaTime;

        // Only perform one raycast.
        float altitude = DistanceToGround();

        // Check movement to make sure we aren't clipping into the floor.
        if (movement.y < -altitude)
            movement.y = -altitude;

        // Set the player level with the ground if they are "floating" and momentum has ceased.
        if (movement.y == 0 && altitude > 0 && altitude < groundedMargin)
            movement.y = -altitude;

        rb2d.MovePosition(rb2d.position + movement);    
    }

    /**
     * Check if the player is on the ground.
     */
    private bool IsGrounded()
    {
        return Physics2D.Raycast(rb2d.position, Vector2.down, boundsOffset + groundedMargin, LayerMask.GetMask("Ground"));
    }

    /**
     * Get the distance from the player to the ground.
     */
    private float DistanceToGround()
    {
        // The raycast hit data.
        RaycastHit2D rayHit = Physics2D.Raycast(rb2d.position, Vector2.down, 10, LayerMask.GetMask("Ground"));

        return rayHit.distance - boundsOffset;
    }
}
