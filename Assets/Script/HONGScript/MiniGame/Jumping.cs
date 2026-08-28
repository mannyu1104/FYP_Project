using UnityEngine;

public class Jumping : MonoBehaviour
{
    public RopeSkipping Skippingfunction;
    public float JumpTime;
    public bool JumpingTime;


    [SerializeField] private Rigidbody rb;
    public float jumpforce = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        JumpTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (JumpingTime)
        {
            JumpTime += Time.deltaTime;
        }
        else if (!JumpingTime)
        {
            GetTime();
        }
    }

    public void PlayerJumping()
    {
        rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
        if (JumpTime == 0f)
        {
            JumpingTime = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (JumpingTime)
        {
            JumpingTime = false;
        }
    }

    public void GetTime()
    {
        Skippingfunction.StartCooldown = JumpTime;
    }
}
