//обновить AdminWindow.xaml.cs - добавить функционал кнопки добавления пользователя

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Views
{
	public partial class AdminWindow : Window
	{
		public AdminWindow()
		{
			InitializeComponent();
			LoadUsers();
			LoadFilters();
		}

		private void LoadUsers()
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				UsersDataGrid.ItemsSource = context.Persons
					.Include(p => p.RoleIdFkNavigation)
					.ToList();
			}
		}

		private void LoadFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var roles = context.PersonRoles.ToList();
					roles.Insert(0, new PersonRole { RoleId = 0, Name = "Все роли" });
					RoleFilterComboBox.ItemsSource = roles;
					RoleFilterComboBox.DisplayMemberPath = "Name";
					RoleFilterComboBox.SelectedIndex = 0;

					var hydroposts = context.Hydroposts.ToList();
					hydroposts.Insert(0, new Hydropost { HydropostId = 0, Name = "Все гидропосты" });
					HydropostFilterComboBox.ItemsSource = hydroposts;
					HydropostFilterComboBox.DisplayMemberPath = "Name";
					HydropostFilterComboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ApplyFilters();
		}

		private void ApplyFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var query = context.Persons
						.Include(p => p.RoleIdFkNavigation)
						.AsQueryable();

					if (RoleFilterComboBox.SelectedItem is PersonRole role && role.RoleId != 0)
					{
						query = query.Where(p => p.RoleIdFk == role.RoleId);
					}

					if (HydropostFilterComboBox.SelectedItem is Hydropost hp && hp.HydropostId != 0)
					{
						query = query.Where(p => p.PersonId == hp.ManagerIdFk);
					}

					if (DateFilterPicker.SelectedDate.HasValue)
					{
						var selectedDate = DateFilterPicker.SelectedDate.Value;
						var nextDay = selectedDate.AddDays(1);
						query = query.Where(p => p.CreatedAt >= selectedDate && p.CreatedAt < nextDay);
					}

					UsersDataGrid.ItemsSource = query.ToList();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка применения фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void BtnAddUser_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var dialog = new Window
					{
						Title = "Добавление пользователя",
						Width = 500,
						Height = 500,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var grid = new Grid();
					for (int i = 0; i < 7; i++)
						grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					grid.Margin = new Thickness(10);

					//фамилия
					var lblLastName = new TextBlock { Text = "Фамилия:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblLastName, 0); Grid.SetColumn(lblLastName, 0);
					var txtLastName = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtLastName, 0); Grid.SetColumn(txtLastName, 1);

					//имя
					var lblFirstName = new TextBlock { Text = "Имя:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblFirstName, 1); Grid.SetColumn(lblFirstName, 0);
					var txtFirstName = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtFirstName, 1); Grid.SetColumn(txtFirstName, 1);

					//отчество
					var lblMiddleName = new TextBlock { Text = "Отчество:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblMiddleName, 2); Grid.SetColumn(lblMiddleName, 0);
					var txtMiddleName = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtMiddleName, 2); Grid.SetColumn(txtMiddleName, 1);

					//логин
					var lblLogin = new TextBlock { Text = "Логин:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblLogin, 3); Grid.SetColumn(lblLogin, 0);
					var txtLogin = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtLogin, 3); Grid.SetColumn(txtLogin, 1);

					//пароль
					var lblPassword = new TextBlock { Text = "Пароль:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblPassword, 4); Grid.SetColumn(lblPassword, 0);
					var txtPassword = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "password123" };
					Grid.SetRow(txtPassword, 4); Grid.SetColumn(txtPassword, 1);

					//роль
					var lblRole = new TextBlock { Text = "Роль:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblRole, 5); Grid.SetColumn(lblRole, 0);
					var cmbRole = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var roles = context.PersonRoles.ToList();
					cmbRole.ItemsSource = roles;
					cmbRole.DisplayMemberPath = "Name";
					cmbRole.SelectedIndex = 2; //гидролог по умолчанию
					Grid.SetRow(cmbRole, 5); Grid.SetColumn(cmbRole, 1);

					//кнопки
					var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
					Grid.SetRow(btnPanel, 7); Grid.SetColumn(btnPanel, 1);
					var btnSave = new Button { Content = "Сохранить", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(44, 125, 160)), Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
					var btnCancel = new Button { Content = "Отмена", Width = 100, Height = 30, Background = Brushes.LightGray };
					btnPanel.Children.Add(btnSave);
					btnPanel.Children.Add(btnCancel);

					btnCancel.Click += (s, args) => dialog.Close();

					btnSave.Click += (s, args) =>
					{
						if (string.IsNullOrWhiteSpace(txtLastName.Text))
						{
							MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (string.IsNullOrWhiteSpace(txtFirstName.Text))
						{
							MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (string.IsNullOrWhiteSpace(txtLogin.Text))
						{
							MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbRole.SelectedItem == null)
						{
							MessageBox.Show("Выберите роль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						//проверка уникальности логина
						if (context.Persons.Any(p => p.Login == txtLogin.Text.Trim()))
						{
							MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						var newPerson = new Person
						{
							LastName = txtLastName.Text.Trim(),
							FirstName = txtFirstName.Text.Trim(),
							MiddleName = txtMiddleName.Text.Trim(),
							Login = txtLogin.Text.Trim(),
							Password = txtPassword.Text.Trim(),
							RoleIdFk = ((PersonRole)cmbRole.SelectedItem).RoleId,
							CreatedAt = DateTime.Now
						};

						context.Persons.Add(newPerson);
						context.SaveChanges();
						MessageBox.Show("Пользователь успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadUsers();
					};

					grid.Children.Add(lblLastName);
					grid.Children.Add(txtLastName);
					grid.Children.Add(lblFirstName);
					grid.Children.Add(txtFirstName);
					grid.Children.Add(lblMiddleName);
					grid.Children.Add(txtMiddleName);
					grid.Children.Add(lblLogin);
					grid.Children.Add(txtLogin);
					grid.Children.Add(lblPassword);
					grid.Children.Add(txtPassword);
					grid.Children.Add(lblRole);
					grid.Children.Add(cmbRole);
					grid.Children.Add(btnPanel);

					dialog.Content = grid;
					dialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Button_Main_Click(object sender, RoutedEventArgs e)
		{
			new MainWindow().Show();
			this.Close();
		}

		private void Button_Projects_Click(object sender, RoutedEventArgs e)
		{
			new ProjectsWindow().Show();
			this.Close();
		}

		private void Button_HydroPosts_Click(object sender, RoutedEventArgs e)
		{
			new HydroPostsWindow().Show();
			this.Close();
		}

		private void Button_Equipment_Click(object sender, RoutedEventArgs e)
		{
			new EquipmentWindow().Show();
			this.Close();
		}

		private void Button_Directories_Click(object sender, RoutedEventArgs e)
		{
			new DirectoriesWindow().Show();
			this.Close();
		}
	}
}