using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crusher : MonoBehaviour
{
    bool thwomp;
    // Start is called before the first frame update
    void Start()
    {
        if(GetComponent<Thwomp>() != null)
        { thwomp = true; }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if (thwomp && GetComponent<Thwomp>().squishing == false)
            { return; }    
            collision.GetComponent<PlayerMovement>().CrushMe(); 
            if(thwomp)
            { GetComponent<Thwomp>().slammedDownTimer = 0; }
        }
    }
}
