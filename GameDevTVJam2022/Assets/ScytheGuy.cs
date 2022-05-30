using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScytheGuy : MonoBehaviour
{
    [SerializeField]Vector2 visionZone;
    [SerializeField] GameObject decapitateZone;
    [SerializeField] LayerMask playerLayer;
    PlayerMovement player;
    Animator myAnimator;
    AudioSource myAS;
    // Start is called before the first frame update
    void Start()
    {
        myAnimator = GetComponent<Animator>();
        player = FindObjectOfType<PlayerMovement>();
        myAS = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player.myState == PlayerMovement.PlayerState.headless)
        { return; }
        if(Physics2D.OverlapBox(transform.position, visionZone, 0, playerLayer))
        {
            myAnimator.Play("ScytheGuyAttack");
        }
    }
    public void ToggleDecapitator()
    {
        if(decapitateZone.activeInHierarchy)
        { decapitateZone.SetActive(false); }
        else
        {
            myAS.Play();
            decapitateZone.SetActive(true); 
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, visionZone);
    }
}
