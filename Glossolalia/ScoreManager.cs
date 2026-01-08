using System;

namespace Glossolalia
{
   /// <summary>
   /// Менеджер счета. Управляет очками и множителем
   /// </summary>
   public class ScoreManager
   {
      #region Свойства

      /// <summary>
      /// Текущие очки
      /// </summary>
      public int Score { get; private set; }

      /// <summary>
      /// Текущий множитель очков
      /// </summary>
      public int Multiplier { get; private set; }

      #endregion

      #region События

      /// <summary>
      /// Событие изменения счета
      /// </summary>
      public event EventHandler<int> ScoreChanged;

      /// <summary>
      /// Событие изменения множителя
      /// </summary>
      public event EventHandler<int> MultiplierChanged;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор. Инициализирует счетчики
      /// </summary>
      public ScoreManager()
      {
         Reset();
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Добавляет очки с учетом текущего множителя
      /// </summary>
      /// <param name="basePoints">Базовое количество очков</param>
      public void AddPoints(int basePoints)
      {
         int points = basePoints * Multiplier;
         Score += points;
         ScoreChanged?.Invoke(this, Score);
      }

      /// <summary>
      /// Увеличивает множитель очков
      /// </summary>
      public void IncreaseMultiplier()
      {
         Multiplier = Math.Min(Multiplier + 1, 9);
         MultiplierChanged?.Invoke(this, Multiplier);
      }

      /// <summary>
      /// Сбрасывает множитель
      /// </summary>
      public void ResetMultiplier()
      {
         Multiplier = 1;
         MultiplierChanged?.Invoke(this, Multiplier);
      }

      /// <summary>
      /// Полностью сбрасывает счет и множитель
      /// </summary>
      public void Reset()
      {
         Score = 0;
         Multiplier = 1;
         ScoreChanged?.Invoke(this, Score);
         MultiplierChanged?.Invoke(this, Multiplier);
      }

      #endregion
   }
}