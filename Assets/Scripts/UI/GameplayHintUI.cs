using System.Collections;
using UnityEngine;

public sealed class GameplayHintUI : MonoBehaviour
{
    [SerializeField] private GameObject hint;
    [Min(0f)] [SerializeField] private float duration = 3f;
    private IEnumerator Start()
    {
        if (hint == null) yield break;
        if (ServiceLocator.Instance.TryGet(out GameManager game))
            while (!game.IsGameplayReady) yield return null;
        hint.SetActive(true);
        yield return new WaitForSeconds(duration);
        hint.SetActive(false);
    }
}
