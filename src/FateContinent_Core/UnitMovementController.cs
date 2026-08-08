using UnityEngine;
using System.Collections.Generic;

public class UnitMovementController : MonoBehaviour
{
    [Header("Связь с Сеткой")]
    public BattleGridNode currentGridNode; // Ячейка, на которой сейчас стоит юнит
    private TacticalBattleGrid gridManager;

    [Header("Настройки Скорости")]
    [SerializeField] private float moveSpeed = 5f;

    private void Start()
    {
        gridManager = FindFirstObjectByType<TacticalBattleGrid>();
        
        // Моментально позиционируем воина ровно по центру его стартовой ячейки
        if (currentGridNode != null)
        {
            transform.position = new Vector3(currentGridNode.transform.position.x, transform.position.y, currentGridNode.transform.position.z);
            currentGridNode.occupyingUnit = this.gameObject;
        }
    }

    /// <summary>
    /// Вызывается при клике игрока на целевую ячейку
    /// </summary>
    public void TryMoveToNode(BattleGridNode targetNode)
    {
        if (gridManager == null || currentGridNode == null || targetNode == null) return;
        
        // Если клетка заблокирована препятствием или там уже кто-то стоит — ходить нельзя!
        if (!targetNode.isWalkable || targetNode.occupyingUnit != null) return;

        // Рассчитываем путь с обходом препятствий по алгоритму A* (A-Star)
        List<BattleGridNode> path = gridManager.FindShortestPath(currentGridNode, targetNode);

        if (path != null && path.Count > 0)
        {
            Debug.Log($"Путь найден! Длина пути: {path.Count} шагов.");
            StopAllCoroutines();
            StartCoroutine(MoveAlongPathCoroutine(path));
        }
        else
        {
            Debug.LogWarning("Путь заблокирован или цель недостижима!");
        }
    }

    private System.Collections.IEnumerator MoveAlongPathCoroutine(List<BattleGridNode> path)
    {
        foreach (BattleGridNode stepNode in path)
        {
            Vector3 targetPosition = new Vector3(stepNode.transform.position.x, transform.position.y, stepNode.transform.position.z);
            
            // Плавно ведем воина к центру следующей плитки
            while (Vector3.Distance(transform.position, targetPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
                yield return null;
            }

            // Обновляем логику сетки
            currentGridNode.occupyingUnit = null;
            currentGridNode = stepNode;
            currentGridNode.occupyingUnit = this.gameObject;
        }

        Debug.Log("Юнит успешно завершил перемещение!");
    }
}
