using System;
using System.Collections.Generic;
using UnityEngine;

public class ServiceLocator
{
    private static ServiceLocator instance;
    private Dictionary<Type, IService> services = new Dictionary<Type, IService>();

    public static ServiceLocator Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ServiceLocator();
            }
            return instance;
        }
    }

    public void RegisterService<T>(T service) where T : class, IService
    {
        services[typeof(T)] = service;
        Debug.Log($"Service registered: {typeof(T).Name}");
    }

    public T GetService<T>() where T : class, IService
    {
        if (services.TryGetValue(typeof(T), out IService service))
        {
            return service as T;
        }
        Debug.LogWarning($"Service not found: {typeof(T).Name}");
        return null;
    }

    public void InitializeAllServices()
    {
        foreach (var service in services.Values)
        {
            service.Initialize();
        }
        Debug.Log($"Initialized {services.Count} services");
    }

    public void CleanupAllServices()
    {
        foreach (var service in services.Values)
        {
            service.Cleanup();
        }
        services.Clear();
        Debug.Log("All services cleaned up");
    }
}
