using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] Transform subject;
    Vector2 startPos;
    float startPosZ;

    Vector2 travel => (Vector2)cam.transform.position - startPos;
    float distanceFromSubject => transform.position.z - subject.position.z;
    float clippingPlane => cam.transform.position.z + (distanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane);
    float parallaxFactor => Mathf.Abs(distanceFromSubject) / clippingPlane;

    private void Start()
    {
        cam = Camera.main;
        subject = FindObjectOfType<PlayerMovement>().transform;
        startPos = transform.position;
        startPosZ = transform.position.z;
    }
    // Update is called once per frame
    void Update()
    {

        Vector2 newPos = startPos + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startPosZ);
    }
}
