using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System.Diagnostics;
using UnityEditor.Build.Reporting;

namespace EAS
{
    public class EASBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            UnityEngine.Debug.Log("[EAS BUILD] OnPreprocessBuild started");
            Stopwatch stopwatch = Stopwatch.StartNew();

            GenerateEASRuntimeData();

            stopwatch.Stop();
            UnityEngine.Debug.Log($"[EAS BUILD] OnPreprocessBuild finished in {stopwatch.ElapsedMilliseconds} ms");
        }

        protected void GenerateEASRuntimeData()
        {
            string[] guids = AssetDatabase.FindAssets("t:prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject gameObject = PrefabUtility.LoadPrefabContents(path);
                if (gameObject != null)
                {
                    try
                    {
                        EASBaseController[] controllersInPrefab = gameObject.GetComponentsInChildren<EASBaseController>();
                        if (controllersInPrefab.Length > 0)
                        {
                            foreach (EASBaseController controller in controllersInPrefab)
                            {
                                if (PrefabUtility.IsPartOfAnyPrefab(controller))
                                {
                                    SerializedObject serializedObject = new SerializedObject(controller);
                                    SerializedProperty serializedProperty = serializedObject.FindProperty("m_RuntimeData");

                                    PrefabUtility.RevertPropertyOverride(serializedProperty, InteractionMode.AutomatedAction);
                                }

                                controller.GenerateRuntimeData();
                                EditorUtility.SetDirty(controller);
                            }

                            EditorUtility.SetDirty(gameObject);
                            PrefabUtility.SaveAsPrefabAsset(gameObject, path);
                        }
                    }
                    catch
                    {
                        UnityEngine.Debug.LogError($"Error generating EASRuntimeData for {path}");
                    }

                    PrefabUtility.UnloadPrefabContents(gameObject);
                }
            }
        }
    }
}
