using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

namespace AI_Assistant_Quantum
{
    public class UnityConnector : EditorWindow
    {
        private string prompt = "";
        private string serverUrl = "http://localhost:3000";
        private Mode mode = Mode.Online;
        private string status = "Ready for Quantum Manifestation";
        private bool isProcessing = false;

        public enum Mode { Online, Offline, NoInternet }

        [MenuItem("AI Assistant/Quantum Singularity Window")]
        public static void ShowWindow()
        {
            GetWindow<UnityConnector>("AI Assistant Quantum");
        }

        private void OnGUI()
        {
            GUILayout.Label("v16.92.0 - Massive Knowledge Expansion (v2)", EditorStyles.boldLabel);
            
            serverUrl = EditorGUILayout.TextField("Server URL", serverUrl);
            mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);

            EditorGUILayout.Space();

            GUILayout.Label("What should I manifest in Unity?");
            prompt = EditorGUILayout.TextArea(prompt, GUILayout.Height(100));

            if (GUILayout.Button("Manifest Code") && !isProcessing)
            {
                if (!string.IsNullOrEmpty(prompt))
                {
                    EditorCoroutineRunner.StartCoroutine(SendRequest());
                }
            }

            EditorGUILayout.Space();
            GUILayout.Label("Status: " + status, EditorStyles.helpBox);
            
            GUILayout.FlexibleSpace();
            GUILayout.Label("Eternal Origin - Unity Bridge Active", EditorStyles.miniLabel);
        }

        private IEnumerator SendRequest()
        {
            isProcessing = true;
            status = "Sending query to Quantum Nexus...";
            
            string url = serverUrl.TrimEnd('/') + "/api/blender/chat"; // Reusing the same logic for simplicity

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("prompt", prompt);
            data.Add("mode", mode.ToString().ToLower());
            
            Dictionary<string, object> context = new Dictionary<string, object>();
            context.Add("unity_version", Application.unityVersion);
            context.Add("platform", Application.platform.ToString());
            data.Add("context", context);

            string json = JsonUtility.ToJson(new RequestData { 
                prompt = prompt, 
                mode = mode.ToString().ToLower(), 
                target = "unity" 
            });

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    status = "Error: " + request.error;
                    Debug.LogError("[AI ASSISTANT] Manifestation failed: " + request.error);
                }
                else
                {
                    string responseBody = request.downloadHandler.text;
                    ResponseData response = JsonUtility.FromJson<ResponseData>(responseBody);

                    if (!string.IsNullOrEmpty(response.code))
                    {
                        status = "Code Manifested. See Console.";
                        Debug.Log("[AI ASSISTANT] Generated Code:\n" + response.code);
                        // Execution in Unity Editor is risky, usually we'd save to a file or use EditorScripts.
                        // For now, we log it. In a real scenario, we could use eval/reflection or save to Assets/AI_Generated.cs
                    }
                    else if (!string.IsNullOrEmpty(response.error))
                    {
                        status = "AI Error: " + response.error;
                    }
                }
            }

            isProcessing = false;
        }

        [System.Serializable]
        public class RequestData
        {
            public string prompt;
            public string mode;
            public string target;
        }

        [System.Serializable]
        public class ResponseData
        {
            public string code;
            public string error;
        }
    }

    // Static helper to run coroutines in Editor
    public static class EditorCoroutineRunner
    {
        public static void StartCoroutine(IEnumerator routine)
        {
            EditorApplication.update += () => 
            {
                if (!routine.MoveNext())
                {
                    EditorApplication.update -= null; // Need better way to stop, but this is simple demo
                }
            };
        }
    }
}
