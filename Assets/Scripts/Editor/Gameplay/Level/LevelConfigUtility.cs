using System.Collections;
using System.Collections.Generic;
using System.IO;
using Bejeweled;
using UnityEngine;
using UnityEditor;

namespace Bejeweled
{
    /// <summary>
    /// Level config utility for editor interface.
    /// </summary>
    public static class LevelConfigUtility
    {
        [MenuItem("Assets/Create/Bejeweled/Level Config")]
        public static void CreateLevelConfig()
        {
            CreateAsset<LevelConfig>();
        }

        /// <summary>
        ///	Create bew Scriptable Object in Assets directory.
        /// </summary>
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "") {
                path = "Assets";
            } else if (Path.GetExtension(path) != "") {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName =
                AssetDatabase.GenerateUniqueAssetPath(path + "/New " + typeof(T).ToString() + ".asset");

            AssetDatabase.CreateAsset(asset, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
