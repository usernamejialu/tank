using UnityEngine;

namespace Tanks.Explosions
{
  public enum ExplosionClass
  {
    Large,
    Small,
    ExtraLarge,
    TankExplosion,
    TurretExplosion,
    BounceExplosion,
    ClusterExplosion,
    FiringExplosion
  }

  [CreateAssetMenu(fileName = "Explosion", menuName = "Explosion Definition", order = 1)]
  public class ExplosionSettings : ScriptableObject
  {
    public string id;
    public ExplosionClass explosionClass;
    public float explosionRadius;
    public float damage;
    public float physicsRadius;
    public float physicsForce;
    [Range(0, 1)]
    public float shakeMagnitude;
  }
}