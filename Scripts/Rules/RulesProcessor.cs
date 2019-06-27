using System.Collections.Generic;
using UnityEngine;
using Tanks.TankControllers;
using Tanks.UI;

namespace Tanks.Rules
{
  public class RulesProcessor : MonoBehaviour
  {
    //The transition time from once the game ends until the game exits
    public static float s_EndGameTime = 10f;

    //The scene in the menu to return to
    [SerializeField]
    protected MenuPage m_ReturnPage;

    public MenuPage returnPage
    {
      get { return m_ReturnPage; }
    }

    //Reference to the game manager
    protected GameManager m_GameManager;

    //Winning tank - null if no winner
    protected TankManager m_Winner;

    //Is the match over
    protected bool m_MatchOver = false;

    //If the match is over
    public bool matchOver
    {
      get { return m_MatchOver; }
    }

    //The color provider - this is used in the lobby to provide different color selection implementation based on the game mode (i.e. individual versus team-based modes)
    protected IColorProvider m_ColorProvider;

    //The score target - how many points (kills or team kills) before the game is won
    public virtual int scoreTarget
    {
      get { return 0; }
    }

    //Whether the game can be started - Offline (single player) game modes implement this to prevent the game from being started until a button is pressed
    public virtual bool canStartGame
    {
      get { return true; }
    }

    //Check if the game has a winner
    public virtual bool hasWinner
    {
      get { return m_Winner != null; }
    }

    //The id of the winner.
    public virtual string winnerId
    {
      get
      {
        return m_Winner.playerTankType.id;
      }
    }

    // Gets the round message.
    public virtual string GetRoundMessage()
    {
      return string.Empty;
    }

    // Sets the game manager.
    public void SetGameManager(GameManager gameManager)
    {
      if (this.m_GameManager == null)
      {
        this.m_GameManager = gameManager;
      }
    }

    // Determines whether it is end of round.
    public virtual bool IsEndOfRound()
    {
      return false;
    }

    // Function called on round start
    public virtual void StartRound()
    {
    }

    // <summary>
    // Called on Match end
    // </summary>
    public virtual void MatchEnd()
    {
    }

    // Handles the death of a tank
    public virtual void TankDies(TankManager tank)
    {
      m_GameManager.HandleKill(tank);
    }

    // Handles the killer score - this different per game mode
    public virtual void HandleKillerScore(TankManager killer, TankManager killed)
    {
    }

    // <summary>
    // Handles the player's suicide - this different per game mode
    // </summary>
    // <param name="killer">The tank that kill themself</param>
    public virtual void HandleSuicide(TankManager killer)
    {
    }

    // Called when a tank disconnects
    public virtual void TankDisconnected(TankManager tank)
    {
    }

    // Handles the round end.
    public virtual void HandleRoundEnd()
    {
    }

    // Gets the round end text.
    public virtual string GetRoundEndText()
    {
      return string.Empty;
    }

    // Returns elements for constructing the leaderboard
    public virtual List<LeaderboardElement> GetLeaderboardElements()
    {
      List<LeaderboardElement> leaderboardElements = new List<LeaderboardElement>();

      List<TankManager> matchTanks = GameManager.s_Tanks;
      int tankCount = matchTanks.Count;

      for (int i = 0; i < tankCount; ++i)
      {
        TankManager currentTank = matchTanks[i];
        LeaderboardElement leaderboardElement = new LeaderboardElement(currentTank.playerName, currentTank.playerColor, currentTank.score);
        leaderboardElements.Add(leaderboardElement);
      }

      leaderboardElements.Sort(LeaderboardSort);
      return leaderboardElements;
    }

    // Used for sorting the leaderboard
    protected int LeaderboardSort(LeaderboardElement player1, LeaderboardElement player2)
    {
      return player2.score - player1.score;
    }

    // Gets the color provider.
    public IColorProvider GetColorProvider()
    {
      SetupColorProvider();
      return m_ColorProvider;
    }

    // Setups the color provider.
    protected virtual void SetupColorProvider()
    {
      if (m_ColorProvider == null)
      {
        m_ColorProvider = new PlayerColorProvider();
      }
    }

    // Handles bailing (i.e. leaving the game)
    public virtual void Bail()
    {
      m_GameManager.ExitGame(m_ReturnPage);
    }

    // Handles the game being complete (including the transitions)
    public virtual void CompleteGame()
    {
      m_GameManager.ExitGame(m_ReturnPage);
    }

    // Gets the rank of a player given the tank index
    public virtual int GetRank(int tankIndex)
    {
      return tankIndex + 1;
    }

    // Gets the award text based on the rank
    public virtual string GetAwardText(int rank)
    {
      string[] rankSuffix = new string[] { "st", "nd", "rd", "th" };
      return string.Format("You ranked {0}{1}", rank, rankSuffix[rank - 1]);
    }

    // Gets the award amount based on the rank
    public virtual int GetAwardAmount(int rank)
    {
      return Mathf.FloorToInt(100 / Mathf.Pow(2f, (float)(rank - 1)));
    }
  }
}