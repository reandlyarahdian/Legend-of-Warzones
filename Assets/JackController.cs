using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackController : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 1.2f;
    public float maxJump = 4f;
    private bool isGrounded = false;

    private Rigidbody2D rb;

    [Header("Sprite orientation")]
    private bool facingRight = true;
    private bool wasCrounching = false;
    private bool wasFiring = false;
    private bool wasFiring2 = false;

    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.1f;
    private float nextFire = 0.1f;

    [Header("Time jump")]
    private float jumpTime = 0.0f;
    public float jumpDelta = 0.1f;
    private float nextJump = 0.1f;

    [Header("Bullet")]
    public GameObject projSpawner;

    [Header("Melee")]
    public float meleeDistance = 0.4f;
    public float damageMelee = 200f;

    private Health health;
    private bool asObjUp = false;
    private bool haveMachineGun = false;

    public GameObject foreground;
    Cinemachine.CinemachineBrain cinemachineBrain;

    public Animator animator;

    public enum CollectibleType
    {
        HeavyMachineGun,
        Ammo,
        MedKit,
    };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cinemachineBrain = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();
        registerHealth();
    }
    private void registerHealth()
    {
        health = GetComponent<Health>();
        // register health delegate
        health.onDead += OnDead;
        health.onHit += OnHit;
    }

    void Update()
    {
        //Block the player from moving if it's death
        if (GameManager.IsGameOver() || !health.IsAlive())
            return;

        CheckHeavyAmmo();
        Fire();
        MoveHorizontally();
        MoveVertically();
        Jump();

        FlipShoot();
    }

    private void OnDead(float damage) // health delegate onDead
    {
        Died();
        GameManager.PlayerDied();
        AudioManager.PlayDeathAudio();
    }

    private void OnHit(float damage) // health delegate onHit
    {
        UIManager.UpdateHealthUI(health.GetHealth(), health.GetMaxHealth());
        AudioManager.PlayMeleeTakeAudio();
    }

    void Died()
    {
        animator.SetBool("isDying", true);
    }

    void Fire()
    {
        shotTime = shotTime + Time.deltaTime;

        if (MobileManager.GetButtonFire1())
        {
            if (!wasFiring)
            {

                animator.SetBool("isFiring", true);

                if (shotTime > nextFire)
                {
                    nextFire = shotTime + fireDelta;

                    StartCoroutine(WaitFire());
                    StartCoroutine(WaitMelee());

                    nextFire = nextFire - shotTime;
                    shotTime = 0.0f;
                }

                wasFiring = true;
            }
            else
            {
                animator.SetBool("isFiring", false);
            }
        }
        else
        {
            animator.SetBool("isFiring", false);
            wasFiring = false;
        }
    }

    bool IsOutsideScreen(float moveH)
    {
        //Return a value between [0;1] - 0.5 if the player is in the mid of the camera
        var playerVPPos = Camera.main.WorldToViewportPoint(transform.position);

        //Prevent walking back when camera is blending
        if (moveH < -Mathf.Epsilon && cinemachineBrain.IsBlending)
            return true;

        //Control if the camera is out of the sprite map
        if ((playerVPPos.x < 0.03f || playerVPPos.x > 1 - 0.03f))
            return true;
        return false;
    }

    void MoveHorizontally()
    {
        float moveH = MobileManager.GetAxisHorizontal();
        if (IsOutsideScreen(moveH))
            return;

        if (moveH != 0 && !(animator.GetBool("isFiring")))
        {
            rb.velocity = new Vector2(moveH * maxSpeed, rb.velocity.y);
            animator.SetBool("isWalking", true);

            //Flip sprite orientantion if the user is walking right or left
            if (moveH > 0 && !facingRight)
            {
                //Moving right
                Flip();
            }
            else if (moveH < 0 && facingRight)
            {
                //Moving left
                Flip();
            }
        }
        else
        {
            animator.SetBool("isWalking", false);
        }
    }

    void MoveVertically()
    {
        float moveV = MobileManager.GetAxisVertical();
        if (moveV != 0)
        {
            if (moveV > 0)
            {
                //Moving UP
                animator.SetBool("isLookingUp", true);
            }
            else if (moveV < 0)
            {
                //Moving down
            }
        }
        else
        {
            //No
            if (animator.GetBool("isLookingUp"))
            {
                animator.SetBool("isLookingUp", false);
            }
        }
    }

    void Jump()
    {

        jumpTime = jumpTime + Time.deltaTime;

        if (MobileManager.GetButtonJump() && isGrounded)
        {
            if (jumpTime > nextJump)
            {
                rb.AddForce(new Vector3(0, maxJump, 0), ForceMode2D.Impulse);
                animator.SetBool("isJumping", true);
                isGrounded = false;

                nextJump = jumpTime + jumpDelta;
                nextJump = nextJump - jumpTime;
                jumpTime = 0.0f;
            }
        }
    }

    //Flip sprite
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        facingRight = !facingRight;
    }

    void FlipShoot()
    {
        if (animator.GetBool("isLookingUp"))
        {
            //Fire up
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        else if (facingRight)
        {
            //Fire right
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            //Fire left
            projSpawner.transform.rotation = Quaternion.Euler(0, 0, -180);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Walkable") || col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Marco Boat"))
        {
            isGrounded = true;
            animator.SetBool("isJumping", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(collision.collider.tag);
        if (collision.collider.CompareTag("Water Dead"))
        {
            health.Hit(100);

            if (foreground != null)
                gameObject.transform.parent = foreground.transform;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Roof"))
        {
            asObjUp = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Roof"))
        {
            asObjUp = false;
        }
    }

    private IEnumerator WaitFire()
    {
        yield return new WaitForSeconds(0.1f);
        if (haveMachineGun)
        {
            AudioManager.PlayHeavyMachineShotAudio();
            BulletManager.GetNormalBulletPool().Spawn(projSpawner.transform.position, projSpawner.transform.rotation);
            yield return new WaitForSeconds(0.05f);
            BulletManager.GetNormalBulletPool().Spawn(projSpawner.transform.position, projSpawner.transform.rotation);
            yield return new WaitForSeconds(0.05f);
            BulletManager.GetNormalBulletPool().Spawn(projSpawner.transform.position, projSpawner.transform.rotation);
            GameManager.RemoveHeavyMachineAmmo();
        }
        else
        {
            AudioManager.PlayNormalShotAudio();
            BulletManager.GetNormalBulletPool().Spawn(projSpawner.transform.position, projSpawner.transform.rotation);
        }

        yield return new WaitForSeconds(0.1f);
    }

    public void getCollectible(CollectibleType type)
    {
        switch (type)
        {
            case CollectibleType.HeavyMachineGun:
                if (!haveMachineGun)
                {
                    GameManager.SetHeavyMachineAmmo(120);
                    UIManager.UpdateAmmoUI();
                    haveMachineGun = true;
                }
                else
                {
                    GameManager.RechargAmmoMG();
                }
                break;
            case CollectibleType.MedKit:
                health.increaseHealth();
                break;
            case CollectibleType.Ammo:
                GameManager.AddAmmo();

                if (!haveMachineGun)
                {
                    GameManager.SetHeavyMachineAmmo(0);
                    UIManager.UpdateAmmoUI();
                }
                break;
            default:
                Debug.Log("Collectible not found");
                break;
        }
    }

    public void CheckHeavyAmmo()
    {
        if (GameManager.GetHeavyMachineAmmo() <= 0 && haveMachineGun)
        {
            haveMachineGun = false;
            UIManager.UpdateAmmoUI();
        }
    }

    private RaycastHit2D MeleeRay()
    {
        Vector2 startPos = transform.position;
        
        float distance = meleeDistance;
        LayerMask layerMask = GameManager.GetEnemyLayer();
        Vector2 direction = (facingRight) ? transform.right : -transform.right;
        Vector2 endPos = startPos + (distance * direction);
        Debug.DrawLine(startPos, endPos, Color.red, 5f);
        return Physics2D.Raycast(startPos, direction, distance, layerMask);
    }

    private bool CanMelee()
    {
        RaycastHit2D hit = MeleeRay();
        return (hit && hit.collider != null);
    }

    private IEnumerator WaitMelee()
    {
        yield return new WaitForSeconds(0.1f);
        RaycastHit2D hit = MeleeRay();
        if (hit && hit.collider != null)
        {
            hit.collider?.GetComponent<Health>()?.Hit(damageMelee);
            AudioManager.PlayMeleeHitAudio();
        }
        yield return new WaitForSeconds(0.2f);
    }
}
