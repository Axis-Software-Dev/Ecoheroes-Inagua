using UnityEngine;

public class RandomCarsRight : MonoBehaviour
{
    Animator anim;
    float randomOffset;
    // Start is called once before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        randomOffset = Random.Range(0f, 1f);
        anim.Play("Armature|Turning_Right", 0, randomOffset);
    }

    
}
