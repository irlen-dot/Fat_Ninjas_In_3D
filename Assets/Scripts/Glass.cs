using System.Collections;
using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f; // How far to check for NPCs
    [SerializeField] private LayerMask npcLayer; // Layer containing NPCs
    private AudioSource audioSource;

    private bool isBroken;

    private MeshRenderer meshRenderer;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        meshRenderer = GetComponent<MeshRenderer>();
        if (audioSource == null)
        {
            Debug.LogWarning("No AudioSource found on " + gameObject.name);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isBroken)
        {
            if (audioSource != null)
            {
                Debug.Log($"Mute: {audioSource.mute}");
                audioSource.PlayOneShot(audioSource.clip); // Use PlayOneShot instead of Play()
                Debug.Log($"Audio is playing: {audioSource.isPlaying}");
                meshRenderer.enabled = false;
                isBroken = true;
            }
            AlertNPCs();
        }
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