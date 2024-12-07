using UnityEngine;

public class EnemyRoute : MonoBehaviour
{
    [SerializeField] private RoutePoint[] routePoints;
    
    [SerializeField] private Color color = Color.red;

    public RoutePoint[] Points => routePoints;

    private void OnDrawGizmos()
    {
        if (routePoints == null || routePoints.Length == 0) return;

        Gizmos.color = color;
        for (int i = 0; i < routePoints.Length; i++)
        {
            if (routePoints[i].point == null) continue;
            
            // Draw spheres at points
            Gizmos.DrawSphere(routePoints[i].point.position, 0.3f);
            
            // Draw lines between points
            if (i + 1 < routePoints.Length && routePoints[i + 1].point != null)
            {
                Gizmos.DrawLine(routePoints[i].point.position, routePoints[i + 1].point.position);
            }
        }
    }
}