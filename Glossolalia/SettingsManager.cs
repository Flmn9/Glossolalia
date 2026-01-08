using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Glossolalia
{
   /// <summary>
   /// Менеджер настроек приложения
   /// </summary>
   public class SettingsManager
   {
      #region Поля

      private readonly MainWindow window;
      private readonly ComboBox resolutionComboBox;
      private readonly ComboBox windowModeComboBox;
      private readonly Slider speedSlider;
      private readonly TextBlock speedValueText;
      private readonly List<Resolution> allResolutions;
      private readonly AppSettings currentSettings;

      private bool isInitializing = true;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор менеджера настроек
      /// </summary>
      /// <param name="window">Главное окно приложения</param>
      /// <param name="resolutionComboBox">Комбобокс выбора разрешения</param>
      /// <param name="windowModeComboBox">Комбобокс выбора режима окна</param>
      /// <param name="speedSlider">Ползунок настройки скорости</param>
      /// <param name="speedValueText">Текстовый блок отображения скорости</param>
      public SettingsManager(MainWindow window, ComboBox resolutionComboBox,
                            ComboBox windowModeComboBox, Slider speedSlider = null,
                            TextBlock speedValueText = null)
      {
         this.window = window;
         this.resolutionComboBox = resolutionComboBox;
         this.windowModeComboBox = windowModeComboBox;
         this.speedSlider = speedSlider;
         this.speedValueText = speedValueText;
         this.currentSettings = AppSettings.Load();

         // Определение списка всех поддерживаемых разрешений
         allResolutions = new List<Resolution>
            {
                new Resolution(800, 600),
                new Resolution(1280, 720),
                new Resolution(1366, 768),
                new Resolution(1920, 1080),
                new Resolution(1920, 1200),
                new Resolution(2560, 1440),
                new Resolution(3100, 1440),
                new Resolution(3840, 2160),
                new Resolution(3840, 2400)
            };
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Инициализирует компоненты настроек
      /// </summary>
      public void Initialize()
      {
         InitializeSpeedSettings();
         InitializeResolutionComboBox();
         InitializeSettingsUI();
         isInitializing = false;
      }

      /// <summary>
      /// Применяет текущие настройки к окну приложения
      /// </summary>
      public void ApplySettings()
      {
         if (currentSettings.WindowMode == WindowMode.Windowed)
         {
            window.Width = currentSettings.Resolution.Width;
            window.Height = currentSettings.Resolution.Height;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
         }

         ApplyWindowMode();
      }

      /// <summary>
      /// Обработчик изменения разрешения экрана
      /// </summary>
      /// <param name="selectedItem">Выбранный элемент комбобокса</param>
      public void OnResolutionChanged(ComboBoxItem selectedItem)
      {
         if (!isInitializing && selectedItem.Tag is Resolution resolution)
         {
            currentSettings.Resolution = resolution;

            if (currentSettings.WindowMode == WindowMode.Windowed)
            {
               window.Width = resolution.Width;
               window.Height = resolution.Height;
            }

            currentSettings.Save();
         }
      }

      /// <summary>
      /// Обработчик изменения режима окна
      /// </summary>
      /// <param name="selectedIndex">Индекс выбранного режима</param>
      public void OnWindowModeChanged(int selectedIndex)
      {
         if (!isInitializing)
         {
            currentSettings.WindowMode = selectedIndex == 0
                ? WindowMode.Windowed
                : WindowMode.Fullscreen;

            ApplyWindowMode();

            if (currentSettings.WindowMode == WindowMode.Windowed)
            {
               window.Width = currentSettings.Resolution.Width;
               window.Height = currentSettings.Resolution.Height;
            }

            currentSettings.Save();
         }
      }

      /// <summary>
      /// Обработчик изменения скорости слов
      /// </summary>
      /// <param name="newSpeed">Новая скорость</param>
      public void OnSpeedChanged(double newSpeed)
      {
         if (!isInitializing)
         {
            double roundedSpeed = Math.Round(newSpeed, 1);
            double clampedSpeed = SpeedSettings.ClampSpeed(roundedSpeed);

            currentSettings.SpeedSettings.WordSpeed = clampedSpeed;
            UpdateSpeedValueDisplay();
            currentSettings.Save();

            if (window is MainWindow mainWindow)
            {
               mainWindow.UpdateWordSpeed(clampedSpeed);
            }
         }
      }

      #endregion

      #region Приватные методы

      /// <summary>
      /// Инициализирует настройки скорости
      /// </summary>
      private void InitializeSpeedSettings()
      {
         if (speedSlider != null)
         {
            speedSlider.Minimum = SpeedSettings.MIN_SPEED;
            speedSlider.Maximum = SpeedSettings.MAX_SPEED;
            speedSlider.Value = currentSettings.SpeedSettings.WordSpeed;
            UpdateSpeedValueDisplay();
         }
      }

      /// <summary>
      /// Обновляет отображение значения скорости
      /// </summary>
      private void UpdateSpeedValueDisplay()
      {
         if (speedValueText != null)
         {
            speedValueText.Text = $"{currentSettings.SpeedSettings.WordSpeed:F1}";
         }
      }

      /// <summary>
      /// Заполняет выпадающий список доступными разрешениями
      /// </summary>
      private void InitializeResolutionComboBox()
      {
         resolutionComboBox.Items.Clear();
         double maxWidth = SystemParameters.PrimaryScreenWidth;
         double maxHeight = SystemParameters.PrimaryScreenHeight;

         // Добавление разрешений, поддерживаемых монитором
         foreach (var resolution in allResolutions
             .Where(r => r.Width <= maxWidth && r.Height <= maxHeight))
         {
            resolutionComboBox.Items.Add(new ComboBoxItem
            {
               Content = resolution.ToString(),
               Tag = resolution
            });
         }
      }

      /// <summary>
      /// Устанавливает текущие настройки в элементы интерфейса
      /// </summary>
      private void InitializeSettingsUI()
      {
         SetSelectedResolution();
         SetWindowMode();
      }

      /// <summary>
      /// Устанавливает выбранное разрешение
      /// </summary>
      private void SetSelectedResolution()
      {
         bool found = false;
         foreach (ComboBoxItem item in resolutionComboBox.Items)
         {
            if (item.Tag is Resolution resolution &&
                resolution.Width == currentSettings.Resolution.Width &&
                resolution.Height == currentSettings.Resolution.Height)
            {
               resolutionComboBox.SelectedItem = item;
               found = true;
               break;
            }
         }

         // Выбор первого доступного разрешения, если текущее не найдено
         if (!found && resolutionComboBox.Items.Count > 0)
         {
            resolutionComboBox.SelectedIndex = 0;
            if (resolutionComboBox.SelectedItem is ComboBoxItem selectedItem &&
                selectedItem.Tag is Resolution resolution)
            {
               currentSettings.Resolution = resolution;
            }
         }
      }

      /// <summary>
      /// Устанавливает режим окна
      /// </summary>
      private void SetWindowMode()
      {
         windowModeComboBox.SelectedIndex = currentSettings.WindowMode == WindowMode.Windowed ? 0 : 1;
      }

      /// <summary>
      /// Применяет выбранный режим окна
      /// </summary>
      private void ApplyWindowMode()
      {
         if (currentSettings.WindowMode == WindowMode.Fullscreen)
         {
            ApplyFullscreenMode();
         }
         else
         {
            ApplyWindowedMode();
         }
      }

      /// <summary>
      /// Применяет полноэкранный режим
      /// </summary>
      private void ApplyFullscreenMode()
      {
         window.WindowState = WindowState.Maximized;
         window.WindowStyle = WindowStyle.None;
         window.ResizeMode = ResizeMode.NoResize;
         window.Topmost = true;
         window.Width = SystemParameters.PrimaryScreenWidth;
         window.Height = SystemParameters.PrimaryScreenHeight;
      }

      /// <summary>
      /// Применяет оконный режим
      /// </summary>
      private void ApplyWindowedMode()
      {
         window.WindowState = WindowState.Normal;
         window.WindowStyle = WindowStyle.SingleBorderWindow;
         window.ResizeMode = ResizeMode.CanMinimize;
         window.Topmost = false;
      }

      #endregion
   }
}