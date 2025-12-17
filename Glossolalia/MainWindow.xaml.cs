using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Glossolalia
{
   /// <summary>
   /// Главное окно приложения. Управляет основным интерфейсом и координацией работы менеджеров.
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
      }

      #region Обработчики событий элементов интерфейса

      /// <summary>
      /// Обработчик нажатия кнопки "Играть".
      /// </summary>
      private void PlayButton_Click(object sender, RoutedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия кнопки паузы.
      /// </summary>
      private void PauseButton_Click(object sender, RoutedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия кнопки "Настройки".
      /// </summary>
      private void SettingsButton_Click(object sender, RoutedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия кнопки "Правила".
      /// </summary>
      private void RulesButton_Click(object sender, RoutedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия кнопки "Выход".
      /// </summary>
      private void ExitButton_Click(object sender, RoutedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик изменения выбора разрешения.
      /// </summary>
      private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик изменения выбора режима окна.
      /// </summary>
      private void WindowModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия клавиш в окне.
      /// </summary>
      private void Window_KeyDown(object sender, KeyEventArgs e)
      {

      }

      /// <summary>
      /// Обработчик нажатия кнопки после окончания игры.
      /// </summary>
      private void GameOverButton_Click(object sender, RoutedEventArgs e)
      {

      }

      #endregion
   }
}
