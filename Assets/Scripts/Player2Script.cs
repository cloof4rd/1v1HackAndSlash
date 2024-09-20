using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player2Script : MonoBehaviour, IDamageable
{
    public KeyCode meleeAttackKey = KeyCode.Slash; // / for melee attack
    public KeyCode jumpKey = KeyCode.RightShift;   // Right Shift for jump
    public string xMoveAxis = "Player2Horizontal"; // Arrow keys horizontal

    public float speed = 5f;
    public float jumpForce = 6f;
    public float groundedLeeway = 0.1f;

    public Transform meleeAttackOrigin = null;
    public float meleeAttackRadius = 1.5f;
    public float meleeDamage = 2f;
    public float meleeAttackDelay = 1.1f;
    public LayerMask enemyLayer;

    public AudioSource attackSound;
    public AudioSource hitSound;

    // Health and Respawn
    public int maxHealth = 3;
    private int currentHealth;
    public float respawnTime = 3f; // Time before respawn
    private bool isAlive = true;

    private Rigidbody2D rb2D;
    private Animator animator;
    private float moveIntentionX = 0;
    private bool attemptJump = false;
    private bool attemptMeleeAttack = false;
    private float timeUntilMeleeReadied = 0;
    private bool isGrounded = false;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        animator = GetComponent<Animator>();

        attemptMeleeAttack = false;
        attemptJump = false;
        timeUntilMeleeReadied = meleeAttackDelay;

        currentHealth = maxHealth; // Initialize health
    }

    void Update()
    {
        if (!isAlive) return; // Do not process actions if the player is dead

        GetInput();
        isGrounded = CheckGrounded();
        HandleJump();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        HandleRun();
    }

    private void GetInput()
    {
        moveIntentionX = Input.GetAxis(xMoveAxis);
        attemptMeleeAttack = Input.GetKeyDown(meleeAttackKey);
        if (attemptMeleeAttack)
        {
            Debug.Log("Melee attack key pressed");
        }
        attemptJump = Input.GetKeyDown(jumpKey);
    }

    private void HandleRun()
    {
        Vector3 currentScale = transform.localScale;

        if (moveIntentionX > 0)
        {
            currentScale.x = Mathf.Abs(currentScale.x);
        }
        else if (moveIntentionX < 0)
        {
            currentScale.x = Mathf.Abs(currentScale.x) * -1;
        }

        transform.localScale = currentScale;
        rb2D.velocity = new Vector2(moveIntentionX * speed, rb2D.velocity.y);
    }

    private void HandleJump()
    {
        if (attemptJump && isGrounded)
        {
            rb2D.velocity = new Vector2(rb2D.velocity.x, jumpForce);
            isGrounded = false;
        }
    }

    private void HandleAttack()
    {
        if (attemptMeleeAttack && timeUntilMeleeReadied <= 0)
        {
            Debug.Log("Player attacking");

            if (attackSound != null)
            {
                attackSound.Play();
            }

            animator.SetTrigger("Slash");

            Collider2D[] hitObjects = Physics2D.OverlapCircleAll(meleeAttackOrigin.position, meleeAttackRadius, enemyLayer);
            Debug.Log("Number of hit objects: " + hitObjects.Length);

            for (int i = 0; i < hitObjects.Length; i++)
            {
                // Skip the player itself to prevent self-damage
                if (hitObjects[i].gameObject != this.gameObject)  // This prevents the player from hitting themselves
                {
                    IDamageable target = hitObjects[i].GetComponent<IDamageable>();
                    if (target != null)
                    {
                        Debug.Log("Enemy or player hit: " + hitObjects[i].name);
                        target.ApplyDamage(meleeDamage); // Apply damage
                        if (hitSound != null)
                        {
                            hitSound.Play();
                        }
                    }
                }
            }

            timeUntilMeleeReadied = meleeAttackDelay;
        }
        else
        {
            timeUntilMeleeReadied -= Time.deltaTime;
        }
    }





    // Implementation of IDamageable
    public void ApplyDamage(float damage)
    {
        if (!isAlive) return;

        currentHealth -= (int)damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isAlive = false;
        rb2D.velocity = Vector2.zero; // Stop movement
        gameObject.SetActive(false);   // Disable the player

        StartCoroutine(Respawn());     // Start the respawn process
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(respawnTime);  // Wait before respawning

        currentHealth = maxHealth;  // Reset health
        gameObject.SetActive(true); // Re-enable the player
        isAlive = true;             // Mark the player as alive again
        transform.position = new Vector3(0, 0, 0);  // Set respawn position
    }

    private bool CheckGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, groundedLeeway);
    }

    private void OnDrawGizmosSelected()
    {
        if (meleeAttackOrigin != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleeAttackOrigin.position, meleeAttackRadius);
        }
    }
}
