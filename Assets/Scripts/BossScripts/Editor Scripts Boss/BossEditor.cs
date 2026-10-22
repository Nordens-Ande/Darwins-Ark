using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TextCore.Text;

[CustomEditor(typeof(Boss))]
public class BossEditor : Editor
{
    private ReorderableList statesList;

    private void OnEnable()
    {
        if (target == null || serializedObject == null)
            return;

        SerializedProperty statesListProperty =
            serializedObject.FindProperty("AIStates");

        statesList = new ReorderableList(
            serializedObject,
            statesListProperty,
            true,  
            true,  
            true, 
            true 
        );

        statesList.drawHeaderCallback = rect =>
        {
            EditorGUI.LabelField(rect, "AIStates");
        };

        statesList.elementHeightCallback = index =>
        {
            SerializedProperty element =
                statesListProperty.GetArrayElementAtIndex(index);

            return EditorGUI.GetPropertyHeight(element, true) + 4f;
        };

        statesList.drawElementCallback =
            (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element =
                    statesListProperty.GetArrayElementAtIndex(index);

                rect.y += 2f;
                rect.height =
                    EditorGUI.GetPropertyHeight(element, true);

                EditorGUI.PropertyField(
                    rect,
                    element,
                    new GUIContent(
                        element.managedReferenceValue == null
                            ? "Missing / no state"
                            : element.managedReferenceValue.GetType().Name
                    ),
                    true
                );
            };

        statesList.onAddDropdownCallback = (rect, list) =>
        {
            ShowStatesMenu(statesListProperty);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawPropertiesExcluding(
            serializedObject,
            "m_Script",
            "AIStates"
        );

        EditorGUILayout.Space();

        statesList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowStatesMenu(SerializedProperty listProperty)
    {
        var menu = new GenericMenu();

        var aiStates = TypeCache
            .GetTypesDerivedFrom<BossAIState>()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsGenericType &&
                type.IsClass 
            )
            .OrderBy(type => type.Name);

        foreach (Type type in aiStates)
        {
            Type selectedType = type;

            menu.AddItem(
                new GUIContent(selectedType.Name),
                false,
                () => AddState(listProperty, selectedType)
            );
        }

        if (menu.GetItemCount() == 0)
        {
            menu.AddDisabledItem(
                new GUIContent("No valid BossAIState subclasses found")
            );
        }

        menu.DropDown(
            new Rect(
                Event.current.mousePosition,
                Vector2.zero
            )
        );
    }

    //private void AddState(
    //    SerializedProperty listProperty,
    //    Type aiState)
    //{
    //    serializedObject.Update();

    //    int index = listProperty.arraySize;
    //    listProperty.arraySize++;

    //    SerializedProperty element =
    //        listProperty.GetArrayElementAtIndex(index);

    //    Boss boss = (Boss)target;

    //    element.managedReferenceValue =
    //        Activator.CreateInstance(aiState, boss);

    //    serializedObject.ApplyModifiedProperties();
    //}

    private void AddState(
    SerializedProperty listProperty,
    Type aiState)
    {
        Boss boss = (Boss)target;

        var constructor = aiState.GetConstructor(
            new[] { typeof(Boss) }
        );

        if (constructor == null)
        {
            Debug.LogError(
                $"{aiState.Name} has no public constructor accepting Boss."
            );
            return;
        }

        object instance = constructor.Invoke(new object[] { boss });

        serializedObject.Update();

        int index = listProperty.arraySize;
        listProperty.arraySize++;

        SerializedProperty element =
            listProperty.GetArrayElementAtIndex(index);

        element.managedReferenceValue = instance;

        serializedObject.ApplyModifiedProperties();
    }
}
