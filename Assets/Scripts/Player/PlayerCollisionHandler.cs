using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    }
}
