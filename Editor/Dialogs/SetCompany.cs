using UnityEditor;
using UnityEngine;

namespace PackageAuthor.Dialogs
{
    public class SetCompany : EditorWindow
    {
        
        
        public bool Completed { get; private set; } = false;

        private string _companyName;
        private string _prefix = "com";
        
        public static SetCompany ShowWindow()
        {
            var window = GetWindow<SetCompany>();

            if (EditorPrefs.HasKey(PackageAuthorContants.PREFIX_CONFIG_NAME))
            {
                window._prefix = EditorPrefs.GetString(PackageAuthorContants.PREFIX_CONFIG_NAME);
            }
            if (EditorPrefs.HasKey(PackageAuthorContants.COMPANY_CONFIG_NAME))
            {
                window._companyName = EditorPrefs.GetString(PackageAuthorContants.COMPANY_CONFIG_NAME);
            }
            window.titleContent = new GUIContent("Set company dialog");
            window.ShowModal();

            return window;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Please set your company.");
            this._prefix = EditorGUILayout.TextField("Prefix", this._prefix);
            this._companyName = EditorGUILayout.TextField("Company", this._companyName);
            EditorGUILayout.LabelField("Example: " + this._prefix + "." + this._companyName + ".examplepackage");

            if (GUILayout.Button("Accept"))
            {
                Completed = true;
                
                EditorPrefs.SetString(PackageAuthorContants.PREFIX_CONFIG_NAME, this._prefix);
                EditorPrefs.SetString(PackageAuthorContants.COMPANY_CONFIG_NAME, this._companyName);
                
                this.Close();
            }
        }

        public static bool HasSetCompany()
        {
            return EditorPrefs.HasKey(PackageAuthorContants.PREFIX_CONFIG_NAME) &&
                   EditorPrefs.HasKey(PackageAuthorContants.COMPANY_CONFIG_NAME);
        }
    }
}
