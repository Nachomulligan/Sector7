using UnityEngine;

/// <summary>
/// Conserva los servicios de progreso durante toda la sesion y evita duplicados
/// cuando se vuelve a cargar la escena Menu.
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class SessionServiceRoot : MonoBehaviour
{
    private static SessionServiceRoot instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
