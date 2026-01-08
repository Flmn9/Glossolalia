using System;
using System.Windows.Threading;

namespace Glossolalia
{
   /// <summary>
   /// Менеджер состояния игры
   /// </summary>
   public class GameStateManager
   {
      #region Перечисления

      /// <summary>
      /// Состояния игры
      /// </summary>
      public enum GameState { Stopped, Running, Paused, GameOver }

      #endregion

      #region Свойства

      /// <summary>
      /// Текущее состояние игры
      /// </summary>
      public GameState CurrentState { get; private set; }

      /// <summary>
      /// Текущее игровое время
      /// </summary>
      public TimeSpan GameTime { get; private set; }

      #endregion

      #region Поля

      private DispatcherTimer gameTimer;

      #endregion

      #region События

      /// <summary>
      /// Событие изменения игрового времени
      /// </summary>
      public event EventHandler<TimeSpan> GameTimeChanged;

      /// <summary>
      /// Событие изменения состояния игры
      /// </summary>
      public event EventHandler<GameState> GameStateChanged;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор. Инициализирует состояние игры
      /// </summary>
      public GameStateManager()
      {
         Initialize();
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Инициализирует начальное состояние игры
      /// </summary>
      public void Initialize()
      {
         GameTime = TimeSpan.Zero;
         CurrentState = GameState.Stopped;
         gameTimer = null;
      }

      /// <summary>
      /// Начинает новую игру
      /// </summary>
      public void StartNewGame()
      {
         GameTime = TimeSpan.Zero;
         SetState(GameState.Running);
         StartTimer();
      }

      /// <summary>
      /// Приостанавливает игру
      /// </summary>
      public void Pause()
      {
         if (CurrentState == GameState.Running)
         {
            SetState(GameState.Paused);
            gameTimer?.Stop();
         }
      }

      /// <summary>
      /// Возобновляет игру
      /// </summary>
      public void Resume()
      {
         if (CurrentState == GameState.Paused)
         {
            SetState(GameState.Running);
            gameTimer?.Start();
         }
      }

      /// <summary>
      /// Завершает игру
      /// </summary>
      public void GameOver()
      {
         SetState(GameState.GameOver);
         StopTimer();
      }

      /// <summary>
      /// Проверяет, находится ли игра в состоянии паузы
      /// </summary>
      /// <returns>True, если игра на паузе</returns>
      public bool IsGamePaused()
      {
         return CurrentState == GameState.Paused;
      }

      #endregion

      #region Приватные методы

      /// <summary>
      /// Запускает игровой таймер
      /// </summary>
      private void StartTimer()
      {
         gameTimer = new DispatcherTimer
         {
            Interval = TimeSpan.FromSeconds(1)
         };

         gameTimer.Tick += (s, e) =>
         {
            if (CurrentState == GameState.Running)
            {
               GameTime = GameTime.Add(TimeSpan.FromSeconds(1));
               GameTimeChanged?.Invoke(this, GameTime);
            }
         };

         gameTimer.Start();
         GameTimeChanged?.Invoke(this, GameTime);
      }

      /// <summary>
      /// Останавливает игровой таймер
      /// </summary>
      private void StopTimer()
      {
         gameTimer?.Stop();
         gameTimer = null;
      }

      /// <summary>
      /// Устанавливает новое состояние игры
      /// </summary>
      private void SetState(GameState newState)
      {
         CurrentState = newState;
         GameStateChanged?.Invoke(this, newState);
      }

      #endregion
   }
}