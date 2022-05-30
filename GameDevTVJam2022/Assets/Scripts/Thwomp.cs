using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thwomp : MonoBehaviour
{
    PlayerMovement player;
    [SerializeField] float HeightTillSquash;
    float bottomHeight;
    float topHeight;
    [SerializeField] float moveUpSpeed;
    public bool squishing;
    [SerializeField] float moveDownSpeedFactor;
    Animator myAnimator;
    [SerializeField] Vector2 PlayerCheckDims;
    [SerializeField] float playerCheckOffset;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] ParticleSystem stompParticles;
    bool slammed;
    AudioSource myAS;
    [Header("Delays")]
    [SerializeField] float slammedDownDelay;
    public float slammedDownTimer;

    // Start is called before the first frame update
    void Start()
    {
        bottomHeight = transform.position.y;
        topHeight = bottomHeight + HeightTillSquash;
        myAnimator = GetComponent<Animator>();
        slammedDownTimer = slammedDownDelay;
        player = FindObjectOfType<PlayerMovement>();
        myAnimator.Play("ThwompFloat");
        myAS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!squishing)
        {
            if (transform.position.y < topHeight)
            { transform.Translate(Vector2.up * Time.deltaTime * moveUpSpeed); }
            else 
            {
                if(Physics2D.OverlapBox(new Vector2(transform.position.x, transform.position.y - playerCheckOffset),PlayerCheckDims,0,playerLayer)
                    && player.myState != PlayerMovement.PlayerState.flat)
                {
                    myAnimator.Play("ThwompIdle");
                    squishing = true;
                }
            }
        }
        else
        {
            if (transform.position.y > bottomHeight)
            { transform.Translate(Vector2.down * Time.deltaTime * (moveUpSpeed * moveDownSpeedFactor)); }
            else
            {
                if (slammed == false)
                { 
                    slammed = true;
                    stompParticles.Play();
                    myAS.Play();
                }
                slammedDownTimer -= Time.deltaTime;
                if(slammedDownTimer <= 0)
                {
                    slammedDownTimer = slammedDownDelay;
                    squishing = false;
                    myAnimator.Play("ThwompFloat");
                    slammed = false;
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.up * HeightTillSquash);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector2(transform.position.x, transform.position.y - playerCheckOffset), PlayerCheckDims);
    }
}
