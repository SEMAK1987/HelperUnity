// ==============================================================================
//            FATE CONTINENT - BATTLE GRID INDIVIDUAL NODE (CELL)
// ==============================================================================
// Version: v18.12.04 (Unity BattleScene Arena Sync)
// Description: Attached to each individual exported Blender Grid Cell.
//             Tracks coordinates, highlights states, hover feedback, selection,
//             and stores references to any occupying combat units (heroes or troops).
// ==============================================================================

using UnityEngine;
using System.Collections.Generic;

public enum DeploymentZoneType
{
    None,
    Player, // Blue deployment zone
    Enemy   // Red deployment zone
}

public class BattleGridNode : MonoBehaviour
{
    [Header("Grid Coordinates")]
    public int gridRow;
    public int gridCol;

    [Header("Cell State")]
    public bool isWalkable = true;
    public GameObject occupyingUnit; // Current Unit (Hero/Troop/Dragon) standing here
    public DeploymentZoneType deploymentZone = DeploymentZoneType.None;

    [Header("Aesthetic Highlights")]
    [SerializeField] private Renderer tileRenderer;
    private Material defaultMaterial;
    private Material highlightMaterial;

    // Run-time pathfinding parameters
    [HideInInspector] public BattleGridNode parentNode;
    [HideInInspector] public int gCost;
    [HideInInspector] public int hCost;
    public int fCost { get { return gCost + hCost; } }

    private void Awake()
    {
        // Auto-find renderer if not set manually
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }

        if (tileRenderer != null)
        {
            // Use instance material so highlights don't bleed onto other cells
            defaultMaterial = tileRenderer.material;
        }
    }

    private void Start()
    {
        // Automatically color code cells according to deployment zone type on load
        ApplyDeploymentAesthetics();
    }

    /// <summary>
    /// Configures the cell's deployment status and updates its visual feedback
    /// </summary>
    public void ConfigureDeployment(DeploymentZoneType zoneType)
    {
        deploymentZone = zoneType;
        ApplyDeploymentAesthetics();
    }

    /// <summary>
    /// Overwrites the default material of the cell renderer to clean up baked model colors.
    /// </summary>
    public void SetBaseMaterial(Material newMat)
    {
        if (tileRenderer == null)
        {
            tileRenderer = GetComponent<Renderer>();
        }

        if (tileRenderer != null && newMat != null)
        {
            // Create a unique instance material so changing colors/emission does not bleed onto other cells
            tileRenderer.material = new Material(newMat);
            defaultMaterial = tileRenderer.material;
        }
    }

    /// <summary>
    /// Updates the color properties to match the active deployment zone type
    /// </summary>
    public void ApplyDeploymentAesthetics()
    {
        if (tileRenderer == null) return;

        // Находим родительский менеджер сетки и проверяем, нужно ли показывать зоны высадки
        TacticalBattleGrid gridManager = GetComponentInParent<TacticalBattleGrid>();
        bool showZones = (gridManager == null) || gridManager.showDeploymentZones;

        if (!showZones || deploymentZone == DeploymentZoneType.None)
        {
            ResetHighlight();
            return;
        }

        switch (deploymentZone)
        {
            case DeploymentZoneType.Player:
                // Smooth emissive Blue for Player spawn
                SetHighlight(new Color(0.12f, 0.45f, 1f), true);
                break;
            case DeploymentZoneType.Enemy:
                // Smooth emissive Red for Enemy spawn
                SetHighlight(new Color(1f, 0.15f, 0.15f), true);
                break;
        }
    }

    /// <summary>
    /// Highlights the grid node based on movement range, attack target, or select pathways
    /// </summary>
    public void SetHighlight(Color glowColor, bool emissionActive = true)
    {
        if (tileRenderer == null) return;

        tileRenderer.material.color = glowColor;
        if (emissionActive)
        {
            tileRenderer.material.EnableKeyword("_EMISSION");
            tileRenderer.material.SetColor("_EmissionColor", glowColor * 0.4f);
        }
    }

    /// <summary>
    /// Resets the cell material back to its pristine default team color
    /// </summary>
    public void ResetHighlight()
    {
        if (tileRenderer == null || defaultMaterial == null) return;
        tileRenderer.material = defaultMaterial;
    }

    private void OnMouseEnter()
    {
        // Dynamic hover feedback (glowing highlight overlay)
        if (isWalkable)
        {
            SetHighlight(new Color(1f, 0.84f, 0.3f), true); // Gold hover glow
        }
    }

    private void OnMouseExit()
    {
        // Revert highlight on mouse leave to original deployment state
        ApplyDeploymentAesthetics();
    }

    private void OnMouseDown()
    {
        // Send click event directly to the tactical grid manager
        TacticalBattleGrid gridManager = GetComponentInParent<TacticalBattleGrid>();
        if (gridManager != null)
        {
            gridManager.OnNodeClicked(this);
        }
    }
}
