using UnityEngine;

public class ZeppelinSpeed : MonoBehaviour
{
    Animator anim;
    void Awake()
    {
        anim = GetComponent<Animator>();

        ChangeAnimationSpeed();
    }
    void ChangeAnimationSpeed()
    {
        anim.speed = 0.5f;
    }
        
}

