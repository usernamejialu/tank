namespace Tanks.Utilities
{
  /// Singleton that persists across multiple scenes
  public class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
  {
    protected override void Awake()
    {
      base.Awake();
      DontDestroyOnLoad(gameObject);
    }
  }
}
