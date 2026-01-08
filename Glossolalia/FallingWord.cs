using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Glossolalia
{
    /// <summary>
    /// Класс, представляющий падающее слово на игровом поле
    /// </summary>
    public class FallingWord
    {
        #region Поля

        private TextBlock textBlock;
        private Rectangle selectionRectangle;
        private readonly Canvas parentCanvas;
        private Size? cachedTextSize;

        #endregion

        #region Свойства

        /// <summary>
        /// Текст слова
        /// </summary>
        public string Word { get; private set; }

        /// <summary>
        /// Текущая скорость падения
        /// </summary>
        public double Speed { get; private set; }

        /// <summary>
        /// Базовая скорость падения
        /// </summary>
        public double BaseSpeed { get; private set; }

        /// <summary>
        /// Позиция слова на холсте
        /// </summary>
        public Point Position { get; private set; }

        /// <summary>
        /// Флаг, указывающий, является ли слово бонусным
        /// </summary>
        public bool IsBonus { get; private set; }

        /// <summary>
        /// Тип бонуса (если слово бонусное)
        /// </summary>
        public string BonusType { get; private set; }

        /// <summary>
        /// Цвет текста слова
        /// </summary>
        public Color WordColor { get; private set; }

        /// <summary>
        /// Количество выделенных букв
        /// </summary>
        public int SelectedLettersCount { get; private set; }

        /// <summary>
        /// Флаг, указывающий, уничтожено ли слово
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Границы слова на холсте
        /// </summary>
        public Rect Bounds
        {
            get
            {
                var textSize = MeasureTextSize();
                return new Rect(Position.X, Position.Y, textSize.Width, textSize.Height);
            }
        }

        #endregion

        #region Конструктор

        /// <summary>
        /// Конструктор падающего слова
        /// </summary>
        /// <param name="word">Текст слова</param>
        /// <param name="baseSpeed">Базовая скорость падения</param>
        /// <param name="parentCanvas">Холст для отображения</param>
        /// <param name="isBonus">Флаг бонусного слова</param>
        /// <param name="bonusType">Тип бонуса</param>
        /// <param name="color">Цвет текста</param>
        public FallingWord(string word, double baseSpeed, Canvas parentCanvas,
                          bool isBonus = false, string bonusType = null, Color? color = null)
        {
            Word = word;
            BaseSpeed = baseSpeed;
            Speed = baseSpeed;
            this.parentCanvas = parentCanvas;
            IsBonus = isBonus;
            BonusType = bonusType;
            WordColor = color ?? Colors.Black;

            InitializeUI();
        }

        #endregion

        #region Публичные методы

        /// <summary>
        /// Обновляет позицию слова на холсте
        /// </summary>
        public void UpdatePosition()
        {
            Canvas.SetLeft(textBlock, Position.X);
            Canvas.SetTop(textBlock, Position.Y);
        }

        /// <summary>
        /// Перемещает слово вниз
        /// </summary>
        /// <param name="deltaTime">Время, прошедшее с предыдущего кадра</param>
        public void MoveDown(double deltaTime)
        {
            double deltaY = Speed * deltaTime;
            Position = new Point(Position.X, Position.Y + deltaY);
            UpdatePosition();
        }

        /// <summary>
        /// Проверяет, достигло ли слово нижней границы холста
        /// </summary>
        /// <returns>True, если слово достигло нижней границы</returns>
        public bool ReachedBottom(double canvasHeight)
        {
            return Bounds.Bottom >= canvasHeight;
        }

        /// <summary>
        /// Устанавливает позицию слова на холсте
        /// </summary>
        public void SetPosition(double x, double y)
        {
            Position = new Point(x, y);
            UpdatePosition();
        }

        /// <summary>
        /// Корректирует позицию слова при столкновении с другим словом
        /// </summary>
        public void AdjustPositionForCollision(FallingWord otherWord)
        {
            if (this == otherWord || IsDestroyed || otherWord.IsDestroyed) return;

            var thisBounds = Bounds;
            var otherBounds = otherWord.Bounds;

            if (thisBounds.Bottom > otherBounds.Top &&
                thisBounds.Top < otherBounds.Bottom &&
                thisBounds.IntersectsWith(otherBounds))
            {
                double newY = otherBounds.Top - thisBounds.Height;
                Position = new Point(Position.X, newY);
                Speed = otherWord.Speed;
                UpdatePosition();
            }
        }

        /// <summary>
        /// Устанавливает новую скорость падения
        /// </summary>
        public void SetSpeed(double newSpeed)
        {
            Speed = newSpeed;
        }

        /// <summary>
        /// Обновляет базовую скорость падения
        /// </summary>
        public void UpdateBaseSpeed(double newBaseSpeed)
        {
            BaseSpeed = newBaseSpeed;
        }

        /// <summary>
        /// Восстанавливает оригинальную скорость падения
        /// </summary>
        public void RestoreOriginalSpeed()
        {
            Speed = BaseSpeed;
        }

        /// <summary>
        /// Проверяет столкновение с другим словом
        /// </summary>
        /// <returns>True, если слова пересекаются</returns>
        public bool CollidesWith(FallingWord other)
        {
            if (this == other || IsDestroyed || other.IsDestroyed) return false;

            var thisBounds = Bounds;
            var otherBounds = other.Bounds;

            thisBounds.Inflate(2, 2);
            otherBounds.Inflate(2, 2);

            return thisBounds.IntersectsWith(otherBounds);
        }

        /// <summary>
        /// Выделяет следующую букву слова
        /// </summary>
        /// <returns>True, если выделение успешно</returns>
        public bool SelectNextLetter()
        {
            if (SelectedLettersCount < Word.Length)
            {
                SelectedLettersCount++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Снимает выделение с последней буквы
        /// </summary>
        public void DeselectLastLetter()
        {
            if (SelectedLettersCount > 0)
            {
                SelectedLettersCount--;
            }
        }

        /// <summary>
        /// Сбрасывает выделение букв
        /// </summary>
        public void ResetSelection()
        {
            SelectedLettersCount = 0;
        }

        /// <summary>
        /// Проверяет, полностью ли выделено слово
        /// </summary>
        /// <returns>True, если все буквы выделены</returns>
        public bool IsFullySelected()
        {
            return SelectedLettersCount == Word.Length;
        }

        /// <summary>
        /// Уничтожает слово, удаляя его с холста
        /// </summary>
        public void Destroy()
        {
            if (IsDestroyed) return;

            IsDestroyed = true;
            parentCanvas.Children.Remove(textBlock);
            if (selectionRectangle != null)
            {
                parentCanvas.Children.Remove(selectionRectangle);
            }
        }

        #endregion

        #region Приватные методы

        /// <summary>
        /// Инициализирует UI элементы слова
        /// </summary>
        private void InitializeUI()
        {
            textBlock = new TextBlock
            {
                Text = Word,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 24,
                Foreground = new SolidColorBrush(WordColor),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            parentCanvas.Children.Add(textBlock);
        }

        /// <summary>
        /// Измеряет размер текста слова
        /// </summary>
        /// <returns>Размер текста</returns>
        private Size MeasureTextSize()
        {
            if (cachedTextSize.HasValue)
            {
                return cachedTextSize.Value;
            }

            var formattedText = new FormattedText(
                Word,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle,
                           textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                VisualTreeHelper.GetDpi(parentCanvas).PixelsPerDip);

            cachedTextSize = new Size(formattedText.Width, formattedText.Height);
            return cachedTextSize.Value;
        }

        #endregion
    }
}