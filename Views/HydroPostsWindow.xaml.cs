//обновить HydroPostsWindow.xaml.cs - добавить функционал кнопки добавления гидропоста

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Views
{
	public partial class HydroPostsWindow : Window
	{
		public HydroPostsWindow()
		{
			InitializeComponent();
			LoadHydroposts();
			LoadFilters();
		}

		private void LoadHydroposts()
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				var query = context.Hydroposts
					.Include(h => h.HydropostTypeIdFkNavigation)
					.Include(h => h.ManagerIdFkNavigation)
					.AsQueryable();

				HydropostsDataGrid.ItemsSource = query.ToList();
			}
		}

		private void LoadFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var types = context.HydropostTypes.ToList();
					types.Insert(0, new HydropostType { HydropostTypeId = 0, Name = "Все типы" });

					TypeFilterComboBox.ItemsSource = types;
					TypeFilterComboBox.DisplayMemberPath = "Name";
					TypeFilterComboBox.SelectedValuePath = "HydropostTypeId";
					TypeFilterComboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void TypeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ApplyFilters();
		}

		private void ApplyFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var query = context.Hydroposts
						.Include(h => h.HydropostTypeIdFkNavigation)
						.Include(h => h.ManagerIdFkNavigation)
						.AsQueryable();

					if (TypeFilterComboBox.SelectedItem is HydropostType selectedType && selectedType.HydropostTypeId != 0)
					{
						query = query.Where(h => h.HydropostTypeIdFk == selectedType.HydropostTypeId);
					}

					HydropostsDataGrid.ItemsSource = query.ToList();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка применения фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void BtnAddHydropost_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var dialog = new Window
					{
						Title = "Добавление гидропоста",
						Width = 500,
						Height = 550,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var grid = new Grid();
					for (int i = 0; i < 8; i++)
						grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					grid.Margin = new Thickness(10);

					//название
					var lblName = new TextBlock { Text = "Название:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblName, 0); Grid.SetColumn(lblName, 0);
					var txtName = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtName, 0); Grid.SetColumn(txtName, 1);

					//описание
					var lblDesc = new TextBlock { Text = "Описание:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblDesc, 1); Grid.SetColumn(lblDesc, 0);
					var txtDesc = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtDesc, 1); Grid.SetColumn(txtDesc, 1);

					//тип гидропоста
					var lblType = new TextBlock { Text = "Тип гидропоста:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblType, 2); Grid.SetColumn(lblType, 0);
					var cmbType = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var types = context.HydropostTypes.ToList();
					cmbType.ItemsSource = types;
					cmbType.DisplayMemberPath = "Name";
					cmbType.SelectedIndex = 0;
					Grid.SetRow(cmbType, 2); Grid.SetColumn(cmbType, 1);

					//менеджер
					var lblManager = new TextBlock { Text = "Менеджер:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblManager, 3); Grid.SetColumn(lblManager, 0);
					var cmbManager = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var managers = context.Persons.Where(p => p.RoleIdFkNavigation.Name == "Менеджер").ToList();
					cmbManager.ItemsSource = managers;
					cmbManager.DisplayMemberPath = "LastName";
					if (managers.Any()) cmbManager.SelectedIndex = 0;
					Grid.SetRow(cmbManager, 3); Grid.SetColumn(cmbManager, 1);

					//широта
					var lblLat = new TextBlock { Text = "Широта:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblLat, 4); Grid.SetColumn(lblLat, 0);
					var txtLat = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "55.0" };
					Grid.SetRow(txtLat, 4); Grid.SetColumn(txtLat, 1);

					//долгота
					var lblLon = new TextBlock { Text = "Долгота:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblLon, 5); Grid.SetColumn(lblLon, 0);
					var txtLon = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "37.0" };
					Grid.SetRow(txtLon, 5); Grid.SetColumn(txtLon, 1);

					//ноль графика
					var lblZero = new TextBlock { Text = "Ноль графика (м):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblZero, 6); Grid.SetColumn(lblZero, 0);
					var txtZero = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "100" };
					Grid.SetRow(txtZero, 6); Grid.SetColumn(txtZero, 1);

					//кнопки
					var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
					Grid.SetRow(btnPanel, 8); Grid.SetColumn(btnPanel, 1);
					var btnSave = new Button { Content = "Сохранить", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(44, 125, 160)), Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
					var btnCancel = new Button { Content = "Отмена", Width = 100, Height = 30, Background = Brushes.LightGray };
					btnPanel.Children.Add(btnSave);
					btnPanel.Children.Add(btnCancel);

					btnCancel.Click += (s, args) => dialog.Close();

					btnSave.Click += (s, args) =>
					{
						if (string.IsNullOrWhiteSpace(txtName.Text))
						{
							MessageBox.Show("Введите название гидропоста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbType.SelectedItem == null)
						{
							MessageBox.Show("Выберите тип гидропоста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbManager.SelectedItem == null)
						{
							MessageBox.Show("Выберите менеджера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!decimal.TryParse(txtLat.Text, out decimal lat))
						{
							MessageBox.Show("Введите корректную широту!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!decimal.TryParse(txtLon.Text, out decimal lon))
						{
							MessageBox.Show("Введите корректную долготу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!decimal.TryParse(txtZero.Text, out decimal zero))
						{
							MessageBox.Show("Введите корректный ноль графика!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						var newHydropost = new Hydropost
						{
							Name = txtName.Text.Trim(),
							Description = txtDesc.Text.Trim(),
							HydropostTypeIdFk = ((HydropostType)cmbType.SelectedItem).HydropostTypeId,
							ManagerIdFk = ((Person)cmbManager.SelectedItem).PersonId,
							Latitude = lat,
							Longtude = lon,
							ZeroWaterLevel = zero
						};

						context.Hydroposts.Add(newHydropost);
						context.SaveChanges();
						MessageBox.Show("Гидропост успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadHydroposts();
					};

					grid.Children.Add(lblName);
					grid.Children.Add(txtName);
					grid.Children.Add(lblDesc);
					grid.Children.Add(txtDesc);
					grid.Children.Add(lblType);
					grid.Children.Add(cmbType);
					grid.Children.Add(lblManager);
					grid.Children.Add(cmbManager);
					grid.Children.Add(lblLat);
					grid.Children.Add(txtLat);
					grid.Children.Add(lblLon);
					grid.Children.Add(txtLon);
					grid.Children.Add(lblZero);
					grid.Children.Add(txtZero);
					grid.Children.Add(btnPanel);

					dialog.Content = grid;
					dialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении гидропоста: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

		private void Button_Admin_Click(object sender, RoutedEventArgs e)
		{
			new AdminWindow().Show();
			this.Close();
		}
	}
}