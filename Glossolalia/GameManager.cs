using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Glossolalia
{
   /// <summary>
   /// Основной менеджер игры, координирующий все игровые системы
   /// </summary>
   public class GameManager : IDisposable
   {
      #region Константы

      private const double WORD_SPAWN_INTERVAL = 2;
      private const int WORDS_FOR_SPEED_INCREASE = 10;
      private const double SPEED_INCREASE_MULTIPLIER = 1.01;
      private const double SPAWN_DECREASE_MULTIPLIER = 0.99;
      private const double TARGET_FPS = 60.0;
      private const double TARGET_FRAME_TIME = 1.0 / TARGET_FPS;

      #endregion

      #region Зависимости

      private readonly Canvas gameCanvas;
      private readonly GameStateManager gameStateManager;
      private readonly ScoreManager scoreManager;
      private readonly BonusManager bonusManager;
      private readonly SynonymDictionary synonymDictionary;

      #endregion

      #region Подсистемы

      private WordSpawnManager spawnManager;
      private WordMovementManager movementManager;
      private InputHandler inputHandler;
      private readonly AdditionalInputSystem additionalInputSystem;

      #endregion

      #region Состояние игры

      private readonly List<FallingWord> activeWords;
      private readonly Random random;

      private double baseWordSpeed = SpeedSettings.DEFAULT_SPEED;
      private DispatcherTimer wordSpawnTimer;
      private int destroyedWordsCount;
      private double currentSpawnInterval;
      private string lastDestroyedWord;

      private bool isFrozen;
      private bool isRegisterCaseActive;

      private DispatcherTimer gameLoopTimer;
      private DateTime lastFrameTime;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор игрового менеджера
      /// </summary>
      /// <param name="gameCanvas">Холст для отображения слов</param>
      /// <param name="gameStateManager">Менеджер состояния игры</param>
      /// <param name="scoreManager">Менеджер счета</param>
      /// <param name="bonusManager">Менеджер бонусов</param>
      /// <param name="synonymDictionary">Словарь синонимов</param>
      public GameManager(Canvas gameCanvas, GameStateManager gameStateManager,
                        ScoreManager scoreManager, BonusManager bonusManager,
                        SynonymDictionary synonymDictionary)
      {
         this.gameCanvas = gameCanvas;
         this.gameStateManager = gameStateManager;
         this.scoreManager = scoreManager;
         this.bonusManager = bonusManager;
         this.synonymDictionary = synonymDictionary;

         random = new Random();
         activeWords = new List<FallingWord>();

         additionalInputSystem = new AdditionalInputSystem(synonymDictionary);
         additionalInputSystem.WordDestroyedByAdditionalSystem += OnWordDestroyedByAdditionalSystem;

         InitializeSubsystems();
         InitializeGameLoop();
      }

      /// <summary>
      /// Инициализирует подсистемы игры
      /// </summary>
      private void InitializeSubsystems()
      {
         spawnManager = new WordSpawnManager(gameCanvas, activeWords, synonymDictionary, bonusManager);
         movementManager = new WordMovementManager(activeWords, gameCanvas);
         inputHandler = new InputHandler(scoreManager, activeWords);

         inputHandler.WordFullySelected += OnWordFullySelected;
      }

      /// <summary>
      /// Инициализирует игровой цикл
      /// </summary>
      private void InitializeGameLoop()
      {
         gameLoopTimer = new DispatcherTimer(DispatcherPriority.Render);
         gameLoopTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
         gameLoopTimer.Tick += GameLoopTimer_Tick;

         wordSpawnTimer = new DispatcherTimer
         {
            Interval = TimeSpan.FromSeconds(WORD_SPAWN_INTERVAL)
         };
         wordSpawnTimer.Tick += WordSpawnTimer_Tick;
      }

      #endregion

      #region Обработчики событий

      /// <summary>
      /// Обработчик игрового цикла
      /// </summary>
      private void GameLoopTimer_Tick(object sender, EventArgs e)
      {
         if (gameStateManager.CurrentState != GameStateManager.GameState.Running || isFrozen)
         {
            return;
         }

         UpdateGameFrame();
      }

      /// <summary>
      /// Обработчик таймера создания слов
      /// </summary>
      private void WordSpawnTimer_Tick(object sender, EventArgs e)
      {
         if (isFrozen) return;

         SpawnWord();
         UpdateSpawnInterval();
      }

      /// <summary>
      /// Обработчик полного выделения слова
      /// </summary>
      private void OnWordFullySelected(FallingWord word)
      {
         DestroyWord(word);

         if (!word.IsBonus)
         {
            destroyedWordsCount++;
            CheckSpeedIncrease();

            if (additionalInputSystem.IsActive)
            {
               additionalInputSystem.NotifyWordDestroyedByMainSystem(word);
            }

            lastDestroyedWord = word.Word;
         }
      }

      /// <summary>
      /// Обработчик уничтожения слова дополнительной системой
      /// </summary>
      private void OnWordDestroyedByAdditionalSystem(int totalScore)
      {
         scoreManager.AddPoints(totalScore);
         destroyedWordsCount++;
         CheckSpeedIncrease();
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Обрабатывает ввод символа с клавиатуры
      /// </summary>
      public void HandleCharInput(char pressedChar)
      {
         if (gameStateManager.CurrentState != GameStateManager.GameState.Running)
            return;

         if (!InputHandler.IsValidInputChar(pressedChar))
            return;

         bool synonymBonusActive = bonusManager.CurrentBonusType == "синоним";
         bool antonymBonusActive = bonusManager.CurrentBonusType == "антоним";
         bool additionalSystemActive = synonymBonusActive || antonymBonusActive;

         bool mainSystemResult = inputHandler.HandleCharInput(pressedChar, isRegisterCaseActive);
         bool additionalSystemResult = true;

         if (additionalSystemActive)
         {
            additionalSystemResult = additionalInputSystem.HandleCharInput(
                pressedChar, activeWords, scoreManager);
         }

         ProcessInputResult(mainSystemResult, additionalSystemResult, additionalSystemActive);
      }

      /// <summary>
      /// Устанавливает базовую скорость слов
      /// </summary>
      public void SetBaseWordSpeed(double speed)
      {
         double oldSpeed = baseWordSpeed;
         baseWordSpeed = SpeedSettings.ClampSpeed(speed);
         double speedRatio = baseWordSpeed / oldSpeed;

         UpdateAllWordSpeeds(speedRatio);
      }

      /// <summary>
      /// Начинает игру
      /// </summary>
      public void StartGame()
      {
         ClearGameField();
         ResetGameState();
         additionalInputSystem.Reset();
         additionalInputSystem.Deactivate();
         scoreManager.Reset();

         StartGameTimers();
         SpawnWord();
      }

      /// <summary>
      /// Приостанавливает игру
      /// </summary>
      public void PauseGame()
      {
         gameLoopTimer.Stop();
         wordSpawnTimer.Stop();
      }

      /// <summary>
      /// Возобновляет игру
      /// </summary>
      public void ResumeGame()
      {
         gameLoopTimer.Start();

         if (!isFrozen)
         {
            wordSpawnTimer.Start();
         }
      }

      /// <summary>
      /// Останавливает игру
      /// </summary>
      public void StopGame()
      {
         gameLoopTimer.Stop();
         wordSpawnTimer.Stop();
         ClearGameField();
         additionalInputSystem.Deactivate();
      }

      /// <summary>
      /// Активирует дополнительную систему (синонимы/антонимы)
      /// </summary>
      public void ActivateAdditionalSystem(string bonusType)
      {
         additionalInputSystem.Activate(bonusType, activeWords);
      }

      /// <summary>
      /// Деактивирует дополнительную систему
      /// </summary>
      public void DeactivateAdditionalSystem()
      {
         additionalInputSystem.Deactivate();
      }

      /// <summary>
      /// Активирует или деактивирует бонус регистра
      /// </summary>
      /// <param name="activate">True для активации, False для деактивации</param>
      public void ActivateRegisterCase(bool activate)
      {
         isRegisterCaseActive = activate;
      }

      /// <summary>
      /// Умножает скорость всех слов на множитель
      /// </summary>
      public void MultiplyAllWordsSpeed(double multiplier)
      {
         foreach (var word in activeWords)
         {
            if (!word.IsDestroyed)
            {
               word.SetSpeed(word.BaseSpeed * multiplier);
            }
         }
      }

      /// <summary>
      /// Восстанавливает оригинальную скорость всех слов
      /// </summary>
      public void RestoreOriginalSpeed()
      {
         foreach (var word in activeWords)
         {
            if (!word.IsDestroyed)
            {
               word.RestoreOriginalSpeed();
            }
         }
      }

      /// <summary>
      /// Замораживает все слова
      /// </summary>
      public void FreezeAllWords()
      {
         isFrozen = true;
         MultiplyAllWordsSpeed(0);
         wordSpawnTimer.Stop();
         gameLoopTimer.Stop();
      }

      /// <summary>
      /// Размораживает все слова
      /// </summary>
      public void UnfreezeAllWords()
      {
         foreach (var word in activeWords)
         {
            if (!word.IsDestroyed)
            {
               word.RestoreOriginalSpeed();
            }
         }

         isFrozen = false;

         if (gameStateManager.CurrentState == GameStateManager.GameState.Running)
         {
            wordSpawnTimer.Start();
            gameLoopTimer.Start();
            lastFrameTime = DateTime.Now;
         }
      }

      /// <summary>
      /// Освобождает ресурсы
      /// </summary>
      public void Dispose()
      {
         gameLoopTimer.Stop();
         wordSpawnTimer.Stop();

         if (inputHandler != null)
         {
            inputHandler.WordFullySelected -= OnWordFullySelected;
         }

         if (additionalInputSystem != null)
         {
            additionalInputSystem.WordDestroyedByAdditionalSystem -= OnWordDestroyedByAdditionalSystem;
         }
      }

      #endregion

      #region Приватные методы

      /// <summary>
      /// Обновляет игровой кадр
      /// </summary>
      private void UpdateGameFrame()
      {
         DateTime currentTime = DateTime.Now;
         double deltaTime = (currentTime - lastFrameTime).TotalSeconds;
         lastFrameTime = currentTime;

         if (deltaTime > 0.1) deltaTime = TARGET_FRAME_TIME;

         if (deltaTime >= TARGET_FRAME_TIME)
         {
            movementManager.UpdateWords(deltaTime);
            movementManager.CheckAndResolveCollisions();

            if (movementManager.CheckGameOver())
            {
               gameStateManager.GameOver();
            }
         }
      }

      /// <summary>
      /// Создает новое слово на игровом поле
      /// </summary>
      private void SpawnWord()
      {
         string wordText = synonymDictionary.GetRandomWord(random);
         var fallingWord = spawnManager.SpawnWord(wordText, baseWordSpeed);

         if (fallingWord != null)
         {
            activeWords.Add(fallingWord);

            if (additionalInputSystem.IsActive)
            {
               additionalInputSystem.UpdateTrackedWords(activeWords);
            }
         }
      }

      /// <summary>
      /// Обновляет интервал создания слов
      /// </summary>
      private void UpdateSpawnInterval()
      {
         double deviation = (random.NextDouble() * 1.0) - 0.5;
         var interval = WORD_SPAWN_INTERVAL * (1 + deviation);
         wordSpawnTimer.Interval = TimeSpan.FromSeconds(Math.Max(0.5, interval));
      }

      /// <summary>
      /// Обрабатывает результат ввода с клавиатуры
      /// </summary>
      private void ProcessInputResult(bool mainSystemResult, bool additionalSystemResult, bool additionalSystemActive)
      {
         bool shouldAwardPoints = (mainSystemResult && additionalSystemResult) ||
                                 (mainSystemResult && !additionalSystemActive) ||
                                 (additionalSystemResult && additionalSystemActive);

         if (shouldAwardPoints)
         {
            int points = inputHandler.CalculatePoints(shouldAwardPoints, isRegisterCaseActive);
            scoreManager.AddPoints(points);
            scoreManager.IncreaseMultiplier();
         }
         else if (!additionalSystemActive && !mainSystemResult)
         {
            scoreManager.ResetMultiplier();
         }
         else if (additionalSystemActive && !mainSystemResult && !additionalSystemResult)
         {
            scoreManager.ResetMultiplier();
         }
      }

      /// <summary>
      /// Уничтожает слово
      /// </summary>
      private void DestroyWord(FallingWord word)
      {
         if (word.IsBonus)
         {
            ActivateBonus(word.BonusType);
         }

         word.Destroy();
      }

      /// <summary>
      /// Активирует бонус
      /// </summary>
      private void ActivateBonus(string bonusType)
      {
         if (bonusManager.IsBonusWord(bonusType))
         {
            double duration = bonusManager.GetBonusDuration(bonusType);
            bonusManager.StartBonus(bonusType, duration);
         }
      }

      /// <summary>
      /// Проверяет необходимость увеличения скорости
      /// </summary>
      private void CheckSpeedIncrease()
      {
         if (destroyedWordsCount >= WORDS_FOR_SPEED_INCREASE)
         {
            baseWordSpeed = Math.Min(baseWordSpeed * SPEED_INCREASE_MULTIPLIER, 50);
            wordSpawnTimer.Interval = TimeSpan.FromSeconds(currentSpawnInterval * SPAWN_DECREASE_MULTIPLIER);
            destroyedWordsCount = 0;

            UpdateAllWordSpeeds();
         }
      }

      /// <summary>
      /// Обновляет скорость всех слов
      /// </summary>
      private void UpdateAllWordSpeeds(double speedRatio = 1.0)
      {
         foreach (var word in activeWords)
         {
            if (!word.IsDestroyed)
            {
               word.SetSpeed(word.BaseSpeed * speedRatio);
               word.UpdateBaseSpeed(word.BaseSpeed * speedRatio);
            }
         }
      }

      /// <summary>
      /// Очищает игровое поле
      /// </summary>
      private void ClearGameField()
      {
         foreach (var word in activeWords)
         {
            word.Destroy();
         }
         activeWords.Clear();
         gameCanvas.Children.Clear();
      }

      /// <summary>
      /// Сбрасывает состояние игры
      /// </summary>
      private void ResetGameState()
      {
         destroyedWordsCount = 0;
         currentSpawnInterval = WORD_SPAWN_INTERVAL;
         lastDestroyedWord = string.Empty;
         isFrozen = false;
         isRegisterCaseActive = false;
      }

      /// <summary>
      /// Запускает игровые таймеры
      /// </summary>
      private void StartGameTimers()
      {
         lastFrameTime = DateTime.Now;
         gameLoopTimer.Start();
         wordSpawnTimer.Start();
      }

      #endregion
   }
}