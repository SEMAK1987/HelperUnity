// ==============================================================================
//            FATE CONTINENT - MASTER TACTICAL ARENA SYSTEM (C#)
// ==============================================================================
// Version: v18.12.04 (A* Pathfinding & Unit Movement Engine)
// Description: Automatically scans imported Blender grid cells, registers
//             coordinates dynamically from names (e.g. Grid_Cell_03_04),
//             and builds a high-performance pathfinding matrix. Works out-of-the-box!
// ==============================================================================

using UnityEngine;
using System.Collections.Generic;

public class TacticalBattleGrid : MonoBehaviour
{
    [Header("Battle Arena Matrix")]
    public int totalRows = 3;
    public int totalCols = 4;
    
    [Header("Runtime Layout Database")]
    private BattleGridNode[,] gridMatrix;
    private List<BattleGridNode> activePath = new List<BattleGridNode>();

    [Header("Visual Path Line (Optional)")]
    public LineRenderer pathLineRenderer;

    [Header("Dynamic Deployment Setup (Test)")]
    public int testBattleIndex = 0;
    
    [Header("Visibility Settings")]
    public bool showDeploymentZones = true;
    public bool unifyGridMaterials = true; // Automatically overrides baked model colors (red/blue/green) with a neutral gray material

    private void Start()
    {
        InitializeGridFromHierarchy();
        SetupDeploymentZonesForBattle(testBattleIndex);
    }

    /// <summary>
    /// Dynamically configures player and enemy deployment zones for a specific battle.
    /// Can be customized based on castle level, region, or battle difficulty!
    /// </summary>
    public void SetupDeploymentZonesForBattle(int battleIndex)
    {
        if (gridMatrix == null) return;

        // Step 1: Reset all cells to None
        for (int r = 0; r < totalRows; r++)
        {
            for (int c = 0; c < totalCols; c++)
            {
                if (gridMatrix[r, c] != null)
                {
                    gridMatrix[r, c].ConfigureDeployment(DeploymentZoneType.None);
                }
            }
        }

        // Step 2: Define spawn layouts based on battle index
        if (battleIndex == 0)
        {
            // Standard Left vs Right (Player on columns 0-1, Enemy on columns totalCols-2 to totalCols-1)
            ConfigureZoneRectangle(0, totalRows - 1, 0, 1, DeploymentZoneType.Player);
            ConfigureZoneRectangle(0, totalRows - 1, totalCols - 2, totalCols - 1, DeploymentZoneType.Enemy);
        }
        else if (battleIndex == 1)
        {
            // Center-divided (Player on bottom rows, Enemy on top rows)
            ConfigureZoneRectangle(0, 1, 0, totalCols - 1, DeploymentZoneType.Player);
            ConfigureZoneRectangle(totalRows - 2, totalRows - 1, 0, totalCols - 1, DeploymentZoneType.Enemy);
        }
        else if (battleIndex == 2)
        {
            // Diagonal Corners (Player bottom-left, Enemy top-right)
            ConfigureZoneRectangle(0, 3, 0, 3, DeploymentZoneType.Player);
            ConfigureZoneRectangle(totalRows - 4, totalRows - 1, totalCols - 4, totalCols - 1, DeploymentZoneType.Enemy);
        }
        else
        {
            // Castles / Regions Custom: Scattered positions
            ConfigureZoneRectangle(2, 5, 1, 2, DeploymentZoneType.Player);
            ConfigureZoneRectangle(totalRows - 6, totalRows - 3, totalCols - 3, totalCols - 2, DeploymentZoneType.Enemy);
        }

        Debug.Log($"[TacticalBattleGrid] Dynamically configured deployment zones for Battle Index {battleIndex}!");
    }

    private void ConfigureZoneRectangle(int startRow, int endRow, int startCol, int endCol, DeploymentZoneType zoneType)
    {
        for (int r = startRow; r <= endRow; r++)
        {
            for (int c = startCol; c <= endCol; c++)
            {
                if (r >= 0 && r < totalRows && c >= 0 && c < totalCols)
                {
                    if (gridMatrix[r, c] != null)
                    {
                        gridMatrix[r, c].ConfigureDeployment(zoneType);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Scans children generated in Blender, extracts rows/cols from names, and registers components.
    /// </summary>
    [ContextMenu("Initialize Grid From Hierarchy")]
    public void InitializeGridFromHierarchy()
    {
        gridMatrix = new BattleGridNode[totalRows, totalCols];
        int registeredCount = 0;

        // Step 1: Optional - Find a neutral standard grey material from a central cell (e.g. Grid_Cell_05_05 or Grid_Cell_10_10)
        Material neutralMaterial = null;
        if (unifyGridMaterials)
        {
            foreach (Transform child in transform)
            {
                if (child.name == "Grid_Cell_05_05" || child.name == "Grid_Cell_10_10" || child.name == "Grid_Cell_07_07")
                {
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null)
                    {
                        neutralMaterial = r.sharedMaterial;
                        break;
                    }
                }
            }
        }

        // Traverse children
        foreach (Transform child in transform)
        {
            string name = child.name;
            // Expected format: Grid_Cell_Row_Col (e.g., Grid_Cell_02_03)
            if (name.StartsWith("Grid_Cell_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length >= 4)
                {
                    int r, c;
                    if (int.TryParse(parts[2], out r) && int.TryParse(parts[3], out c))
                    {
                        // Ensure inside array bounds
                        if (r < totalRows && c < totalCols)
                        {
                            BattleGridNode node = child.gameObject.GetComponent<BattleGridNode>();
                            if (node == null)
                            {
                                node = child.gameObject.AddComponent<BattleGridNode>();
                            }
                            
                            node.gridRow = r;
                            node.gridCol = c;
                            gridMatrix[r, c] = node;
                            registeredCount++;

                            // Override baked colors if unify is enabled
                            if (unifyGridMaterials && neutralMaterial != null)
                            {
                                node.SetBaseMaterial(neutralMaterial);
                            }
                        }
                    }
                }
            }
        }

        Debug.Log($"[TacticalBattleGrid] Successfully compiled and aligned {registeredCount} Blender cells inside the {totalRows}x{totalCols} combat arena!");
    }

    /// <summary>
    /// Triggered when a cell is clicked by the player (e.g., for selection or movement)
    /// </summary>
    public void OnNodeClicked(BattleGridNode clickedNode)
    {
        Debug.Log($"[TacticalBattleGrid] Clicked node: ({clickedNode.gridRow}, {clickedNode.gridCol}) | Walkable: {clickedNode.isWalkable}");
        
        // Интерактивный тест движения: находим любого юнита на сцене и приказываем ему идти на эту ячейку!
        UnitMovementController activeUnit = FindFirstObjectByType<UnitMovementController>();
        if (activeUnit != null)
        {
            activeUnit.TryMoveToNode(clickedNode);
        }
    }

    /// <summary>
    /// Computes A* or BFS shortest path between two tactical cells
    /// </summary>
    public List<BattleGridNode> FindShortestPath(BattleGridNode startNode, BattleGridNode targetNode)
    {
        if (startNode == null || targetNode == null || !targetNode.isWalkable) 
            return null;

        List<BattleGridNode> openSet = new List<BattleGridNode>();
        HashSet<BattleGridNode> closedSet = new HashSet<BattleGridNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            BattleGridNode currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                return RetracePath(startNode, targetNode);
            }

            foreach (BattleGridNode neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.isWalkable || closedSet.Contains(neighbor)) 
                    continue;

                int movementCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (movementCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = movementCostToNeighbor;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parentNode = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    private List<BattleGridNode> RetracePath(BattleGridNode startNode, BattleGridNode endNode)
    {
        List<BattleGridNode> path = new List<BattleGridNode>();
        BattleGridNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        path.Reverse();
        return path;
    }

    public List<BattleGridNode> GetNeighbors(BattleGridNode node)
    {
        List<BattleGridNode> neighbors = new List<BattleGridNode>();

        // 4-Way cardinal direction checks
        int[] dRow = { -1, 1, 0, 0 };
        int[] dCol = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int checkRow = node.gridRow + dRow[i];
            int checkCol = node.gridCol + dCol[i];

            if (checkRow >= 0 && checkRow < totalRows && checkCol >= 0 && checkCol < totalCols)
            {
                BattleGridNode neighbor = gridMatrix[checkRow, checkCol];
                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        return neighbors;
    }

    private int GetDistance(BattleGridNode nodeA, BattleGridNode nodeB)
    {
        // Manhattan distance calculation
        int dstX = Mathf.Abs(nodeA.gridCol - nodeB.gridCol);
        int dstY = Mathf.Abs(nodeA.gridRow - nodeB.gridRow);
        return dstX + dstY;
    }

    /// <summary>
    /// Включает или выключает подсветку синей/красной зоны высадки.
    /// Передайте false при старте боя, чтобы убрать все цвета и оставить сетку чистой!
    /// </summary>
    public void ToggleDeploymentZones(bool show)
    {
        showDeploymentZones = show;
        
        if (gridMatrix != null)
        {
            for (int r = 0; r < totalRows; r++)
            {
                for (int c = 0; c < totalCols; c++)
                {
                    if (gridMatrix[r, c] != null)
                    {
                        gridMatrix[r, c].ApplyDeploymentAesthetics();
                    }
                }
            }
        }
    }
}
