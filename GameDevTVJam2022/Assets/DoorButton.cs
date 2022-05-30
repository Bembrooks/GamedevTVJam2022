using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [SerializeField] Vector2 buttonPressDims;
    [SerializeField] LayerMask playerLayer;
    [SerializeField] Door myDoor;
    bool buttonPressed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Physics2D.OverlapBox(transform.position, buttonPressDims, 0, playerLayer) && !buttonPressed)
        {
            buttonPressed = true;
            myDoor.DoorOpen();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, buttonPressDims);
    }
}
