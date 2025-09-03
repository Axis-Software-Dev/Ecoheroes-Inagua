using UnityEngine;

public class RandomCarsMove : MonoBehaviour
{
    Animator anim;
    float randomOffset;
    // Start is called once before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        randomOffset = Random.Range(0f, 1f);
        anim.Play("Armature|JustForward", 0, randomOffset);
    }

    
}
