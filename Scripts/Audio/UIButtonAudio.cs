using UnityEngine;

namespace Tanks.Audio
{
  // Call the UIAudioManager to play the default button sound
  public class UIButtonAudio : MonoBehaviour
  {
    [SerializeField]
    protected AudioClip m_OverrideClip;

    public void OnClick()
    {
      if (UIAudioManager.s_InstanceExists)
      {
        UIAudioManager.s_Instance.PlayButtonEffect(m_OverrideClip);
      }
    }
  }
}
