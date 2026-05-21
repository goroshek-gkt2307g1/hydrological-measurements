//обновить ProjectsWindow.xaml.cs - добавить функционал кнопки добавления проекта

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Views
{
	public partial class ProjectsWindow : Window
	{
		public ProjectsWindow()
		{
			InitializeComponent();
			LoadProjects();
			LoadFilters();
		}

		private void LoadProjects()
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				ProjectsDataGrid.ItemsSource = context.Projects
					.Include(p => p.ContractorIdFkNavigation)
					.Include(p => p.StatusIdFkNavigation)
					.ToList();
			}
		}

		private void LoadFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var statuses = context.ProjectStatuses.ToList();
					statuses.Insert(0, new ProjectStatus { ProjectStatusId = 0, Name = "Все статусы" });
					StatusFilterComboBox.ItemsSource = statuses;
					StatusFilterComboBox.DisplayMemberPath = "Name";
					StatusFilterComboBox.SelectedValuePath = "ProjectStatusId";
					StatusFilterComboBox.SelectedIndex = 0;

					var managers = context.Persons
						.Where(p => p.RoleIdFkNavigation.Name == "Менеджер")
						.ToList();
					managers.Insert(0, new Person { PersonId = 0, LastName = "Все менеджеры" });
					ManagerFilterComboBox.ItemsSource = managers;
					ManagerFilterComboBox.DisplayMemberPath = "LastName";
					ManagerFilterComboBox.SelectedValuePath = "PersonId";
					ManagerFilterComboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) ApplyFilters();
		}

		private void ManagerFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) ApplyFilters();
		}

		private void StartDateFilterPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) ApplyFilters();
		}

		private void ApplyFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var query = context.Projects
						.Include(p => p.ContractorIdFkNavigation)
						.Include(p => p.StatusIdFkNavigation)
						.AsQueryable();

					if (StatusFilterComboBox.SelectedItem is ProjectStatus status && status.ProjectStatusId != 0)
					{
						query = query.Where(p => p.StatusIdFk == status.ProjectStatusId);
					}

					if (ManagerFilterComboBox.SelectedItem is Person manager && manager.PersonId != 0)
					{
						query = query.Where(p => p.ContractorIdFk == manager.PersonId);
					}

					if (StartDateFilterPicker.SelectedDate.HasValue)
					{
						var selectedDate = DateOnly.FromDateTime(StartDateFilterPicker.SelectedDate.Value);
						query = query.Where(p => p.StartDate == selectedDate);
					}

					ProjectsDataGrid.ItemsSource = query.ToList();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка применения фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void BtnAddProject_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var dialog = new Window
					{
						Title = "Добавление проекта",
						Width = 550,
						Height = 580,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var grid = new Grid();
					for (int i = 0; i < 9; i++)
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

					//заказчик
					var lblClient = new TextBlock { Text = "Заказчик:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblClient, 1); Grid.SetColumn(lblClient, 0);
					var txtClient = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtClient, 1); Grid.SetColumn(txtClient, 1);

					//менеджер
					var lblManager = new TextBlock { Text = "Менеджер:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblManager, 2); Grid.SetColumn(lblManager, 0);
					var cmbManager = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var managers = context.Persons.Where(p => p.RoleIdFkNavigation.Name == "Менеджер").ToList();
					cmbManager.ItemsSource = managers;
					cmbManager.DisplayMemberPath = "LastName";
					if (managers.Any()) cmbManager.SelectedIndex = 0;
					Grid.SetRow(cmbManager, 2); Grid.SetColumn(cmbManager, 1);

					//дата начала
					var lblStart = new TextBlock { Text = "Дата начала:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblStart, 3); Grid.SetColumn(lblStart, 0);
					var dpStart = new DatePicker { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					dpStart.SelectedDate = DateTime.Now;
					Grid.SetRow(dpStart, 3); Grid.SetColumn(dpStart, 1);

					//дата окончания
					var lblEnd = new TextBlock { Text = "Дата окончания:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblEnd, 4); Grid.SetColumn(lblEnd, 0);
					var dpEnd = new DatePicker { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					dpEnd.SelectedDate = DateTime.Now.AddMonths(6);
					Grid.SetRow(dpEnd, 4); Grid.SetColumn(dpEnd, 1);

					//цель
					var lblPurpose = new TextBlock { Text = "Цель:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblPurpose, 5); Grid.SetColumn(lblPurpose, 0);
					var txtPurpose = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtPurpose, 5); Grid.SetColumn(txtPurpose, 1);

					//система высот
					var lblElevation = new TextBlock { Text = "Система высот:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblElevation, 6); Grid.SetColumn(lblElevation, 0);
					var txtElevation = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "Балтийская" };
					Grid.SetRow(txtElevation, 6); Grid.SetColumn(txtElevation, 1);

					//статус
					var lblStatus = new TextBlock { Text = "Статус:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblStatus, 7); Grid.SetColumn(lblStatus, 0);
					var cmbStatus = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var statuses = context.ProjectStatuses.ToList();
					cmbStatus.ItemsSource = statuses;
					cmbStatus.DisplayMemberPath = "Name";
					cmbStatus.SelectedIndex = 0;
					Grid.SetRow(cmbStatus, 7); Grid.SetColumn(cmbStatus, 1);

					//кнопки
					var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
					Grid.SetRow(btnPanel, 9); Grid.SetColumn(btnPanel, 1);
					var btnSave = new Button { Content = "Сохранить", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(44, 125, 160)), Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
					var btnCancel = new Button { Content = "Отмена", Width = 100, Height = 30, Background = Brushes.LightGray };
					btnPanel.Children.Add(btnSave);
					btnPanel.Children.Add(btnCancel);

					btnCancel.Click += (s, args) => dialog.Close();

					btnSave.Click += (s, args) =>
					{
						if (string.IsNullOrWhiteSpace(txtName.Text))
						{
							MessageBox.Show("Введите название проекта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (string.IsNullOrWhiteSpace(txtClient.Text))
						{
							MessageBox.Show("Введите заказчика!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbManager.SelectedItem == null)
						{
							MessageBox.Show("Выберите менеджера!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!dpStart.SelectedDate.HasValue)
						{
							MessageBox.Show("Выберите дату начала!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!dpEnd.SelectedDate.HasValue)
						{
							MessageBox.Show("Выберите дату окончания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbStatus.SelectedItem == null)
						{
							MessageBox.Show("Выберите статус проекта!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						var newProject = new Project
						{
							Name = txtName.Text.Trim(),
							Client = txtClient.Text.Trim(),
							ContractorIdFk = ((Person)cmbManager.SelectedItem).PersonId,
							StartDate = DateOnly.FromDateTime(dpStart.SelectedDate.Value),
							EndDate = DateOnly.FromDateTime(dpEnd.SelectedDate.Value),
							Purpose = txtPurpose.Text.Trim(),
							ElevationSystem = txtElevation.Text.Trim(),
							StatusIdFk = ((ProjectStatus)cmbStatus.SelectedItem).ProjectStatusId
						};

						context.Projects.Add(newProject);
						context.SaveChanges();
						MessageBox.Show("Проект успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadProjects();
					};

					grid.Children.Add(lblName);
					grid.Children.Add(txtName);
					grid.Children.Add(lblClient);
					grid.Children.Add(txtClient);
					grid.Children.Add(lblManager);
					grid.Children.Add(cmbManager);
					grid.Children.Add(lblStart);
					grid.Children.Add(dpStart);
					grid.Children.Add(lblEnd);
					grid.Children.Add(dpEnd);
					grid.Children.Add(lblPurpose);
					grid.Children.Add(txtPurpose);
					grid.Children.Add(lblElevation);
					grid.Children.Add(txtElevation);
					grid.Children.Add(lblStatus);
					grid.Children.Add(cmbStatus);
					grid.Children.Add(btnPanel);

					dialog.Content = grid;
					dialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении проекта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Button_Main_Click(object sender, RoutedEventArgs e)
		{
			new MainWindow().Show();
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

		private void Button_Admin_Click(object sender, RoutedEventArgs e)
		{
			new AdminWindow().Show();
			this.Close();
		}
	}
}