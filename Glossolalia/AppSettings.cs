using System;
using System.IO;
using System.Xml.Serialization;

namespace Glossolalia
{
   /// <summary>
   /// Настройки приложения. Сериализуются в XML файл
   /// </summary>
   [Serializable]
   public class AppSettings
   {
      /// <summary>
      /// Текущее разрешение экрана.
      /// </summary>
      public Resolution Resolution { get; set; }

      /// <summary>
      /// Текущий режим окна (оконный или полноэкранный).
      /// </summary>
      public WindowMode WindowMode { get; set; }

      /// <summary>
      /// Конструктор. Устанавливает значения по умолчанию.
      /// </summary>
      public AppSettings()
      {
         // Значения по умолчанию
         Resolution = new Resolution(800, 600);
         WindowMode = WindowMode.Windowed;
      }

      /// <summary>
      /// Сохраняет настройки в файл.
      /// </summary>
      public void Save(string fileName = "settings.xml")
      {
         try
         {
            XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
            using (StreamWriter writer = new StreamWriter(fileName))
            {
               serializer.Serialize(writer, this);
            }
         }
         catch (Exception)
         {
            // Игнорируем ошибки сохранения
         }
      }

      /// <summary>
      /// Загружает настройки из файла.
      /// </summary>
      /// <returns>Загруженные настройки или настройки по умолчанию</returns>
      public static AppSettings Load(string fileName = "settings.xml")
      {
         try
         {
            if (File.Exists(fileName))
            {
               XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
               using (StreamReader reader = new StreamReader(fileName))
               {
                  return (AppSettings)serializer.Deserialize(reader);
               }
            }
         }
         catch (Exception)
         {
            // Если ошибка загрузки - возвращаем настройки по умолчанию
         }

         return new AppSettings();
      }
   }

   /// <summary>
   /// Режимы окна приложения.
   /// </summary>
   public enum WindowMode
   {
      Windowed,
      Fullscreen
   }
}