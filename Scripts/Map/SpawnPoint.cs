using UnityEngine;
using Tanks.TankControllers;

namespace Tanks.Map
{
  // Spawn point - has a collider to check if any player is in the zone
  [RequireComponent(typeof(Collider))]
  public class SpawnPoint : MonoBehaviour
  {
    [SerializeField]
    protected Transform m_SpawnPointTransform;

    public Transform spawnPointTransform
    {
      get
      {
        if (m_SpawnPointTransform == null)
        {
          m_SpawnPointTransform = transform;
        }
        return m_SpawnPointTransform;
      }
    }

    private bool m_IsDirty = false;

    private int m_NumberOfTanksInZone = 0;

    public bool isEmptyZone
    {
      get { return !m_IsDirty && m_NumberOfTanksInZone == 0; }
    }

    // Raises the trigger enter event - if the collider is a tank then increase the number of tanks in zone
    private void OnTriggerEnter(Collider c)
    {
      TankHealth tankHealth = c.GetComponentInParent<TankHealth>();

      if (tankHealth != null)
      {
        m_NumberOfTanksInZone++;
        tankHealth.currentSpawnPoint = this;
      }
    }

    // Raises the trigger exit event - if the collider is a tank then decrease the number of tanks in zone
    private void OnTriggerExit(Collider c)
    {
      TankHealth tankHealth = c.GetComponentInParent<TankHealth>();

      if (tankHealth != null)
      {
        Decrement();
        tankHealth.NullifySpawnPoint(this);
      }
    }

    // Safely decrement the number of tanks in zone and set isDirty to false
    public void Decrement()
    {
      m_NumberOfTanksInZone--;
      if (m_NumberOfTanksInZone < 0)
      {
        m_NumberOfTanksInZone = 0;
      }

      m_IsDirty = false;
    }

    // Used to set the spawn point to dirty to prevent simultaneous spawns from occurring at the same point
    public void SetDirty()
    {
      m_IsDirty = true;
    }

    // Resets/cleans up the spawn point
    public void Cleanup()
    {
      m_IsDirty = false;
      m_NumberOfTanksInZone = 0;
    }
  }
}