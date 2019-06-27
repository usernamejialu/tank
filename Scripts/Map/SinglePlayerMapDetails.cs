using UnityEngine;
using Tanks.Rules;
using System;

namespace Tanks.Map
{
  [Serializable]
  public class SinglePlayerMapDetails : MapDetails
  {
    //This is marked as a serialized field for debugging purposes only
    [SerializeField]
    protected int m_MedalCountRequired;

    public int medalCountRequired
    {
      get { return m_MedalCountRequired; }
    }

    [SerializeField]
    protected RulesProcessor m_RulesProcessor;

    public RulesProcessor rulesProcessor
    {
      get { return m_RulesProcessor; }
    }
  }
}