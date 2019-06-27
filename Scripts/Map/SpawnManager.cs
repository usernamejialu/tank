using UnityEngine;
using System.Collections.Generic;
using Tanks.Utilities;
using System.Linq;

namespace Tanks.Map
{
  public class SpawnManager : Singleton<SpawnManager>
  {
    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    protected override void Awake()
    {
      base.Awake();
      LazyLoadSpawnPoints();
    }

    private void Start()
    {
      LazyLoadSpawnPoints();
    }

    private void LazyLoadSpawnPoints()
    {
      if (spawnPoints != null && spawnPoints.Count > 0)
      {
        return;
      }

      SpawnPoint[] foundSpawnPoints = GetComponentsInChildren<SpawnPoint>();
      spawnPoints.AddRange(foundSpawnPoints);
    }

    public int GetRandomEmptySpawnPointIndex()
    {
      LazyLoadSpawnPoints();
      List<SpawnPoint> emptySpawnPoints = spawnPoints.Where(sp => sp.isEmptyZone).ToList();

      if (emptySpawnPoints.Count == 0)
      {
        return 0;
      }

      SpawnPoint emptySpawnPoint = emptySpawnPoints[Random.Range(0, emptySpawnPoints.Count)];

      emptySpawnPoint.SetDirty();

      return spawnPoints.IndexOf(emptySpawnPoint);
    }

    public SpawnPoint GetSpawnPointByIndex(int i)
    {
      LazyLoadSpawnPoints();
      return spawnPoints[i];
    }

    public Transform GetSpawnPointTransformByIndex(int i)
    {
      return GetSpawnPointByIndex(i).spawnPointTransform;
    }

    public void CleanupSpawnPoints()
    {
      for (int i = 0; i < spawnPoints.Count(); i++)
      {
        spawnPoints[i].Cleanup();
      }
    }
  }
}