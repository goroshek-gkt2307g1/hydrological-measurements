using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Services
{
	public class DataGeneratorService
	{
		private readonly DatabaseOfHydrologicalMeasurementsContext _context;
		private readonly Random _random;

		private List<string> _фамилии;
		private List<string> _имена;
		private List<string> _отчества;
		private List<string> _названияГидропостов;
		private List<string> _описанияГидропостов;
		private List<string> _названияОборудования;
		private List<string> _заказчики;
		private List<string> _целиПроектов;
		private List<string> _ледовыеЯвления;
		private List<string> _комментарии;
		private List<string> _названияУчастков;

		public DataGeneratorService()
		{
			_context = new DatabaseOfHydrologicalMeasurementsContext();
			_random = new Random();
			InitializeLists();
		}

		//добавить метод для создания администратора по умолчанию
		/// <summary>
		/// Создает администратора по умолчанию если его еще нет в базе
		/// </summary>
		public static void EnsureDefaultAdminExists()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					//проверяем существование ролей
					if (!context.PersonRoles.Any())
					{
						context.PersonRoles.AddRange(new List<PersonRole>
						{
							new PersonRole { Name = "Администратор" },
							new PersonRole { Name = "Менеджер" },
							new PersonRole { Name = "Гидролог" }
						});
						context.SaveChanges();
					}

					//проверяем существование администратора
					var adminRole = context.PersonRoles.FirstOrDefault(r => r.Name == "Администратор");
					if (adminRole != null && !context.Persons.Any(p => p.RoleIdFk == adminRole.RoleId))
					{
						var admin = new Person
						{
							LastName = "Администратор",
							FirstName = "Системы",
							MiddleName = "Главный",
							Login = "admin",
							Password = "admin",
							RoleIdFk = adminRole.RoleId,
							CreatedAt = DateTime.Now
						};
						context.Persons.Add(admin);
						context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка создания администратора по умолчанию: {ex.Message}",
								"Ошибка инициализации", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void InitializeLists()
		{
			_фамилии = new List<string> { "Иванов", "Петров", "Сидоров", "Смирнов", "Кузнецов", "Васильев", "Попов", "Соколов", "Михайлов", "Новиков", "Федоров", "Морозов" };
			_имена = new List<string> { "Александр", "Дмитрий", "Максим", "Сергей", "Андрей", "Алексей", "Мария", "Елена", "Анна", "Татьяна", "Ольга", "Наталья" };
			_отчества = new List<string> { "Александрович", "Дмитриевич", "Максимович", "Сергеевич", "Андреевич", "Алексеевич", "Александровна", "Дмитриевна", "Максимовна", "Сергеевна" };
			_названияГидропостов = new List<string>
			{
				"р. Волга у г. Твери", "р. Ока у г. Калуги", "р. Дон у г. Воронежа",
				"р. Кама у г. Перми", "р. Обь у г. Новосибирска", "р. Енисей у г. Красноярска",
				"оз. Байкал, п. Листвянка", "р. Нева у г. Санкт-Петербурга", "р. Лена у г. Якутска",
				"р. Амур у г. Хабаровска", "р. Урал у г. Оренбурга", "р. Волхов у г. Великий Новгород"
			};
			_описанияГидропостов = new List<string>
			{
				"Расположен в нижнем бьефе", "Горный участок реки", "Равнинный участок",
				"Вблизи населенного пункта", "В труднодоступной местности", "Автоматизированный пост"
			};
			_названияОборудования = new List<string>
			{
				"Уровнемер", "Расходомер", "Термометр", "Мутномер", "Ледомер", "Батиметр",
				"Гидрологическая вертушка", "Эхолот", "Профилограф", "Солемер"
			};
			_заказчики = new List<string>
			{
				"Росгидромет", "Минприроды РФ", "Администрация области", "НИИ водного хозяйства",
				"Экологический фонд", "Водоканал", "МЧС России", "Росводресурсы"
			};
			_целиПроектов = new List<string>
			{
				"Мониторинг уровня воды", "Оценка водных ресурсов", "Прогнозирование паводков",
				"Изучение гидрологического режима", "Оценка антропогенного воздействия",
				"Разработка водохозяйственного баланса", "Оценка рисков наводнений"
			};
			_ледовыеЯвления = new List<string>
			{
				"Нет", "Шуга", "Ледяной покров", "Забереги", "Полынья", "Ледовый затор",
				"Ледостав", "Вскрытие реки", "Ледяное сало"
			};
			_комментарии = new List<string>
			{
				"Измерения проводились в хороших погодных условиях", "Отмечен повышенный уровень",
				"После прошедших дождей", "В период весеннего половодья", "В межень",
				"Ветреная погода", "Туман", "Осадки в виде дождя"
			};
			_названияУчастков = new List<string>
			{
				"Основной створ", "Дополнительный створ", "Верхний створ", "Нижний створ",
				"Центральный участок", "Прибрежный участок", "Створ у плотины", "Контрольный створ"
			};
		}

		//генерация всех данных
		public void GenerateAllData(int personsCount = 30, int hydropostsCount = 15,
					int equipmentCount = 10, int projectsCount = 10,
					int measurementsPerProject = 30)
		{
			try
			{
				using (var transaction = _context.Database.BeginTransaction())
				{
					//справочники
					EnsureReferenceData();
					//пользователи
					GeneratePersons(personsCount);
					//гидропосты
					GenerateHydroposts(hydropostsCount);
					//оборудование
					GenerateEquipment(equipmentCount);
					//проекты
					GenerateProjects(projectsCount);
					//участки съемки
					GenerateSurveyLines();
					//измерения
					GenerateMeasurements(measurementsPerProject);

					transaction.Commit();
					MessageBox.Show($"Данные успешно сгенерированы!\n\nПользователей: {personsCount}\nГидропостов: {hydropostsCount}\nОборудования: {equipmentCount}\nПроектов: {projectsCount}\nИзмерений: ~{measurementsPerProject * projectsCount}", "Успех");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при генерации:\n{GetFullExceptionMessage(ex)}", "Ошибка");
			}
		}

		//обеспечение наличия справочных данных
		private void EnsureReferenceData()
		{
			//роли пользователей
			if (!_context.PersonRoles.Any())
			{
				_context.PersonRoles.AddRange(new List<PersonRole>
				{
					new PersonRole { Name = "Администратор" },
					new PersonRole { Name = "Менеджер" },
					new PersonRole { Name = "Гидролог" }
				});
				_context.SaveChanges();
			}
			//типы гидропостов
			if (!_context.HydropostTypes.Any())
			{
				_context.HydropostTypes.AddRange(new List<HydropostType>
				{
					new HydropostType { Name = "Речной" },
					new HydropostType { Name = "Озерный" },
					new HydropostType { Name = "Водохранилищный" },
					new HydropostType { Name = "Морской" }
				});
				_context.SaveChanges();
			}
			//типы оборудования
			if (!_context.EquipmentTypes.Any())
			{
				_context.EquipmentTypes.AddRange(new List<EquipmentType>
				{
					new EquipmentType { Name = "Уровнемер" },
					new EquipmentType { Name = "Расходомер" },
					new EquipmentType { Name = "Термометр" },
					new EquipmentType { Name = "Мутномер" },
					new EquipmentType { Name = "Ледомер" }
				});
				_context.SaveChanges();
			}
			//статусы оборудования
			if (!_context.EquipmentStatuses.Any())
			{
				_context.EquipmentStatuses.AddRange(new List<EquipmentStatus>
				{
					new EquipmentStatus { Name = "Исправен" },
					new EquipmentStatus { Name = "На калибровке" },
					new EquipmentStatus { Name = "В ремонте" },
					new EquipmentStatus { Name = "Списано" }
				});
				_context.SaveChanges();
			}
			//статусы проектов
			if (!_context.ProjectStatuses.Any())
			{
				_context.ProjectStatuses.AddRange(new List<ProjectStatus>
				{
					new ProjectStatus { Name = "Планируется" },
					new ProjectStatus { Name = "Активный" },
					new ProjectStatus { Name = "Приостановлен" },
					new ProjectStatus { Name = "Завершен" }
				});
				_context.SaveChanges();
			}
		}

		//генерация пользователей
		private void GeneratePersons(int count)
		{
			var roles = _context.PersonRoles.ToList();
			var adminRole = roles.First(r => r.Name == "Администратор");
			var managerRole = roles.First(r => r.Name == "Менеджер");
			var hydrologRole = roles.First(r => r.Name == "Гидролог");

			var newPersons = new List<Person>();
			var existingLogins = _context.Persons.Select(p => p.Login).ToHashSet();

			for (int i = 0; i < count; i++)
			{
				int roleId;
				int randomValue = _random.Next(100);
				//админ ~5%, менеджер ~20%, гидролог ~75%
				if (randomValue < 5)
					roleId = adminRole.RoleId;
				else if (randomValue < 25)
					roleId = managerRole.RoleId;
				else
					roleId = hydrologRole.RoleId;

				string фамилия = _фамилии[_random.Next(_фамилии.Count)];
				string имя = _имена[_random.Next(_имена.Count)];
				string отчество = _отчества[_random.Next(_отчества.Count)];

				//генерация уникального логина
				string login;
				int attempt = 0;
				do
				{
					int randomNum = _random.Next(100, 9999);
					string translitSurname = Transliterate(фамилия.ToLower());
					login = $"{translitSurname}.{имя.ToLower()[0]}{randomNum}";
					if (login.Length > 100)
						login = login.Substring(0, 97) + randomNum.ToString().Substring(0, 3);
					attempt++;
					if (attempt > 100)
					{
						login = $"{translitSurname}.{имя.ToLower()[0]}{randomNum}_{Guid.NewGuid().ToString().Substring(0, 8)}";
						break;
					}
				} while (existingLogins.Contains(login) || newPersons.Any(p => p.Login == login));

				existingLogins.Add(login);

				newPersons.Add(new Person
				{
					LastName = TruncateString(фамилия, 70),
					FirstName = TruncateString(имя, 50),
					MiddleName = TruncateString(отчество, 65),
					Login = login,
					Password = "password123",
					RoleIdFk = roleId,
					CreatedAt = DateTime.Now.AddDays(-_random.Next(0, 730))
				});
			}
			_context.Persons.AddRange(newPersons);
			_context.SaveChanges();
		}

		private string TruncateString(string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			if (value.Length <= maxLength) return value;
			return value.Substring(0, maxLength - 3) + "...";
		}

		//транслитерация кириллицы в латиницу
		private string Transliterate(string russianText)
		{
			var translitMap = new Dictionary<char, string>
			{
				{'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
				{'е', "e"}, {'ё', "yo"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
				{'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
				{'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
				{'у', "u"}, {'ф', "f"}, {'х', "kh"}, {'ц', "ts"}, {'ч', "ch"},
				{'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
				{'э', "e"}, {'ю', "yu"}, {'я', "ya"}
			};
			var result = new StringBuilder();
			foreach (char c in russianText)
			{
				if (translitMap.ContainsKey(c))
					result.Append(translitMap[c]);
				else
					result.Append(c);
			}
			return result.ToString();
		}

		//генерация гидропостов
		private void GenerateHydroposts(int count)
		{
			//все менеджеры без гидропоста
			var availableManagers = _context.Persons
				.Where(p => p.RoleIdFkNavigation.Name == "Менеджер")
				.Where(p => !_context.Hydroposts.Any(h => h.ManagerIdFk == p.PersonId))
				.ToList();

			//если менеджеров меньше, создаем дополнительных
			if (availableManagers.Count < count)
			{
				int needed = count - availableManagers.Count;
				GeneratePersons(needed);
				availableManagers = _context.Persons
					.Where(p => p.RoleIdFkNavigation.Name == "Менеджер")
					.Where(p => !_context.Hydroposts.Any(h => h.ManagerIdFk == p.PersonId))
					.ToList();
			}

			var hydropostTypes = _context.HydropostTypes.ToList();
			var newHydroposts = new List<Hydropost>();

			for (int i = 0; i < count && i < availableManagers.Count; i++)
			{
				newHydroposts.Add(new Hydropost
				{
					Name = _названияГидропостов[_random.Next(_названияГидропостов.Count)] + $" {i + 1}",
					Description = _описанияГидропостов[_random.Next(_описанияГидропостов.Count)],
					HydropostTypeIdFk = hydropostTypes[_random.Next(hydropostTypes.Count)].HydropostTypeId,
					ManagerIdFk = availableManagers[i].PersonId,
					Latitude = (decimal)(45.0 + _random.NextDouble() * 25.0),
					Longtude = (decimal)(30.0 + _random.NextDouble() * 60.0),
					ZeroWaterLevel = (decimal)(50 + _random.Next(300))
				});
			}
			_context.Hydroposts.AddRange(newHydroposts);
			_context.SaveChanges();
		}

		//генерация оборудования
		private void GenerateEquipment(int count)
		{
			var equipmentTypes = _context.EquipmentTypes.ToList();
			var equipmentStatuses = _context.EquipmentStatuses.ToList();
			var hydroposts = _context.Hydroposts.ToList();

			var newEquipment = new List<Equipment>();

			for (int i = 0; i < count; i++)
			{
				string тип = equipmentTypes[_random.Next(equipmentTypes.Count)].Name;
				newEquipment.Add(new Equipment
				{
					Name = $"{тип}-{_random.Next(100, 999)}",
					Description = $"Серийный номер: {_random.Next(10000, 99999)}",
					EquipmentTypeIdFk = equipmentTypes[_random.Next(equipmentTypes.Count)].EquipmentTypeId,
					EquipmentStatusIdFk = equipmentStatuses[_random.Next(equipmentStatuses.Count)].EquipmentStatusId,
					HydropostIdFk = hydroposts[_random.Next(hydroposts.Count)].HydropostId,
					LastCalibration = DateOnly.FromDateTime(DateTime.Now.AddDays(-_random.Next(0, 365))),
					CreatedAt = DateTime.Now.AddDays(-_random.Next(0, 730))
				});
			}
			_context.Equipments.AddRange(newEquipment);
			_context.SaveChanges();
		}

		//генерация проектов
		private void GenerateProjects(int count)
		{
			var managers = _context.Persons
				.Where(p => p.RoleIdFkNavigation.Name == "Менеджер")
				.ToList();

			if (!managers.Any())
			{
				GeneratePersons(5); //создаем менеджеров если нет
				managers = _context.Persons
					.Where(p => p.RoleIdFkNavigation.Name == "Менеджер")
					.ToList();
			}

			var projectStatuses = _context.ProjectStatuses.ToList();
			var newProjects = new List<Project>();
			var usedNames = new HashSet<string>();

			for (int i = 0; i < count; i++)
			{
				DateTime startDate = DateTime.Now.AddMonths(-_random.Next(0, 24));
				DateTime endDate = startDate.AddMonths(_random.Next(3, 18));

				string baseName = _целиПроектов[_random.Next(_целиПроектов.Count)];
				string projectName;
				int attempt = 0;
				do
				{
					string uniqueId = Guid.NewGuid().ToString().Substring(0, 4);
					projectName = $"Пр. {baseName.Substring(0, Math.Min(15, baseName.Length))}-{uniqueId}";
					projectName = TruncateString(projectName, 50);
					attempt++;
				} while (usedNames.Contains(projectName) && attempt < 100);
				usedNames.Add(projectName);

				var project = new Project
				{
					Name = projectName,
					Client = TruncateString(_заказчики[_random.Next(_заказчики.Count)], 200),
					ContractorIdFk = managers[_random.Next(managers.Count)].PersonId,
					StartDate = DateOnly.FromDateTime(startDate),
					EndDate = DateOnly.FromDateTime(endDate),
					Purpose = TruncateString(_целиПроектов[_random.Next(_целиПроектов.Count)], 500),
					ElevationSystem = "Балтийская",
					StatusIdFk = projectStatuses[_random.Next(projectStatuses.Count)].ProjectStatusId
				};
				newProjects.Add(project);
			}
			_context.Projects.AddRange(newProjects);
			_context.SaveChanges();
		}

		//генерация участков съемки
		private void GenerateSurveyLines()
		{
			var hydroposts = _context.Hydroposts.ToList();
			var newSurveyLines = new List<SurveyLine>();

			foreach (var hydropost in hydroposts)
			{
				int linesCount = _random.Next(2, 6);
				for (int i = 0; i < linesCount; i++)
				{
					newSurveyLines.Add(new SurveyLine
					{
						HydropostIdFk = hydropost.HydropostId,
						Name = _названияУчастков[_random.Next(_названияУчастков.Count)] + $" {i + 1}",
						DistanceFromBase = (decimal)(_random.Next(0, 5000) + _random.NextDouble())
					});
				}
			}
			_context.SurveyLines.AddRange(newSurveyLines);
			_context.SaveChanges();
		}

		//генерация измерений
		private void GenerateMeasurements(int measurementsPerProject)
		{
			try
			{
				var projects = _context.Projects
					.Include(p => p.ContractorIdFkNavigation)
					.ToList();

				if (!projects.Any())
				{
					MessageBox.Show("Нет проектов для генерации измерений.", "Предупреждение");
					return;
				}

				var allSurveyLines = _context.SurveyLines
					.Include(s => s.HydropostIdFkNavigation)
					.ToList();

				if (!allSurveyLines.Any())
				{
					MessageBox.Show("Нет участков съемки для генерации измерений.", "Предупреждение");
					return;
				}

				var allEquipments = _context.Equipments
					.Include(e => e.HydropostIdFkNavigation)
					.Where(e => e.EquipmentStatusIdFkNavigation.Name == "Исправен")
					.ToList();

				if (!allEquipments.Any())
				{
					MessageBox.Show("Нет исправного оборудования для измерений.", "Предупреждение");
					return;
				}

				var persons = _context.Persons
					.Where(p => p.RoleIdFkNavigation.Name == "Гидролог")
					.ToList();

				if (!persons.Any())
				{
					MessageBox.Show("Нет гидрологов для выполнения измерений.", "Предупреждение");
					return;
				}

				var allHydroposts = _context.Hydroposts.ToList();
				var newMeasurements = new List<Measurement>();
				int totalGenerated = 0;

				foreach (var project in projects)
				{
					DateTime projectStart = project.StartDate.ToDateTime(TimeOnly.MinValue);
					DateTime projectEnd = project.EndDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now;
					int daysRange = (projectEnd - projectStart).Days;
					if (daysRange <= 0) daysRange = 30;

					//приоритет гидропостам менеджера проекта
					var managerHydropostIds = allHydroposts
						.Where(h => h.ManagerIdFk == project.ContractorIdFk)
						.Select(h => h.HydropostId)
						.ToList();

					var projectSurveyLines = allSurveyLines
						.Where(sl => managerHydropostIds.Contains(sl.HydropostIdFk))
						.ToList();

					if (!projectSurveyLines.Any())
						projectSurveyLines = allSurveyLines.ToList();

					if (!projectSurveyLines.Any())
						continue;

					var surveyLinesByHydropost = projectSurveyLines
						.GroupBy(sl => sl.HydropostIdFk)
						.ToDictionary(g => g.Key, g => g.ToList());

					int measurementsCount = _random.Next(measurementsPerProject / 2, measurementsPerProject * 2);

					for (int i = 0; i < measurementsCount; i++)
					{
						DateTime measureDate = projectStart.AddDays(_random.Next(Math.Max(1, daysRange)));
						int targetHydropostId;
						if (managerHydropostIds.Any() && _random.Next(100) < 70)
							targetHydropostId = managerHydropostIds[_random.Next(managerHydropostIds.Count)];
						else
							targetHydropostId = allHydroposts[_random.Next(allHydroposts.Count)].HydropostId;

						if (!surveyLinesByHydropost.ContainsKey(targetHydropostId))
						{
							var fallbackLines = allSurveyLines
								.Where(sl => sl.HydropostIdFk == targetHydropostId)
								.ToList();
							if (!fallbackLines.Any())
								continue;
							surveyLinesByHydropost[targetHydropostId] = fallbackLines;
						}

						var availableLines = surveyLinesByHydropost[targetHydropostId];
						var surveyLine = availableLines[_random.Next(availableLines.Count)];
						var compatibleEquipments = allEquipments
							.Where(e => e.HydropostIdFk == targetHydropostId)
							.ToList();

						if (!compatibleEquipments.Any())
							compatibleEquipments = allEquipments.ToList();
						if (!compatibleEquipments.Any())
							continue;

						var equipment = compatibleEquipments[_random.Next(compatibleEquipments.Count)];
						var person = persons[_random.Next(persons.Count)];
						double seasonFactor = GetSeasonFactor(measureDate.Month);

						var measurement = new Measurement
						{
							SurveyLineIdFk = surveyLine.SurveyLineId,
							ProjectIdFk = project.ProjectId,
							EquipmentIdFk = equipment.EquipmentId,
							PersonIdFk = person.PersonId,
							MeasuredAt = measureDate,
							WaterLevel = Math.Round(100m + (decimal)(_random.NextDouble() * 50 * seasonFactor), 3),
							WaterConsumption = Math.Round(50m + (decimal)(_random.NextDouble() * 100 * seasonFactor), 3),
							WaterTemperature = Math.Round(GetTemperatureByMonth(measureDate.Month) + (decimal)(_random.NextDouble() * 4 - 2), 2),
							WaterTransparency = Math.Round((decimal)(_random.NextDouble() * 3) + 0.5m, 1),
							IcePhenomena = measureDate.Month >= 11 || measureDate.Month <= 3
								? _ледовыеЯвления[_random.Next(_ледовыеЯвления.Count)]
								: "Нет",
							Area = Math.Round(100m + (decimal)(_random.NextDouble() * 500), 3),
							Width = Math.Round(20m + (decimal)(_random.NextDouble() * 200), 2),
							Comment = _random.Next(5) == 0 ? _комментарии[_random.Next(_комментарии.Count)] : null
						};
						newMeasurements.Add(measurement);
						totalGenerated++;

						if (newMeasurements.Count >= 100)
						{
							_context.Measurements.AddRange(newMeasurements);
							_context.SaveChanges();
							newMeasurements.Clear();
						}
					}
				}
				if (newMeasurements.Any())
				{
					_context.Measurements.AddRange(newMeasurements);
					_context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				throw new Exception($"Ошибка в GenerateMeasurements: {ex.Message}", ex);
			}
		}

		private string GetFullExceptionMessage(Exception ex)
		{
			var message = ex.Message;
			if (ex.InnerException != null)
				message += "\n\nInner: " + ex.InnerException.Message;
			return message;
		}

		private double GetSeasonFactor(int month)
		{
			if (month >= 3 && month <= 5)
				return 1.5;  //весеннее половодье
			else if (month >= 6 && month <= 8)
				return 0.8;  //летняя межень
			else if (month >= 9 && month <= 11)
				return 0.9;  //осенний период
			else
				return 0.5;  //зимний период
		}

		private decimal GetTemperatureByMonth(int month)
		{
			switch (month)
			{
				case 1: return 0.5m;
				case 2: return 0.2m;
				case 3: return 2.0m;
				case 4: return 6.5m;
				case 5: return 12.0m;
				case 6: return 18.0m;
				case 7: return 20.5m;
				case 8: return 19.0m;
				case 9: return 14.0m;
				case 10: return 8.0m;
				case 11: return 3.0m;
				case 12: return 1.0m;
				default: return 10.0m;
			}
		}

		/// <summary>
		/// Удаляет все записи из всех таблиц базы данных
		/// </summary>
		public void ClearAllTables()
		{
			try
			{
				using (var context = new DatabaseOfHydrologicalMeasurementsContext())
				{
					//отключаем проверку внешних ключей
					context.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'");
					//очищаем таблицы в правильном порядке
					context.Database.ExecuteSqlRaw("DELETE FROM MEASUREMENTS");
					context.Database.ExecuteSqlRaw("DELETE FROM SURVEY_LINES");
					context.Database.ExecuteSqlRaw("DELETE FROM EQUIPMENTS");
					context.Database.ExecuteSqlRaw("DELETE FROM PROJECTS");
					context.Database.ExecuteSqlRaw("DELETE FROM HYDROPOSTS");
					context.Database.ExecuteSqlRaw("DELETE FROM PERSONS");
					context.Database.ExecuteSqlRaw("DELETE FROM PERSON_ROLES");
					context.Database.ExecuteSqlRaw("DELETE FROM HYDROPOST_TYPES");
					context.Database.ExecuteSqlRaw("DELETE FROM EQUIPMENT_TYPES");
					context.Database.ExecuteSqlRaw("DELETE FROM EQUIPMENT_STATUSES");
					context.Database.ExecuteSqlRaw("DELETE FROM PROJECT_STATUSES");
					//сбрасываем автоинкремент
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('MEASUREMENTS', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('SURVEY_LINES', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('EQUIPMENTS', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PROJECTS', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('HYDROPOSTS', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PERSONS', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PERSON_ROLES', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('HYDROPOST_TYPES', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('EQUIPMENT_TYPES', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('EQUIPMENT_STATUSES', RESEED, 0)");
					context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('PROJECT_STATUSES', RESEED, 0)");
					//включаем проверку внешних ключей обратно
					context.Database.ExecuteSqlRaw("EXEC sp_MSforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL'");
					MessageBox.Show("Все таблицы базы данных успешно очищены!", "Очистка БД");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Ошибка при очистке базы данных:\n{ex.Message}", "Ошибка");
			}
		}
	}
}