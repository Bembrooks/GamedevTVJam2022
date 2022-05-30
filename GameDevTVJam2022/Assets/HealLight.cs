using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealLight : MonoBehaviour
{
    PlayerMovement player;
    AudioSource myAS;
    private void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        myAS = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            if(player.myState != PlayerMovement.PlayerState.walking)
            {
                collision.GetComponent<PlayerMovement>().ChangePlayerState(PlayerMovement.PlayerState.walking);
                myAS.Play();
            }
            
        }
    }
}
