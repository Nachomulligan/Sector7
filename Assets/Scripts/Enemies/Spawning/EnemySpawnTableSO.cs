using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Sector7/Spawning/Enemy Spawn Table", fileName = "EnemyTable_")]
public class EnemySpawnTableSO : ScriptableObject
{
    [Serializable]
    private class WeightedEnemy
    {
        [SerializeField] private GameObject prefab;
        [Min(0f)] [SerializeField] private float weight = 1f;

        public GameObject Prefab => prefab;
        public float Weight => weight;
    }

    [SerializeField] private List<WeightedEnemy> enemies = new List<WeightedEnemy>();

    public GameObject GetRandomPrefab()
    {
        float totalWeight = 0f;

        foreach (WeightedEnemy enemy in enemies)
        {
            if (enemy.Prefab != null && enemy.Weight > 0f)
            {
                totalWeight += enemy.Weight;
            }
        }

        if (totalWeight <= 0f)
        {
            return null;
        }

        float roll = UnityEngine.Random.value * totalWeight;

        foreach (WeightedEnemy enemy in enemies)
        {
            if (enemy.Prefab == null || enemy.Weight <= 0f)
            {
                continue;
            }

            roll -= enemy.Weight;

            if (roll <= 0f)
            {
                return enemy.Prefab;
            }
        }

        return null;
    }
}
