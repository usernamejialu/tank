using UnityEngine;
using Tanks.Utilities;

namespace Tanks.Audio
{
  [RequireComponent(typeof(AudioSource))]

  public class UIAudioManager : PersistentSingleton<UIAudioManager>
  {
    [SerializeField]
    protected AudioClip m_DefaultButtonSound;

    [SerializeField]
    protected AudioClip m_RoundStartEffect;

    [SerializeField]
    protected AudioClip m_VictoryEffect;

    [SerializeField]
    protected AudioClip m_FailureEffect;

    [SerializeField]
    protected AudioClip m_CoinEffect;

    private AudioSource m_ButtonSource;


    protected override void Awake()
    {
      base.Awake();

      m_ButtonSource = GetComponent<AudioSource>();
    }

    public void PlayButtonEffect(AudioClip overrideSound = null)
    {
      m_ButtonSource.Stop();

      if (overrideSound != null)
      {
        PlaySound(overrideSound);
      }
      else
      {
        PlaySound(m_DefaultButtonSound);
      }
    }

    public void PlayRoundStartSound()
    {
      m_ButtonSource.PlayOneShot(m_RoundStartEffect);
    }

    public void PlayVictorySound()
    {
      PlaySound(m_VictoryEffect);
    }

    public void PlayFailureSound()
    {
      PlaySound(m_FailureEffect);
    }

    public void PlayCoinSound()
    {
      m_ButtonSource.PlayOneShot(m_CoinEffect);
    }

    private void PlaySound(AudioClip sound)
    {
      m_ButtonSource.clip = sound;
      m_ButtonSource.Play();
    }
  }
}
