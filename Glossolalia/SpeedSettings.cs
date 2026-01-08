namespace Glossolalia
{
   /// <summary>
   /// Настройки скорости игры
   /// </summary>
   public class SpeedSettings
   {
      #region Константы

      /// <summary>
      /// Минимальная скорость слов
      /// </summary>
      public const double MIN_SPEED = 1.0;

      /// <summary>
      /// Максимальная скорость слов
      /// </summary>
      public const double MAX_SPEED = 100.0;

      /// <summary>
      /// Скорость слов по умолчанию
      /// </summary>
      public const double DEFAULT_SPEED = 10.0;

      #endregion

      #region Свойства

      /// <summary>
      /// Текущая базовая скорость слов
      /// </summary>
      public double WordSpeed { get; set; }

      #endregion

      #region Конструктор

      /// <summary>
      /// Конструктор с настройками по умолчанию
      /// </summary>
      public SpeedSettings()
      {
         WordSpeed = DEFAULT_SPEED;
      }

      #endregion

      #region Статические методы

      /// <summary>
      /// Проверяет, находится ли скорость в допустимом диапазоне
      /// </summary>
      /// <returns>True, если скорость в допустимом диапазоне</returns>
      public static bool IsValidSpeed(double speed)
      {
         return speed >= MIN_SPEED && speed <= MAX_SPEED;
      }

      /// <summary>
      /// Приводит скорость к допустимому диапазону
      /// </summary>
      /// <param name="speed">Исходная скорость</param>
      /// <returns>Скорость в допустимом диапазоне</returns>
      public static double ClampSpeed(double speed)
      {
         if (speed < MIN_SPEED) return MIN_SPEED;
         if (speed > MAX_SPEED) return MAX_SPEED;
         return speed;
      }

      #endregion
   }
}