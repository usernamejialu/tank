using UnityEngine;
using System.Collections.Generic;

namespace Tanks.Map
{
  public abstract class MapListBase<T> : ScriptableObject where T : MapDetails
  {
    [SerializeField]
    private List<T> m_Details;

    public T this[int index]
    {
      get { return m_Details[index]; }
    }

    public int Count
    {
      get { return m_Details.Count; }
    }
  }
}