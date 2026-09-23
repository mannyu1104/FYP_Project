using UnityEngine;

public class RopeDetector : MonoBehaviour
{
    [SerializeField] private RopeSkipping RopeMecha;
    public RopeSkipping RopeSuccessCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RopeSuccessCount = RopeMecha.GetComponent<RopeSkipping>();
    }

    //// Update is called once per frame
    //void Update()
    //{

    //}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        RopeSuccessCount.Touching();
    }
}
