#if UNITY_EDITOR
using UnityEngine;

public class GizmoMesh : MonoBehaviour
{
    [SerializeField]
    private Mesh mesh = null;

    private void OnDrawGizmosSelected()
    {
        if (mesh != null)
        {
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}
#endif
