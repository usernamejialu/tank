using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Tanks.TankControllers;
using Tanks.Pickups;
using Tanks.Rules;
using Tanks.UI;
using Tanks.Map;
using Tanks.Hazards;
using Tanks.Explosions;
using Tanks.Rules.SinglePlayer;
using TanksNetworkManager = Tanks.Networking.NetworkManager;
using TanksNetworkPlayer = Tanks.Networking.NetworkPlayer;

namespace Tanks
{
  public enum GameState
  {
    Inactive,
    TimedTransition,
    StartUp,
    Preplay,
    Playing,
    RoundEnd,
    EndGame,
    PostGame
  }

  public class GameManager : MonoBehaviour
  {
    // Singleton reference
    static public GameManager s_Instance;

    // This list is ordered descending by player score.
    static public List<TankManager> s_Tanks = new List<TankManager>();

    [SerializeField]
    protected ExplosionManager m_ExplosionManagerPrefab;

    [SerializeField]
    protected EndGameModal m_DefaultSinglePlayerModal;

    [SerializeField]
    protected Transform m_EndGameUiParent;

    [SerializeField]
    protected HUDSinglePlayer m_SinglePlayerHud;

    [SerializeField]
    protected FadingGroup m_EndScreen;

    protected GameSettings m_GameSettings;

    protected GameState m_State = GameState.Inactive;

    public GameState state
    {
      get { return m_State; }
    }

    private float m_TransitionTime = 0f;
    private GameState m_NextState;

    protected bool m_GameIsFinished = false;

    //Various UI references to hide the screen between rounds.
    private FadingGroup m_LoadingScreen;

    //The local player
    private TankManager m_LocalPlayer;

    private int m_LocalPlayerNumber = 0;

    //Crate spawners
    private List<CrateSpawner> m_CrateSpawnerList;
    //Pickups
    private List<PickupBase> m_PowerupList;
    //Hazards
    private List<LevelHazard> m_HazardList;
    //if the tanks are active
    private bool m_HazardsActive;

    //The rules processor being used
    private RulesProcessor m_RulesProcessor;

    public RulesProcessor rulesProcessor
    {
      get { return m_RulesProcessor; }
    }

    //The end game modal that is actually used
    protected EndGameModal m_EndGameModal;

    //if everyone has bailed
    private bool m_HasEveryoneBailed = false;

    public bool hasEveryoneBailed
    {
      get
      {
        return m_HasEveryoneBailed;
      }
    }

    //The modal displayed at the beginning of the game
    protected StartGameModal m_StartGameModal;

    //Cached network manager
    private TanksNetworkManager m_NetManager;

    //Cached reference to singleton InGameLeaderboardModal
    //protected InGameLeaderboardModal m_Leaderboard;

    //Cached reference to singleton AnnouncerModal
    protected AnnouncerModal m_Announcer;


    private void Awake()
    {
      s_Instance = this;

      // Sets up the lists
      m_CrateSpawnerList = new List<CrateSpawner>();
      m_PowerupList = new List<PickupBase>();
      m_HazardList = new List<LevelHazard>();

      // Cache the NetworkManager instance
      m_NetManager = TanksNetworkManager.s_Instance;
    }

    private void OnDestroy()
    {
      s_Tanks.Clear();
    }

    private void Start()
    {
      m_State = GameState.StartUp;

      m_GameSettings = GameSettings.s_Instance;

      m_RulesProcessor = Instantiate<RulesProcessor>(m_GameSettings.mode.rulesProcessor);
      m_RulesProcessor.SetGameManager(this);

      if (m_ExplosionManagerPrefab != null)
      {
        ExplosionManager explosionManager = Instantiate<ExplosionManager>(m_ExplosionManagerPrefab);
        NetworkServer.Spawn(explosionManager.gameObject);
      }

      SetupSinglePlayerModals();
    }

    // Setups the single player modals.
    private void SetupSinglePlayerModals()
    {
      OfflineRulesProcessor offlineRulesProcessor = m_RulesProcessor as OfflineRulesProcessor;
      EndGameModal endGame = offlineRulesProcessor.endGameModal;

      if (endGame == null)
      {
        endGame = m_DefaultSinglePlayerModal;
      }

      InstantiateEndGameModal(endGame);

      if (m_EndGameModal != null)
      {
        m_EndGameModal.SetRulesProcessor(m_RulesProcessor);
      }

      // Handle start game modal
      if (offlineRulesProcessor.startGameModal != null)
      {
        m_StartGameModal = Instantiate(offlineRulesProcessor.startGameModal);
        m_StartGameModal.transform.SetParent(m_EndGameUiParent, false);
        m_StartGameModal.gameObject.SetActive(false);
        m_StartGameModal.Setup(offlineRulesProcessor);
        m_StartGameModal.Show();
        LazyLoadLoadingPanel();
        m_LoadingScreen.transform.SetAsLastSibling();
      }
    }

    // Instantiates the end game modal.
    private void InstantiateEndGameModal(EndGameModal endGame)
    {
      if (endGame == null)
      {
        return;
      }

      if (m_EndGameModal != null)
      {
        Destroy(m_EndGameModal);
        m_EndGameModal = null;
      }

      m_EndGameModal = Instantiate<EndGameModal>(endGame);
      m_EndGameModal.transform.SetParent(m_EndGameUiParent, false);
      m_EndGameModal.gameObject.SetActive(false);
    }

    // Add a tank from the lobby hook
    static public void AddTank(TankManager tank)
    {
      if (s_Tanks.IndexOf(tank) == -1)
      {
        s_Tanks.Add(tank);
        tank.MoveToSpawnLocation(SpawnManager.s_Instance.GetSpawnPointTransformByIndex(tank.playerNumber));
      }
    }


    // Removes the tank.
    public void RemoveTank(TankManager tank)
    {
      int tankIndex = s_Tanks.IndexOf(tank);

      if (tankIndex >= 0)
      {
        s_Tanks.RemoveAt(tankIndex);
      }
    }


    // Exits the game.
    public void ExitGame(MenuPage returnPage)
    {
      for (int i = 0; i < s_Tanks.Count; i++)
      {
        TankManager tank = s_Tanks[i];
        if (tank != null)
        {
          TanksNetworkPlayer player = tank.player;
          if (player != null)
          {
            player.tank = null;
          }

          NetworkServer.Destroy(s_Tanks[i].gameObject);
        }
      }

      s_Tanks.Clear();
      m_NetManager.ReturnToMenu(returnPage);
    }

    // Adds the crate spawner.
    public void AddCrateSpawner(CrateSpawner newCrate)
    {
      m_CrateSpawnerList.Add(newCrate);
    }

    // Adds the powerup.
    public void AddPowerup(PickupBase powerUp)
    {
      m_PowerupList.Add(powerUp);
    }

    // Removes the powerup.
    public void RemovePowerup(PickupBase powerup)
    {
      m_PowerupList.Remove(powerup);
    }

    // Adds the hazard.
    public void AddHazard(LevelHazard hazard)
    {
      m_HazardList.Add(hazard);
    }

    // Removes the hazard.
    public void RemoveHazard(LevelHazard hazard)
    {
      m_HazardList.Remove(hazard);
    }

    // Gets the local player ID.
    public int GetLocalPlayerId()
    {
      return m_LocalPlayerNumber;
    }

    protected void Update()
    {
      HandleStateMachine();
    }


    // Handles the state machine.
    protected void HandleStateMachine()
    {
      switch (m_State)
      {
        case GameState.StartUp:
          StartUp();
          break;
        case GameState.TimedTransition:
          TimedTransition();
          break;
        case GameState.Preplay:
          Preplay();
          break;
        case GameState.Playing:
          Playing();
          break;
        case GameState.RoundEnd:
          RoundEnd();
          break;
        case GameState.EndGame:
          EndGame();
          break;
        default:
          break;
      }
    }

    // State up state function
    protected void StartUp()
    {
      LazyLoadLoadingPanel();
      m_LoadingScreen.StartFade(Fade.Out, 0.5f, SinglePlayerLoadedEvent);
      m_State = GameState.Inactive;
    }

    protected void SinglePlayerLoadedEvent()
    {
      m_State = GameState.Preplay;
    }

    // Time transition state function
    protected void TimedTransition()
    {
      m_TransitionTime -= Time.deltaTime;
      if (m_TransitionTime <= 0f)
      {
        m_State = m_NextState;
      }
    }

    // Preplay state function
    protected void Preplay()
    {
      RoundStarting();
      EnableTankControl();
      LazyLoadAnnouncer();
      m_Announcer.Hide();
    }

    // Playing state function
    protected void Playing()
    {
      // Activate hazards the second we enter the gameplay loop
      if (!m_HazardsActive)
      {
        ActivateHazards();
        m_HazardsActive = true;
      }

      if (m_RulesProcessor.IsEndOfRound())
      {
        m_State = GameState.RoundEnd;
      }
    }

    // RoundEnd state function
    protected void RoundEnd()
    {
      if (m_CrateSpawnerList != null && m_CrateSpawnerList.Count != 0)
      {
        m_CrateSpawnerList[0].DeactivateSpawner();
      }

      m_RulesProcessor.HandleRoundEnd();

      if (m_RulesProcessor.matchOver)
      {
        SetTimedTransition(GameState.EndGame, 1f);
      }
    }

    // EndGame state function
    protected void EndGame()
    {
      m_GameIsFinished = true;

      GameEnd();

      m_RulesProcessor.MatchEnd();

      m_State = GameState.PostGame;
    }


    // Sets the timed transition
    protected void SetTimedTransition(GameState nextState, float transitionTime)
    {
      this.m_NextState = nextState;
      this.m_TransitionTime = transitionTime;
      m_State = GameState.TimedTransition;
    }


    // Starts the round
    private void RoundStarting()
    {
      //we notify all clients that the round is starting
      m_RulesProcessor.StartRound();
      ResetAllTanks();
      DisableTankControl();
      InitHudAndLocalPlayer();
      EnableHUD();
      CleanupPowerups();
      ResetHazards();

      m_HazardsActive = false;

      if (m_CrateSpawnerList != null && m_CrateSpawnerList.Count != 0)
      {
        m_CrateSpawnerList[0].ActivateSpawner();
      }

      SetTimedTransition(GameState.Playing, 2f);
    }

    // Cleanups the powerups
    private void CleanupPowerups()
    {
      for (int i = (m_PowerupList.Count - 1); i >= 0; i--)
      {
        if (m_PowerupList[i] != null)
        {
          NetworkServer.Destroy(m_PowerupList[i].gameObject);
        }
      }
    }

    // Resets the hazards
    private void ResetHazards()
    {
      for (int i = 0; i < m_HazardList.Count; i++)
      {
        m_HazardList[i].ResetHazard();
      }
    }

    // Activates the hazards
    private void ActivateHazards()
    {
      for (int i = 0; i < m_HazardList.Count; i++)
      {
        m_HazardList[i].ActivateHazard();
      }
    }

    // Enables the HUD
    private void EnableHUD()
    {
      HUDController.s_Instance.SetHudEnabled(true);
    }


    // Fades the out end round screen
    private void FadeOutEndRoundScreen()
    {
      m_EndScreen.StartFade(Fade.Out, 2f);
    }

    // Game End
    private void GameEnd()
    {
      HUDController.s_Instance.SetHudEnabled(false);
      DisableTankControl();
      m_GameIsFinished = true;
      if (m_EndGameModal != null)
      {
        m_EndGameModal.Show();
      }

      if (Everyplay.IsRecording())
      {
        int tankIndex = s_Tanks.IndexOf(m_LocalPlayer);
        if (tankIndex >= 0)
        {
          Everyplay.SetMetadata("final_position", tankIndex + 1);
        }
        Everyplay.StopRecording();
      }

    

      LazyLoadLoadingPanel();
      m_LoadingScreen.transform.SetAsLastSibling();
    }


    // Handles the kill
    public void HandleKill(TankManager killed)
    {
      TankManager killer = GetTankByPlayerNumber(killed.health.lastDamagedByPlayerNumber);
      string explosionId = killed.health.lastDamagedByExplosionId;

      if (killer != null)
      {
        if (killer.playerNumber == killed.playerNumber)
        {
          m_RulesProcessor.HandleSuicide(killer);
        }
        else
        {
          m_RulesProcessor.HandleKillerScore(killer, killed);
        }
      }
    }

    // This function is used to turn all the tanks back on and reset their positions and properties
    private void ResetAllTanks()
    {
      for (int i = 0; i < s_Tanks.Count; i++)
      {
        s_Tanks[i].Reset(SpawnManager.s_Instance.GetSpawnPointTransformByIndex(s_Tanks[i].playerNumber));
      }
    }

    // Convenience function for showing the leaderboard
    //public void ShowLeaderboard(TankManager tank, string heading)
    //{
    //  if (tank != null && !tank.removedTank && tank.hasAuthority && !m_GameIsFinished)
    //  {
    //    LazyLoadLeaderboard();
    //    m_Leaderboard.Show(heading);
    //  }
    //}

    // Convenience function for hiding the leaderboard
    //public void ClearLeaderboard(TankManager tank)
    //{
    //  if (!tank.removedTank && tank.hasAuthority && !m_GameIsFinished)
    //  {
    //    LazyLoadLeaderboard();
    //    m_Leaderboard.Hide();
    //  }
    //}

    // Enables the tank control
    public void EnableTankControl()
    {
      for (int i = 0; i < s_Tanks.Count; i++)
      {
        s_Tanks[i].EnableControl();
      }
    }

    // Disables the tank control
    public void DisableTankControl()
    {
      for (int i = 0; i < s_Tanks.Count; i++)
      {
        s_Tanks[i].DisableControl();
      }
    }

    private void InitHudAndLocalPlayer()
    {
      for (int i = 0; i < s_Tanks.Count; i++)
      {
        m_LocalPlayer = s_Tanks[i];
        HUDController.s_Instance.InitHudPlayer(s_Tanks[i]);
        m_LocalPlayerNumber = s_Tanks[i].playerNumber;
      }
    }

    // Setups the single player HUD
    public void SetupSinglePlayerHud()
    {
      if (m_SinglePlayerHud == null)
      {
        return;
      }

      m_SinglePlayerHud.ShowHud(m_RulesProcessor);
    }

    // Gets the tank by player number
    private TankManager GetTankByPlayerNumber(int playerNumber)
    {
      int length = s_Tanks.Count;
      for (int i = 0; i < length; i++)
      {
        TankManager tank = s_Tanks[i];
        if (tank.playerNumber == playerNumber)
        {
          return tank;
        }
      }
      return null;
    }

    // Lazy loads the loading panel
    public void LazyLoadLoadingPanel()
    {
      if (m_LoadingScreen != null)
      {
        return;
      }
      m_LoadingScreen = LoadingModal.s_Instance.fader;
    }

    // Lazy loads the leaderboard
    //protected void LazyLoadLeaderboard()
    //{
    //  if (m_Leaderboard != null)
    //  {
    //    return;
    //  }
    //  m_Leaderboard = InGameLeaderboardModal.s_Instance;
    //}

    // Lazy loads the announcer
    protected void LazyLoadAnnouncer()
    {
      if (m_Announcer != null)
      {
        return;
      }
      m_Announcer = AnnouncerModal.s_Instance;
    }
  }
}