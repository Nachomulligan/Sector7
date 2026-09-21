using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class ServiceLocator : MonoBehaviour
{
    private static ServiceLocator instance;

    private readonly Dictionary<Type, List<object>> services = new Dictionary<Type, List<object>>();

    public static ServiceLocator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ServiceLocator>();
            }

            if (instance == null)
            {
                GameObject serviceObject = new GameObject(nameof(ServiceLocator));
                instance = serviceObject.AddComponent<ServiceLocator>();
            }

            return instance;
        }
    }

    public static bool HasInstance => instance != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            services.Clear();
            instance = null;
        }
    }

    public void Register<T>(T service) where T : class
    {
        if (service == null)
        {
            return;
        }

        Type serviceType = typeof(T);

        if (!services.TryGetValue(serviceType, out List<object> registeredServices))
        {
            registeredServices = new List<object>();
            services.Add(serviceType, registeredServices);
        }

        if (!registeredServices.Contains(service))
        {
            registeredServices.Add(service);
        }
    }

    public void Unregister<T>(T service) where T : class
    {
        if (service == null || !services.TryGetValue(typeof(T), out List<object> registeredServices))
        {
            return;
        }

        registeredServices.Remove(service);

        if (registeredServices.Count == 0)
        {
            services.Remove(typeof(T));
        }
    }

    public bool TryGet<T>(out T service) where T : class
    {
        service = null;

        if (!services.TryGetValue(typeof(T), out List<object> registeredServices))
        {
            return false;
        }

        RemoveInvalidEntries(registeredServices);

        if (registeredServices.Count == 0)
        {
            services.Remove(typeof(T));
            return false;
        }

        service = registeredServices[0] as T;
        return service != null;
    }

    public void GetAll<T>(List<T> results) where T : class
    {
        results.Clear();

        if (!services.TryGetValue(typeof(T), out List<object> registeredServices))
        {
            return;
        }

        RemoveInvalidEntries(registeredServices);

        foreach (object registeredService in registeredServices)
        {
            if (registeredService is T typedService)
            {
                results.Add(typedService);
            }
        }
    }

    private static void RemoveInvalidEntries(List<object> registeredServices)
    {
        for (int i = registeredServices.Count - 1; i >= 0; i--)
        {
            object service = registeredServices[i];

            if (service == null || service is UnityEngine.Object unityObject && unityObject == null)
            {
                registeredServices.RemoveAt(i);
            }
        }
    }
}
