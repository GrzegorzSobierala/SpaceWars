using NaughtyAttributes.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class SpaceWarsInspector : NaughtyInspector
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            TryAutoFill();
        }

        private void TryAutoFill()
        {
            if (target is not MonoBehaviour monoBehaviour)
                return;

            FieldInfo[] fields = monoBehaviour.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (FieldInfo field in fields)
            {

                if (field.GetCustomAttribute<AutoFillAttribute>() == null)
                    continue;

                if (!field.FieldType.IsSubclassOf(typeof(Component)))
                {
                    Debug.LogError($" Field: {field.Name} is not subclass od Component", monoBehaviour);
                    continue;
                }

                if (field.GetCustomAttribute<SerializeField>() == null && !field.IsPublic)
                {
                    Debug.LogError($" Field: {field.Name} has not SerializeField or is not public"
                        , monoBehaviour);

                    continue;
                }

                object ob = field.GetValue(monoBehaviour);

                if (ob != null)
                    continue;

                Component component = monoBehaviour.GetComponent(field.FieldType);

                if (component == null)
                {
                    component = monoBehaviour.gameObject.AddComponent(field.FieldType);
                }

                field.SetValue(monoBehaviour, component);
                EditorUtility.SetDirty(monoBehaviour);
            }
        }
    }
}
