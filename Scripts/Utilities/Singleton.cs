using UnityEngine;
using System;

namespace Tanks.Utilities
{
  /// Singleton class
  public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
  {
    private static T s_instance;

    // The static reference to the instance
    public static T s_Instance
    {
      get
      {
        return s_instance;
      }
      protected set
      {
        s_instance = value;
      }
    }

    // Gets whether an instance of this singleton exists
    public static bool s_InstanceExists { get { return s_instance != null; } }

    public static event Action InstanceSet;

    // Awake method to associate singleton with instance
    protected virtual void Awake()
    {
      if (s_instance != null)
      {
        Destroy(gameObject);
      }
      else
      {
        s_instance = (T)this;
        if (InstanceSet != null)
        {
          InstanceSet();
        }
      }
    }

    // OnDestroy method to clear singleton association
    protected virtual void OnDestroy()
    {
      if (s_instance == this)
      {
        s_instance = null;
      }
    }
  }
}
