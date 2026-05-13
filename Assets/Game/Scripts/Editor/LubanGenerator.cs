using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace OffTrail.Editor
{
    public class LubanGeneratorWindow : EditorWindow
    {
        private const string LubanDll = "Tools/Luban/Luban/Luban.dll";
        private const string DataTablesRoot = "Tools/Luban/DataTables";
        private const string DefinesDir = "Defines";
        private const string DatasDir = "Datas";

        private Vector2 _scrollPos;

        private static GUIStyle _headerStyle;
        private static GUIStyle _statusBarStyle;

        private static bool? _prerequisitesValid;

        [MenuItem("JulyGF/配置表/生成窗口", priority = 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<LubanGeneratorWindow>("Luban 配置表");
            window.minSize = new Vector2(300, 200);
        }

        [MenuItem("JulyGF/配置表/生成全部", priority = 11)]
        public static void MenuGenerateAll() => GenerateAll();

        private static void EnsureStyles()
        {
            if (_headerStyle != null) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 6, 6),
            };

            _statusBarStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 10,
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(0, 0, 0, 0),
            };
        }

        private void OnGUI()
        {
            EnsureStyles();

            EditorGUILayout.Space(4);
            GUILayout.Label("Luban 配置表生成", _headerStyle);
            DrawSeparator();
            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope())
                {
                    var prevBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.35f, 0.75f, 0.35f, 1f);
                    if (GUILayout.Button("生成全部", GUILayout.Height(32)))
                        GenerateAll();
                    GUI.backgroundColor = prevBg;
                }
                GUILayout.Space(8);
            }

            GUILayout.FlexibleSpace();
            DrawSeparator();
            GUILayout.Label("  配置表路径: Tools/Luban/DataTables", _statusBarStyle);
            EditorGUILayout.Space(2);
        }

        #region Public API

        public static bool GenerateAll()
        {
            if (!CheckPrerequisitesCached()) return false;

            try
            {
                EditorUtility.DisplayProgressBar("Luban", "Generating Common...", 0.5f);
                if (!Generate())
                    return false;
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            Debug.Log("[Luban] 生成完成");
            return true;
        }

        public static bool ValidatePrerequisites()
        {
            var dllPath = Path.Combine(ProjectRoot, LubanDll);
            if (!File.Exists(dllPath))
            {
                Debug.LogError($"[Luban] 找不到 Luban.dll: {dllPath}");
                return false;
            }

            var dotnet = ResolveDotnet();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = dotnet,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                using var p = Process.Start(psi);
                var stdout = p?.StandardOutput.ReadToEnd()?.Trim();
                p?.WaitForExit();
                if (p == null || p.ExitCode != 0)
                {
                    Debug.LogError($"[Luban] dotnet 运行时不可用 (路径: {dotnet})，请安装 .NET SDK");
                    return false;
                }

                Debug.Log($"[Luban] dotnet {stdout} 已就绪 ({dotnet})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Luban] 无法启动 dotnet (路径: {dotnet})，请安装 .NET SDK\n{ex.Message}");
                return false;
            }

            return true;
        }

        #endregion

        #region Internal

        private static bool Generate()
        {
            const string jsonOut = "Assets/Game/Res/Configs";
            const string codeOut = "Assets/Game/Scripts/Generated/Configs";
            const string topModule = "cfg";
            var dataDir = $"{DatasDir}";

            var schemaFiles = BuildSchemaFiles();
            if (schemaFiles == null) return false;

            var confPath = WriteConf(topModule, schemaFiles, dataDir);
            var success = RunLuban(confPath, jsonOut, codeOut);

            if (success)
            {
                var codeOutAbs = Path.GetFullPath(Path.Combine(ProjectRoot, codeOut));
                GenerateTablesExt(codeOutAbs, topModule);
                Debug.Log("[Luban] Common 生成完成");
            }

            return success;
        }

        private static bool RunLuban(string confPath, string jsonOutRelative, string codeOutRelative)
        {
            var dllPath = Path.Combine(ProjectRoot, LubanDll);
            var jsonOutAbs = Path.GetFullPath(Path.Combine(ProjectRoot, jsonOutRelative));
            var codeOutAbs = Path.GetFullPath(Path.Combine(ProjectRoot, codeOutRelative));

            EnsureDirectory(jsonOutAbs);
            EnsureDirectory(codeOutAbs);

            var lubanArgs = $"-t all -d json " +
                            $"--conf \"{confPath}\" " +
                            $"-x outputDataDir=\"{jsonOutAbs}\" " +
                            $"-x outputCodeDir=\"{codeOutAbs}\" " +
                            $"-c cs-simple-json";

            var psi = new ProcessStartInfo
            {
                FileName = ResolveDotnet(),
                Arguments = $"\"{dllPath}\" {lubanArgs}",
                WorkingDirectory = Path.Combine(ProjectRoot, DataTablesRoot),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            try
            {
                using var process = Process.Start(psi);
                if (process == null)
                {
                    Debug.LogError("[Luban] 无法启动 dotnet 进程");
                    CleanTempConf(confPath);
                    return false;
                }

                var stderrTask = process.StandardError.ReadToEndAsync();
                var stdout = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var stderr = stderrTask.Result;

                if (!string.IsNullOrEmpty(stdout))
                    Debug.Log($"[Luban] {stdout.TrimEnd()}");

                if (process.ExitCode != 0)
                {
                    Debug.LogError($"[Luban] 生成失败 (exit {process.ExitCode}):\n{stderr}");
                    return false;
                }

                if (!string.IsNullOrEmpty(stderr))
                    Debug.LogWarning($"[Luban] {stderr.TrimEnd()}");

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Luban] 无法启动进程: {ex.Message}");
                return false;
            }
            finally
            {
                CleanTempConf(confPath);
            }
        }

        private static List<string> BuildSchemaFiles()
        {
            var basePath = Path.Combine(ProjectRoot, DataTablesRoot, DatasDir);

            var tablesFile = Path.Combine(basePath, "__tables__.xlsx");
            if (!File.Exists(tablesFile))
            {
                Debug.LogError($"[Luban] Common 缺少 __tables__.xlsx");
                return null;
            }

            var files = new List<string> { DefinesDir, $"{DatasDir}/__tables__.xlsx" };

            var beansFile = Path.Combine(basePath, "__beans__.xlsx");
            if (File.Exists(beansFile))
                files.Add($"{DatasDir}/__beans__.xlsx");

            var enumsFile = Path.Combine(basePath, "__enums__.xlsx");
            if (File.Exists(enumsFile))
                files.Add($"{DatasDir}/__enums__.xlsx");

            return files;
        }

        private static string WriteConf(string topModule, List<string> schemaFiles, string dataDir)
        {
            var sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine("  \"groups\": [");
            sb.AppendLine("    {\"names\":[\"c\"], \"default\":true},");
            sb.AppendLine("    {\"names\":[\"s\"], \"default\":true},");
            sb.AppendLine("    {\"names\":[\"e\"], \"default\":true}");
            sb.AppendLine("  ],");
            sb.AppendLine("  \"schemaFiles\": [");
            for (var i = 0; i < schemaFiles.Count; i++)
            {
                var f = schemaFiles[i];
                var type = f.EndsWith("__tables__.xlsx") ? "table"
                    : f.EndsWith("__beans__.xlsx") ? "bean"
                    : f.EndsWith("__enums__.xlsx") ? "enum"
                    : "";
                var comma = i < schemaFiles.Count - 1 ? "," : "";
                sb.AppendLine($"    {{\"fileName\":\"{f}\", \"type\":\"{type}\"}}{comma}");
            }
            sb.AppendLine("  ],");
            sb.AppendLine($"  \"dataDir\": \"{dataDir}\",");
            sb.AppendLine("  \"targets\": [");
            sb.AppendLine(
                $"    {{\"name\":\"all\", \"manager\":\"Tables\", \"groups\":[\"c\",\"s\",\"e\"], \"topModule\":\"{topModule}\"}}");
            sb.AppendLine("  ]");
            sb.Append("}");

            var confPath = Path.Combine(ProjectRoot, DataTablesRoot, ".luban_common_temp.conf");
            File.WriteAllText(confPath, sb.ToString());
            return confPath;
        }

        #endregion

        #region TablesExt

        private struct TablePropInfo
        {
            public string TypeName;
            public string PropName;
            public string LoadKey;
        }

        private static List<TablePropInfo> ParseTablesCs(string codeOutAbs)
        {
            var tablesPath = Path.Combine(codeOutAbs, "Tables.cs");
            if (!File.Exists(tablesPath)) return null;

            var content = File.ReadAllText(tablesPath);
            var props = new List<TablePropInfo>();

            var propMatches = Regex.Matches(content, @"public\s+(\w+)\s+(\w+)\s+\{get;");
            var keyMatches = Regex.Matches(content, @"loader\(""(\w+)""\)");

            for (var i = 0; i < propMatches.Count; i++)
            {
                props.Add(new TablePropInfo
                {
                    TypeName = propMatches[i].Groups[1].Value,
                    PropName = propMatches[i].Groups[2].Value,
                    LoadKey = i < keyMatches.Count
                        ? keyMatches[i].Groups[1].Value
                        : propMatches[i].Groups[2].Value.ToLower()
                });
            }

            return props;
        }

        private static void GenerateTablesExt(string codeOutAbs, string topModule)
        {
            var props = ParseTablesCs(codeOutAbs);
            if (props == null || props.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated/>");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine($"namespace {topModule}");
            sb.AppendLine("{");
            sb.AppendLine("    public partial class Tables");
            sb.AppendLine("    {");

            var names = string.Join(", ", props.Select(p => $"\"{p.LoadKey}\""));
            sb.AppendLine($"        public static readonly string[] TableNames = {{ {names} }};");
            sb.AppendLine();

            sb.AppendLine("        public void RegisterTo(Dictionary<Type, object> registry)");
            sb.AppendLine("        {");
            foreach (var p in props)
                sb.AppendLine($"            registry[typeof({p.TypeName})] = {p.PropName};");
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            var outPath = Path.Combine(codeOutAbs, "TablesExt.cs");
            File.WriteAllText(outPath, sb.ToString());
        }

        #endregion

        #region Helpers

        private static bool CheckPrerequisitesCached()
        {
            if (_prerequisitesValid.HasValue)
                return _prerequisitesValid.Value;

            _prerequisitesValid = ValidatePrerequisites();
            return _prerequisitesValid.Value;
        }

        private static void DrawSeparator()
        {
            var rect = GUILayoutUtility.GetRect(0f, 1f, GUILayout.ExpandWidth(true));
            rect.height = 1f;
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 0.6f));
        }

        private static string _resolvedDotnet;

        private static string ResolveDotnet()
        {
            if (_resolvedDotnet != null) return _resolvedDotnet;
            _resolvedDotnet = "dotnet";
            return "dotnet";
        }

        private static void CleanTempConf(string path)
        {
            if (File.Exists(path) && path.Contains("_temp.conf"))
                File.Delete(path);
        }

        private static void EnsureDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
        }

        private static string ProjectRoot => Path.GetDirectoryName(Application.dataPath);

        #endregion
    }
}
