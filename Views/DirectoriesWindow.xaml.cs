//обновить DirectoriesWindow.xaml.cs - добавить функционал кнопки добавления значения справочника

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Views
{
	public partial class DirectoriesWindow : Window
	{
		private string _currentDirectoryType;

		public DirectoriesWindow()
		{
			InitializeComponent();
			LoadDirectories();
		}

		private void LoadDirectories()
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				var directories = new[]
				{
					new { Name = "Роли пользователей", Type = "roles",
						  Description = $"Записей: {context.PersonRoles.Count()}" },
					new { Name = "Типы гидропостов", Type = "hydropost_types",
						  Description = $"Записей: {context.HydropostTypes.Count()}" },
					new { Name = "Типы оборудования", Type = "equipment_types",
						  Description = $"Записей: {context.EquipmentTypes.Count()}" },
					new { Name = "Статусы оборудования", Type = "equipment_statuses",
						  Description = $"Записей: {context.EquipmentStatuses.Count()}" },
					new { Name = "Статусы проектов", Type = "project_statuses",
						  Description = $"Записей: {context.ProjectStatuses.Count()}" }
				};
				DirectoriesDataGrid.ItemsSource = directories;
			}
		}

		private void DirectoriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (DirectoriesDataGrid.SelectedItem == null) return;

			dynamic selected = DirectoriesDataGrid.SelectedItem;
			_currentDirectoryType = selected.Type;
			LoadDirectoryValues(_currentDirectoryType);
		}

		private void LoadDirectoryValues(string directoryType)
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				switch (directoryType)
				{
					case "roles":
						var roles = context.PersonRoles
							.Select(r => new { Id = r.RoleId, Name = r.Name })
							.ToList();
						DirectoryValuesDataGrid.ItemsSource = roles;
						break;

					case "hydropost_types":
						var hpTypes = context.HydropostTypes
							.Select(t => new { Id = t.HydropostTypeId, Name = t.Name })
							.ToList();
						DirectoryValuesDataGrid.ItemsSource = hpTypes;
						break;

					case "equipment_types":
						var eqTypes = context.EquipmentTypes
							.Select(t => new { Id = t.EquipmentTypeId, Name = t.Name })
							.ToList();
						DirectoryValuesDataGrid.ItemsSource = eqTypes;
						break;

					case "equipment_statuses":
						var eqStatuses = context.EquipmentStatuses
							.Select(s => new { Id = s.EquipmentStatusId, Name = s.Name })
							.ToList();
						DirectoryValuesDataGrid.ItemsSource = eqStatuses;
						break;

					case "project_statuses":
						var prStatuses = context.ProjectStatuses
							.Select(s => new { Id = s.ProjectStatusId, Name = s.Name })
							.ToList();
						DirectoryValuesDataGrid.ItemsSource = prStatuses;
						break;
				}
			}
		}

		private void BtnAddValue_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(_currentDirectoryType))
			{
				MessageBox.Show("Выберите справочник из списка!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var dialog = new Window
					{
						Title = "Добавление значения справочника",
						Width = 400,
						Height = 200,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var grid = new Grid();
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					grid.Margin = new Thickness(10);

					//название
					var lblName = new TextBlock { Text = "Название:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblName, 0); Grid.SetColumn(lblName, 0);
					var txtName = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtName, 0); Grid.SetColumn(txtName, 1);

					//кнопки
					var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
					Grid.SetRow(btnPanel, 2); Grid.SetColumn(btnPanel, 1);
					var btnSave = new Button { Content = "Сохранить", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(44, 125, 160)), Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
					var btnCancel = new Button { Content = "Отмена", Width = 100, Height = 30, Background = Brushes.LightGray };
					btnPanel.Children.Add(btnSave);
					btnPanel.Children.Add(btnCancel);

					btnCancel.Click += (s, args) => dialog.Close();

					btnSave.Click += (s, args) =>
					{
						if (string.IsNullOrWhiteSpace(txtName.Text))
						{
							MessageBox.Show("Введите название значения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						string newName = txtName.Text.Trim();

						switch (_currentDirectoryType)
						{
							case "roles":
								context.PersonRoles.Add(new PersonRole { Name = newName });
								break;
							case "hydropost_types":
								context.HydropostTypes.Add(new HydropostType { Name = newName });
								break;
							case "equipment_types":
								context.EquipmentTypes.Add(new EquipmentType { Name = newName });
								break;
							case "equipment_statuses":
								context.EquipmentStatuses.Add(new EquipmentStatus { Name = newName });
								break;
							case "project_statuses":
								context.ProjectStatuses.Add(new ProjectStatus { Name = newName });
								break;
						}

						context.SaveChanges();
						MessageBox.Show("Значение успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadDirectoryValues(_currentDirectoryType);
						LoadDirectories(); //обновляем количество записей
					};

					grid.Children.Add(lblName);
					grid.Children.Add(txtName);
					grid.Children.Add(btnPanel);

					dialog.Content = grid;
					dialog.ShowDialog();
				}
			}
			catch (System.Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении значения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

		private void Button_Admin_Click(object sender, RoutedEventArgs e)
		{
			new AdminWindow().Show();
			this.Close();
		}
	}
}