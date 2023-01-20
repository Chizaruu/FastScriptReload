using System;
using System.Collections.Generic;
using FastScriptReload.Scripts.Runtime;
using HarmonyLib;
using ImmersiveVRTools.Editor.Common.Utilities;
using ImmersiveVrToolsCommon.Runtime.Logging;
using UnityEditor;
using UnityEngine;

namespace FastScriptReload.Editor.NewFields
{
    [InitializeOnLoad]
    public class NewFieldsRendererDefaultEditorPatch
    {
        private static List<string> _cachedKeys = new List<string>();
        
        static NewFieldsRendererDefaultEditorPatch()
        {
            var harmony = new Harmony(nameof(NewFieldsRendererDefaultEditorPatch));
            
            var original =  AccessTools.Method("UnityEditor.GenericInspector:OnOptimizedInspectorGUI");
            var prefix = AccessTools.Method(typeof(NewFieldsRendererDefaultEditorPatch), nameof(OnOptimizedInspectorGUI));
            
            harmony.Patch(original, postfix: new HarmonyMethod(prefix));
        }

        private static void OnOptimizedInspectorGUI(Rect contentRect, UnityEditor.Editor __instance)
        {
            //TODO: perf optimize, this will be used for many types, perhaps keep which types changed and just pass type?
            if (__instance.target)
            {
                if (TemporaryNewFieldValues.TryGetDynamicallyAddedFieldValues(__instance.target, out var addedFieldValues))
                {
                    EditorGUILayout.Space(10);
                    
                    EditorGUILayout.BeginHorizontal(); 
                    EditorGUILayout.LabelField("[FSR] Dynamically Added Fields:");
                    GuiTooltipHelper.AddHelperTooltip("Fields were dynamically added for hot-reload, you can adjust their values and on full reload they'll disappear from this section and move back to main one.");
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(5); 

                    try
                    {
                        _cachedKeys.AddRange(addedFieldValues.Keys);
                        
                        foreach (var addedFieldValueKey in _cachedKeys)
                        {
                            var existingValueType = addedFieldValues[addedFieldValueKey].GetType();

                            //rendering types come from UnityEditor.EditorGUI.DefaultPropertyField - that should handle all cases that editor can render
                            if (existingValueType == typeof(int)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.IntField(addedFieldValueKey, (int)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(bool)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Toggle(addedFieldValueKey, (bool)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(float)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.FloatField(addedFieldValueKey, (float)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(string)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.TextField(addedFieldValueKey, (string)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Color)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.ColorField(addedFieldValueKey, (Color)addedFieldValues[addedFieldValueKey]);
                            //TODO: how to handle SerializedPropertyType.ObjectReference? //initially disallow?
                            // else if (existingValueType == typeof(object)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Toggle(addedFieldValueKey, (bool)addedFieldValues[addedFieldValueKey]);
                            //TODO: SerializedPropertyType.LayerMask
                            else if (existingValueType == typeof(Enum)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.EnumPopup(addedFieldValueKey, (Enum)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Vector2)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Vector2Field(addedFieldValueKey, (Vector2)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Vector3)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Vector3Field(addedFieldValueKey, (Vector3)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Vector4)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Vector4Field(addedFieldValueKey, (Vector4)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Rect)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.RectField(addedFieldValueKey, (Rect)addedFieldValues[addedFieldValueKey]);
                            //TODO: SerializedPropertyType.ArraySize
                            //TODO: SerializedPropertyType.Character
                            // else if (existingValueType == typeof(char)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.TextField(addedFieldValueKey, (char)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(AnimationCurve)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.CurveField(addedFieldValueKey, (AnimationCurve)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Bounds)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.BoundsField(addedFieldValueKey, (Bounds)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Gradient)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.GradientField(addedFieldValueKey, (Gradient)addedFieldValues[addedFieldValueKey]);
                            //TODO: SerializedPropertyType.FixedBufferSize
                            else if (existingValueType == typeof(Vector2Int)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Vector2IntField(addedFieldValueKey, (Vector2Int)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(Vector3Int)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.Vector3IntField(addedFieldValueKey, (Vector3Int)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(RectInt)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.RectIntField(addedFieldValueKey, (RectInt)addedFieldValues[addedFieldValueKey]);
                            else if (existingValueType == typeof(BoundsInt)) addedFieldValues[addedFieldValueKey] = EditorGUILayout.BoundsIntField(addedFieldValueKey, (BoundsInt)addedFieldValues[addedFieldValueKey]);
                            //TODO: SerializedPropertyType.Hash128
                            else if (existingValueType == typeof(Quaternion))
                            {
                                //TODO: handled bit differently via EditorGUI.QuaternionEulerField(position, property, label);
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{existingValueType.Name} - Unable to render");
                                GuiTooltipHelper.AddHelperTooltip("Unable to handle added-field rendering for type: {existingValueType.Name}, it won't be rendered. Best workaround for now is to use Vector3 instead.");
                                EditorGUILayout.EndHorizontal();
                            }
                            else
                            {
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField($"{existingValueType.Name} - Unable to render");
                                GuiTooltipHelper.AddHelperTooltip("Unable to handle added-field rendering for type: {existingValueType.Name}, it won't be rendered. Best workaround is to not add this type dynamically in current version.");
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                    finally
                    {
                        _cachedKeys.Clear();
                    }
                }
            }
        }
    }
}