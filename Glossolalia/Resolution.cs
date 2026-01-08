using System;

namespace Glossolalia
{
   /// <summary>
   /// Класс для представления разрешения экрана
   /// </summary>
   public class Resolution
   {
      /// <summary>
      /// Ширина разрешения в пикселях.
      /// </summary>
      public int Width { get; set; }

      /// <summary>
      /// Высота разрешения в пикселях.
      /// </summary>
      public int Height { get; set; }

      /// <summary>
      /// Конструктор по умолчанию.
      /// </summary>
      public Resolution() { }

      /// <summary>
      /// Конструктор с параметрами.
      /// </summary>
      public Resolution(int width, int height)
      {
         Width = width;
         Height = height;
      }

      /// <summary>
      /// Возвращает строковое представление разрешения.
      /// </summary>
      /// <returns>Строка в формате "Ширина x Высота"</returns>
      public override string ToString()
      {
         return $"{Width} x {Height}";
      }

      /// <summary>
      /// Преобразует строку в формате "Ширина x Высота" в объект Resolution.
      /// </summary>
      /// <returns>Объект Resolution. В случае ошибки - разрешение 800x600</returns>
      public static Resolution Parse(string resolutionString)
      {
         var parts = resolutionString.Split(new[] { " x " }, StringSplitOptions.RemoveEmptyEntries);
         if (parts.Length == 2 &&
             int.TryParse(parts[0].Trim(), out int width) &&
             int.TryParse(parts[1].Trim(), out int height))
         {
            return new Resolution(width, height);
         }
         return new Resolution(800, 600); // Значение по умолчанию
      }
   }
}