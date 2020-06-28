using UnityEngine;

public class GizmoMesh : MonoBehaviour
{
    [SerializeField]
    private Mesh mesh = null;

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (mesh != null)
        {
            if (gameObject.activeSelf == true)
            {
                Gizmos.DrawMesh(mesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }
    }
#endif
}
