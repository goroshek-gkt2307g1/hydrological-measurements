//обновить EquipmentWindow.xaml.cs - добавить функционал кнопки добавления оборудования

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Views
{
	public partial class EquipmentWindow : Window
	{
		private bool _requiresCalibrationOnly = false;

		public EquipmentWindow()
		{
			InitializeComponent();
			LoadEquipment();
			LoadFilters();
			FindCheckBoxesAndSubscribe();
		}

		private void FindCheckBoxesAndSubscribe()
		{
			var stackPanel = FindVisualChild<StackPanel>(this, "Калибровка");
			if (stackPanel != null)
			{
				foreach (var child in stackPanel.Children)
				{
					if (child is CheckBox cb)
					{
						if (cb.Content?.ToString()?.Contains("Все") == true)
						{
							cb.Checked += CalibrationFilter_Changed;
							cb.Unchecked += CalibrationFilter_Changed;
						}
						else if (cb.Content?.ToString()?.Contains("Требуется") == true)
						{
							cb.Checked += CalibrationFilter_Changed;
							cb.Unchecked += CalibrationFilter_Changed;
						}
					}
				}
			}
		}

		private T FindVisualChild<T>(DependencyObject parent, string name) where T : FrameworkElement
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T element && element.Name == name)
					return element;
				var result = FindVisualChild<T>(child, name);
				if (result != null)
					return result;
			}
			return null;
		}

		private void LoadEquipment()
		{
			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				EquipmentDataGrid.ItemsSource = context.Equipments
					.Include(e => e.EquipmentTypeIdFkNavigation)
					.Include(e => e.EquipmentStatusIdFkNavigation)
					.Include(e => e.HydropostIdFkNavigation)
					.ToList();
			}
		}

		private void LoadFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var types = context.EquipmentTypes.ToList();
					types.Insert(0, new EquipmentType { EquipmentTypeId = 0, Name = "Все типы" });
					TypeFilterComboBox.ItemsSource = types;
					TypeFilterComboBox.DisplayMemberPath = "Name";
					TypeFilterComboBox.SelectedIndex = 0;

					var statuses = context.EquipmentStatuses.ToList();
					statuses.Insert(0, new EquipmentStatus { EquipmentStatusId = 0, Name = "Все статусы" });
					StatusFilterComboBox.ItemsSource = statuses;
					StatusFilterComboBox.DisplayMemberPath = "Name";
					StatusFilterComboBox.SelectedIndex = 0;

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

		private void CalibrationFilter_Changed(object sender, RoutedEventArgs e)
		{
			var cb = sender as CheckBox;
			if (cb == null) return;

			if (cb.Content?.ToString()?.Contains("Все") == true && cb.IsChecked == true)
			{
				_requiresCalibrationOnly = false;
				UncheckOtherCheckBox("Требуется");
			}
			else if (cb.Content?.ToString()?.Contains("Требуется") == true && cb.IsChecked == true)
			{
				_requiresCalibrationOnly = true;
				UncheckOtherCheckBox("Все");
			}
			ApplyFilters();
		}

		private void UncheckOtherCheckBox(string excludeContent)
		{
			var stackPanel = FindVisualChild<StackPanel>(this, "Калибровка");
			if (stackPanel != null)
			{
				foreach (var child in stackPanel.Children)
				{
					if (child is CheckBox cb && cb.Content?.ToString()?.Contains(excludeContent) != true)
					{
						cb.IsChecked = false;
					}
				}
			}
		}

		private void ApplyFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var query = context.Equipments
						.Include(e => e.EquipmentTypeIdFkNavigation)
						.Include(e => e.EquipmentStatusIdFkNavigation)
						.Include(e => e.HydropostIdFkNavigation)
						.AsQueryable();

					if (TypeFilterComboBox.SelectedItem is EquipmentType type && type.EquipmentTypeId != 0)
					{
						query = query.Where(e => e.EquipmentTypeIdFk == type.EquipmentTypeId);
					}

					if (StatusFilterComboBox.SelectedItem is EquipmentStatus status && status.EquipmentStatusId != 0)
					{
						query = query.Where(e => e.EquipmentStatusIdFk == status.EquipmentStatusId);
					}

					if (HydropostFilterComboBox.SelectedItem is Hydropost hp && hp.HydropostId != 0)
					{
						query = query.Where(e => e.HydropostIdFk == hp.HydropostId);
					}

					if (_requiresCalibrationOnly)
					{
						var threshold = DateOnly.FromDateTime(DateTime.Now.AddDays(-180));
						query = query.Where(e => e.LastCalibration < threshold);
					}

					EquipmentDataGrid.ItemsSource = query.ToList();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка применения фильтров: {ex.Message}",
								"Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void BtnAddEquipment_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					//создаем диалоговое окно для ввода данных
					var dialog = new Window
					{
						Title = "Добавление оборудования",
						Width = 500,
						Height = 500,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var grid = new Grid();
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
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

					//тип оборудования
					var lblType = new TextBlock { Text = "Тип оборудования:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblType, 2); Grid.SetColumn(lblType, 0);
					var cmbType = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var types = context.EquipmentTypes.ToList();
					cmbType.ItemsSource = types;
					cmbType.DisplayMemberPath = "Name";
					cmbType.SelectedIndex = 0;
					Grid.SetRow(cmbType, 2); Grid.SetColumn(cmbType, 1);

					//статус
					var lblStatus = new TextBlock { Text = "Статус:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblStatus, 3); Grid.SetColumn(lblStatus, 0);
					var cmbStatus = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var statuses = context.EquipmentStatuses.ToList();
					cmbStatus.ItemsSource = statuses;
					cmbStatus.DisplayMemberPath = "Name";
					cmbStatus.SelectedIndex = 0;
					Grid.SetRow(cmbStatus, 3); Grid.SetColumn(cmbStatus, 1);

					//гидропост
					var lblHydropost = new TextBlock { Text = "Гидропост:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblHydropost, 4); Grid.SetColumn(lblHydropost, 0);
					var cmbHydropost = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var hydroposts = context.Hydroposts.ToList();
					cmbHydropost.ItemsSource = hydroposts;
					cmbHydropost.DisplayMemberPath = "Name";
					if (hydroposts.Any()) cmbHydropost.SelectedIndex = 0;
					Grid.SetRow(cmbHydropost, 4); Grid.SetColumn(cmbHydropost, 1);

					//дата калибровки
					var lblCalibration = new TextBlock { Text = "Дата калибровки:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblCalibration, 5); Grid.SetColumn(lblCalibration, 0);
					var dpCalibration = new DatePicker { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					dpCalibration.SelectedDate = DateTime.Now;
					Grid.SetRow(dpCalibration, 5); Grid.SetColumn(dpCalibration, 1);

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
						if (string.IsNullOrWhiteSpace(txtName.Text))
						{
							MessageBox.Show("Введите название оборудования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbType.SelectedItem == null)
						{
							MessageBox.Show("Выберите тип оборудования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbStatus.SelectedItem == null)
						{
							MessageBox.Show("Выберите статус оборудования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbHydropost.SelectedItem == null)
						{
							MessageBox.Show("Выберите гидропост!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						var newEquipment = new Equipment
						{
							Name = txtName.Text.Trim(),
							Description = txtDesc.Text.Trim(),
							EquipmentTypeIdFk = ((EquipmentType)cmbType.SelectedItem).EquipmentTypeId,
							EquipmentStatusIdFk = ((EquipmentStatus)cmbStatus.SelectedItem).EquipmentStatusId,
							HydropostIdFk = ((Hydropost)cmbHydropost.SelectedItem).HydropostId,
							LastCalibration = dpCalibration.SelectedDate.HasValue ? DateOnly.FromDateTime(dpCalibration.SelectedDate.Value) : DateOnly.FromDateTime(DateTime.Now),
							CreatedAt = DateTime.Now
						};

						context.Equipments.Add(newEquipment);
						context.SaveChanges();
						MessageBox.Show("Оборудование успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadEquipment();
					};

					grid.Children.Add(lblName);
					grid.Children.Add(txtName);
					grid.Children.Add(lblDesc);
					grid.Children.Add(txtDesc);
					grid.Children.Add(lblType);
					grid.Children.Add(cmbType);
					grid.Children.Add(lblStatus);
					grid.Children.Add(cmbStatus);
					grid.Children.Add(lblHydropost);
					grid.Children.Add(cmbHydropost);
					grid.Children.Add(lblCalibration);
					grid.Children.Add(dpCalibration);
					grid.Children.Add(btnPanel);

					dialog.Content = grid;
					dialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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