using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCollisionHandler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Debug.Log("Is hit bro");
        if (hit.gameObject.tag.Equals("Glass"))
        {
            hit.gameObject.GetComponent<Glass>().TriggerGlassBreakage();
        }
        if (hit.gameObject.tag.Equals(""))
        {

        }
    }


    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Bro is triggered {other.gameObject.tag}");
    }
}
