using UnityEngine;

public class carMovementADjust : MonoBehaviour
{
    Animator carAnimator;
    Animator collisionAnimator;
    bool adjustSpeed = false;

    void Start()
    {
        carAnimator = transform.parent.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (adjustSpeed)
        {
            adjustCarSpeed();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            if (other.gameObject.CompareTag("car"))
            {
                collisionAnimator = other.gameObject.GetComponent<Animator>();
                adjustSpeed = true;
                Debug.Log("Collision with: " + other);
            }
            if (other.gameObject.CompareTag("destroy"))
            {
                Destroy(this);
            }

        }
    }
    void adjustCarSpeed()
    {
        float otherCarSpeed = collisionAnimator.speed;
        carAnimator.speed = otherCarSpeed;
        Debug.Log("Speed Change to: " + carAnimator.speed);
    }

}
