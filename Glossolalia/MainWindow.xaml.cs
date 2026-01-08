using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Glossolalia
{
   /// <summary>
   /// Главное окно приложения. Управляет основным интерфейсом и координацией работы менеджеров
   /// </summary>
   public partial class MainWindow : Window
   {
      #region Поля

      private GameManager gameManager;
      private GameStateManager gameStateManager;
      private ScoreManager scoreManager;
      private SettingsManager settingsManager;
      private UIManager uiManager;
      private BonusManager bonusManager;
      private SynonymDictionary synonymDictionary;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор главного окна. Инициализирует компоненты и создает менеджеры
      /// </summary>
      public MainWindow()
      {
         InitializeComponent();

         InitializeDependencies();
         InitializeManagers();
         InitializeEventHandlers();
         InitializeFocus();
      }

      #endregion

      #region Инициализация

      /// <summary>
      /// Инициализирует зависимости приложения
      /// </summary>
      private void InitializeDependencies()
      {
         var settings = AppSettings.Load();
         synonymDictionary = new SynonymDictionary("aRU.bin", "bRU.bin");
      }

      /// <summary>
      /// Инициализирует менеджеры приложения
      /// </summary>
      private void InitializeManagers()
      {
         gameStateManager = new GameStateManager();
         scoreManager = new ScoreManager();
         settingsManager = new SettingsManager(this, ResolutionComboBox, WindowModeComboBox, SpeedSlider, SpeedValueText);
         uiManager = new UIManager(SettingsScrollViewer, RulesScrollViewer,
             MainMenuContainer, GameContainer, ScoreText, TimeText,
             GlossolaliaLabel, GameCanvas, GameOverStackPanel, MultiplierText);
         bonusManager = new BonusManager(ProgressBar, BonusText);

         gameManager = new GameManager(GameCanvas, gameStateManager,
             scoreManager, bonusManager, synonymDictionary);

         InitializeSpeedSettings();
         InitializeBonusCallbacks();
      }

      /// <summary>
      /// Инициализирует настройки скорости
      /// </summary>
      private void InitializeSpeedSettings()
      {
         double initialSpeed = AppSettings.Load().SpeedSettings.WordSpeed;
         gameManager.SetBaseWordSpeed(initialSpeed);
         SpeedSlider.Value = initialSpeed;
         SpeedValueText.Text = $"{initialSpeed:F1}";
      }

      /// <summary>
      /// Инициализирует колбэки для бонусов
      /// </summary>
      private void InitializeBonusCallbacks()
      {
         bonusManager.SetSpeedCallbacks(
             gameManager.MultiplyAllWordsSpeed,
             gameManager.RestoreOriginalSpeed
         );

         bonusManager.SetFreezeCallbacks(
             gameManager.FreezeAllWords,
             gameManager.UnfreezeAllWords
         );

         bonusManager.SetRegisterCaseCallback(gameManager.ActivateRegisterCase);
         bonusManager.SetAdditionalSystemCallback(
             gameManager.ActivateAdditionalSystem,
             gameManager.DeactivateAdditionalSystem
         );
      }

      /// <summary>
      /// Инициализирует обработчики событий
      /// </summary>
      private void InitializeEventHandlers()
      {
         gameStateManager.GameStateChanged += OnGameStateChanged;
         gameStateManager.GameTimeChanged += OnGameTimeChanged;
         scoreManager.ScoreChanged += OnScoreChanged;
         scoreManager.MultiplierChanged += OnMultiplierChanged;

         settingsManager.Initialize();
         settingsManager.ApplySettings();
      }

      /// <summary>
      /// Инициализирует фокус окна для обработки клавиатуры
      /// </summary>
      private void InitializeFocus()
      {
         Focusable = true;
         Focus();
      }

      #endregion

      #region Методы управления игрой

      /// <summary>
      /// Обновляет скорость слов в игре
      /// </summary>
      public void UpdateWordSpeed(double speed)
      {
         gameManager.SetBaseWordSpeed(speed);
      }

      /// <summary>
      /// Начинает новую игру
      /// </summary>
      private void StartNewGame()
      {
         scoreManager.Reset();
         bonusManager.Reset();
         gameStateManager.StartNewGame();
         gameManager.StartGame();
         uiManager.ShowGame();
      }

      #endregion

      #region Обработчики событий

      /// <summary>
      /// Обработчик изменения значения ползунка скорости
      /// </summary>
      private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
      {
         if (settingsManager != null && SpeedSlider.IsLoaded)
         {
            double roundedSpeed = Math.Round(e.NewValue, 1);
            SpeedValueText.Text = $"{roundedSpeed:F1}";
            settingsManager.OnSpeedChanged(roundedSpeed);
         }
      }

      /// <summary>
      /// Обработчик изменения состояния игры
      /// </summary>
      private void OnGameStateChanged(object sender, GameStateManager.GameState state)
      {
         switch (state)
         {
            case GameStateManager.GameState.Running:
               uiManager.ShowGame();
               bonusManager.SetPaused(false);
               gameManager.ResumeGame();
               break;

            case GameStateManager.GameState.Paused:
               uiManager.ShowPauseMenu();
               bonusManager.SetPaused(true);
               gameManager.PauseGame();
               break;

            case GameStateManager.GameState.GameOver:
               uiManager.ShowGameOver();
               bonusManager.Reset();
               gameManager.StopGame();
               break;

            case GameStateManager.GameState.Stopped:
               scoreManager.Reset();
               bonusManager.Reset();
               uiManager.ShowMainMenu();
               gameManager.StopGame();
               break;
         }
      }

      /// <summary>
      /// Обработчик изменения счета
      /// </summary>
      private void OnScoreChanged(object sender, int score)
      {
         uiManager.UpdateScore(score);
      }

      /// <summary>
      /// Обработчик изменения множителя
      /// </summary>
      private void OnMultiplierChanged(object sender, int multiplier)
      {
         uiManager.UpdateMultiplier(multiplier);
      }

      /// <summary>
      /// Обработчик изменения игрового времени
      /// </summary>
      private void OnGameTimeChanged(object sender, TimeSpan time)
      {
         uiManager.UpdateTime(time);
      }

      #endregion

      #region Обработчики кнопок

      /// <summary>
      /// Обработчик нажатия кнопки "Играть"
      /// </summary>
      private void PlayButton_Click(object sender, RoutedEventArgs e)
      {
         switch (gameStateManager.CurrentState)
         {
            case GameStateManager.GameState.Stopped:
            case GameStateManager.GameState.GameOver:
               StartNewGame();
               break;

            case GameStateManager.GameState.Paused:
               gameStateManager.Resume();
               break;
         }
      }

      /// <summary>
      /// Обработчик нажатия кнопки паузы
      /// </summary>
      private void PauseButton_Click(object sender, RoutedEventArgs e)
      {
         gameStateManager.Pause();
      }

      /// <summary>
      /// Обработчик нажатия кнопки "Настройки"
      /// </summary>
      private void SettingsButton_Click(object sender, RoutedEventArgs e)
      {
         uiManager.ShowSettings();
      }

      /// <summary>
      /// Обработчик нажатия кнопки "Правила"
      /// </summary>
      private void RulesButton_Click(object sender, RoutedEventArgs e)
      {
         uiManager.ShowRules();
      }

      /// <summary>
      /// Обработчик нажатия кнопки "Выход"
      /// </summary>
      private void ExitButton_Click(object sender, RoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      /// <summary>
      /// Обработчик нажатия кнопки после окончания игры
      /// </summary>
      private void GameOverButton_Click(object sender, RoutedEventArgs e)
      {
         gameStateManager.GameOver();
      }

      #endregion

      #region Обработчики UI элементов

      /// <summary>
      /// Обработчик изменения выбора разрешения
      /// </summary>
      private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         if (ResolutionComboBox.SelectedItem is ComboBoxItem selectedItem)
         {
            settingsManager.OnResolutionChanged(selectedItem);
         }
      }

      /// <summary>
      /// Обработчик изменения выбора режима окна
      /// </summary>
      private void WindowModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         settingsManager.OnWindowModeChanged(WindowModeComboBox.SelectedIndex);
      }

      #endregion

      #region Обработчики клавиатуры

      /// <summary>
      /// Обработчик нажатия клавиш в окне
      /// </summary>
      private void Window_KeyDown(object sender, KeyEventArgs e)
      {
         // Обработка клавиши Escape для паузы/возобновления игры
         if (e.Key == Key.Escape)
         {
            e.Handled = true;

            if (GameContainer.Visibility == Visibility.Visible && !gameStateManager.IsGamePaused())
            {
               gameStateManager.Pause();
            }
            else if (gameStateManager.IsGamePaused() && MainMenuContainer.Visibility == Visibility.Visible)
            {
               gameStateManager.Resume();
            }
         }
         else if (e.Key == Key.Back)
         {
            // Обработка Backspace
            if (gameStateManager.CurrentState == GameStateManager.GameState.Running)
            {
               gameManager.HandleBackspace();
               e.Handled = true;
            }
         }
      }

      /// <summary>
      /// Обработчик текстового ввода (получает символы с учетом Shift/Caps Lock)
      /// </summary>
      private void Window_TextInput(object sender, TextCompositionEventArgs e)
      {
         if (gameStateManager.CurrentState == GameStateManager.GameState.Running && e.Text.Length > 0)
         {
            gameManager.HandleCharInput(e.Text[0]);
            e.Handled = true;
         }
      }

      #endregion
   }
}