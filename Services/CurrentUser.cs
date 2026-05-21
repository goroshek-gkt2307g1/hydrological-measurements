using Гидрологические_измерения.Entities;

namespace Гидрологические_измерения.Services
{
	/// <summary>
	/// Хранит информацию о текущем авторизованном пользователе
	/// </summary>
	public static class CurrentUser
	{
		public static int PersonId { get; set; }
		public static string LastName { get; set; }
		public static string FirstName { get; set; }
		public static string RoleName { get; set; }
		public static string Login { get; set; }

		public static bool IsAdmin => RoleName == "Администратор";
		public static bool IsManager => RoleName == "Менеджер";
		public static bool IsHydrolog => RoleName == "Гидролог";

		/// <summary>
		/// Установить данные пользователя после входа
		/// </summary>
		public static void SetUser(Person person)
		{
			PersonId = person.PersonId;
			LastName = person.LastName;
			FirstName = person.FirstName;
			Login = person.Login;
			RoleName = person.RoleIdFkNavigation?.Name ?? "Гидролог";
		}

		/// <summary>
		/// Очистить данные при выходе
		/// </summary>
		public static void Clear()
		{
			PersonId = 0;
			LastName = null;
			FirstName = null;
			Login = null;
			RoleName = null;
		}
	}
}