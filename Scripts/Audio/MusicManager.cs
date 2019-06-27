using UnityEngine;
using UnityEngine.SceneManagement;
using Tanks.Utilities;

namespace Tanks.Audio
{
  [RequireComponent(typeof(AudioSource))]
  public class MusicManager : PersistentSingleton<MusicManager>
  {
    [SerializeField]
    protected AudioClip m_MenuMusic;

    private AudioSource m_MusicSource;

    public AudioSource musicSource
    {
      get { return m_MusicSource; }
    }

    [SerializeField]
    protected float m_StartDelay = 0.5f;

    [SerializeField]
    protected float m_FadeRate = 2f;

    private float m_OriginalVolume;

    //Proportion of fading.
    private float m_FadeLevel = 1f;

    protected void Start()
    {
      m_MusicSource = GetComponent<AudioSource>();
      SceneManager.activeSceneChanged += OnSceneChanged;

      m_OriginalVolume = m_MusicSource.volume;

      PlayMusic(m_MenuMusic);
    }

    //Volume fade-in logic happens here, assuming the relevant parameters are set.
    protected void Update()
    {
      if (m_FadeLevel < 1f && m_FadeRate > 0f)
      {
        m_FadeLevel = Mathf.Lerp(m_FadeLevel, 1f, Time.deltaTime * m_FadeRate);

        if (m_FadeLevel >= 0.99f)
        {
          m_FadeLevel = 1f;
        }

        m_MusicSource.volume = m_OriginalVolume * m_FadeLevel;
      }
    }

    private void OnSceneChanged(Scene scene1, Scene newScene)
    {
      if (m_MusicSource != null)
      {
        // Make sure to reset pitch
        m_MusicSource.pitch = 1;
      }

      if (newScene.name == "LobbyScene")
      {
        if (m_MusicSource.clip != m_MenuMusic)
        {
          PlayMusic(m_MenuMusic, true);
        }
      }
      else
      {
        PlayMusic(GameSettings.s_Instance.map.levelMusic, true);
      }
    }

    public void StopMusic()
    {
      m_MusicSource.Stop();
    }

    public void PlayCurrentMusic()
    {
      m_MusicSource.Play();
    }

    private void PlayMusic(AudioClip music, bool fadeIn = false, bool loop = true)
    {
      m_MusicSource.Stop();

      m_MusicSource.loop = loop;
      m_MusicSource.clip = music;
      m_MusicSource.PlayDelayed(m_StartDelay);

      if (fadeIn)
      {
        m_FadeLevel = -m_StartDelay;
      }
    }

    protected override void OnDestroy()
    {
      SceneManager.activeSceneChanged -= OnSceneChanged;
      base.OnDestroy();
    }
  }
}
