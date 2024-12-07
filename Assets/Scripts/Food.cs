using UnityEngine;

public class Food : MonoBehaviour
{
    private PlayerFatness playerFatness;

    private void Start()
    {
        playerFatness = FindFirstObjectByType<PlayerFatness>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("You ate some food!");
            gameObject.SetActive(false);
            playerFatness.RaiseFatnessLevel();
        }
    }
}