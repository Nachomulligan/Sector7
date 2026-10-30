using UnityEngine;

/// <summary>
/// Fuente única de datos visuales y descriptivos de un objeto coleccionable.
/// </summary>
public interface IItemPresentationSource
{
    string DisplayName { get; }
    string Description { get; }
    Sprite Icon { get; }
    Color IconColor { get; }
}
