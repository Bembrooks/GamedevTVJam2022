using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransCanvas : MonoBehaviour
{
    Animator myAnimator;
    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }
    public void TransIn()
    { myAnimator.Play("TransIn"); }
    public void TransOut()
    { myAnimator.Play("TransOut"); }
}
