using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    private bool underTrapLight = false;
    public bool UnderTrapLight { get { return underTrapLight; } }


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


    void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Trap Light"))
        {
            underTrapLight = true;
            Debug.Log($"You are under the light: {underTrapLight}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Trap Light"))
        {
            underTrapLight = false;
        }
    }
}
