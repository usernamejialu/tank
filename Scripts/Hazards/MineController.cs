using UnityEngine;
using Tanks.Explosions;
using Tanks.TankControllers;

namespace Tanks.Hazards
{
  public class MineController : LevelHazard, IDamageObject
  {
    [SerializeField]
    protected ExplosionSettings m_MineExplosionSettings;

    [SerializeField]
    protected float m_DamageThreshold = 20f;

    [SerializeField]
    protected GameObject m_MineMesh;

    [SerializeField]
    protected float m_TriggerCountdownDuration = 3f;
    private float m_TriggerTime = 0f;
    private bool m_Triggered = false;

    //Reference to the light on top of the mine.
    [SerializeField]
    protected Renderer m_MineLight;

    //References to the two light materials that will be swapped between to denote the mine's state.
    [SerializeField]
    protected Material m_IdleLightMaterial;
    [SerializeField]
    protected Material m_TriggeredLightMaterial;

    //Reference to the collider acting as the proximity trigger for the mine.
    [SerializeField]
    protected Collider m_TriggerCollider;

    private int m_LastHitBy = TankHealth.TANK_SUICIDE_INDEX;
    private int m_DetonatedByPlayer = TankHealth.TANK_SUICIDE_INDEX;

    //Is the mine alive and active? Also needed to implement IDamageObject.
    public bool isAlive { get; protected set; }

    protected override void Start()
    {
      base.Start();
      isAlive = true;
      m_TriggerCollider = GetComponent<Collider>();
    }

    protected void Update()
    {
      if (m_TriggerTime > 0f)
      {
        if (m_TriggerTime <= Time.time)
        {
          ExplodeMine();
        }
      }
    }

    private void OnTriggerEnter(Collider other)
    {
      //If the mine's already been triggered, ignore any new entrants
      if (m_Triggered)
      {
        return;
      }

      if (other.gameObject.layer == LayerMask.NameToLayer("Players"))
      {
        m_TriggerTime = Time.time + m_TriggerCountdownDuration;
        m_Triggered = true;
        RpcSetTriggeredEffects();
      }
    }

    private void ExplodeMine()
    {
      Debug.Log("<color=orange>Your mine asplode. Detonated by player " + m_DetonatedByPlayer + "</color>");
      m_TriggerTime = 0f;
      isAlive = false;

      m_TriggerCollider.enabled = false;

      //Spawn the explosion through the ExplosionManager. The explosion itself will be broadcast to clients by the ExplosionManager.
      ExplosionManager.s_Instance.SpawnExplosion(transform.position, Vector3.up, gameObject, m_DetonatedByPlayer, m_MineExplosionSettings, false);

      RpcExplodeMine();
    }

    // Perform server-side reset logic.
    public override void ResetHazard()
    {
      m_Triggered = true;
      m_TriggerTime = 0f;
      m_TriggerCollider.enabled = false;

      m_LastHitBy = TankHealth.TANK_SUICIDE_INDEX;
      m_DetonatedByPlayer = TankHealth.TANK_SUICIDE_INDEX;

      RpcResetMine();
    }

    public override void ActivateHazard()
    {
      m_TriggerCollider.enabled = true;
      isAlive = true;
      m_Triggered = false;
    }

    //Fired on all clients to start the mine's trigger effects (light to red and warning beep sound effect).
    private void RpcSetTriggeredEffects()
    {
      m_MineLight.material = m_TriggeredLightMaterial;

      GetComponent<AudioSource>().Play();
    }

    //Fired on all clients to hide the visible mine and stop any trigger sound effects.
    private void RpcExplodeMine()
    {
      m_MineMesh.SetActive(false);
      GetComponent<AudioSource>().Stop();
    }

    //Reset mines client-side. Makes them visible again.
    private void RpcResetMine()
    {
      GetComponent<AudioSource>().Stop();
      m_MineLight.material = m_IdleLightMaterial;
      m_MineMesh.SetActive(true);
    }

    public Vector3 GetPosition()
    {
      return transform.position;
    }

    //This is called when a mine receives damage from a player's weapon.
    public void Damage(float damage)
    {
      //We only player-detonate this mine if damage exceeds a certain threshold
      if (isAlive && (damage >= m_DamageThreshold))
      {
        //Since we know who detonated this mine, we can formally assign any kills due to the resulting explosion to them.
        m_DetonatedByPlayer = m_LastHitBy;

        ExplodeMine();
      }
    }

    public void SetDamagedBy(int playerNumber, string explosionId)
    {
      if (isAlive)
      {
        m_LastHitBy = playerNumber;
      }
    }
  }
}
