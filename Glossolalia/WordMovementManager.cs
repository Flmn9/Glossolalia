using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Glossolalia
{
   /// <summary>
   /// Менеджер движения слов на игровом поле
   /// </summary>
   public class WordMovementManager
   {
      #region Поля

      private readonly List<FallingWord> activeWords;
      private readonly Canvas gameCanvas;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор менеджера движения
      /// </summary>
      /// <param name="activeWords">Список активных слов</param>
      /// <param name="gameCanvas">Игровой холст</param>
      public WordMovementManager(List<FallingWord> activeWords, Canvas gameCanvas)
      {
         this.activeWords = activeWords;
         this.gameCanvas = gameCanvas;
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Обновляет положение всех слов
      /// </summary>
      /// <param name="deltaTime">Время, прошедшее с предыдущего кадра</param>
      public void UpdateWords(double deltaTime)
      {
         for (int i = activeWords.Count - 1; i >= 0; i--)
         {
            var word = activeWords[i];

            if (word.IsDestroyed)
            {
               activeWords.RemoveAt(i);
               continue;
            }

            word.MoveDown(deltaTime);
         }
      }

      /// <summary>
      /// Проверяет и разрешает столкновения между словами
      /// </summary>
      public void CheckAndResolveCollisions()
      {
         if (activeWords.Count < 2) return;

         for (int i = 0; i < activeWords.Count; i++)
         {
            var word1 = activeWords[i];
            if (word1.IsDestroyed) continue;

            for (int j = i + 1; j < activeWords.Count; j++)
            {
               var word2 = activeWords[j];
               if (word2.IsDestroyed) continue;

               if (word1.CollidesWith(word2))
               {
                  ResolveCollision(word1, word2);
               }
            }

            RestoreSpeedIfNotColliding(word1);
         }
      }

      /// <summary>
      /// Проверяет условия окончания игры
      /// </summary>
      /// <returns>True, если игра должна завершиться</returns>
      public bool CheckGameOver()
      {
         double canvasHeight = gameCanvas.ActualHeight;

         foreach (var word in activeWords)
         {
            if (word.IsDestroyed || word.IsBonus) continue;

            if (word.ReachedBottom(canvasHeight))
            {
               return true;
            }
         }

         return false;
      }

      #endregion

      #region Приватные методы

      /// <summary>
      /// Разрешает столкновение между двумя словами
      /// </summary>
      private void ResolveCollision(FallingWord word1, FallingWord word2)
      {
         if (word1.Bounds.Top < word2.Bounds.Top)
         {
            word1.AdjustPositionForCollision(word2);
         }
         else
         {
            word2.AdjustPositionForCollision(word1);
         }
      }

      /// <summary>
      /// Восстанавливает скорость слова, если оно не сталкивается с другими
      /// </summary>
      private void RestoreSpeedIfNotColliding(FallingWord word)
      {
         bool isColliding = false;
         foreach (var other in activeWords)
         {
            if (other == word || other.IsDestroyed) continue;

            if (word.CollidesWith(other))
            {
               isColliding = true;
               break;
            }
         }

         if (!isColliding)
         {
            word.RestoreOriginalSpeed();
         }
      }

      #endregion
   }
}