using System;

namespace Glossolalia
{
   /// <summary>
   /// Класс для представления разрешения экрана
   /// </summary>
   public class Resolution
   {
      #region Свойства

      /// <summary>
      /// Ширина разрешения в пикселях
      /// </summary>
      public int Width { get; set; }

      /// <summary>
      /// Высота разрешения в пикселях
      /// </summary>
      public int Height { get; set; }

      #endregion

      #region Конструкторы

      /// <summary>
      /// Конструктор по умолчанию
      /// </summary>
      public Resolution() { }

      /// <summary>
      /// Конструктор с параметрами ширины и высоты
      /// </summary>
      public Resolution(int width, int height)
      {
         Width = width;
         Height = height;
      }

      #endregion

      #region Методы

      /// <summary>
      /// Возвращает строковое представление разрешения
      /// </summary>
      /// <returns>Строка в формате "Ширина x Высота"</returns>
      public override string ToString()
      {
         return $"{Width} x {Height}";
      }

      /// <summary>
      /// Преобразует строку в формате "Ширина x Высота" в объект Resolution
      /// </summary>
      /// <param name="resolutionString">Строка с разрешением</param>
      /// <returns>Объект Resolution или разрешение 800x600 по умолчанию</returns>
      public static Resolution Parse(string resolutionString)
      {
         var parts = resolutionString.Split(new[] { " x " }, StringSplitOptions.RemoveEmptyEntries);
         if (parts.Length == 2 &&
             int.TryParse(parts[0].Trim(), out int width) &&
             int.TryParse(parts[1].Trim(), out int height))
         {
            return new Resolution(width, height);
         }
         return new Resolution(800, 600);
      }

      #endregion
   }
}