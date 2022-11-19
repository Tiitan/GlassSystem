using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Swing : MonoBehaviour
{
    private Rigidbody _rigidBody = new Rigidbody();

    public Vector3 Force;
    public float Frequency = 1;
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        _rigidBody.AddForce(Force * Mathf.Cos(Frequency * Time.time));
    }
}
