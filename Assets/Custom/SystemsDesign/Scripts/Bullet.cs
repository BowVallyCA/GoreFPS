using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float timerMax = 5f;

    private float timerCurrent;

    private void Awake()
    {
        timerCurrent = timerMax;
    }

    private void Update()
    {
        timerCurrent -= Time.deltaTime;

        if (timerCurrent <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
