using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Glossolalia
{
   /// <summary>
   /// Менеджер бонусов
   /// </summary>
   public class BonusManager
   {
      #region Поля

      private readonly ProgressBar progressBar;
      private readonly TextBlock bonusText;
      private DispatcherTimer bonusTimer;

      private double duration;
      private double elapsed;
      private bool isPaused;
      private bool isActive;
      private string currentBonusType;

      // Колбэки для взаимодействия с игровым менеджером
      private Action<double> multiplySpeedCallback;
      private Action restoreSpeedCallback;
      private Action freezeWordsCallback;
      private Action unfreezeWordsCallback;
      private Action<bool> registerCaseCallback;

      // Типы бонусов и их настройки: (Цвет, Длительность, Описание)
      private readonly Dictionary<string, (Color Color, double Duration, string Description)> bonusTypes =
          new Dictionary<string, (Color, double, string)>
          {
                { "заморозка", (Colors.Cyan, 10.0, "Остановка всех слов на 10 сек") },
                { "регистр", (Colors.Gold, 20.0, "Случайные буквы становятся заглавными на 20 сек") }
          };

      // Текущие активные эффекты
      private bool isFreezeActive;
      private bool isRegisterCaseActive;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор менеджера бонусов
      /// </summary>
      /// <param name="progressBar">Прогресс-бар отображения времени бонуса</param>
      /// <param name="bonusText">Текстовый блок отображения типа бонуса</param>
      public BonusManager(ProgressBar progressBar, TextBlock bonusText)
      {
         this.progressBar = progressBar;
         this.bonusText = bonusText;
         Reset();
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Устанавливает колбэки для управления скоростью слов
      /// </summary>
      /// <param name="multiplySpeedCallback">Колбэк умножения скорости</param>
      /// <param name="restoreSpeedCallback">Колбэк восстановления скорости</param>
      public void SetSpeedCallbacks(Action<double> multiplySpeedCallback, Action restoreSpeedCallback)
      {
         this.multiplySpeedCallback = multiplySpeedCallback;
         this.restoreSpeedCallback = restoreSpeedCallback;
      }

      /// <summary>
      /// Устанавливает колбэки для заморозки
      /// </summary>
      /// <param name="freezeWordsCallback">Колбэк заморозки слов</param>
      /// <param name="unfreezeWordsCallback">Колбэк разморозки слов</param>
      public void SetFreezeCallbacks(Action freezeWordsCallback, Action unfreezeWordsCallback)
      {
         this.freezeWordsCallback = freezeWordsCallback;
         this.unfreezeWordsCallback = unfreezeWordsCallback;
      }

      /// <summary>
      /// Устанавливает колбэк для бонуса регистра
      /// </summary>
      /// <param name="registerCaseCallback">Колбэк управления регистром</param>
      public void SetRegisterCaseCallback(Action<bool> registerCaseCallback)
      {
         this.registerCaseCallback = registerCaseCallback;
      }

      /// <summary>
      /// Выбирает случайный бонус
      /// </summary>
      /// <param name="random">Генератор случайных чисел</param>
      /// <returns>Тип бонуса или null, если нет доступных бонусов</returns>
      public string GetRandomBonusType(Random random)
      {
         var bonusList = new List<string>(bonusTypes.Keys);
         return bonusList.Count > 0 ? bonusList[random.Next(bonusList.Count)] : null;
      }

      /// <summary>
      /// Начинает действие бонуса
      /// </summary>
      public void StartBonus(string bonusName, double durationSeconds)
      {
         if (!bonusTypes.ContainsKey(bonusName)) return;

         StopCurrentBonus();
         ShowBonusUI(bonusName);
         ResetBonusState(bonusName, durationSeconds);
         ActivateBonusEffect(bonusName, durationSeconds);
         StartBonusTimer();
      }

      /// <summary>
      /// Приостанавливает или возобновляет бонус
      /// </summary>
      /// <param name="paused">True для приостановки, False для возобновления</param>
      public void SetPaused(bool paused)
      {
         isPaused = paused;

         if (paused)
         {
            bonusTimer?.Stop();
         }
         else
         {
            bonusTimer?.Start();
         }
      }

      /// <summary>
      /// Полностью сбрасывает состояние менеджера бонусов
      /// </summary>
      public void Reset()
      {
         StopCurrentBonus();
         bonusTimer?.Stop();
         bonusTimer = null;
         ResetState();
         HideBonusUI();
      }

      /// <summary>
      /// Получает цвет для бонусного слова
      /// </summary>
      /// <returns>Цвет бонуса</returns>
      public Color GetBonusColor(string bonusType)
      {
         return bonusTypes.ContainsKey(bonusType)
             ? bonusTypes[bonusType].Color
             : Colors.Black;
      }

      /// <summary>
      /// Получает длительность бонуса по умолчанию
      /// </summary>
      /// <returns>Длительность бонуса</returns>
      public double GetBonusDuration(string bonusType)
      {
         return bonusTypes.ContainsKey(bonusType)
             ? bonusTypes[bonusType].Duration
             : 10.0;
      }

      /// <summary>
      /// Проверяет, является ли слово бонусным
      /// </summary>
      /// <returns>True, если слово является названием бонуса</returns>
      public bool IsBonusWord(string word)
      {
         return bonusTypes.ContainsKey(word);
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Проверяет, активна ли в данный момент бонус
      /// </summary>
      public bool IsActive => isActive;

      /// <summary>
      /// Проверяет, активна ли заморозка
      /// </summary>
      public bool IsFreezeActive => isFreezeActive;

      /// <summary>
      /// Проверяет, активен ли бонус регистра
      /// </summary>
      public bool IsRegisterCaseActive => isRegisterCaseActive;

      /// <summary>
      /// Возвращает оставшееся время бонуса в секундах
      /// </summary>
      public double RemainingTime => Math.Max(0, duration - elapsed);

      /// <summary>
      /// Возвращает текущий активный тип бонуса
      /// </summary>
      public string CurrentBonusType => currentBonusType;

      #endregion

      #region Приватные методы

      /// <summary>
      /// Останавливает текущий активный бонус
      /// </summary>
      private void StopCurrentBonus()
      {
         if (isActive && !string.IsNullOrEmpty(currentBonusType))
         {
            DeactivateBonusEffect(currentBonusType);
         }

         bonusTimer?.Stop();
         bonusTimer = null;
         ResetState();
         HideBonusUI();
      }

      /// <summary>
      /// Показывает UI элементы бонуса
      /// </summary>
      private void ShowBonusUI(string bonusName)
      {
         bonusText.Visibility = Visibility.Visible;
         bonusText.Text = $"{bonusName}";
         bonusText.Foreground = new SolidColorBrush(bonusTypes[bonusName].Color);
         progressBar.Visibility = Visibility.Visible;
      }

      /// <summary>
      /// Скрывает UI элементы бонуса
      /// </summary>
      private void HideBonusUI()
      {
         progressBar.Value = 0;
         bonusText.Text = string.Empty;
         bonusText.Visibility = Visibility.Hidden;
         progressBar.Visibility = Visibility.Hidden;
      }

      /// <summary>
      /// Сбрасывает состояние бонуса
      /// </summary>
      private void ResetBonusState(string bonusName, double durationSeconds)
      {
         progressBar.Value = 1;
         elapsed = 0;
         duration = durationSeconds;
         isPaused = false;
         isActive = true;
         currentBonusType = bonusName;
      }

      /// <summary>
      /// Активирует эффект бонуса
      /// </summary>
      private void ActivateBonusEffect(string bonusType, double duration)
      {
         switch (bonusType)
         {
            case "заморозка":
               ActivateFreezeEffect(duration);
               break;
            case "регистр":
               ActivateRegisterCaseEffect(duration);
               break;
         }
      }

      /// <summary>
      /// Активирует эффект заморозки
      /// </summary>
      private void ActivateFreezeEffect(double duration)
      {
         isFreezeActive = true;
         freezeWordsCallback?.Invoke();
      }

      /// <summary>
      /// Активирует эффект изменения регистра
      /// </summary>
      private void ActivateRegisterCaseEffect(double duration)
      {
         isRegisterCaseActive = true;
         registerCaseCallback?.Invoke(true);
      }

      /// <summary>
      /// Запускает таймер бонуса
      /// </summary>
      private void StartBonusTimer()
      {
         bonusTimer = new DispatcherTimer(DispatcherPriority.Render);
         bonusTimer.Interval = TimeSpan.FromSeconds(1);
         bonusTimer.Tick += BonusTimer_Tick;
         bonusTimer.Start();
      }

      /// <summary>
      /// Обработчик тика таймера бонуса
      /// </summary>
      private void BonusTimer_Tick(object sender, EventArgs e)
      {
         if (isPaused || !isActive) return;

         elapsed += 1;
         double progress = 1 - (elapsed / duration);

         progressBar.Value = Math.Max(0, progress);

         if (progress <= 0.001)
         {
            StopCurrentBonus();
         }
      }

      /// <summary>
      /// Деактивирует эффект бонуса
      /// </summary>
      private void DeactivateBonusEffect(string bonusType)
      {
         switch (bonusType)
         {
            case "заморозка":
               unfreezeWordsCallback?.Invoke();
               isFreezeActive = false;
               break;
            case "регистр":
               registerCaseCallback?.Invoke(false);
               isRegisterCaseActive = false;
               break;
         }
      }

      /// <summary>
      /// Сбрасывает состояние менеджера
      /// </summary>
      private void ResetState()
      {
         isActive = false;
         isPaused = false;
         elapsed = 0;
         duration = 0;
         currentBonusType = null;
         isFreezeActive = false;
         isRegisterCaseActive = false;
      }

      #endregion
   }
}