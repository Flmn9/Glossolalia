using System;
using System.Collections.Generic;
using System.Linq;

namespace Glossolalia
{
    /// <summary>
    /// Обработчик ввода с клавиатуры
    /// </summary>
    public class InputHandler
    {
        #region Константы

        private const int REGISTER_BONUS_MULTIPLIER = 2;

        #endregion

        #region Поля

        private readonly ScoreManager scoreManager;
        private readonly List<FallingWord> activeWords;

        #endregion

        #region События

        /// <summary>
        /// Событие полного выделения слова
        /// </summary>
        public event Action<FallingWord> WordFullySelected;

        /// <summary>
        /// Событие уничтожения слов
        /// </summary>
        public event Action<int> WordsDestroyed;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор обработчика ввода
        /// </summary>
        /// <param name="scoreManager">Менеджер счета</param>
        /// <param name="activeWords">Список активных слов</param>
        public InputHandler(ScoreManager scoreManager, List<FallingWord> activeWords)
        {
            this.scoreManager = scoreManager;
            this.activeWords = activeWords;
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Обрабатывает ввод символа с клавиатуры
        /// </summary>
        /// <param name="pressedChar">Введенный символ</param>
        /// <param name="isRegisterCaseActive">Флаг активности бонуса регистра</param>
        /// <returns>True, если ввод был обработан успешно</returns>
        public bool HandleCharInput(char pressedChar, bool isRegisterCaseActive)
        {
            bool anyWordSelected = activeWords.Any(w => !w.IsDestroyed && w.SelectedLettersCount > 0);
            bool foundMatch = false;
            var wordsToRemove = new List<FallingWord>();

            if (!anyWordSelected)
            {
                foundMatch = ProcessFirstLetterInput(pressedChar, isRegisterCaseActive);
            }
            else
            {
                foundMatch = ProcessNextLetterInput(pressedChar, isRegisterCaseActive, wordsToRemove);
            }

            // Уничтожение полностью выделенных слов
            foreach (var word in wordsToRemove)
            {
                WordFullySelected?.Invoke(word);
                WordsDestroyed?.Invoke(1);
            }

            return foundMatch;
        }

        /// <summary>
        /// Вычисляет количество очков за ввод
        /// </summary>
        /// <param name="shouldAward">Нужно ли начислять очки</param>
        /// <param name="isRegisterCaseActive">Флаг активности бонуса регистра</param>
        /// <returns>Количество очков</returns>
        public int CalculatePoints(bool shouldAward, bool isRegisterCaseActive)
        {
            if (!shouldAward) return 0;

            int basePoints = 1;
            if (isRegisterCaseActive)
            {
                basePoints *= REGISTER_BONUS_MULTIPLIER;
            }

            return basePoints;
        }

        /// <summary>
        /// Проверяет, является ли символ допустимым для ввода
        /// </summary>
        /// <returns>True, если символ допустим</returns>
        public static bool IsValidInputChar(char c)
        {
            return IsRussianLetter(c);
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Обрабатывает ввод первой буквы
        /// </summary>
        private bool ProcessFirstLetterInput(char pressedChar, bool isRegisterCaseActive)
        {
            bool foundMatch = false;

            foreach (var word in activeWords)
            {
                if (word.IsDestroyed) continue;

                if (StartsWithLetter(word, pressedChar, isRegisterCaseActive))
                {
                    word.SelectNextLetter();
                    foundMatch = true;
                }
            }

            return foundMatch;
        }

        /// <summary>
        /// Обрабатывает ввод следующей буквы
        /// </summary>
        private bool ProcessNextLetterInput(char pressedChar, bool isRegisterCaseActive,
                                          List<FallingWord> wordsToRemove)
        {
            bool foundMatch = false;
            var selectedWords = activeWords
                .Where(w => !w.IsDestroyed && w.SelectedLettersCount > 0)
                .ToList();

            foreach (var word in selectedWords)
            {
                if (NextLetterMatches(word, pressedChar, isRegisterCaseActive))
                {
                    word.SelectNextLetter();
                    foundMatch = true;

                    if (word.IsFullySelected())
                    {
                        wordsToRemove.Add(word);
                    }
                }
                else
                {
                    word.ResetSelection();
                }
            }

            // Сброс выделения в невыделенных словах
            if (foundMatch)
            {
                foreach (var word in activeWords)
                {
                    if (!word.IsDestroyed && word.SelectedLettersCount > 0 && !selectedWords.Contains(word))
                    {
                        word.ResetSelection();
                    }
                }
            }

            return foundMatch;
        }

        /// <summary>
        /// Проверяет, начинается ли слово с указанной буквы
        /// </summary>
        private bool StartsWithLetter(FallingWord word, char pressedChar, bool isRegisterCaseActive)
        {
            if (string.IsNullOrEmpty(word.Word) || word.IsDestroyed)
                return false;

            char firstChar = word.Word[0];

            return isRegisterCaseActive
                ? firstChar == pressedChar
                : char.ToUpperInvariant(firstChar) == char.ToUpperInvariant(pressedChar);
        }

        /// <summary>
        /// Проверяет, совпадает ли следующая буква слова с введенной
        /// </summary>
        private bool NextLetterMatches(FallingWord word, char pressedChar, bool isRegisterCaseActive)
        {
            if (string.IsNullOrEmpty(word.Word) || word.IsDestroyed ||
                word.SelectedLettersCount >= word.Word.Length)
                return false;

            char nextChar = word.Word[word.SelectedLettersCount];

            return isRegisterCaseActive
                ? nextChar == pressedChar
                : char.ToUpperInvariant(nextChar) == char.ToUpperInvariant(pressedChar);
        }

        /// <summary>
        /// Проверяет, является ли символ русской буквой
        /// </summary>
        private static bool IsRussianLetter(char c)
        {
            return (c >= 'А' && c <= 'Я') ||
                   (c >= 'а' && c <= 'я') ||
                   c == 'Ё' || c == 'ё';
        }

        #endregion
    }
}