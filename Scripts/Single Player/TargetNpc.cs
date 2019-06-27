using UnityEngine;
using System.Collections;

namespace Tanks.SinglePlayer
{
  /// A specific NPC - used in chase and VIP missions
  public class TargetNpc : Npc
  {
    [SerializeField]
    protected bool m_IsPrimaryObjective = false;

    public bool isPrimaryObjective
    {
      get { return m_IsPrimaryObjective; }
    }
  }
}
