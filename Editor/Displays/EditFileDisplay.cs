using System.Collections.Generic;
using System.IO;
using PackageAuthor.Values;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace PackageAuthor.Displays
{
    internal sealed class EditFileDisplay : IPackageAuthorDisplay
    {
        internal class EditorField
        {
            public string Name;
            public string Value;
            public string TargetKey;

            public EditorField(string name, string targetKey)
            {
                this.Name = name;
                this.TargetKey = targetKey;
            }
        }

        private Dictionary<string, List<EditorField>> _editorFields = new Dictionary<string, List<EditorField>>()
        {
            {
                "Essentials", 
                new List<EditorField>()
                {
                    new EditorField("Identifier", "name"),
                    new EditorField("Package Version", "version"),
                    new EditorField("Unity Version", "unity"),
                    new EditorField("Unity Release", "unityRelease")
                }
            },
            {
                "General", 
                new List<EditorField>()
                {
                    new EditorField("Display Name", "displayName"),
                    new EditorField("Description", "description"),
                    new EditorField("Changelog URL", "changelogURL"),
                    new EditorField("Documentation URL", "documentationURL"),
                    new EditorField("Licence", "license"),
                    new EditorField("Licence URL", "licenseURL")
                }
            },
            {
                "Author",
                new List<EditorField>()
                {
                    new EditorField("Name", "author.name"),
                    new EditorField("E-Mail", "author.email"),
                    new EditorField("URL", "author.url"),
                }
            }
        };
        
        private string _jsonPath;

        private bool _hideInEditor;
        private List<string> _keywords = new List<string>();
        private IList<Dependency> _dependencies = new List<Dependency>();
        private IList<Sample> _samples = new List<Sample>();

        private PackageAuthor _authorWindow;
        private Vector2 _scrollPos = Vector2.zero;
        private bool _keywordListFoldout = true;
        private bool _dependencyListFoldout;
        private bool _sampleListFoldout;

        public EditFileDisplay(
            PackageAuthor author, string jsonPath
        )
        {
            this._jsonPath = jsonPath;
            this._authorWindow = author;
            this.SetFields();
        }



        public void OnHeader()
        {
            EditorGUILayout.LabelField("");
            if (GUILayout.Button("Apply", GUILayout.MaxWidth(100)))
            {
                this.ApplyChanges();
            }
        }

        public void OnGUI()
        {
            
            this._scrollPos = EditorGUILayout.BeginScrollView(this._scrollPos);

            foreach (KeyValuePair<string,List<EditorField>> keyValue in this._editorFields)
            {
                EditorGUIExt.HeaderField(keyValue.Key);

                foreach (EditorField editorField in keyValue.Value)
                {
                    editorField.Value = EditorGUILayout.TextField(editorField.Name, editorField.Value);
                }
            }
            
            EditorGUIExt.HeaderField("Additionals");
            this._hideInEditor = EditorGUILayout.Toggle("Hide in Editor", this._hideInEditor);
            
            GUIStyle keywordStyle = new GUIStyle(EditorStyles.foldoutHeader)
            {
                fontStyle = FontStyle.Normal
            };

            this._keywordListFoldout = EditorListDisplay.Display(
                this._keywordListFoldout,
                "Keywords",
                keywordStyle,
                this._keywords,
                s => {
                    this._keywords[s] = EditorGUILayout.TextField(this._keywords[s]);
                }, 
                () => this._keywords.Add(""), 
                s => this._keywords.Remove(s), 
                () => this._authorWindow.Repaint()
            );
            
            _dependencyListFoldout = EditorListDisplay.Display(
                _dependencyListFoldout, 
                "Dependencies", 
                _dependencies, 
                DependencyDisplay, 
                () => this._dependencies.Add(new Dependency()), 
                dependency => _dependencies.Remove(dependency), 
                () => this._authorWindow.Repaint()
            );
            
            _sampleListFoldout = EditorListDisplay.Display(
                _sampleListFoldout, 
                "Samples", 
                _samples,
                i => {
                    Sample sample = _samples[i];
                    EditorGUILayout.LabelField("Sample "+ (i+1) + (!string.IsNullOrEmpty(sample.DisplayName) ? " - " + sample.DisplayName : ""), EditorStyles.boldLabel);
                    EditorGUI.indentLevel++;
                    sample.DisplayName = EditorGUILayout.TextField("Name", sample.DisplayName);
                    EditorGUILayout.LabelField("Description");
                    sample.Description = EditorGUILayout.TextArea(sample.Description, GUILayout.MaxHeight(75));
                    
                    sample.Path = EditorGUILayout.TextField("Path", sample.Path);
                }, 
                () => _samples.Add(new Sample()), 
                sample => _samples.Remove(sample), 
                () => this._authorWindow.Repaint()
            );

            EditorGUILayout.EndScrollView();
        }

        private void DependencyDisplay(int i)
        {
            Dependency dependency = this._dependencies[i];

            EditorGUILayout.LabelField(
                "Dependency " + (i + 1) +
                (!string.IsNullOrEmpty(dependency.PackageName) ? " - " + dependency.PackageName : ""),
                EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUIExt.CheckedObjectField(
                "Package File",
                dependency.PackageFile,
                typeof(TextAsset),
                o => AssetDatabase.GetAssetPath(o).EndsWith("package.json"),
                out dependency.PackageFile
            );
            if (dependency.PackageFile != null && GUILayout.Button("Apply", GUILayout.MaxWidth(50)))
            {
                JObject json = JObject.Parse(((TextAsset)dependency.PackageFile).text);
                dependency.PackageName = json.Value<string>("name");
                dependency.PackageVersion = json.Value<string>("version");
                dependency.PackageFile = null;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);


            dependency.PackageName = EditorGUILayout.TextField("Package Identifier", dependency.PackageName);
            dependency.PackageVersion = EditorGUILayout.TextField("Package Version", dependency.PackageVersion);
        }

        private void SetFields()
        {
            JObject jsonObject = JObject.Parse(File.ReadAllText(this._jsonPath));
            
            foreach (KeyValuePair<string,List<EditorField>> v in this._editorFields)
                foreach (EditorField field in v.Value)
                {
                    string val = "";
                    
                    JToken token = jsonObject.SelectToken(field.TargetKey);
                    if (token != null)
                    {
                        val = token.ToString();
                    }

                    field.Value = val;
                }

            this._hideInEditor = true;
            if (jsonObject.ContainsKey("hideInEditor"))
            {
                this._hideInEditor = jsonObject.Value<bool>("hideInEditor");
            }
        }

        private void ApplyChanges()
        {
            JObject json = new JObject();

            foreach (KeyValuePair<string, List<EditorField>> pair in this._editorFields)
            {
                string name = "";
                JObject intermediaryObject = new JObject();
                foreach (EditorField field in pair.Value)
                {
                    if (string.IsNullOrEmpty(field.Value))
                    {
                        continue;
                    }
                    
                    if (field.TargetKey.Contains("."))
                    {
                        string[] splitkeys = field.TargetKey.Split(new []{ '.' }, 2);
                        name = splitkeys[0];
                        intermediaryObject.Add(splitkeys[1], field.Value);
                        continue;
                    }

                    json.Add(field.TargetKey, field.Value);
                }

                if (intermediaryObject.HasValues)
                {
                    json.Add(name, intermediaryObject);
                }
            }

            json.Add("hideInEditor", this._hideInEditor);

            if (this._keywords.Count > 0)
            {
                JArray keywordList = new JArray(this._keywords);
                json.Add("keywords", keywordList);
            }

            if (this._dependencies.Count > 0)
            {
                JObject dependencies = new JObject();
                foreach (Dependency dependency in this._dependencies)
                {
                    dependencies.Add(dependency.PackageName, dependency.PackageVersion);
                }

                json.Add("dependencies", dependencies);
            }

            if (this._samples.Count > 0)
            {
                JArray samples = new JArray();
                foreach (Sample sample in this._samples)
                {
                    samples.Add(JObject.FromObject(sample));
                }

                json.Add("samples", samples);
            }

            string jsonResult = json.ToString();
            
            File.WriteAllText(this._jsonPath, jsonResult);
            AssetDatabase.Refresh();
        }
    }
}
