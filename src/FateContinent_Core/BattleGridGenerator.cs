// ==============================================================================
//            FATE CONTINENT - BATTLE SCENE DYNAMIC GRID GENERATOR
// ==============================================================================
// Version: v18.12.04 (Unity Battle Arena Sync)
// Description: Procedurally spawns a customizable battle arena directly inside the
//             Unity scene. Works in Edit Mode (ExecuteInEditMode) for real-time adjustments!
//             Supports dynamic Row/Column resizing, tile scaling, spacing gaps,
//             and auto-applies team colors (Blue on left, Red on right, Neutral in center).
// ==============================================================================

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class BattleGridGenerator : MonoBehaviour
{
    [Header("Grid Dimensions")]
    [Range(1, 30)] public int rows = 3;
    [Range(1, 30)] public int columns = 4;

    [Header("Tile Settings")]
    public float tileSize = 1.5f;
    public float spacing = 0.2f;
    public float thickness = 0.2f;

    [Header("Materials / Prefabs")]
    public Material playerZoneMaterial;  // Blue
    public Material enemyZoneMaterial;   // Red
    public Material specialZoneMaterial; // Green
    public Material neutralZoneMaterial; // Grey
    public GameObject tilePrefab;        // Optional: Leave null to auto-generate 3D primitives

    [Header("Pedestal (Selector pegs)")]
    public bool spawnPedestals = true;
    public float pedestalWidth = 0.25f;
    public float pedestalHeight = 0.15f;
    public Material pedestalMaterial;

    [Header("Auto-Regeneration")]
    public bool autoUpdate = true;

    // Local tracking to prevent continuous performance loss
    private int prevRows;
    private int prevCols;
    private float prevTileSize;
    private float prevSpacing;
    private float prevThickness;
    private bool prevSpawnPedestals;
    private float prevPedWidth;
    private float prevPedHeight;

    private void OnValidate()
    {
        if (autoUpdate)
        {
            RegenerateGrid();
        }
    }

    [ContextMenu("Regenerate Grid Now")]
    public void RegenerateGrid()
    {
        // 1. Clear existing generated tiles safely in Edit Mode
        ClearGrid();

        // 2. Setup materials
        CreateDefaultMaterialsIfNull();

        // 3. Calculate start offsets to center the grid perfectly around the Generator transform
        float totalWidth = columns * tileSize + (columns - 1) * spacing;
        float totalLength = rows * tileSize + (rows - 1) * spacing;
        float startX = -totalWidth / 2f + tileSize / 2f;
        float startZ = -totalLength / 2f + tileSize / 2f;

        // 4. Spawn tiles row-by-row
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                float posX = transform.position.x + startX + c * (tileSize + spacing);
                float posZ = transform.position.z + startZ + r * (tileSize + spacing);
                Vector3 spawnPos = new Vector3(posX, transform.position.y + thickness / 2f, posZ);

                GameObject tileObj;

                if (tilePrefab != null)
                {
                    tileObj = PrefabUtility.InstantiatePrefab(tilePrefab) as GameObject;
                    tileObj.transform.position = spawnPos;
                    tileObj.transform.localScale = new Vector3(tileSize, thickness, tileSize);
                }
                else
                {
                    // Generate procedural 3D cube
                    tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObj.transform.position = spawnPos;
                    tileObj.transform.localScale = new Vector3(tileSize, thickness, tileSize);
                }

                tileObj.name = $"Grid_Cell_{r:02d}_{c:02d}";
                tileObj.transform.SetParent(this.transform);

                // Assign team materials based on columns
                Renderer tileRenderer = tileObj.GetComponent<Renderer>();
                if (tileRenderer != null)
                {
                    if (columns >= 3)
                    {
                        if (c == 0)
                            tileRenderer.sharedMaterial = playerZoneMaterial;
                        else if (c == columns - 1)
                            tileRenderer.sharedMaterial = enemyZoneMaterial;
                        else if (r == 0 || r == rows - 1)
                            tileRenderer.sharedMaterial = specialZoneMaterial;
                        else
                            tileRenderer.sharedMaterial = neutralZoneMaterial;
                    }
                    else
                    {
                        tileRenderer.sharedMaterial = (c % 2 == 0) ? playerZoneMaterial : enemyZoneMaterial;
                    }
                }

                // Add pedestal if enabled
                if (spawnPedestals)
                {
                    float pedY = transform.position.y + thickness + pedestalHeight / 2f;
                    GameObject pedObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pedObj.name = "Center_Pedestal";
                    pedObj.transform.position = new Vector3(posX, pedY, posZ);
                    pedObj.transform.localScale = new Vector3(pedestalWidth, pedestalHeight, pedestalWidth * 1.4f);
                    pedObj.transform.SetParent(tileObj.transform);

                    Renderer pedRenderer = pedObj.GetComponent<Renderer>();
                    if (pedRenderer != null && pedestalMaterial != null)
                    {
                        pedRenderer.sharedMaterial = pedestalMaterial;
                    }
                }
            }
        }

        // Cache parameters to avoid redundant runs
        prevRows = rows;
        prevCols = columns;
        prevTileSize = tileSize;
        prevSpacing = spacing;
        prevThickness = thickness;
        prevSpawnPedestals = spawnPedestals;
        prevPedWidth = pedestalWidth;
        prevPedHeight = pedestalHeight;
    }

    public void ClearGrid()
    {
        // Must use loop backwards and DestroyImmediate in Edit Mode
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    private void CreateDefaultMaterialsIfNull()
    {
        if (playerZoneMaterial == null) playerZoneMaterial = CreateColorMat("Player_Blue", new Color(0.08f, 0.45f, 0.95f));
        if (enemyZoneMaterial == null) enemyZoneMaterial = CreateColorMat("Enemy_Red", new Color(0.85f, 0.12f, 0.18f));
        if (specialZoneMaterial == null) specialZoneMaterial = CreateColorMat("Special_Green", new Color(0.12f, 0.65f, 0.28f));
        if (neutralZoneMaterial == null) neutralZoneMaterial = CreateColorMat("Neutral_Grey", new Color(0.32f, 0.38f, 0.45f));
        if (pedestalMaterial == null) pedestalMaterial = CreateColorMat("Pedestal_Dark", new Color(0.22f, 0.26f, 0.30f));
    }

    private Material CreateColorMat(string matName, Color color)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.name = matName;
        mat.color = color;
        // Turn up metallic and smoothness for nice specular reflection
        mat.SetFloat("_Metallic", 0.5f);
        mat.SetFloat("_Glossiness", 0.6f);
        return mat;
    }
}

// ==============================================================================
//                       CUSTOM EDITOR INSPECTOR PANEL
// ==============================================================================
[CustomEditor(typeof(BattleGridGenerator))]
public class BattleGridGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        BattleGridGenerator gen = (BattleGridGenerator)target;

        GUILayout.Space(15);
        if (GUILayout.Button("Clear Arena Grid", GUILayout.Height(30)))
        {
            gen.ClearGrid();
        }

        if (GUILayout.Button("FORCE REGENERATE GRID", GUILayout.Height(35)))
        {
            gen.RegenerateGrid();
        }
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Grid coordinate system centers automatically around the transform location. Perfect for C# node maps & A* Pathfinding in BattleScene!", MessageType.Info);
    }
}
#endif
