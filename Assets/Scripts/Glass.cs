using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f; // How far to check for NPCs
    [SerializeField] private LayerMask npcLayer; // Layer containing NPCs
    private AudioSource audioSource;

    private bool isBroken;

    private BoxCollider boxCollider;
    private MeshRenderer meshRenderer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        boxCollider = GetComponent<BoxCollider>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + gameObject.name);
        }
    }


    // private void OnControllerColliderHit(ControllerColliderHit hit)
    // {
    //     // Debug.Log("");
    //     Debug.Log("Is collided bro");
    //     if (hit.gameObject.CompareTag("Player") && !isBroken)
    //     {
    //         if (audioSource != null)
    //         {
    //             Debug.Log($"Mute: {audioSource.mute}");
    //             audioSource.PlayOneShot(audioSource.clip); // Use PlayOneShot instead of Play()
    //             Debug.Log($"Audio is playing: {audioSource.isPlaying}");
    //             // gameObject.SetActive(false);
    //             meshRenderer.enabled = false;
    //             isBroken = true;
    //             boxCollider.enabled = false;
    //             // box
    //         }
    //         AlertNPCs();
    //     }

    // }

    // void OnCollisionEnter(Collision collision)
    // {
    //     Debug.Log("Is collided bro");
    //     if (collision.gameObject.CompareTag("Player") && !isBroken)
    //     {
    //         if (audioSource != null)
    //         {
    //             Debug.Log($"Mute: {audioSource.mute}");
    //             audioSource.PlayOneShot(audioSource.clip); // Use PlayOneShot instead of Play()
    //             Debug.Log($"Audio is playing: {audioSource.isPlaying}");
    //             // gameObject.SetActive(false);
    //             meshRenderer.enabled = false;
    //             isBroken = true;
    //             boxCollider.enabled = false;
    //             // box
    //         }
    //         AlertNPCs();
    //     }
    // }

    public void TriggerGlassBreakage()
    {
        if (audioSource != null)
        {
            Debug.Log($"Mute: {audioSource.mute}");

            audioSource.PlayOneShot(audioSource.clip); // Use PlayOneShot instead of Play()

            Debug.Log($"Audio is playing: {audioSource.isPlaying}");

            // gameObject.SetActive(false);

            meshRenderer.enabled = false;
            isBroken = true;
            boxCollider.enabled = false;
            // box
        }
        AlertNPCs();

    }

    private IEnumerator DisableAfterSound()
    {
        // Wait for the audio clip to finish
        if (audioSource != null && audioSource.clip != null)
        {
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        gameObject.SetActive(false);
    }

    private void AlertNPCs()
    {
        // Find all NPCs in range
        Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, detectionRadius, npcLayer);

        // Set destination for each NPC
        foreach (Collider npc in nearbyNPCs)
        {
            Enemy enemy = npc.GetComponent<Enemy>();
            if (enemy != null && enemy.IsProvokedByGlass)
            {
                enemy.SetTargetPosition(transform.position);
            }
        }

    }

    // Optional: Visualize the detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}