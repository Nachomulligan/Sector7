using System.Collections.Generic;
using UnityEngine;

public enum FormationShape
{
    Single,
    Line,
    Column,
    Grid,
    V,
    Arc,
    Custom
}

[CreateAssetMenu(menuName = "Sector7/Spawning/Formation Preset", fileName = "Formation_")]
public class FormationPresetSO : ScriptableObject
{
    [Header("Forma")]
    [SerializeField] private FormationShape shape = FormationShape.Single;
    [Min(1)] [SerializeField] private int count = 1;

    [Header("Separación")]
    [Min(0f)] [SerializeField] private float horizontalSpacing = 1f;
    [Min(0f)] [SerializeField] private float verticalSpacing = 0.75f;
    [Min(1)] [SerializeField] private int gridColumns = 3;

    [Header("Arco")]
    [Min(0f)] [SerializeField] private float arcRadius = 2f;
    [Range(0f, 360f)] [SerializeField] private float arcAngle = 120f;

    [Header("Entrada")]
    [Min(0f)] [SerializeField] private float spawnInterval = 0.1f;
    [Range(0f, 1f)] [SerializeField] private float mirrorChance;

    [Header("Layout personalizado")]
    [SerializeField] private List<Vector2> customOffsets = new List<Vector2>();

    public int Count => shape == FormationShape.Custom ? customOffsets.Count : Mathf.Max(1, count);
    public float SpawnInterval => spawnInterval;

    public bool ShouldMirror()
    {
        return mirrorChance > 0f && Random.value < mirrorChance;
    }

    public void FillOffsets(List<Vector2> results, bool mirrored)
    {
        results.Clear();

        switch (shape)
        {
            case FormationShape.Line:
                AddLine(results);
                break;
            case FormationShape.Column:
                AddColumn(results);
                break;
            case FormationShape.Grid:
                AddGrid(results);
                break;
            case FormationShape.V:
                AddV(results);
                break;
            case FormationShape.Arc:
                AddArc(results);
                break;
            case FormationShape.Custom:
                results.AddRange(customOffsets);
                break;
            default:
                results.Add(Vector2.zero);
                break;
        }

        if (!mirrored)
        {
            return;
        }

        for (int i = 0; i < results.Count; i++)
        {
            Vector2 offset = results[i];
            offset.x *= -1f;
            results[i] = offset;
        }
    }

    private void AddLine(List<Vector2> results)
    {
        float startX = -((Count - 1) * horizontalSpacing * 0.5f);

        for (int i = 0; i < Count; i++)
        {
            results.Add(new Vector2(startX + (i * horizontalSpacing), 0f));
        }
    }

    private void AddColumn(List<Vector2> results)
    {
        for (int i = 0; i < Count; i++)
        {
            results.Add(new Vector2(0f, i * verticalSpacing));
        }
    }

    private void AddGrid(List<Vector2> results)
    {
        int columns = Mathf.Max(1, gridColumns);

        for (int i = 0; i < Count; i++)
        {
            int row = i / columns;
            int column = i % columns;
            int itemsInRow = Mathf.Min(columns, Count - (row * columns));
            float startX = -((itemsInRow - 1) * horizontalSpacing * 0.5f);
            results.Add(new Vector2(startX + (column * horizontalSpacing), row * verticalSpacing));
        }
    }

    private void AddV(List<Vector2> results)
    {
        results.Add(Vector2.zero);

        for (int i = 1; i < Count; i++)
        {
            int row = (i + 1) / 2;
            float side = i % 2 == 1 ? -1f : 1f;
            results.Add(new Vector2(side * row * horizontalSpacing, row * verticalSpacing));
        }
    }

    private void AddArc(List<Vector2> results)
    {
        if (Count == 1)
        {
            results.Add(Vector2.zero);
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            float normalized = i / (Count - 1f);
            float angle = Mathf.Lerp(-arcAngle * 0.5f, arcAngle * 0.5f, normalized) * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * arcRadius;
            float y = (1f - Mathf.Cos(angle)) * arcRadius;
            results.Add(new Vector2(x, y));
        }
    }
}
