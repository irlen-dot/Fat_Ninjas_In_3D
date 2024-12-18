using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private LayerMask npcLayer;
    [SerializeField] private bool showDebugInfo = true;

    // Add this line instead:
    [SerializeField] private List<Transform> targetPositions = new List<Transform>();

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
            Debug.LogWarning($"No AudioSource found on {gameObject.name}");
        }

        if (npcLayer.value == 0)
        {
            Debug.LogError($"NPC Layer not set on {gameObject.name}. NPCs won't be detected!");
        }

        if (targetPositions.Count == 0)
        {
            Debug.LogWarning($"No Target Positions set on {gameObject.name}. Adding glass position as default target.");
            targetPositions.Add(transform);
        }
    }

    private void AlertNPCs()
    {
        // Use glass position for detection
        Vector3 detectionPoint = transform.position;
        Collider[] nearbyNPCs = Physics.OverlapSphere(detectionPoint, detectionRadius, npcLayer);

        if (showDebugInfo)
        {
            Debug.Log($"Found {nearbyNPCs.Length} objects in OverlapSphere");
        }

        foreach (Collider npc in nearbyNPCs)
        {
            Enemy enemy = npc.GetComponent<Enemy>();
            if (enemy != null && enemy.IsProvokedByGlass)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"Setting investigation points for {npc.gameObject.name}");
                }
                enemy.SetInvestigationPoints(targetPositions);
            }
        }
    }

    public void TriggerGlassBreakage()
    {
        if (isBroken) return;

        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        meshRenderer.enabled = false;
        isBroken = true;
        boxCollider.enabled = false;

        AlertNPCs();
        StartCoroutine(DisableAfterSound());
    }

    private IEnumerator DisableAfterSound()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            yield return new WaitForSeconds(audioSource.clip.length);
        }
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        // Draw detection sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Draw target positions and their connections
        if (targetPositions != null && targetPositions.Count > 0)
        {
            for (int i = 0; i < targetPositions.Count; i++)
            {
                if (targetPositions[i] == null) continue;

                // Draw point
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPositions[i].position, 0.3f);

                // Draw number
                UnityEditor.Handles.Label(targetPositions[i].position + Vector3.up, i.ToString());

                // Draw line to next point
                if (i < targetPositions.Count - 1 && targetPositions[i + 1] != null)
                {
                    Gizmos.DrawLine(targetPositions[i].position, targetPositions[i + 1].position);
                }
            }
        }

        // Draw debug lines in play mode
        if (showDebugInfo && Application.isPlaying)
        {
            Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, detectionRadius, npcLayer);
            foreach (Collider col in nearbyNPCs)
            {
                // Line from glass to NPC
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, col.transform.position);
            }
        }
    }
}