using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour, IDamageable
{
    public float healthPool = 10f;
    public float moveSpeed = 50f;    // Increased movement speed for rapid movement
    public float jumpForce = 25f;    // Increased jump force for higher jumps
    public float attackIntervalMin = 1f;  // Minimum time between attacks
    public float attackIntervalMax = 3f;  // Maximum time between attacks
    public float respawnTime = 2f;  // Time in seconds to respawn after death
    public LayerMask groundLayer;   // Layer used to detect the ground

    private float currentHealth;
    private Rigidbody2D rb2D;
    private Vector3 originalPosition;  // Save the original position for respawning
    private bool isGrounded = true;
    private float nextAttackTime = 0f;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer; // To hide the enemy
    private Collider2D enemyCollider;      // To disable physics

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = healthPool;
        originalPosition = transform.position;  // Store the initial position

        // Get Rigidbody2D component and set it up
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.isKinematic = false; // Enable physics-based movement
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation; // Prevent rotation

        // Get Renderer and Collider components
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();

        // Start movement and attack behaviors
        StartCoroutine(RandomMovement());
        StartCoroutine(RandomAttacks());
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            CheckGrounded();
        }
    }

    // Apply damage logic for the enemy
    public virtual void ApplyDamage(float amount)
    {
        if (!isDead)
        {
            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    // Enemy death logic
    private void Die()
    {
        isDead = true;

        // Disable visibility and collision
        spriteRenderer.enabled = false;
        enemyCollider.enabled = false;

        // Stop movement by resetting velocity
        rb2D.velocity = Vector2.zero;

        StartCoroutine(Respawn());  // Start the respawn process
    }

    // Coroutine for respawning the enemy after death
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);  // Wait before respawning

        // Respawn settings
        currentHealth = healthPool;  // Reset health
        transform.position = originalPosition;  // Respawn at the original position

        // Re-enable visibility and collision
        spriteRenderer.enabled = true;
        enemyCollider.enabled = true;

        isDead = false;  // Enemy is no longer dead
    }

    // Coroutine for more sporadic random movement and jumping
    IEnumerator RandomMovement()
    {
        while (true)
        {
            if (!isDead)
            {
                // More sporadic movement: Choose a random direction more frequently
                float moveDirection = Random.Range(-1f, 1f);
                rb2D.velocity = new Vector2(moveDirection * moveSpeed, rb2D.velocity.y);

                // Sporadic jump logic: Randomize the chance to jump more often
                if (isGrounded && Random.Range(0f, 1f) > 0.4f) // 60% chance to jump
                {
                    Jump();
                }

                // Wait for a shorter, more varied time to make movement more sporadic
                yield return new WaitForSeconds(Random.Range(0.2f, 0.7f)); // Even shorter intervals
            }
            else
            {
                yield return null;  // Do nothing while dead
            }
        }
    }

    // Jump mechanic for the enemy
    private void Jump()
    {
        if (isGrounded)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce); // Apply jump force
        }
    }

    // Coroutine for random attacks
    IEnumerator RandomAttacks()
    {
        while (true)
        {
            if (!isDead && Time.time >= nextAttackTime)
            {
                Attack();
                // Randomize the next attack time within a specified range
                nextAttackTime = Time.time + Random.Range(attackIntervalMin, attackIntervalMax);
            }

            yield return null; // Check again next frame
        }
    }

    // Simulated attack method (you can implement actual attack logic here)
    private void Attack()
    {
        Debug.Log(gameObject.name + " is attacking!");
        // Add your attack logic here (e.g., deal damage, trigger animation, etc.)
    }

    // Check if the enemy is on the ground
    private void CheckGrounded()
    {
        // Use raycasting to check if the enemy is grounded
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 0.2f, groundLayer);
        isGrounded = hit.collider != null;
    }

    // Draw raycast for debugging purposes
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.down * 0.2f);
    }
}
