// [FATE CONTINENT - UNITY EDITOR AUTOMATION v18.12.06]
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class FateMixamoExporter : EditorWindow
{
    [MenuItem("Assets/Fate Tools/Export Clean OBJ for Mixamo", false, 10)]
    public static void ExportSelectedFBXToCleanOBJ()
    {
        // Получаем выделенный объект в окне Project
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, выделите импортированный FBX персонажа в окне Project!", "OK");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedObject);
        if (string.IsNullOrEmpty(assetPath) || !assetPath.ToLower().EndsWith(".fbx"))
        {
            EditorUtility.DisplayDialog("Ошибка", "Выбранный объект должен быть FBX файлом!", "OK");
            return;
        }

        // Пытаемся получить MeshFilter или SkinnedMeshRenderer
        Mesh mesh = null;
        MeshFilter meshFilter = selectedObject.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null)
        {
            mesh = meshFilter.sharedMesh;
        }
        else
        {
            SkinnedMeshRenderer skinnedRenderer = selectedObject.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedRenderer != null)
            {
                mesh = skinnedRenderer.sharedMesh;
            }
        }

        if (mesh == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Внутри выделенного FBX не найден компонент Mesh!", "OK");
            return;
        }

        // Создаем диалоговое окно сохранения файла
        string defaultName = selectedObject.name + "_Clean_For_Mixamo";
        string savePath = EditorUtility.SaveFilePanel("Сохранить очищенный OBJ для Mixamo", "", defaultName, "obj");

        if (string.IsNullOrEmpty(savePath)) return;

        // Конвертируем меш в стандартный формат Wavefront OBJ без костей
        string objData = MeshToOBJString(mesh, selectedObject.name);
        // КРИТИЧЕСКИ ВАЖНО: Заменяем Windows-переносы строк на Unix-переносы строк (\n), чтобы Mixamo корректно считывал файл
        objData = objData.Replace("\r\n", "\n");
        // КРИТИЧЕСКИ ВАЖНО: Пишем строго в UTF-8 БЕЗ сигнатуры BOM (Byte Order Mark, сигнатура EF BB BF в начале), 
        // иначе авто-риггер Mixamo считает заголовок невалидным бинарным файлом и выдает ошибку "Unexpected File Type"!
        File.WriteAllText(savePath, objData, new UTF8Encoding(false));

        EditorUtility.DisplayDialog("Успех!", $"Чистый файл меша успешно экспортирован!\nСкелет удален.\n\nПуть: {savePath}", "Ура!");
    }

    private static string MeshToOBJString(Mesh mesh, string name)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"# Fate Continent Clean Mesh Exporter v18.12.06");
        sb.AppendLine($"# Object Name: {name}");
        sb.AppendLine($"g {name}");

        // Записываем вершины
        foreach (Vector3 vertex in mesh.vertices)
        {
            // Инвертируем X координату для соответствия систем координат Unity и Blender/Mixamo
            sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "v {0:F6} {1:F6} {2:F6}", -vertex.x, vertex.y, vertex.z));
        }
        sb.AppendLine();

        // Записываем текстурные координаты (UV)
        if (mesh.uv != null && mesh.uv.Length > 0)
        {
            foreach (Vector2 uv in mesh.uv)
            {
                sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "vt {0:F6} {1:F6}", uv.x, uv.y));
            }
            sb.AppendLine();
        }

        // Записываем нормали
        if (mesh.normals != null && mesh.normals.Length > 0)
        {
            foreach (Vector3 normal in mesh.normals)
            {
                sb.AppendLine(string.Format(System.Globalization.CultureInfo.InvariantCulture, "vn {0:F6} {1:F6} {2:F6}", -normal.x, normal.y, normal.z));
            }
            sb.AppendLine();
        }

        // Записываем грани (полигоны)
        for (int materialIndex = 0; materialIndex < mesh.subMeshCount; materialIndex++)
        {
            int[] triangles = mesh.GetTriangles(materialIndex);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // OBJ-индексы начинаются с 1
                int v1 = triangles[i] + 1;
                int v2 = triangles[i + 1] + 1;
                int v3 = triangles[i + 2] + 1;

                if (mesh.uv != null && mesh.uv.Length > 0 && mesh.normals != null && mesh.normals.Length > 0)
                {
                    sb.AppendLine($"f {v3}/{v3}/{v3} {v2}/{v2}/{v2} {v1}/{v1}/{v1}");
                }
                else if (mesh.uv != null && mesh.uv.Length > 0)
                {
                    sb.AppendLine($"f {v3}/{v3} {v2}/{v2} {v1}/{v1}");
                }
                else
                {
                    sb.AppendLine($"f {v3} {v2} {v1}");
                }
            }
        }

        return sb.ToString();
    }
}
#endif
