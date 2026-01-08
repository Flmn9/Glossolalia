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
         if (IsDestroyed) return;

         Canvas.SetLeft(textBlock, Position.X);
         Canvas.SetTop(textBlock, Position.Y);

         UpdateSelection();
         selectionRectangle.Visibility = SelectedLettersCount > 0
             ? Visibility.Visible
             : Visibility.Hidden;
      }

      /// <summary>
      /// Перемещает слово вниз
      /// </summary>
      /// <param name="deltaTime">Время, прошедшее с предыдущего кадра</param>
      public void MoveDown(double deltaTime)
      {
         if (IsDestroyed) return;

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
            UpdatePosition();
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
            UpdatePosition();
         }
      }

      /// <summary>
      /// Сбрасывает выделение букв
      /// </summary>
      public void ResetSelection()
      {
         SelectedLettersCount = 0;
         UpdatePosition();
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
         parentCanvas.Children.Remove(selectionRectangle);
      }

      /// <summary>
      /// Устанавливает новый текст слова
      /// </summary>
      public void SetWord(string newWord)
      {
         Word = newWord;
         textBlock.Text = newWord;
         cachedTextSize = null;
         UpdatePosition();
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
            VerticalAlignment = VerticalAlignment.Top,
            CacheMode = new BitmapCache()
         };

         selectionRectangle = new Rectangle
         {
            Fill = new SolidColorBrush(Color.FromArgb(100, 0, 120, 215)),
            Stroke = new SolidColorBrush(Colors.DarkBlue),
            StrokeThickness = 1,
            Visibility = Visibility.Hidden,
            RadiusX = 2,
            RadiusY = 2,
            CacheMode = new BitmapCache()
         };

         parentCanvas.Children.Add(selectionRectangle);
         parentCanvas.Children.Add(textBlock);
      }

      /// <summary>
      /// Обновляет отображение выделения букв
      /// </summary>
      private void UpdateSelection()
      {
         if (SelectedLettersCount <= 0) return;

         if (SelectedLettersCount > Word.Length)
         {
            SelectedLettersCount = Word.Length;
         }

         var selectedText = Word.Substring(0, SelectedLettersCount);
         var formattedText = CreateFormattedText(selectedText);

         selectionRectangle.Width = formattedText.Width + 4;
         selectionRectangle.Height = formattedText.Height + 4;
         Canvas.SetLeft(selectionRectangle, Position.X - 2);
         Canvas.SetTop(selectionRectangle, Position.Y - 2);
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

         var formattedText = CreateFormattedText(Word);
         cachedTextSize = new Size(formattedText.Width, formattedText.Height);
         return cachedTextSize.Value;
      }

      /// <summary>
      /// Создает форматированный текст для измерения
      /// </summary>
      /// <returns>Форматированный текст</returns>
      private FormattedText CreateFormattedText(string text)
      {
         return new FormattedText(
             text,
             System.Globalization.CultureInfo.CurrentCulture,
             FlowDirection.LeftToRight,
             new Typeface(textBlock.FontFamily, textBlock.FontStyle,
                        textBlock.FontWeight, textBlock.FontStretch),
             textBlock.FontSize,
             Brushes.Black,
             VisualTreeHelper.GetDpi(parentCanvas).PixelsPerDip);
      }

      #endregion
   }
}