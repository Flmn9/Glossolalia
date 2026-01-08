using System;
using System.Windows;
using System.Windows.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Glossolalia
{
    /// <summary>
    /// Менеджер интерфейса пользователя
    /// </summary>
    public class UIManager
    {
        #region Поля

        private readonly ScrollViewer settingsScrollViewer;
        private readonly ScrollViewer rulesScrollViewer;
        private readonly UIElement mainMenuContainer;
        private readonly UIElement gameContainer;
        private readonly TextBlock scoreText;
        private readonly TextBlock timeText;
        private readonly UIElement glossolaliaLabel;
        private readonly UIElement gameCanvas;
        private readonly UIElement gameOverStackPanel;
        private readonly TextBlock multiplierText;

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор. Инициализирует ссылки на элементы UI
        /// </summary>
        /// <param name="settingsScrollViewer">ScrollViewer настроек</param>
        /// <param name="rulesScrollViewer">ScrollViewer правил</param>
        /// <param name="mainMenuContainer">Контейнер главного меню</param>
        /// <param name="gameContainer">Контейнер игрового интерфейса</param>
        /// <param name="scoreText">Текстовый блок счета</param>
        /// <param name="timeText">Текстовый блок времени</param>
        /// <param name="glossolaliaLabel">Заголовок игры</param>
        /// <param name="gameCanvas">Игровой холст</param>
        /// <param name="gameOverStackPanel">Панель окончания игры</param>
        /// <param name="multiplierText">Текстовый блок множителя</param>
        public UIManager(ScrollViewer settingsScrollViewer, ScrollViewer rulesScrollViewer,
                        UIElement mainMenuContainer, UIElement gameContainer,
                        TextBlock scoreText, TextBlock timeText,
                        UIElement glossolaliaLabel, UIElement gameCanvas, UIElement gameOverStackPanel,
                        TextBlock multiplierText)
        {
            this.settingsScrollViewer = settingsScrollViewer;
            this.rulesScrollViewer = rulesScrollViewer;
            this.mainMenuContainer = mainMenuContainer;
            this.gameContainer = gameContainer;
            this.scoreText = scoreText;
            this.timeText = timeText;
            this.glossolaliaLabel = glossolaliaLabel;
            this.gameCanvas = gameCanvas;
            this.gameOverStackPanel = gameOverStackPanel;
            this.multiplierText = multiplierText;

            HideAllScrollViewers();
        }

        #endregion

        #region Методы управления UI

        /// <summary>
        /// Скрывает все ScrollViewer'ы (окна настроек и правил)
        /// </summary>
        public void HideAllScrollViewers()
        {
            settingsScrollViewer.Visibility = Visibility.Collapsed;
            rulesScrollViewer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Показывает окно настроек
        /// </summary>
        public void ShowSettings()
        {
            ResetScrollViewer(settingsScrollViewer);
            settingsScrollViewer.Visibility = Visibility.Visible;
            rulesScrollViewer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Показывает окно правил
        /// </summary>
        public void ShowRules()
        {
            ResetScrollViewer(rulesScrollViewer);
            rulesScrollViewer.Visibility = Visibility.Visible;
            settingsScrollViewer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Показывает главное меню
        /// </summary>
        /// <param name="showTitle">Показывать ли заголовок игры</param>
        public void ShowMainMenu(bool showTitle = true)
        {
            mainMenuContainer.Visibility = Visibility.Visible;
            gameContainer.Visibility = Visibility.Collapsed;
            gameOverStackPanel.Visibility = Visibility.Collapsed;
            glossolaliaLabel.Visibility = showTitle ? Visibility.Visible : Visibility.Collapsed;
            HideAllScrollViewers();
        }


/// <summary>
/// Показывает игровой интерфейс
/// </summary>
      public void ShowGame()
        {
            mainMenuContainer.Visibility = Visibility.Collapsed;
            gameContainer.Visibility = Visibility.Visible;
            gameCanvas.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Показывает меню паузы (главное меню без заголовка)
        /// </summary>
        public void ShowPauseMenu()
        {
            gameCanvas.Visibility = Visibility.Collapsed;
            ShowMainMenu(false);
            gameContainer.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Показывает экран окончания игры
        /// </summary>
        public void ShowGameOver()
        {
            gameOverStackPanel.Visibility = Visibility.Visible;
        }

        #endregion

        #region Методы обновления данных

        /// <summary>
        /// Обновляет отображение счета
        /// </summary>
        public void UpdateScore(int score)
        {
            scoreText.Text = score.ToString();
        }

        /// <summary>
        /// Обновляет отображение времени
        /// </summary>
        public void UpdateTime(TimeSpan time)
        {
            timeText.Text = time.ToString(@"mm\:ss");
        }

        /// <summary>
        /// Обновляет отображение множителя
        /// </summary>
        public void UpdateMultiplier(int multiplier)
        {
            multiplierText.Text = $"x{multiplier}";
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Сбрасывает прокрутку ScrollViewer'а в начальное положение
        /// </summary>
        private void ResetScrollViewer(ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset(0);
            scrollViewer.ScrollToHorizontalOffset(0);
        }

        #endregion
    }
}
