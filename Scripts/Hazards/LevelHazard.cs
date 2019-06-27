using UnityEngine;

namespace Tanks.Hazards
{
  // A base class for all environmental hazards.
  public class LevelHazard : MonoBehaviour
  {
    protected virtual void Start()
    {
      GameManager.s_Instance.AddHazard(this);
    }

    protected virtual void OnDestroy()
    {
      GameManager.s_Instance.RemoveHazard(this);
    }

    public virtual void ResetHazard() { }

    public virtual void ActivateHazard() { }
  }
}
