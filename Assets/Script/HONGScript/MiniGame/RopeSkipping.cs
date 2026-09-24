using TMPro;
using UnityEngine;
using System.Collections;


public class RopeSkipping : MonoBehaviour
{
    public event System.Action<bool> MatchFinished;
    private bool finished;
    private bool countdownStarted;
    [SerializeField] private GameObject abandonConfirmation;
    private bool confirmingAbandon;
    private float previousTimeScale;

    public void AskAbandon()
    {
        if (finished || confirmingAbandon || abandonConfirmation == null) return;
        previousTimeScale = Time.timeScale;
        confirmingAbandon = true;
        Time.timeScale = 0f;
        abandonConfirmation.SetActive(true);
    }
    public void CancelAbandon()
    {
        if (!confirmingAbandon) return;
        Time.timeScale = previousTimeScale;
        confirmingAbandon = false;
        if (abandonConfirmation != null) abandonConfirmation.SetActive(false);
    }
    public void ConfirmAbandon()
    {
        if (!confirmingAbandon || finished) return;
        CancelAbandon();
        StopAllCoroutines();
        finished = true;
        playSkip = false;
        PrepareToReturn();
        MatchFinished?.Invoke(false);
    }
    // Call before deactivating the match, never during a hierarchy callback.
    private void PrepareToReturn()
    {
        if (Camera == null) return;
        Camera.transform.SetParent(null, true);
        Camera.SetActive(false);
    }

    private void OnDisable()
    {
        CancelAbandon();
        // An attached camera is disabled with its parent automatically.
        // A camera detached on normal completion is already inactive.
    }
    private void OnDestroy()
    {
        if (Camera != null && Camera.transform.parent == null) Destroy(Camera);
    }


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

    [SerializeField] private RectTransform jumpBackground;
    [SerializeField, Min(0f)] private float jumpVisualPixelsPerUnit = 65f;
    private Vector2 backgroundPosition;
    private float backgroundGroundY;
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

        if (jumpBackground == null)
        {
            foreach (var image in transform.root.GetComponentsInChildren<UnityEngine.UI.Image>(true))
                if (image.name == "Image" && image.rectTransform.sizeDelta.x >= 1900)
                { jumpBackground = image.rectTransform; break; }
        }
        if (jumpBackground != null) backgroundPosition = jumpBackground.anchoredPosition;
        if (Player != null) backgroundGroundY = Player.transform.position.y;

        // The child dialogue explains the controls before this match is activated.
        StartGame();
    }

    public void StartGame()
    {
        if (countdownStarted || finished) return;
        countdownStarted = true;
        if (abandonConfirmation != null) abandonConfirmation.SetActive(false);
        ShowCount.SetActive(false);
        playSkip = true;
        // Keep the camera fixed. The UI backdrop provides first-person jump motion.
    }

    void LateUpdate()
    {
        if (jumpBackground != null && Player != null)
            jumpBackground.anchoredPosition = backgroundPosition +
                Vector2.down * Mathf.Max(0f, Player.transform.position.y - backgroundGroundY) * jumpVisualPixelsPerUnit;
    }

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
            PrepareToReturn();
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

    public void Touching()
    {
        Touched = true;
    }


}
