using TMPro;
using UnityEngine;
using System.Collections;


public class RopeSkipping : MonoBehaviour
{
    public int currentJump;
    public int successJump;
    private int maxJump;
    [SerializeField] private TMP_Text JumpCount;
    [SerializeField] private GameObject PlayerCollider;
    [SerializeField] private Animator Anim;
    public Jumping PlayerJump;

    private float JumpTime;
    private float Jumping;
    public float StartCooldown;
    private bool Jumped;

    [SerializeField] float cooldown;
    private float forcooldown;
    private bool DoneCooldown;

    [SerializeField] private float baseSwing = 2f;
    private float maxSwing;
    private float minSwing;
    private bool Swinged;
    private bool Touched;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxJump = 0;
        currentJump = 0;
        successJump = 0;
        Jumping = 0f;
        Jumped = false;
        DoneCooldown = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Jumped && DoneCooldown)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                PlayerJump.PlayerJumping();
                Jumping = 0f;
                Jumped = true;
                DoneCooldown = false;
            }
        }

        if (Jumped)
        {
            Jumping += Time.deltaTime;

            if (StartCooldown != 0f)
            {
                JumpTime = StartCooldown * 0.8f;
            }

            if (Jumping < JumpTime)
            {
                PlayerCollider.SetActive(false);
            }

            if (Jumping > JumpTime)
            {
                PlayerCollider.SetActive(true);
            }

            if (Jumping > StartCooldown)
            {
                CanJumpCool();
                Jumped = false; 
            }
        }

        if (!DoneCooldown && Jumped == false)
        {
            forcooldown += Time.deltaTime;
            if (forcooldown > cooldown)
            {
                DoneCooldown = true;
            }
        }

        if (Swinged)
        {
            CheckSuccess();
        }

        if (currentJump < maxJump)
        {
            RopeSwingAnim();
        }
    }

    void CanJumpCool()
    {
        forcooldown = 0f;
    }

    void RopeSwingAnim()
    {
        StartCoroutine(PlaySwingDown());
    }

    IEnumerator PlaySwingDown()
    {
        float randomSwing = Random.Range(minSwing, maxSwing);

        Anim.speed = baseSwing / randomSwing;
        Anim.Play("MyAnimation");

        yield return new WaitForSeconds(baseSwing * randomSwing);

        Swinged = true;
        currentJump += 1;
    }

    void CheckSuccess()
    {
        if (Touched)
        {
            Swinged = false;
            return;
        }
        else if (!Touched)
        {
            Swinged = false;
            successJump += 1;
        }
    }
 
}
