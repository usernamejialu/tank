using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tanks.UI
{
	/// <summary>
	/// Basic splash screen that fades in the logo and pulses start text
	/// </summary>
	public class SimpleSplashScreen : MonoBehaviour
	{
		//The scene to load
		[SerializeField]
		protected string m_SceneName = "LobbyScene";

		protected virtual void Update()
		{
			//Go to menu
			if (Input.anyKeyDown)
			{
				ProgressToNextScene();
			}
		}

		//Helper for going to menu
		private void ProgressToNextScene()
		{
			SceneManager.LoadScene(m_SceneName);
		}
	}
}