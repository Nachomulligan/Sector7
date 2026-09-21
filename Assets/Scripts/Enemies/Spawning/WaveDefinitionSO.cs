using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WaveSpawnInstruction
{
    [Header("Qué aparece")]
    [Tooltip("Si se asigna, toda la formación usa este prefab.")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("Se usa cuando Enemy Prefab está vacío. Permite elegir un enemigo distinto por posición.")]
    [SerializeField] private EnemySpawnTableSO enemyTable;

    [Header("Cómo aparece")]
    [SerializeField] private FormationPresetSO formation;
    [SerializeField] private Vector2 anchor = new Vector2(0f, 6f);
    [Min(0f)] [SerializeField] private float delayBeforeSpawn;
    [Min(0f)] [SerializeField] private float randomHorizontalOffset;

    [Header("Overrides opcionales")]
    [SerializeField] private bool overrideSpawnInterval;
    [Min(0f)] [SerializeField] private float spawnInterval;
    [SerializeField] private bool forceMirror;

    public FormationPresetSO Formation => formation;
    public Vector2 Anchor => anchor;
    public float DelayBeforeSpawn => delayBeforeSpawn;
    public float RandomHorizontalOffset => randomHorizontalOffset;
    public float SpawnInterval => overrideSpawnInterval ? spawnInterval : formation != null ? formation.SpawnInterval : 0f;
    public bool Mirror => forceMirror || formation != null && formation.ShouldMirror();

    public GameObject GetEnemyPrefab()
    {
        return enemyPrefab != null ? enemyPrefab : enemyTable != null ? enemyTable.GetRandomPrefab() : null;
    }
}

[CreateAssetMenu(menuName = "Sector7/Spawning/Wave Definition", fileName = "Wave_")]
public class WaveDefinitionSO : ScriptableObject
{
    [SerializeField] private List<WaveSpawnInstruction> spawnInstructions = new List<WaveSpawnInstruction>();
    [Tooltip("La wave termina únicamente cuando todos sus enemigos murieron o salieron de pantalla.")]
    [SerializeField] private bool waitForAllEnemies = true;
    [Min(0f)] [SerializeField] private float delayAfterWave = 1f;

    public IReadOnlyList<WaveSpawnInstruction> SpawnInstructions => spawnInstructions;
    public bool WaitForAllEnemies => waitForAllEnemies;
    public float DelayAfterWave => delayAfterWave;
}
