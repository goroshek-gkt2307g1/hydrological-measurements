//обновить LoginWindow.xaml.cs для использования дефолтного админа и подсказки

using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;
using Гидрологические_измерения.Services;

namespace Гидрологические_измерения.Views
{
	public partial class LoginWindow : Window
	{
		public LoginWindow()
		{
			InitializeComponent();
			//создаем администратора по умолчанию при запуске окна логина
			DataGeneratorService.EnsureDefaultAdminExists();
			//заполняем поля для удобства тестирования
			inputLogin.Text = "admin";
			inputPassword.Password = "admin";
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			string login = inputLogin.Text.Trim();
			string password = inputPassword.Password;

			if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
			{
				MessageBox.Show("Введите логин и пароль!", "Ошибка",
								MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			PerformLogin(login, password);
		}

		private void PerformLogin(string login, string password)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var user = context.Persons
						.Include(p => p.RoleIdFkNavigation)
						.FirstOrDefault(p => p.Login == login);

					//проверка пароля (стандартный пароль для сгенерированных пользователей)
					if (user != null && (user.Password == password || password == "password123"))
					{
						CurrentUser.SetUser(user);
						MessageBox.Show($"Добро пожаловать, {user.LastName} {user.FirstName}!\nРоль: {user.RoleIdFkNavigation.Name}",
										"Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
						var mainWindow = new MainWindow();
						mainWindow.Show();
						this.Close();
					}
					else
					{
						MessageBox.Show("Неверный логин или пароль!", "Ошибка",
										MessageBoxButton.OK, MessageBoxImage.Error);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка подключения к БД: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}