using UnityEngine;

namespace Tanks.Effects
{
  public class DamageOutlineFlash : MonoBehaviour
  {
    [SerializeField]
    protected Color m_DamageColor = Color.red;

    [SerializeField]
    protected float m_DamageBorderPulseAmount = 4f;

    [SerializeField]
    protected float m_DamageFadeDuration = 0.15f;
    private float m_DamageFadeTime = 0f;

    [SerializeField]
    protected Renderer[] m_BorderRenderers;

    private Material m_BorderBaseMaterial;

    private float m_BorderBaseThickness;

    public void StartDamageFlash()
    {
      m_DamageFadeTime = m_DamageFadeDuration;
    }

    private void Awake()
    {
      if (m_BorderRenderers.Length > 0)
      {
        m_BorderBaseMaterial = m_BorderRenderers[0].sharedMaterial;
        m_BorderBaseThickness = m_BorderBaseMaterial.GetFloat("_OutlineWidth");
      }
    }

    private void Update()
    {
      if (m_DamageFadeTime > 0f)
      {
        m_DamageFadeTime -= Time.deltaTime;

        for (int i = 0; i < m_BorderRenderers.Length; i++)
        {
          m_BorderRenderers[i].material.color = Color.Lerp(Color.black, m_DamageColor, m_DamageFadeTime / m_DamageFadeDuration);

          m_BorderRenderers[i].material.SetFloat("_OutlineWidth", Mathf.Lerp(m_BorderBaseThickness, m_DamageBorderPulseAmount, m_DamageFadeTime / m_DamageFadeDuration));
        }

        if (m_DamageFadeTime <= 0f)
        {
          for (int i = 0; i < m_BorderRenderers.Length; i++)
          {
            Destroy(m_BorderRenderers[i].material);
            m_BorderRenderers[i].material = m_BorderBaseMaterial;
          }
        }
      }
    }
  }
}
