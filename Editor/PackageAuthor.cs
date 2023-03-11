using System.IO;
using System.Reflection;
using PackageAuthor.Displays;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GUILayout;

namespace PackageAuthor
{
    public class PackageAuthor : UnityEditor.EditorWindow
    {
        public string CurrentPath => this._currentPath;
        
        private string _currentPath;
        private IPackageAuthorDisplay _currentDisplay = null;

        private bool _locked = false;
        
        [UnityEditor.MenuItem("Tools/Package Author")]
        private static void ShowWindow()
        {
            PackageAuthor window = EditorWindow.GetWindow<PackageAuthor>();

            window.titleContent = new UnityEngine.GUIContent("Package Author");
            window.Show();
        }

        private void OnGUI()
        {
            this.HandlePathChange();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload", GUILayout.MaxWidth(50)))
            {
                this.RecheckPath();
            }

            string lockButtonText = _locked ? "Unlock" : "Lock";
            if (GUILayout.Button(lockButtonText, GUILayout.MaxWidth(75)))
            {
                this._locked = !this._locked;
            }
            
            this._currentDisplay.OnHeader();
            
            EditorGUILayout.EndHorizontal();

            this._currentDisplay.OnGUI();
        }

        private void HandlePathChange(bool force = false)
        {
            string path = this.GetCurrentPath();
            if (this._locked)
            {
                path = this._currentPath;
            }
            
            if (!force && path == this._currentPath && this._currentDisplay != null)
            {
                return;
            }

            this._currentPath = path;
            string packageJSONPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..",  this._currentPath, "package.json"));

            if (!File.Exists(packageJSONPath))
            {
                this._currentDisplay = new CreateFileDisplay(this, packageJSONPath);
                return;
            }

            this._currentDisplay = new EditFileDisplay(this, packageJSONPath);
        }

        public void RecheckPath()
        {
            this.HandlePathChange(true);
        }
        
        private string GetCurrentPath()
        {
            MethodInfo tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );
            if (tryGetActiveFolderPath == null)
            {
                return "Assets";
            }
            
            object[] args = { null };
            bool found = (bool)tryGetActiveFolderPath.Invoke( null, args );

            if (!found)
            {
                return "Assets";
            }
            return (string)args[0];
        }
    }
}