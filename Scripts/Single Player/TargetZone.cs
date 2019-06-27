using UnityEngine;
using Tanks.Rules.SinglePlayer;

namespace Tanks.SinglePlayer
{
  [RequireComponent(typeof(Collider))]
  public class TargetZone : MonoBehaviour
  {
    private OfflineRulesProcessor m_RuleProcessor;

    protected virtual void OnTriggerEnter(Collider c)
    {
      LazyLoadRuleProcessor();
      if (m_RuleProcessor != null)
      {
        m_RuleProcessor.EntersZone(c.gameObject, this);
      }

      HandleTrigger(c.gameObject);
    }

    // Set the navigator to be complte
    protected virtual void HandleTrigger(GameObject zoneObject)
    {
      Navigator navigator = zoneObject.GetComponent<Navigator>();
      if (navigator != null)
      {
        navigator.SetComplete();
      }
    }

    // Lazy load the rule processor
    private void LazyLoadRuleProcessor()
    {
      if (m_RuleProcessor != null)
      {
        return;
      }

      if (GameManager.s_Instance != null)
      {
        m_RuleProcessor = GameManager.s_Instance.rulesProcessor as OfflineRulesProcessor;
      }
    }
  }
}
