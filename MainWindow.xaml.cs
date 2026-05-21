//добавить в MainWindow.xaml.cs методы для кнопок навигации и управления доступом

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.EntityFrameworkCore;
using Гидрологические_измерения.Entities;
using Гидрологические_измерения.Services;
using Гидрологические_измерения.Views;

namespace Гидрологические_измерения
{
	public partial class MainWindow : Window
	{
		private string _currentGraphType = "Гидрограф";

		public MainWindow()
		{
			try
			{
				InitializeComponent();
				LoadMeasurements();
				LoadFilters();
				SetupAccessRights();
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка");
			}
		}

		#region Управление доступом

		private void SetupAccessRights()
		{
			//обновляем заголовок окна
			this.Title = $"Главная | {CurrentUser.LastName} {CurrentUser.FirstName} ({CurrentUser.RoleName})";

			//настройка видимости кнопок в зависимости от роли
			if (CurrentUser.IsHydrolog)
			{
				//гидролог видит только главную и проекты
				BtnDirectories.Visibility = Visibility.Collapsed;
				BtnAdmin.Visibility = Visibility.Collapsed;
				//скрываем кнопки управления данными
				var clearDbButton = FindVisualChild<Button>(this, "Button_ClearDatabase");
				if (clearDbButton != null)
					clearDbButton.Visibility = Visibility.Collapsed;
				var addSyntheticButton = FindVisualChild<Button>(this, "Button_AddSyntheticData");
				if (addSyntheticButton != null)
					addSyntheticButton.Visibility = Visibility.Collapsed;
			}
			else if (CurrentUser.IsManager)
			{
				//менеджер не видит администрирование и справочники
				BtnDirectories.Visibility = Visibility.Collapsed;
				BtnAdmin.Visibility = Visibility.Collapsed;
			}
			else if (CurrentUser.IsAdmin)
			{
				//администратор видит всё
				BtnDirectories.Visibility = Visibility.Visible;
				BtnAdmin.Visibility = Visibility.Visible;
			}
		}

		private T FindVisualChild<T>(DependencyObject parent, string elementName = null) where T : FrameworkElement
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);
				if (child is T element && (elementName == null || element.Name == elementName))
					return element;
				var result = FindVisualChild<T>(child, elementName);
				if (result != null)
					return result;
			}
			return null;
		}

		#endregion

		#region Загрузка данных и фильтры

		private void LoadMeasurements()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var measurements = context.Measurements
						.Include(m => m.ProjectIdFkNavigation)
						.Include(m => m.SurveyLineIdFkNavigation)
							.ThenInclude(s => s.HydropostIdFkNavigation)
						.Include(m => m.EquipmentIdFkNavigation)
						.Include(m => m.PersonIdFkNavigation)
						.OrderByDescending(m => m.MeasuredAt)
						.ToList();

					MeasurementsDataGrid.ItemsSource = measurements;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки измерений: {ex.Message}", "Ошибка");
			}
		}

		private void LoadFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var projects = context.Projects.ToList();
					projects.Insert(0, new Project { ProjectId = 0, Name = "Все проекты" });
					ProjectFilterComboBox.ItemsSource = projects;
					ProjectFilterComboBox.DisplayMemberPath = "Name";
					ProjectFilterComboBox.SelectedValuePath = "ProjectId";
					ProjectFilterComboBox.SelectedIndex = 0;

					var hydroposts = context.Hydroposts.ToList();
					hydroposts.Insert(0, new Hydropost { HydropostId = 0, Name = "Все гидропосты" });
					HydropostFilterComboBox.ItemsSource = hydroposts;
					HydropostFilterComboBox.DisplayMemberPath = "Name";
					HydropostFilterComboBox.SelectedValuePath = "HydropostId";
					HydropostFilterComboBox.SelectedIndex = 0;

					var surveyLines = context.SurveyLines
						.Include(s => s.HydropostIdFkNavigation)
						.ToList();
					surveyLines.Insert(0, new SurveyLine { SurveyLineId = 0, Name = "Все участки" });
					SurveyLineFilterComboBox.ItemsSource = surveyLines;
					SurveyLineFilterComboBox.DisplayMemberPath = "Name";
					SurveyLineFilterComboBox.SelectedValuePath = "SurveyLineId";
					SurveyLineFilterComboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка загрузки фильтров: {ex.Message}", "Ошибка");
			}
		}

		private void ApplyFilters()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var query = context.Measurements
						.Include(m => m.ProjectIdFkNavigation)
						.Include(m => m.SurveyLineIdFkNavigation)
							.ThenInclude(s => s.HydropostIdFkNavigation)
						.Include(m => m.EquipmentIdFkNavigation)
						.Include(m => m.PersonIdFkNavigation)
						.AsQueryable();

					if (ProjectFilterComboBox.SelectedItem is Project selectedProject && selectedProject.ProjectId != 0)
						query = query.Where(m => m.ProjectIdFk == selectedProject.ProjectId);

					if (HydropostFilterComboBox.SelectedItem is Hydropost selectedHydropost && selectedHydropost.HydropostId != 0)
						query = query.Where(m => m.SurveyLineIdFkNavigation.HydropostIdFk == selectedHydropost.HydropostId);

					if (SurveyLineFilterComboBox.SelectedItem is SurveyLine selectedSurveyLine && selectedSurveyLine.SurveyLineId != 0)
						query = query.Where(m => m.SurveyLineIdFk == selectedSurveyLine.SurveyLineId);

					if (DateFilterPicker.SelectedDate.HasValue)
					{
						var selectedDate = DateFilterPicker.SelectedDate.Value.Date;
						var nextDay = selectedDate.AddDays(1);
						query = query.Where(m => m.MeasuredAt >= selectedDate && m.MeasuredAt < nextDay);
					}

					query = query.OrderByDescending(m => m.MeasuredAt);
					MeasurementsDataGrid.ItemsSource = query.ToList();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка применения фильтров: {ex.Message}", "Ошибка");
			}
		}

		private void UpdateSurveyLinesFilter()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					var selectedHydropost = HydropostFilterComboBox.SelectedItem as Hydropost;
					IQueryable<SurveyLine> surveyLinesQuery = context.SurveyLines
						.Include(s => s.HydropostIdFkNavigation);

					if (selectedHydropost != null && selectedHydropost.HydropostId != 0)
						surveyLinesQuery = surveyLinesQuery.Where(s => s.HydropostIdFk == selectedHydropost.HydropostId);

					var surveyLines = surveyLinesQuery.ToList();
					surveyLines.Insert(0, new SurveyLine { SurveyLineId = 0, Name = "Все участки" });

					SurveyLineFilterComboBox.ItemsSource = surveyLines;
					SurveyLineFilterComboBox.DisplayMemberPath = "Name";
					SurveyLineFilterComboBox.SelectedValuePath = "SurveyLineId";
					SurveyLineFilterComboBox.SelectedIndex = 0;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка обновления списка участков: {ex.Message}", "Ошибка");
			}
		}

		#endregion

		#region Обработчики событий фильтров

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			DrawSurveyScheme();
			DrawGraph();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);
			if (IsLoaded)
			{
				DrawSurveyScheme();
				DrawGraph();
			}
		}

		private void ProjectFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) { ApplyFilters(); DrawGraph(); }
		}

		private void HydropostFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded)
			{
				UpdateSurveyLinesFilter();
				DrawSurveyScheme();
				ApplyFilters();
				DrawGraph();
			}
		}

		private void SurveyLineFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) { ApplyFilters(); DrawGraph(); }
		}

		private void DateFilterPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{
			if (IsLoaded) { ApplyFilters(); DrawGraph(); }
		}

		private void Button_ResetFilters_Click(object sender, RoutedEventArgs e)
		{
			ProjectFilterComboBox.SelectedIndex = 0;
			HydropostFilterComboBox.SelectedIndex = 0;
			SurveyLineFilterComboBox.SelectedIndex = 0;
			DateFilterPicker.SelectedDate = null;
			LoadMeasurements();
			DrawSurveyScheme();
			DrawGraph();
		}

		#endregion

		#region Графическая визуализация

		private void GraphType_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button button)
			{
				_currentGraphType = button.Content.ToString();
				var parent = button.Parent as StackPanel;
				if (parent != null)
				{
					foreach (var child in parent.Children)
					{
						if (child is Button btn)
							btn.Style = btn == button
								? (Style)FindResource("ActiveButtonStyle")
								: (Style)FindResource("InactiveButtonStyle");
					}
				}
				DrawGraph();
			}
		}

		// ==================== КАРТА ====================

		private void DrawSurveyScheme()
		{
			try
			{
				MapCanvas.Children.Clear();

				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					Hydropost selectedHydropost = null;
					if (HydropostFilterComboBox.SelectedItem is Hydropost hp && hp.HydropostId != 0)
						selectedHydropost = hp;
					else
						selectedHydropost = context.Hydroposts.FirstOrDefault();

					if (selectedHydropost == null)
					{
						DrawNoDataOnMap("Нет данных о гидропостах");
						return;
					}

					var surveyLines = context.SurveyLines
						.Where(s => s.HydropostIdFk == selectedHydropost.HydropostId)
						.ToList();

					double canvasWidth = MapCanvas.ActualWidth > 0 ? MapCanvas.ActualWidth : 500;
					double canvasHeight = MapCanvas.ActualHeight > 0 ? MapCanvas.ActualHeight : 350;
					double margin = 40;

					//заголовок
					TextBlock titleText = new TextBlock
					{
						Text = $"Гидропост: {selectedHydropost.Name}",
						FontSize = 12,
						FontWeight = FontWeights.Bold,
						Foreground = Brushes.DarkBlue
					};
					Canvas.SetLeft(titleText, 10);
					Canvas.SetTop(titleText, 5);
					MapCanvas.Children.Add(titleText);

					//координаты
					TextBlock coordsText = new TextBlock
					{
						Text = $"Координаты: {selectedHydropost.Latitude:F4}°, {selectedHydropost.Longtude:F4}°",
						FontSize = 10,
						Foreground = Brushes.Gray
					};
					Canvas.SetLeft(coordsText, 10);
					Canvas.SetTop(coordsText, 20);
					MapCanvas.Children.Add(coordsText);

					if (!surveyLines.Any())
					{
						DrawNoDataOnMap("Нет участков съемки");
						return;
					}

					//река
					Line riverLine = new Line
					{
						X1 = canvasWidth / 2,
						Y1 = margin,
						X2 = canvasWidth / 2,
						Y2 = canvasHeight - margin,
						Stroke = Brushes.Blue,
						StrokeThickness = 3
					};
					MapCanvas.Children.Add(riverLine);

					//створы
					double stepY = (canvasHeight - 2 * margin) / (surveyLines.Count + 1);
					for (int i = 0; i < surveyLines.Count; i++)
					{
						double y = margin + stepY * (i + 1);
						Line surveyLine = new Line
						{
							X1 = canvasWidth / 2 - 60,
							Y1 = y,
							X2 = canvasWidth / 2 + 60,
							Y2 = y,
							Stroke = new SolidColorBrush(Colors.Red) { Opacity = 0.7 },
							StrokeThickness = 2,
							StrokeDashArray = new DoubleCollection { 4, 2 }
						};
						MapCanvas.Children.Add(surveyLine);

						TextBlock label = new TextBlock
						{
							Text = surveyLines[i].Name,
							FontSize = 10,
							Foreground = Brushes.DarkRed
						};
						Canvas.SetLeft(label, canvasWidth / 2 + 65);
						Canvas.SetTop(label, y - 8);
						MapCanvas.Children.Add(label);
					}

					//легенда
					StackPanel legend = new StackPanel { Orientation = Orientation.Vertical };
					Canvas.SetRight(legend, 10);
					Canvas.SetBottom(legend, 10);
					legend.Children.Add(CreateLegendItem(Brushes.Blue, "Река"));
					legend.Children.Add(CreateLegendItem(new SolidColorBrush(Colors.Red) { Opacity = 0.7 }, "Створ"));
					MapCanvas.Children.Add(legend);
				}
			}
			catch (Exception ex)
			{
				DrawNoDataOnMap($"Ошибка: {ex.Message}");
			}
		}

		private void DrawNoDataOnMap(string message)
		{
			MapCanvas.Children.Clear();
			TextBlock msg = new TextBlock
			{
				Text = message,
				FontSize = 14,
				Foreground = Brushes.Gray
			};
			Canvas.SetLeft(msg, MapCanvas.ActualWidth / 2 - 80);
			Canvas.SetTop(msg, MapCanvas.ActualHeight / 2);
			MapCanvas.Children.Add(msg);
		}

		private StackPanel CreateLegendItem(Brush color, string text)
		{
			StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
			Rectangle rect = new Rectangle { Width = 20, Height = 3, Fill = color, Margin = new Thickness(0, 0, 5, 0) };
			TextBlock label = new TextBlock { Text = text, FontSize = 10 };
			panel.Children.Add(rect);
			panel.Children.Add(label);
			return panel;
		}

		// ==================== ГРАФИКИ ====================

		private void DrawGraph()
		{
			GraphCanvas.Children.Clear();

			using (var context = new DatabaseOfHydrologicalMeasurementsContext())
			{
				var measurements = GetFilteredMeasurements(context);

				if (!measurements.Any())
				{
					DrawNoDataMessage();
					return;
				}

				double canvasWidth = GraphCanvas.ActualWidth > 0 ? GraphCanvas.ActualWidth : 500;
				double canvasHeight = GraphCanvas.ActualHeight > 0 ? GraphCanvas.ActualHeight : 450;
				double marginLeft = 60, marginRight = 30, marginTop = 30, marginBottom = 40;
				double plotWidth = canvasWidth - marginLeft - marginRight;
				double plotHeight = canvasHeight - marginTop - marginBottom;

				DrawAxes(marginLeft, marginTop, plotWidth, plotHeight);

				switch (_currentGraphType)
				{
					case "Гидрограф":
						DrawHydrograph(measurements, marginLeft, marginTop, plotWidth, plotHeight);
						break;
					case "Уровень-расход":
						DrawLevelFlowRelation(measurements, marginLeft, marginTop, plotWidth, plotHeight);
						break;
					case "Продольный профиль":
						DrawLongitudinalProfile(context, marginLeft, marginTop, plotWidth, plotHeight);
						break;
				}
			}
		}

		private List<Measurement> GetFilteredMeasurements(DatabaseOfHydrologicalMeasurementsContext context)
		{
			var query = context.Measurements
				.Include(m => m.SurveyLineIdFkNavigation)
				.OrderBy(m => m.MeasuredAt)
				.AsQueryable();

			if (ProjectFilterComboBox.SelectedItem is Project proj && proj.ProjectId != 0)
				query = query.Where(m => m.ProjectIdFk == proj.ProjectId);
			if (HydropostFilterComboBox.SelectedItem is Hydropost hp && hp.HydropostId != 0)
				query = query.Where(m => m.SurveyLineIdFkNavigation.HydropostIdFk == hp.HydropostId);
			if (SurveyLineFilterComboBox.SelectedItem is SurveyLine sl && sl.SurveyLineId != 0)
				query = query.Where(m => m.SurveyLineIdFk == sl.SurveyLineId);
			if (DateFilterPicker.SelectedDate.HasValue)
			{
				var date = DateFilterPicker.SelectedDate.Value.Date;
				var nextDay = date.AddDays(1);
				query = query.Where(m => m.MeasuredAt >= date && m.MeasuredAt < nextDay);
			}

			return query.ToList();
		}

		private void DrawAxes(double marginLeft, double marginTop, double plotWidth, double plotHeight)
		{
			Line yAxis = new Line
			{
				X1 = marginLeft,
				Y1 = marginTop,
				X2 = marginLeft,
				Y2 = marginTop + plotHeight,
				Stroke = Brushes.Black,
				StrokeThickness = 1
			};
			GraphCanvas.Children.Add(yAxis);

			Line xAxis = new Line
			{
				X1 = marginLeft,
				Y1 = marginTop + plotHeight,
				X2 = marginLeft + plotWidth,
				Y2 = marginTop + plotHeight,
				Stroke = Brushes.Black,
				StrokeThickness = 1
			};
			GraphCanvas.Children.Add(xAxis);
		}

		private void DrawHydrograph(List<Measurement> measurements, double marginLeft, double marginTop, double plotWidth, double plotHeight)
		{
			var validData = measurements.Where(m => m.WaterLevel.HasValue).ToList();
			if (!validData.Any()) { DrawNoDataMessage(); return; }

			double minLevel = (double)validData.Min(m => m.WaterLevel.Value);
			double maxLevel = (double)validData.Max(m => m.WaterLevel.Value);
			double range = maxLevel - minLevel;
			if (range == 0) range = 1;

			//заливка
			Polygon areaFill = new Polygon
			{
				Fill = new SolidColorBrush(Color.FromArgb(40, 30, 144, 255)),
				Stroke = null
			};
			areaFill.Points.Add(new Point(marginLeft, marginTop + plotHeight));

			Polyline polyline = new Polyline
			{
				Stroke = Brushes.DodgerBlue,
				StrokeThickness = 2
			};

			for (int i = 0; i < validData.Count; i++)
			{
				double x = marginLeft + (i / (double)(validData.Count - 1)) * plotWidth;
				double y = marginTop + plotHeight - (((double)validData[i].WaterLevel.Value - minLevel) / range * plotHeight);
				polyline.Points.Add(new Point(x, y));
				areaFill.Points.Add(new Point(x, y));
			}

			areaFill.Points.Add(new Point(marginLeft + plotWidth, marginTop + plotHeight));

			GraphCanvas.Children.Add(areaFill);
			GraphCanvas.Children.Add(polyline);
			DrawGraphTitle("Гидрограф (уровень воды по времени)");
			DrawAxisLabels(minLevel, maxLevel, marginLeft, marginTop, plotHeight, "м");
		}

		private void DrawLevelFlowRelation(List<Measurement> measurements, double marginLeft, double marginTop, double plotWidth, double plotHeight)
		{
			var validData = measurements.Where(m => m.WaterLevel.HasValue && m.WaterConsumption.HasValue).ToList();
			if (!validData.Any()) { DrawNoDataMessage(); return; }

			double minLevel = (double)validData.Min(m => m.WaterLevel.Value);
			double maxLevel = (double)validData.Max(m => m.WaterLevel.Value);
			double minFlow = (double)validData.Min(m => m.WaterConsumption.Value);
			double maxFlow = (double)validData.Max(m => m.WaterConsumption.Value);

			double levelRange = maxLevel - minLevel;
			double flowRange = maxFlow - minFlow;
			if (levelRange == 0) levelRange = 1;
			if (flowRange == 0) flowRange = 1;

			foreach (var m in validData)
			{
				double x = marginLeft + (((double)m.WaterLevel.Value - minLevel) / levelRange * plotWidth);
				double y = marginTop + plotHeight - (((double)m.WaterConsumption.Value - minFlow) / flowRange * plotHeight);

				Ellipse point = new Ellipse
				{
					Width = 6,
					Height = 6,
					Fill = Brushes.Green,
					Stroke = Brushes.DarkGreen
				};
				Canvas.SetLeft(point, x - 3);
				Canvas.SetTop(point, y - 3);
				GraphCanvas.Children.Add(point);
			}

			DrawGraphTitle("Кривая расходов Q = f(H)");
			DrawAxisLabels(minLevel, maxLevel, marginLeft, marginTop, plotHeight, "м");
		}

		private void DrawLongitudinalProfile(DatabaseOfHydrologicalMeasurementsContext context,
											  double marginLeft, double marginTop, double plotWidth, double plotHeight)
		{
			var surveyLinesQuery = context.SurveyLines
				.Where(sl => sl.DistanceFromBase.HasValue)
				.OrderBy(sl => sl.DistanceFromBase)
				.AsQueryable();

			if (HydropostFilterComboBox.SelectedItem is Hydropost hp && hp.HydropostId != 0)
				surveyLinesQuery = surveyLinesQuery.Where(sl => sl.HydropostIdFk == hp.HydropostId);

			var surveyLines = surveyLinesQuery.ToList();
			if (!surveyLines.Any() || surveyLines.Count < 2) { DrawNoDataMessage(); return; }

			var profileData = new List<(double distance, double waterLevel, double bedLevel, string name)>();
			foreach (var sl in surveyLines)
			{
				var avgLevel = context.Measurements
					.Where(m => m.SurveyLineIdFk == sl.SurveyLineId && m.WaterLevel.HasValue)
					.Select(m => m.WaterLevel.Value).ToList();

				double distance = (double)sl.DistanceFromBase.Value;
				double waterLevel = avgLevel.Any() ? (double)avgLevel.Average() : 50;
				double bedLevel = waterLevel - (2 + new Random(sl.SurveyLineId).NextDouble() * 8);
				profileData.Add((distance, waterLevel, bedLevel, sl.Name));
			}

			double minDist = profileData.Min(d => d.distance);
			double maxDist = profileData.Max(d => d.distance);
			double distRange = maxDist - minDist;
			if (distRange == 0) distRange = 1;

			double minLevel = profileData.Min(d => d.bedLevel);
			double maxLevel = profileData.Max(d => d.waterLevel);
			double levelRange = maxLevel - minLevel;
			if (levelRange == 0) levelRange = 1;

			Polygon waterArea = new Polygon
			{
				Fill = new SolidColorBrush(Color.FromArgb(60, 30, 144, 255)),
				Stroke = null
			};
			Polyline waterProfile = new Polyline { Stroke = Brushes.DodgerBlue, StrokeThickness = 2 };
			Polyline bedProfile = new Polyline { Stroke = Brushes.Brown, StrokeThickness = 2 };

			foreach (var data in profileData)
			{
				double x = marginLeft + ((data.distance - minDist) / distRange * plotWidth);
				double waterY = marginTop + plotHeight - ((data.waterLevel - minLevel) / levelRange * plotHeight * 0.8);
				double bedY = marginTop + plotHeight - ((data.bedLevel - minLevel) / levelRange * plotHeight * 0.8);

				waterProfile.Points.Add(new Point(x, waterY));
				bedProfile.Points.Add(new Point(x, bedY));
				waterArea.Points.Add(new Point(x, waterY));
			}

			for (int i = profileData.Count - 1; i >= 0; i--)
			{
				double x = marginLeft + ((profileData[i].distance - minDist) / distRange * plotWidth);
				double bedY = marginTop + plotHeight - ((profileData[i].bedLevel - minLevel) / levelRange * plotHeight * 0.8);
				waterArea.Points.Add(new Point(x, bedY));
			}

			GraphCanvas.Children.Add(waterArea);
			GraphCanvas.Children.Add(bedProfile);
			GraphCanvas.Children.Add(waterProfile);

			DrawGraphTitle("Продольный профиль участка");
			DrawGraphInfo($"Длина: {maxDist - minDist:F0} м | Перепад: {maxLevel - minLevel:F2} м");
		}

		private void DrawGraphTitle(string title)
		{
			TextBlock titleText = new TextBlock
			{
				Text = title,
				FontSize = 11,
				FontWeight = FontWeights.Bold,
				Foreground = Brushes.DarkBlue
			};
			Canvas.SetLeft(titleText, 10);
			Canvas.SetTop(titleText, 5);
			GraphCanvas.Children.Add(titleText);
		}

		private void DrawAxisLabels(double minVal, double maxVal, double marginLeft, double marginTop, double plotHeight, string unit)
		{
			TextBlock maxLabel = new TextBlock
			{
				Text = $"{maxVal:F1} {unit}",
				FontSize = 9,
				Foreground = Brushes.Gray
			};
			Canvas.SetLeft(maxLabel, marginLeft - 45);
			Canvas.SetTop(maxLabel, marginTop - 5);
			GraphCanvas.Children.Add(maxLabel);

			TextBlock minLabel = new TextBlock
			{
				Text = $"{minVal:F1} {unit}",
				FontSize = 9,
				Foreground = Brushes.Gray
			};
			Canvas.SetLeft(minLabel, marginLeft - 45);
			Canvas.SetTop(minLabel, marginTop + plotHeight - 5);
			GraphCanvas.Children.Add(minLabel);
		}

		private void DrawGraphInfo(string info)
		{
			TextBlock infoText = new TextBlock
			{
				Text = info,
				FontSize = 9,
				Foreground = Brushes.DarkGray
			};
			Canvas.SetLeft(infoText, 10);
			Canvas.SetTop(infoText, GraphCanvas.ActualHeight - 18);
			GraphCanvas.Children.Add(infoText);
		}

		private void DrawNoDataMessage()
		{
			TextBlock msg = new TextBlock
			{
				Text = "Нет данных для построения графика",
				FontSize = 14,
				Foreground = Brushes.Gray
			};
			Canvas.SetLeft(msg, GraphCanvas.ActualWidth / 2 - 100);
			Canvas.SetTop(msg, GraphCanvas.ActualHeight / 2);
			GraphCanvas.Children.Add(msg);
		}

		#endregion

		#region Координаты и мышь

		private void MapCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var pos = e.GetPosition(MapCanvas);
			UpdateCoordinateFields(pos.X, pos.Y);
		}

		private void GraphCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var pos = e.GetPosition(GraphCanvas);
			UpdateCoordinateFields(pos.X, pos.Y);
		}

		private void UpdateCoordinateFields(double x, double y)
		{
			if (CoordXTextBox != null) CoordXTextBox.Text = $"{x:F1}";
			if (CoordYTextBox != null) CoordYTextBox.Text = $"{y:F1}";
		}

		#endregion

		#region Кнопки управления

		private void Button_AddSyntheticData_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				var generator = new DataGeneratorService();
				generator.GenerateAllData();
				LoadFilters();
				LoadMeasurements();
				DrawSurveyScheme();
				DrawGraph();
				Mouse.OverrideCursor = null;
				MessageBox.Show("Данные успешно сгенерированы!", "Успех",
								MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				Mouse.OverrideCursor = null;
				MessageBox.Show($"Ошибка при генерации: {ex.Message}", "Ошибка",
								MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void Button_ClearDatabase_Click(object sender, RoutedEventArgs e)
		{
			var result = MessageBox.Show("Удалить ВСЕ данные из базы данных?\n\nЭто действие нельзя отменить!",
										  "Подтверждение очистки", MessageBoxButton.YesNo, MessageBoxImage.Warning);
			if (result == MessageBoxResult.Yes)
			{
				try
				{
					Mouse.OverrideCursor = Cursors.Wait;
					var generator = new DataGeneratorService();
					generator.ClearAllTables();
					LoadFilters();
					LoadMeasurements();
					MapCanvas.Children.Clear();
					GraphCanvas.Children.Clear();
					Mouse.OverrideCursor = null;
					MessageBox.Show("База данных успешно очищена!", "Очистка БД",
									MessageBoxButton.OK, MessageBoxImage.Information);
				}
				catch (Exception ex)
				{
					Mouse.OverrideCursor = null;
					MessageBox.Show($"Ошибка при очистке: {ex.Message}", "Ошибка",
									MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		}

		private void BtnAddMeasurement_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					//проверяем наличие необходимых данных
					if (!context.Projects.Any())
					{
						MessageBox.Show("Нет проектов для добавления измерения. Сначала создайте проект или сгенерируйте данные.",
										"Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
					if (!context.SurveyLines.Any())
					{
						MessageBox.Show("Нет участков съемки для добавления измерения. Сначала создайте участки или сгенерируйте данные.",
										"Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
					if (!context.Equipments.Any())
					{
						MessageBox.Show("Нет оборудования для добавления измерения. Сначала добавьте оборудование или сгенерируйте данные.",
										"Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}
					if (!context.Persons.Any(p => p.RoleIdFkNavigation.Name == "Гидролог"))
					{
						MessageBox.Show("Нет гидрологов для выполнения измерения. Сначала добавьте гидролога или сгенерируйте данные.",
										"Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
						return;
					}

					var dialog = new Window
					{
						Title = "Добавление измерения",
						Width = 600,
						Height = 700,
						WindowStartupLocation = WindowStartupLocation.CenterOwner,
						Owner = this,
						ResizeMode = ResizeMode.NoResize
					};

					var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
					var grid = new Grid();
					for (int i = 0; i < 13; i++)
						grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });
					grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
					grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
					grid.Margin = new Thickness(10);

					int row = 0;

					//проект
					var lblProject = new TextBlock { Text = "Проект:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblProject, row); Grid.SetColumn(lblProject, 0);
					var cmbProject = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var projects = context.Projects.ToList();
					cmbProject.ItemsSource = projects;
					cmbProject.DisplayMemberPath = "Name";
					if (projects.Any()) cmbProject.SelectedIndex = 0;
					Grid.SetRow(cmbProject, row); Grid.SetColumn(cmbProject, 1);
					row++;

					//гидропост
					var lblHydropost = new TextBlock { Text = "Гидропост:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblHydropost, row); Grid.SetColumn(lblHydropost, 0);
					var cmbHydropost = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var allHydroposts = context.Hydroposts.ToList();
					cmbHydropost.ItemsSource = allHydroposts;
					cmbHydropost.DisplayMemberPath = "Name";
					if (allHydroposts.Any()) cmbHydropost.SelectedIndex = 0;
					Grid.SetRow(cmbHydropost, row); Grid.SetColumn(cmbHydropost, 1);
					row++;

					//участок съемки
					var lblSurveyLine = new TextBlock { Text = "Участок съемки:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblSurveyLine, row); Grid.SetColumn(lblSurveyLine, 0);
					var cmbSurveyLine = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var allSurveyLines = context.SurveyLines.Include(s => s.HydropostIdFkNavigation).ToList();
					cmbSurveyLine.ItemsSource = allSurveyLines;
					cmbSurveyLine.DisplayMemberPath = "Name";
					if (allSurveyLines.Any()) cmbSurveyLine.SelectedIndex = 0;
					Grid.SetRow(cmbSurveyLine, row); Grid.SetColumn(cmbSurveyLine, 1);
					row++;

					//обновление списка участков при смене гидропоста
					cmbHydropost.SelectionChanged += (s, args) =>
					{
						if (cmbHydropost.SelectedItem is Hydropost selectedHp)
						{
							var filteredLines = context.SurveyLines
								.Include(sl => sl.HydropostIdFkNavigation)
								.Where(sl => sl.HydropostIdFk == selectedHp.HydropostId)
								.ToList();
							cmbSurveyLine.ItemsSource = filteredLines;
							if (filteredLines.Any()) cmbSurveyLine.SelectedIndex = 0;
						}
					};

					//оборудование
					var lblEquipment = new TextBlock { Text = "Оборудование:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblEquipment, row); Grid.SetColumn(lblEquipment, 0);
					var cmbEquipment = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var equipments = context.Equipments
						.Include(eq => eq.EquipmentTypeIdFkNavigation)
						.Where(eq => eq.EquipmentStatusIdFkNavigation.Name == "Исправен")
						.ToList();
					if (!equipments.Any()) equipments = context.Equipments.Include(eq => eq.EquipmentTypeIdFkNavigation).ToList();
					cmbEquipment.ItemsSource = equipments;
					cmbEquipment.DisplayMemberPath = "Name";
					if (equipments.Any()) cmbEquipment.SelectedIndex = 0;
					Grid.SetRow(cmbEquipment, row); Grid.SetColumn(cmbEquipment, 1);
					row++;

					//гидролог
					var lblPerson = new TextBlock { Text = "Гидролог:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblPerson, row); Grid.SetColumn(lblPerson, 0);
					var cmbPerson = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var persons = context.Persons.Where(p => p.RoleIdFkNavigation.Name == "Гидролог").ToList();
					cmbPerson.ItemsSource = persons;
					cmbPerson.DisplayMemberPath = "LastName";
					if (persons.Any()) cmbPerson.SelectedIndex = 0;
					Grid.SetRow(cmbPerson, row); Grid.SetColumn(cmbPerson, 1);
					row++;

					//дата и время
					var lblDateTime = new TextBlock { Text = "Дата и время:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblDateTime, row); Grid.SetColumn(lblDateTime, 0);
					var dpDateTime = new DatePicker { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					dpDateTime.SelectedDate = DateTime.Now;
					Grid.SetRow(dpDateTime, row); Grid.SetColumn(dpDateTime, 1);
					row++;

					//время
					var lblTime = new TextBlock { Text = "Время:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblTime, row); Grid.SetColumn(lblTime, 0);
					var timePanel = new StackPanel { Orientation = Orientation.Horizontal };
					var txtHour = new TextBox { Width = 40, FontSize = 14, Text = DateTime.Now.Hour.ToString("D2"), TextAlignment = TextAlignment.Center };
					var txtMinute = new TextBox { Width = 40, FontSize = 14, Text = DateTime.Now.Minute.ToString("D2"), TextAlignment = TextAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
					var lblTimeSep = new TextBlock { Text = ":", FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(2, 0, 2, 0) };
					timePanel.Children.Add(txtHour);
					timePanel.Children.Add(lblTimeSep);
					timePanel.Children.Add(txtMinute);
					Grid.SetRow(timePanel, row); Grid.SetColumn(timePanel, 1);
					row++;

					//уровень воды
					var lblWaterLevel = new TextBlock { Text = "Уровень воды (м):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblWaterLevel, row); Grid.SetColumn(lblWaterLevel, 0);
					var txtWaterLevel = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "100,0" };
					Grid.SetRow(txtWaterLevel, row); Grid.SetColumn(txtWaterLevel, 1);
					row++;

					//расход воды
					var lblConsumption = new TextBlock { Text = "Расход воды (м³/с):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblConsumption, row); Grid.SetColumn(lblConsumption, 0);
					var txtConsumption = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "50,0" };
					Grid.SetRow(txtConsumption, row); Grid.SetColumn(txtConsumption, 1);
					row++;

					//температура
					var lblTemperature = new TextBlock { Text = "Температура (°C):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblTemperature, row); Grid.SetColumn(lblTemperature, 0);
					var txtTemperature = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "15,0" };
					Grid.SetRow(txtTemperature, row); Grid.SetColumn(txtTemperature, 1);
					row++;

					//прозрачность
					var lblTransparency = new TextBlock { Text = "Прозрачность (м):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblTransparency, row); Grid.SetColumn(lblTransparency, 0);
					var txtTransparency = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "1,5" };
					Grid.SetRow(txtTransparency, row); Grid.SetColumn(txtTransparency, 1);
					row++;

					//ледовые явления
					var lblIce = new TextBlock { Text = "Ледовые явления:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblIce, row); Grid.SetColumn(lblIce, 0);
					var cmbIce = new ComboBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					var icePhenomena = new List<string> { "Нет", "Шуга", "Ледяной покров", "Забереги", "Полынья", "Ледовый затор", "Ледостав", "Вскрытие реки", "Ледяное сало" };
					cmbIce.ItemsSource = icePhenomena;
					cmbIce.SelectedIndex = 0;
					Grid.SetRow(cmbIce, row); Grid.SetColumn(cmbIce, 1);
					row++;

					//площадь
					var lblArea = new TextBlock { Text = "Площадь (м²):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblArea, row); Grid.SetColumn(lblArea, 0);
					var txtArea = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "100,0" };
					Grid.SetRow(txtArea, row); Grid.SetColumn(txtArea, 1);
					row++;

					//ширина
					var lblWidth = new TextBlock { Text = "Ширина (м):", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblWidth, row); Grid.SetColumn(lblWidth, 0);
					var txtWidth = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center, Text = "20,0" };
					Grid.SetRow(txtWidth, row); Grid.SetColumn(txtWidth, 1);
					row++;

					//комментарий
					var lblComment = new TextBlock { Text = "Комментарий:", FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(lblComment, row); Grid.SetColumn(lblComment, 0);
					var txtComment = new TextBox { FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
					Grid.SetRow(txtComment, row); Grid.SetColumn(txtComment, 1);
					row++;

					//кнопки
					var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
					Grid.SetRow(btnPanel, row); Grid.SetColumn(btnPanel, 1);
					var btnSave = new Button { Content = "Сохранить", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(44, 125, 160)), Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) };
					var btnCancel = new Button { Content = "Отмена", Width = 100, Height = 30, Background = Brushes.LightGray };
					btnPanel.Children.Add(btnSave);
					btnPanel.Children.Add(btnCancel);

					btnCancel.Click += (s, args) => dialog.Close();

					btnSave.Click += (s, args) =>
					{
						//валидация
						if (cmbProject.SelectedItem == null)
						{
							MessageBox.Show("Выберите проект!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbSurveyLine.SelectedItem == null)
						{
							MessageBox.Show("Выберите участок съемки!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbEquipment.SelectedItem == null)
						{
							MessageBox.Show("Выберите оборудование!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (cmbPerson.SelectedItem == null)
						{
							MessageBox.Show("Выберите гидролога!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!dpDateTime.SelectedDate.HasValue)
						{
							MessageBox.Show("Выберите дату измерения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						//парсинг времени
						if (!int.TryParse(txtHour.Text, out int hour) || hour < 0 || hour > 23)
						{
							MessageBox.Show("Введите корректный час (0-23)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}
						if (!int.TryParse(txtMinute.Text, out int minute) || minute < 0 || minute > 59)
						{
							MessageBox.Show("Введите корректные минуты (0-59)!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
							return;
						}

						var measuredAt = dpDateTime.SelectedDate.Value.Date.AddHours(hour).AddMinutes(minute);

						var newMeasurement = new Measurement
						{
							SurveyLineIdFk = ((SurveyLine)cmbSurveyLine.SelectedItem).SurveyLineId,
							ProjectIdFk = ((Project)cmbProject.SelectedItem).ProjectId,
							EquipmentIdFk = ((Equipment)cmbEquipment.SelectedItem).EquipmentId,
							PersonIdFk = ((Person)cmbPerson.SelectedItem).PersonId,
							MeasuredAt = measuredAt,
							IcePhenomena = cmbIce.SelectedItem?.ToString() ?? "Нет",
							Comment = string.IsNullOrWhiteSpace(txtComment.Text) ? null : txtComment.Text.Trim()
						};

						//парсинг числовых значений (необязательные поля)
						if (decimal.TryParse(txtWaterLevel.Text, out decimal waterLevel))
							newMeasurement.WaterLevel = waterLevel;
						if (decimal.TryParse(txtConsumption.Text, out decimal consumption))
							newMeasurement.WaterConsumption = consumption;
						if (decimal.TryParse(txtTemperature.Text, out decimal temperature))
							newMeasurement.WaterTemperature = temperature;
						if (decimal.TryParse(txtTransparency.Text, out decimal transparency))
							newMeasurement.WaterTransparency = transparency;
						if (decimal.TryParse(txtArea.Text, out decimal area))
							newMeasurement.Area = area;
						if (decimal.TryParse(txtWidth.Text, out decimal width))
							newMeasurement.Width = width;

						using (var saveContext = new DatabaseOfHydrologicalMeasurementsContext())
						{
							saveContext.Measurements.Add(newMeasurement);
							saveContext.SaveChanges();
						}

						MessageBox.Show("Измерение успешно добавлено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
						dialog.Close();
						LoadMeasurements();
						ApplyFilters();
						DrawGraph();
					};

					//добавляем элементы на форму
					grid.Children.Add(lblProject);
					grid.Children.Add(cmbProject);
					grid.Children.Add(lblHydropost);
					grid.Children.Add(cmbHydropost);
					grid.Children.Add(lblSurveyLine);
					grid.Children.Add(cmbSurveyLine);
					grid.Children.Add(lblEquipment);
					grid.Children.Add(cmbEquipment);
					grid.Children.Add(lblPerson);
					grid.Children.Add(cmbPerson);
					grid.Children.Add(lblDateTime);
					grid.Children.Add(dpDateTime);
					grid.Children.Add(lblTime);
					grid.Children.Add(timePanel);
					grid.Children.Add(lblWaterLevel);
					grid.Children.Add(txtWaterLevel);
					grid.Children.Add(lblConsumption);
					grid.Children.Add(txtConsumption);
					grid.Children.Add(lblTemperature);
					grid.Children.Add(txtTemperature);
					grid.Children.Add(lblTransparency);
					grid.Children.Add(txtTransparency);
					grid.Children.Add(lblIce);
					grid.Children.Add(cmbIce);
					grid.Children.Add(lblArea);
					grid.Children.Add(txtArea);
					grid.Children.Add(lblWidth);
					grid.Children.Add(txtWidth);
					grid.Children.Add(lblComment);
					grid.Children.Add(txtComment);
					grid.Children.Add(btnPanel);

					scrollViewer.Content = grid;
					dialog.Content = scrollViewer;
					dialog.ShowDialog();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при добавлении измерения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		#endregion

		#region Навигация

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
			if (CurrentUser.IsAdmin)
			{
				new DirectoriesWindow().Show();
				this.Close();
			}
			else
			{
				MessageBox.Show("Доступ запрещен! Только для администратора.",
								"Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void Button_Admin_Click(object sender, RoutedEventArgs e)
		{
			if (CurrentUser.IsAdmin)
			{
				new AdminWindow().Show();
				this.Close();
			}
			else
			{
				MessageBox.Show("Доступ запрещен! Только для администратора.",
								"Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		#endregion
	}
}