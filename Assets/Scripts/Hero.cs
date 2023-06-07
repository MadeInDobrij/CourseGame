using System;
using System.Collections;
//using System.Collections.Generic;
//using System.Management.Instrumentation;
//using System.Runtime.InteropServices;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Hero : Entity
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int health;
    [SerializeField] private float jumpforce = 15f;
    private bool isGrounded = false;

    [SerializeField] private Image[] hearts;

    [SerializeField] private Sprite aliveHeart;
    [SerializeField] private Sprite deadHeart;

    public bool isAttacking = false;
    public bool isRecharged = true;

    public Transform attackPos;
    public float attackRange;
    public LayerMask enemy;
    public Joystick joystick;



    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;

    public static Hero Instance { get; set; }

    private States State
    {
        get { return (States)anim.GetInteger("State"); }
        set { anim.SetInteger("State", (int)value); }
    }

    public void Attack()
    {
        if (isGrounded && isRecharged)
        {
            State = States.Attack;
            isAttacking = true;
            isRecharged = false;

            StartCoroutine(AttackAnimation());
            StartCoroutine(AttackCoolDown());
        }
    }

    private void OnAttack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemy);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].GetComponent<Entity>().GetDamage();
        }
    }

    private void Awake()
    {
        base.health = 5;
        health = base.health;
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        isRecharged = true;
        
    }

    private void FixedUpdate()
    {
        CheckGround();
        //if (isAttacking)
            //if (!isGrounded) State = States.jump;
    }

    private void Update()
    {
        
        if (isGrounded && !isAttacking) State = States.Idle;
        if (!isAttacking && joystick.Horizontal != 0)
            Run();
        if (!isAttacking && isGrounded && joystick.Vertical > 0.5f)
            Jump();

        if (health > base.health)
            health = base.health;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
                hearts[i].sprite = aliveHeart;
            else
                hearts[i].sprite = deadHeart;

            if (i < base.health)
                hearts[i].enabled = true;
            else
                hearts[i].enabled = false;
        }

        if (transform.position.y < -6f)
            GetDamage();
        
    }

    private void Run()
    {
        if (isGrounded) State = States.Run;

        Vector3 dir = transform.right * joystick.Horizontal;
        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);
        sprite.flipX = dir.x < 0.0f;
    }

    private void Jump()
    {
        rb.velocity = Vector2.up * jumpforce;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }

    private IEnumerator AttackAnimation()
    {
        yield return new WaitForSeconds(0.6f);
        isAttacking = false;
    }

    private IEnumerator AttackCoolDown()
    {
        yield return new WaitForSeconds(0.5f);
        isRecharged = true;
    }

    private void CheckGround()
    {

        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, 1f);
        isGrounded = collider.Length > 1;
        if (!isAttacking)
            if (!isGrounded) State = States.Jump;
    }

    public override void GetDamage()
    {
        health -= 1;
        if (health == 0)
        {
            foreach (var h in hearts)
                h.sprite = deadHeart;
            Die();
            if (health == 0)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        
    }

    public void ExitMain()
    {
        SceneManager.LoadScene(0);
    }

}


public enum States
{
    Idle,
    Run,
    Jump,
    Attack
}
