namespace Logic  
{
    public static class TableConstants
    {
        // имена таблиц
        public const string TableApplicants = "Абитуриент";
        public const string TableUsers = "Пользователи";
        public const string TableSpecializations = "Специализация";
        public const string TableSpecialties = "Специальность";
        public const string TableFaculties = "Факультет";
        public const string TableSchools = "Что_окончил";

        // поля
        public const string FieldRegNumber = "Регистрационный_номер";
        public const string FieldFullName = "ФИО";
        public const string FieldBirthDate = "Дата_рождения";
        public const string FieldSchoolCode = "Код_учебного_заведения";
        public const string FieldHasMedal = "Наличие_медали";
        public const string FieldSpecializationCode = "Код_специализации";
        public const string FieldUserId = "ID";
        public const string FieldLogin = "Логин";
        public const string FieldPasswordHash = "Хэш_пароля";
        public const string FieldRole = "Роль";

        // роли
        public const string RoleAdmin = "Администратор";
        public const string RoleGuest = "Гость";
    }
}