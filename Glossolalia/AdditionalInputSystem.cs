using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Glossolalia
{
   /// <summary>
   /// Дополнительная система ввода для обработки синонимов и антонимов
   /// </summary>
   public class AdditionalInputSystem
   {
      #region Поля

      private readonly SynonymDictionary synonymDictionary;
      private readonly TextBlock synonymTextBlock;
      private readonly Random random = new Random();

      private string currentInput = "";
      private bool isActive;
      private string bonusType;
      private List<FallingWord> trackedWords = new List<FallingWord>();
      private List<FallingWord> lastMatchingWords = new List<FallingWord>();

      #endregion

      #region События

      /// <summary>
      /// Событие уничтожения слова дополнительной системой
      /// </summary>
      public event Action<int, int> WordDestroyedByAdditionalSystem;

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор дополнительной системы ввода
      /// </summary>
      /// <param name="synonymDictionary">Словарь синонимов</param>
      /// <param name="synonymTextBlock">Текстовый блок для отображения ввода</param>
      public AdditionalInputSystem(SynonymDictionary synonymDictionary, TextBlock synonymTextBlock)
      {
         this.synonymDictionary = synonymDictionary;
         this.synonymTextBlock = synonymTextBlock;
      }

      #endregion

      #region Публичные методы

      /// <summary>
      /// Активирует дополнительную систему ввода
      /// </summary>
      /// <param name="bonusType">Тип бонуса (синоним/антоним)</param>
      /// <param name="activeWords">Список активных слов</param>
      public void Activate(string bonusType, List<FallingWord> activeWords)
      {
         isActive = true;
         this.bonusType = bonusType;
         ResetState();
         UpdateTrackedWords(activeWords);
         UpdateTextBlock();
      }

      /// <summary>
      /// Деактивирует дополнительную систему ввода
      /// </summary>
      public void Deactivate()
      {
         isActive = false;
         ResetState();
         UpdateTextBlock();
      }

      /// <summary>
      /// Обрабатывает ввод символа
      /// </summary>
      /// <param name="pressedChar">Введенный символ</param>
      /// <param name="activeWords">Список активных слов</param>
      /// <param name="scoreManager">Менеджер счета</param>
      /// <returns>True, если ввод был успешно обработан</returns>
      public bool HandleCharInput(char pressedChar, List<FallingWord> activeWords, ScoreManager scoreManager)
      {
         if (!isActive) return true;

         string newInput = currentInput + pressedChar;
         var matchingWords = FindMatchingWords(newInput);

         if (matchingWords.Count > 0)
         {
            currentInput = newInput;
            lastMatchingWords = matchingWords;

            CheckForCompleteInput(activeWords, scoreManager);
            UpdateTextBlock();
            return true;
         }

         ResetInput();
         UpdateTextBlock();
         return false;
      }

      /// <summary>
      /// Обрабатывает нажатие Backspace
      /// </summary>
      public void HandleBackspace()
      {
         if (!isActive || string.IsNullOrEmpty(currentInput)) return;

         currentInput = currentInput.Substring(0, currentInput.Length - 1);
         if (string.IsNullOrEmpty(currentInput))
         {
            lastMatchingWords.Clear();
         }

         UpdateTextBlock();
      }

      /// <summary>
      /// Обновляет список отслеживаемых слов
      /// </summary>
      /// <param name="activeWords">Список активных слов</param>
      public void UpdateTrackedWords(List<FallingWord> activeWords)
      {
         if (!isActive) return;

         trackedWords = activeWords
             .Where(w => !w.IsDestroyed && !w.IsBonus)
             .ToList();
      }

      /// <summary>
      /// Уведомляет о уничтожении слова основной системой
      /// </summary>
      /// <param name="word">Уничтоженное слово</param>
      public void NotifyWordDestroyedByMainSystem(FallingWord word)
      {
         if (isActive)
         {
            ResetInput();
            trackedWords.Remove(word);
            lastMatchingWords.Remove(word);
         }
      }

      /// <summary>
      /// Сбрасывает состояние системы
      /// </summary>
      public void Reset()
      {
         ResetState();
         UpdateTextBlock();
      }

      #endregion

      #region Свойства

      /// <summary>
      /// Проверяет, активна ли дополнительная система
      /// </summary>
      public bool IsActive => isActive;

      #endregion

      #region Приватные методы

      /// <summary>
      /// Ищет слова, соответствующие введенной строке
      /// </summary>
      private List<FallingWord> FindMatchingWords(string input)
      {
         var sourceWords = lastMatchingWords.Count > 0 ? lastMatchingWords : trackedWords;
         var matchingWords = new List<FallingWord>();

         foreach (var word in sourceWords)
         {
            if (word.IsDestroyed || word.IsBonus) continue;

            var relatedWords = GetRelatedWords(word.Word);
            if (relatedWords.Any(w => w.StartsWith(input, StringComparison.OrdinalIgnoreCase)))
            {
               matchingWords.Add(word);
            }
         }

         return matchingWords;
      }

      /// <summary>
      /// Получает связанные слова (синонимы или антонимы)
      /// </summary>
      private List<string> GetRelatedWords(string word)
      {
         var result = new List<string>();

         if (bonusType == "синоним")
         {
            result.AddRange(synonymDictionary.GetSynonyms(word));
         }
         else if (bonusType == "антоним")
         {
            result.AddRange(synonymDictionary.GetAntonyms(word));
         }

         return result.Select(w => w.ToLowerInvariant()).Distinct().ToList();
      }

      /// <summary>
      /// Проверяет, введено ли полное слово
      /// </summary>
      private void CheckForCompleteInput(List<FallingWord> activeWords, ScoreManager scoreManager)
      {
         var wordsToDestroy = new List<FallingWord>();

         foreach (var word in lastMatchingWords)
         {
            if (word.IsDestroyed) continue;

            var relatedWords = GetRelatedWords(word.Word);
            if (relatedWords.Any(w => w.Equals(currentInput, StringComparison.OrdinalIgnoreCase)))
            {
               wordsToDestroy.Add(word);
            }
         }

         if (wordsToDestroy.Count > 0)
         {
            DestroyWords(wordsToDestroy, activeWords, scoreManager);
         }
      }

      /// <summary>
      /// Уничтожает слова и начисляет очки
      /// </summary>
      private void DestroyWords(List<FallingWord> wordsToDestroy, List<FallingWord> activeWords, ScoreManager scoreManager)
      {
         int totalScore = 0;
         int totalDestroyed = wordsToDestroy.Count;

         // Уничтожаем основные слова
         foreach (var word in wordsToDestroy)
         {
            word.Destroy();
            totalScore += word.Word.Length * scoreManager.Multiplier;

            // Уничтожаем связанные слова
            var relatedWords = FindRelatedWords(word, activeWords);
            foreach (var relatedWord in relatedWords)
            {
               relatedWord.Destroy();
               totalScore += relatedWord.Word.Length * scoreManager.Multiplier;
               totalDestroyed++;
            }
         }

         // Уничтожаем случайные дополнительные слова
         var additionalWords = GetRandomAdditionalWords(activeWords, 4 - totalDestroyed + 1);
         foreach (var word in additionalWords)
         {
            word.Destroy();
            totalScore += word.Word.Length * scoreManager.Multiplier;
            totalDestroyed++;
         }

         WordDestroyedByAdditionalSystem?.Invoke(totalScore, totalDestroyed);
         ResetInput();
         UpdateTrackedWords(activeWords);
      }

      /// <summary>
      /// Ищет связанные слова
      /// </summary>
      private List<FallingWord> FindRelatedWords(FallingWord sourceWord, List<FallingWord> activeWords)
      {
         var result = new List<FallingWord>();
         var exactMatch = currentInput.ToLowerInvariant();

         foreach (var word in activeWords)
         {
            if (word.IsDestroyed || word.IsBonus || word == sourceWord) continue;

            var relatedWords = GetRelatedWords(word.Word);
            if (relatedWords.Any(rw => rw.Equals(exactMatch, StringComparison.OrdinalIgnoreCase)))
            {
               result.Add(word);
            }
         }

         return result;
      }

      /// <summary>
      /// Получает случайные дополнительные слова
      /// </summary>
      private List<FallingWord> GetRandomAdditionalWords(List<FallingWord> activeWords, int count)
      {
         var regularWords = activeWords
             .Where(w => !w.IsDestroyed && !w.IsBonus)
             .ToList();

         var result = new List<FallingWord>();
         int takeCount = Math.Min(count, regularWords.Count);

         for (int i = 0; i < takeCount; i++)
         {
            int index = random.Next(regularWords.Count);
            result.Add(regularWords[index]);
            regularWords.RemoveAt(index);
         }

         return result;
      }

      /// <summary>
      /// Сбрасывает состояние системы
      /// </summary>
      private void ResetState()
      {
         currentInput = "";
         lastMatchingWords.Clear();
         trackedWords.Clear();
      }

      /// <summary>
      /// Сбрасывает текущий ввод
      /// </summary>
      private void ResetInput()
      {
         currentInput = "";
         lastMatchingWords.Clear();
      }

      /// <summary>
      /// Обновляет текстовый блок с текущим вводом
      /// </summary>
      private void UpdateTextBlock()
      {
         if (synonymTextBlock == null) return;

         if (isActive && !string.IsNullOrEmpty(currentInput))
         {
            synonymTextBlock.Text = currentInput.Length > 0
                ? currentInput[currentInput.Length - 1].ToString()
                : "";
            synonymTextBlock.Visibility = System.Windows.Visibility.Visible;
         }
         else
         {
            synonymTextBlock.Text = "";
            synonymTextBlock.Visibility = System.Windows.Visibility.Hidden;
         }
      }

      #endregion
   }
}