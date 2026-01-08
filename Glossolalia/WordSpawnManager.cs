using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Glossolalia
{
    /// <summary>
    /// Менеджер создания новых слов на игровом поле
    /// </summary>
    public class WordSpawnManager
    {
        #region Константы

        private const double BONUS_SPAWN_CHANCE = 0.10;
        private const int MAX_ACTIVE_WORDS = 30;

        #endregion

        #region Поля

        private readonly Canvas gameCanvas;
        private readonly Random random;
        private readonly List<FallingWord> activeWords;
        private readonly SynonymDictionary synonymDictionary;
        private readonly BonusManager bonusManager;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор менеджера создания слов
        /// </summary>
        /// <param name="gameCanvas">Игровой холст</param>
        /// <param name="activeWords">Список активных слов</param>
        /// <param name="synonymDictionary">Словарь синонимов</param>
        /// <param name="bonusManager">Менеджер бонусов</param>
        private WordSpawnManager(Canvas gameCanvas, List<FallingWord> activeWords,
                              SynonymDictionary synonymDictionary, BonusManager bonusManager)
        {
            this.gameCanvas = gameCanvas;
            this.activeWords = activeWords;
            this.synonymDictionary = synonymDictionary;
            this.bonusManager = bonusManager;
            random = new Random();
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Создает новое слово на игровом поле
        /// </summary>
        /// <param name="wordText">Текст слова</param>
        /// <param name="baseSpeed">Базовая скорость падения</param>
        /// <param name="forceBonus">Принудительно создать бонусное слово</param>
        /// <returns>Созданное слово или null, если не удалось создать</returns>
        public FallingWord SpawnWord(string wordText, double baseSpeed, bool forceBonus = false)
        {
            if (activeWords.Count(w => !w.IsDestroyed) >= MAX_ACTIVE_WORDS)
                return null;

            bool isBonus = forceBonus || random.NextDouble() < BONUS_SPAWN_CHANCE;
            string bonusType = null;
            Color wordColor = Colors.Black;
            string displayText = wordText; // Текст для отображения

            if (isBonus)
            {
                bonusType = bonusManager.GetRandomBonusType(random);
                if (bonusType != null)
                {
                    // Для бонусных слов используем название бонуса в качестве текста
                    displayText = bonusType;
                    wordColor = bonusManager.GetBonusColor(bonusType);
                }
                else
                {
                    // Если не удалось получить тип бонуса, не создаем бонусное слово
                    isBonus = false;
                    displayText = GetRandomWordFromDictionary();
                }
            }

            double speed = CalculateWordSpeed(baseSpeed);
            var (x, y) = CalculateSpawnPosition(displayText);

            return CreateFallingWord(displayText, speed, isBonus, bonusType, wordColor, x, y);
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Вычисляет скорость слова с учетом случайного отклонения
        /// </summary>
        private double CalculateWordSpeed(double baseSpeed)
        {
            // Добавление случайного отклонения к скорости (-5% до +50%)
            double speedDeviation = (random.NextDouble() * 0.55) - 0.05;
            return baseSpeed * (1 + speedDeviation);
        }

        /// <summary>
        /// Вычисляет позицию для создания нового слова
        /// </summary>
        private (double x, double y) CalculateSpawnPosition(string word)
        {
            double canvasWidth = gameCanvas.ActualWidth;
            double canvasHeight = gameCanvas.ActualHeight;

            if (canvasWidth <= 0 || canvasHeight <= 0)
            {
                canvasWidth = 800;
                canvasHeight = 550;
            }

            var formattedText = new FormattedText(
                word,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                24,
                Brushes.Black,
                VisualTreeHelper.GetDpi(gameCanvas).PixelsPerDip);

            double wordWidth = formattedText.Width;

            double maxX = canvasWidth - wordWidth - 20;
            double minX = 20;

            if (maxX < minX) maxX = minX;

            double x = minX + random.NextDouble() * (maxX - minX);
            double y = 0;

            return (x, y);
        }

        /// <summary>
        /// Получает случайное слово из словаря
        /// </summary>
        private string GetRandomWordFromDictionary()
        {
            var allWords = synonymDictionary.GetAllWords();
            if (allWords.Count == 0) return "СЛОВО";
            return allWords[random.Next(allWords.Count)];
        }

        /// <summary>
        /// Создает объект падающего слова
        /// </summary>
        private FallingWord CreateFallingWord(string wordText, double speed, bool isBonus,
                                            string bonusType, Color wordColor, double x, double y)
        {
            var fallingWord = new FallingWord(
                wordText,
                speed,
                gameCanvas,
                isBonus,
                bonusType,
                wordColor);

            fallingWord.SetPosition(x, y);
            return fallingWord;
        }

        #endregion
    }
}