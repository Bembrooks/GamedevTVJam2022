using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public bool activeCheckpoint;
    Animator myAnimator;
    [SerializeField] Checkpoint[] checkPoints;
    AudioSource myAS;
    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        checkPoints = FindObjectsOfType<Checkpoint>();
        myAS = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Head"))
        {
            if(activeCheckpoint)
            { return; }
            foreach(Checkpoint checkpoint in checkPoints)
            {
                if(checkpoint.activeCheckpoint && checkpoint != this)
                { checkpoint.DeactivateCheckpoint(); }
            }
            ActivateCheckpoint();
        }
    }
    void ActivateCheckpoint()
    {
        activeCheckpoint = true;
        myAS.Play();
        myAnimator.Play("CheckPointActivate");
    }
    public void DeactivateCheckpoint()
    {
        activeCheckpoint = false;
        myAnimator.Play("CheckPointDeactivate");
    }
}
