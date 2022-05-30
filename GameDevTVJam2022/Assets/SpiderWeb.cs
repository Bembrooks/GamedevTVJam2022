using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [SerializeField] Vector2 spotToBurn;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] ParticleSystem flameParticles;
    [SerializeField] bool burnAway;
    PlayerMovement player;
    Animator myAnimator;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerMovement>();
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics2D.OverlapBox(transform.position, spotToBurn, 0, playerLayer) && player.myState == PlayerMovement.PlayerState.flaming)
        {
            if(burnAway == false)
            {
                burnAway = true;
                flameParticles.Play();
                myAnimator.Play("WebBurn");
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, spotToBurn);
    }
}
