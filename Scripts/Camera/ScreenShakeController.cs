using UnityEngine;
using System.Collections.Generic;
using System;
using Tanks.Utilities;

namespace Tanks.CameraControl
{
  [RequireComponent(typeof(Camera))]
  public class ScreenShakeController : Singleton<ScreenShakeController>
  {
    [Serializable]
    public struct ShakeSettings
    {
      public float maxShake;
      public float maxAngle;
    }


    /// Struct to contain specific instances of shaking
    protected struct ShakeInstance
    {
      public float maxDuration, duration, magnitude;
      public Vector2 direction;
      public int shakeId;

      public float normalizedProgress
      {
        get
        {
          return Mathf.Clamp01(duration / maxDuration);
        }
      }

      public bool done
      {
        get
        {
          return maxDuration > 0 && duration >= maxDuration - Mathf.Epsilon;
        }
      }

      public void StopShake()
      {
        // Set durations to ended
        maxDuration = duration = 1;
      }
    }

    /// Perspective camera settings
    [SerializeField]
    protected ShakeSettings m_PerspectiveSettings;
    /// Orthographic settings
    [SerializeField]
    protected ShakeSettings m_OrthographicSettings;
    /// Scaling factor for directional noise
    [SerializeField]
    protected float m_DirectionNoiseScale;
    /// Scaling factor for magnitudinal noise
    [SerializeField]
    protected float m_MagnitudeNoiseScale;

    /// Collection of current shakes
    private List<ShakeInstance> m_CurrentShakes;
    /// Reference to our camera
    private Camera m_ShakingCamera;

    /// Shake ID counter
    private int m_ShakeCounter = 0;

    /// Initialize shake collection, noise generator, and find child camera
    protected override void Awake()
    {
      base.Awake();

      m_CurrentShakes = new List<ShakeInstance>();

      m_ShakingCamera = GetComponent<Camera>();
      // Disable ourselves if we have no camera
      if (m_ShakingCamera == null)
      {
        enabled = false;
        Debug.LogWarning("No camera for ScreenShakeController.");
      }
    }


    /// Do shakes
    protected virtual void Update()
    {
      // Double check that our camera still exists
      if (m_ShakingCamera == null)
        return;

      Vector2 shakeIntensity = Vector2.zero;

      // Count backwards so we can remove shakes with simpler logic
      for (int i = m_CurrentShakes.Count - 1; i >= 0; --i)
      {
        ShakeInstance shake = m_CurrentShakes[i];

        ProcessShake(ref shake, ref shakeIntensity);

        if (shake.done)
        {
          m_CurrentShakes.RemoveAt(i);
        }
        else
        {
          // Update list
          m_CurrentShakes[i] = shake;
        }
      }

      Vector3 shake3D = new Vector3(shakeIntensity.x, shakeIntensity.y, 0);

      if (m_ShakingCamera.orthographic)
      {
        // Orthographic cameras get translated
        transform.localPosition = shake3D;
        transform.localRotation = Quaternion.identity;
      }
      else
      {
        // Perspective cameras get a shake
        Vector3 rotateAxis = Vector3.Cross(Vector3.forward, shake3D).normalized;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.AngleAxis(shake3D.magnitude, rotateAxis);
      }
    }

    /// Perform a screen shake with a world space origin
    public void DoShake(Vector3 worldPosition, float magnitude, float duration)
    {
      // Calculate relative screen direction
      Vector3 viewportPos = m_ShakingCamera.WorldToViewportPoint(worldPosition);
      Vector2 relativePos = new Vector2(viewportPos.x * 2 - 1, viewportPos.y * 2 - 1);

      DoShake(relativePos.normalized, magnitude, duration);
    }


    /// Perform a screen shake with a world space origin, scaling its magnitude by distance from screen center
    public void DoShake(Vector3 worldPosition, float magnitude, float duration,
                        float minScale, float maxScale)
    {
      // Calculate relative screen direction
      Vector3 viewportPos = m_ShakingCamera.WorldToViewportPoint(worldPosition);
      Vector2 relativePos = new Vector2(viewportPos.x * 2 - 1, viewportPos.y * 2 - 1);

      // Scale magnitude based on distance to center of screen
      float distanceScalar = Mathf.Clamp01(relativePos.magnitude / Mathf.Sqrt(2));
      distanceScalar = (1 - distanceScalar);
      distanceScalar *= distanceScalar;
      float durationScalar = distanceScalar * 0.5f + 0.5f;
      magnitude *= Mathf.Lerp(minScale, maxScale, distanceScalar);

      DoShake(relativePos.normalized, magnitude, duration * durationScalar);
    }


    /// Perform a screen shake
    public void DoShake(Vector2 direction, float magnitude, float duration)
    {
      // Add a new shake
      ShakeInstance shake = new ShakeInstance
      {
        shakeId = m_ShakeCounter++,
        maxDuration = duration,
        duration = 0,
        magnitude = magnitude,
        direction = direction
      };

      m_CurrentShakes.Add(shake);

      if (m_ShakeCounter == int.MaxValue)
        m_ShakeCounter = 0;
    }


    /// Enable a repeating screen shake
    public int DoPerpetualShake(Vector2 direction, float magnitude)
    {
      int result = m_ShakeCounter;

      // Add a new shake
      ShakeInstance shake = new ShakeInstance
      {
        shakeId = m_ShakeCounter++,
        maxDuration = -1,
        duration = 0,
        magnitude = magnitude,
        direction = direction
      };

      m_CurrentShakes.Add(shake);

      if (m_ShakeCounter == int.MaxValue)
      {
        m_ShakeCounter = 0;
      }

      return result;
    }


    /// Stop a perpetual screenshake
    public bool StopShake(int shakeId)
    {
      // Find shake
      for (int i = m_CurrentShakes.Count - 1; i >= 0; --i)
      {
        ShakeInstance shake = m_CurrentShakes[i];

        if (shake.shakeId == shakeId)
        {
          shake.StopShake();
          m_CurrentShakes[i] = shake;
          return true;
        }
      }

      return false;
    }


    /// Process and accumulate each shake
    protected virtual void ProcessShake(ref ShakeInstance shake, ref Vector2 shakeVector)
    {
      if (shake.maxDuration > 0)
      {
        shake.duration = Mathf.Clamp(shake.duration + Time.deltaTime, 0, shake.maxDuration);
      }

      ShakeSettings settings = m_ShakingCamera.orthographic ? m_OrthographicSettings : m_PerspectiveSettings;
      float magnitude = CalculateShakeMagnitude(ref shake, settings);
      Vector2 additionalShake = CalculateRandomVector(ref shake, settings);

      shakeVector += additionalShake * magnitude;
    }


    private float CalculateShakeMagnitude(ref ShakeInstance shake, ShakeSettings currentSettings)
    {
      float t = shake.normalizedProgress;

      float noise = Mathf.PerlinNoise(Time.realtimeSinceStartup * m_MagnitudeNoiseScale, shake.duration);
      // Rescale noise so it shakes primarily towards direction rather than in both directions
      // This changes the noise range from [1,-1] to [1, -0.2],
      noise *= 0.6f + 0.4f;

      return Mathf.Lerp(shake.magnitude, 0, t) * noise * currentSettings.maxShake;
    }


    private Vector2 CalculateRandomVector(ref ShakeInstance shake, ShakeSettings currentSettings)
    {
      float noise = Mathf.PerlinNoise(Time.realtimeSinceStartup * m_DirectionNoiseScale, shake.duration);
      float deviation = noise * shake.magnitude * currentSettings.maxAngle;

      return shake.direction.Rotate(deviation);
    }
  }
}