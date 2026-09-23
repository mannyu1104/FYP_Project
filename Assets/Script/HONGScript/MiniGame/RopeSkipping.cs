using TMPro;
using UnityEngine;
using System.Collections;


public class RopeSkipping : MonoBehaviour
{
    public event System.Action<bool> MatchFinished;
    private bool finished;
    public int currentJump;
    public int successJump;
    private int maxJump = 20;
    [SerializeField] private GameObject Camera;
    [SerializeField] private GameObject Player;
    [SerializeField] private TMP_Text JumpCount;
    [SerializeField] private TMP_Text SuccessCount;
    [SerializeField] private GameObject PlayerCollider;
    [SerializeField] private Animator Anim;
    [SerializeField] private TMP_Text CountDownText;
    [SerializeField] private GameObject ShowCount;
    public Jumping PlayerJump;

    private float JumpTime;
    private float Jumping;
    public float StartCooldown;
    private bool Jumped;

    [SerializeField] float cooldown;
    private float forcooldown;
    private bool DoneCooldown;
    public float CountDown;

    [SerializeField] private float baseSwing = 3.666667f;
    private float maxSwing = 0.9f;
    private float minSwing = 1.8f;
    private bool Swinged;
    public bool Touched;
    public bool playSkip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentJump = 0;
        successJump = 0;
        CountDown = 3f;
        Jumping = 0f;
        Jumped = false;
        DoneCooldown = true;
        playSkip = false;
        Touched = false;
        SuccessCount.text = successJump.ToString();
        JumpCount.text = currentJump.ToString();

        StartGame();
    }

    public void StartGame()
    {
        StartCoroutine(CountDownPlay());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!Jumped && DoneCooldown && playSkip)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                PlayerJump.PlayerJumping();
                Jumping = 0f;
                Jumped = true;
                DoneCooldown = false;
            }
        }

        if (Jumped && playSkip)
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

        if (!DoneCooldown && Jumped == false && playSkip)
        {
            forcooldown += Time.deltaTime;
            if (forcooldown > cooldown)
            {
                DoneCooldown = true;
            }
        }

        if (playSkip && !Swinged && currentJump < maxJump)
        {
            Swinged = true;
            RopeSwingAnim();
        }

        if (playSkip && currentJump >= maxJump && !finished)
        {
            finished = true;
            playSkip = false;
            MatchFinished?.Invoke(successJump >= 10);
        }
    }


    void CheckSuccess()
    {
        if (Touched)
        {
            Touched = false;
            Swinged = false;
            SuccessCount.text = successJump.ToString();
        }
        else if (!Touched)
        {
            successJump += 1;
            Touched = false;
            Swinged = false;
            SuccessCount.text = successJump.ToString();
        }
    }

    void CanJumpCool()
    {
        forcooldown = 0f;
    }

    void RopeSwingAnim()
    {
        StartCoroutine(PlaySwingUp());
    }

    IEnumerator PlaySwingUp()
    {
        float randomSwing = Random.Range(maxSwing, minSwing);

        Anim.speed = 1.0f * randomSwing;
        Anim.Play("SwingUp", 0, 0f);

        Debug.Log(randomSwing);

        yield return new WaitForSeconds(baseSwing / randomSwing);

        currentJump += 1;
        JumpCount.text = currentJump.ToString();
        CheckSuccess();
    }

    IEnumerator CountDownPlay()
    {
        ShowCount.SetActive(true);
        CountDownText.text = CountDown.ToString();
        yield return new WaitForSeconds(1f);
        CountDown -= 1f;
        CountDownText.text = CountDown.ToString();
        yield return new WaitForSeconds(1f);
        CountDown -= 1f;
        CountDownText.text = CountDown.ToString();
        yield return new WaitForSeconds(1f);
        CountDownText.text = "Start !!!!";
        yield return new WaitForSeconds(0.8f);

        playSkip = true;
        ShowCount.SetActive(false);
        // Keep the authored camera framing; jumping must not move or detach the camera.
    }

    public void Touching()
    {
        Touched = true;
    }


}
