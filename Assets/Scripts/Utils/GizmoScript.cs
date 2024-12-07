using UnityEngine;

public class GizmoScript : MonoBehaviour
{
   [SerializeField] private float gizmoRadius = 2f;

   void OnDrawGizmos()
   {
       Gizmos.color = Color.yellow;
       Gizmos.DrawWireSphere(transform.position, gizmoRadius); 
   }
}