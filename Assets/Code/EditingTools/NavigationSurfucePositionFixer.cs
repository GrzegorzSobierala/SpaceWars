using NavMeshPlus.Components;
using UnityEngine;

namespace Game.Tools
{
    [RequireComponent(typeof(NavMeshSurface)), ExecuteInEditMode]
    public class NavigationSurfucePositionFixer : MonoBehaviour
    {
        NavMeshSurface _navMeshSurface;

        private void Awake()
        {
            _navMeshSurface = GetComponent<NavMeshSurface>();
        }

        public void SetPositionBeforeBake()
        {
            transform.position = Vector3.zero;
        }

        public void SetPositionAfterBake()
        {
            transform.position = new Vector3(0.0f, 0.0f, _navMeshSurface.voxelSize/2);
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(NavigationSurfucePositionFixer))]
    public class NavigationSurfucePositionFixerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            NavigationSurfucePositionFixer colliderEditor = (NavigationSurfucePositionFixer)target;

            if (GUILayout.Button("Set Position Before Bake"))
            {
                colliderEditor.SetPositionBeforeBake();
            }

            if (GUILayout.Button("Set Position After Bake"))
            {
                colliderEditor.SetPositionAfterBake();
            }
        }
    }
#endif

}
