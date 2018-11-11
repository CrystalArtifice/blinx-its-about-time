using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    // Horizontal walking movement speed of the player.
    public float walkSpeed;

    // The sprint multiplier of the movement speed.
    public float sprintMultiplier;

    // If the player is spriting.
    private bool isSprinting = false;

    // The time between the player ceasing movement and the character responding in seconds.
    public float stopDelay = 0.5f;

    // The counter of how long the player has left to press a sprint button.
    private float stopCounter = 0.0f;

    // The last walking direction of the player.
    private float walkMomentum = 0.0f;

    // The last control direction of the player.
    private float lastWalkDirection1 = 0.0f;

    // The second last control direction of the player.
    private float lastWalkDirection2 = 0.0f;

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
        float deltaH;

        // If the direction is zero, continue along the last walk direction.
        if (Input.GetAxis("Horizontal") == 0.0f)
        {
            deltaH = walkSpeed * walkMomentum;
        }
        else
        {
            // If the new press matches to the old.
            if (Mathf.Sign(Input.GetAxis("Horizontal")) == Mathf.Sign(walkMomentum))
            {
                // If there was a break between presses - sprint. Break can be detected via the sudden acceleration of the player after slowing.
                if (Mathf.Abs(lastWalkDirection1) < Mathf.Abs(Input.GetAxis("Horizontal")) && Mathf.Abs(lastWalkDirection1) < Mathf.Abs(lastWalkDirection2))
                    isSprinting = true;
            }
            // If the press is of the opposite direction or done, slow down and slide.
            else
            {
                isSprinting = false;
            }

            // If not sprinting or moving in the same direction as the sprint, move normally.
            if (!isSprinting || Mathf.Sign(Input.GetAxis("Horizontal")) == Mathf.Sign(walkMomentum))
            {
                // Calculate the horizontal speed of the player to offset position by.
                deltaH = walkSpeed * Input.GetAxis("Horizontal");
                walkMomentum = Input.GetAxis("Horizontal");
                stopCounter = stopDelay;
            }
            // Otherwise ignore movement if spriting and sliding to a stop.
            else
            {
                deltaH = walkSpeed * walkMomentum;
            }
        }

        // Multiply speed by sprinting speed.
        if (isSprinting)
            deltaH *= sprintMultiplier;

        // Update the last walk direction.
        lastWalkDirection2 = lastWalkDirection1;
        lastWalkDirection1 = Input.GetAxis("Horizontal");

        // Decrement the sprint counter.
        if (stopCounter > 0.0f)
            stopCounter -= Time.deltaTime;

        // Reset the last walk direction and sprint status if expired.
        if (stopCounter <= 0.0f)
        {
            stopCounter = 0.0f;
            isSprinting = false;
            walkMomentum = 0.0f;
        }

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
        if (IsGrounded() && (Input.GetButtonDown("Jump")))
            deltaV = jumpVelocity;

        // Sum movement and add.
        Vector2 movement = new Vector2(deltaH, deltaV) * Time.deltaTime;

        // Only perform one raycast.
        float altitude = DistanceToGround();

        if (IsAboveGround())
        { 
        // Check movement to make sure we aren't clipping into the floor.
        if (movement.y < -altitude)
            movement.y = -altitude;

        // Set the player level with the ground if they are "floating" and momentum has ceased.
        if (movement.y == 0 && altitude > 0 && altitude < groundedMargin)
            movement.y = -altitude;
        }

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
    private bool IsAboveGround()
    {
        // The raycast hit data.
        RaycastHit2D rayHit = Physics2D.Raycast(rb2d.position, Vector2.down, 10, LayerMask.GetMask("Ground"));

        return rayHit.collider != null;
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
