using System;
using System.IO;
using PackageAuthor.Dialogs;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PackageAuthor.Displays
{
    internal sealed class CreateFileDisplay: IPackageAuthorDisplay
    {
        private PackageAuthor _authorWindow;

        private bool _createEditorFolder = true;
        private bool _createRuntimeFolder = true;
        private bool _createTestsFolder = false;

        private string _authorName; 
        private string _authorURL; 
        private string _authorEMail;

        private string _targetJsonPath;

        public CreateFileDisplay(PackageAuthor packageAuthorWindow, string packageJsonPath)
        {
            this._authorWindow = packageAuthorWindow;
            
            this._authorName = EditorPrefs.HasKey(PackageAuthorContants.AUTHOR_NAME_CONFIG_NAME) 
                ? EditorPrefs.GetString(PackageAuthorContants.AUTHOR_NAME_CONFIG_NAME) 
                : "";
            this._authorURL = EditorPrefs.HasKey(PackageAuthorContants.AUTHOR_URL_CONFIG_NAME) 
                ? EditorPrefs.GetString(PackageAuthorContants.AUTHOR_URL_CONFIG_NAME) 
                : "";
            this._authorEMail = EditorPrefs.HasKey(PackageAuthorContants.AUTHOR_EMAIL_CONFIG_NAME) 
                ? EditorPrefs.GetString(PackageAuthorContants.AUTHOR_EMAIL_CONFIG_NAME)
                : "";

            this._targetJsonPath = packageJsonPath;
        }
        
        public void OnGUI()
        {
            GUILayout.Space(10);

            GUILayout.Label("Current Path:");
            EditorGUILayout.LabelField(this._authorWindow.CurrentPath, EditorStyles.boldLabel);
            
            EditorGUIExt.HorizontalLine();

            EditorGUILayout.LabelField("Created File: " + this._targetJsonPath, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Create folders:", EditorStyles.boldLabel);

            this._createRuntimeFolder = EditorGUILayout.Toggle("Runtime folder", this._createRuntimeFolder);
            this._createEditorFolder = EditorGUILayout.Toggle("Editor folder", this._createEditorFolder);
            this._createTestsFolder = EditorGUILayout.Toggle("Test folders", this._createTestsFolder);
            
            EditorGUIExt.HorizontalLine();
            
            if (GUILayout.Button("Create Package"))
            {
                this.CreatePackage();
            }
            
            GUILayout.Space(50);

            EditorGUIExt.HeaderField("Author");
            this._authorName = EditorGUILayout.TextField("Name", this._authorName);
            this._authorEMail = EditorGUILayout.TextField("E-Mail", this._authorEMail);
            this._authorURL = EditorGUILayout.TextField("URL", this._authorURL);
        }

        public void OnHeader()
        { }

        private void CreatePackage()
        {
            if (!SetCompany.HasSetCompany())
            {
                SetCompany dialog = SetCompany.ShowWindow();
                if (!dialog.Completed)
                {
                    return;
                }
            }
            
            EditorPrefs.SetString(PackageAuthorContants.AUTHOR_NAME_CONFIG_NAME, this._authorName);
            EditorPrefs.SetString(PackageAuthorContants.AUTHOR_URL_CONFIG_NAME, this._authorURL);
            EditorPrefs.SetString(PackageAuthorContants.AUTHOR_EMAIL_CONFIG_NAME, this._authorEMail);

            string prefix = EditorPrefs.GetString(PackageAuthorContants.PREFIX_CONFIG_NAME);
            string company = EditorPrefs.GetString(PackageAuthorContants.COMPANY_CONFIG_NAME);
            string packagename = Path.GetFileName(this._authorWindow.CurrentPath).ToLower();

            string identifier = $"{prefix}.{company}.{packagename}";
            
            string[] unityVersionSplit = Application.unityVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
            string unityVersion = unityVersionSplit[0] + "." + unityVersionSplit[1]; 

            JObject json = new JObject();
            json.Add("name", identifier);
            json.Add("version", "1.0.0");
            json.Add("unity", unityVersion);
            json.Add("hideInEditor", true);
            json.Add("license", "MIT");
            json.Add("description", "[DESCRIPTION]");
            json.Add("displayName", "[DISPLAYNAME]");
            json.Add("author", JObject.FromObject(
                new {
                    name = this._authorName,
                    url = this._authorURL,
                    email = this._authorEMail
                })
            );
            
            string jsonResult = json.ToString();

            File.WriteAllText(this._targetJsonPath, jsonResult);

            this.CreateFolders();
            
            AssetDatabase.Refresh();
            this._authorWindow.RecheckPath();
        }

        private void CreateFolders()
        {
            if (this._createRuntimeFolder)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this._targetJsonPath) + "/Runtime");
            }
            if (this._createEditorFolder)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this._targetJsonPath) + "/Editor");
            }
            if (this._createTestsFolder)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this._targetJsonPath) + "/Tests");
                if (this._createRuntimeFolder)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this._targetJsonPath) + "/Tests/Runtime");
                }
                if (this._createEditorFolder)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(this._targetJsonPath) + "/Tests/Editor");
                }
            }
        }
    }
}