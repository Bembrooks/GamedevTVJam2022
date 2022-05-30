using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndBrain : MonoBehaviour
{
    TransCanvas transCanvas;
    bool collected;
    // Start is called before the first frame update
    void Start()
    {
        transCanvas = FindObjectOfType<TransCanvas>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(collected == true)
            { return; }
            collected = true;
            transCanvas.TransOut();
            FindObjectOfType<PlayerMovement>().gameEnd = true;
            FindObjectOfType<EndScreen>().EndScreenEnter();
        }
    }
}
