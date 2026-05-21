//обновить App.xaml.cs для глобальной обработки исключений

using System;
using System.Windows;
using System.Windows.Threading;

namespace Гидрологические_измерения
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			//обработка необработанных исключений
			this.DispatcherUnhandledException += App_DispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			//запускаем окно логина
			var loginWindow = new Views.LoginWindow();
			loginWindow.Show();
		}

		private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show($"Произошла ошибка: {e.Exception.Message}\n\n{e.Exception.StackTrace}",
							"Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			e.Handled = true;
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			if (e.ExceptionObject is Exception ex)
			{
				MessageBox.Show($"Необработанная ошибка: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}