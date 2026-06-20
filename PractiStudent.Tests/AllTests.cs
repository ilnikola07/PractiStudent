using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Logic;                      
using Exceptions;                 
using PractiStudent.Data;         
namespace PractiStudent.Tests
{
    [TestClass]
    public class AllTests
    {
        #region Validator Tests (4 теста)

        [TestMethod]
        public void ValidateCredentials_ShouldReturnErrorsForEmptyLogin()
        {
            var errors = Validator.ValidateCredentials("", "password", "", false);
            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Exists(e => e.Contains("Логин")));
        }

        [TestMethod]
        public void ValidateCredentials_ShouldReturnErrorsForShortLogin()
        {
            var errors = Validator.ValidateCredentials("ab", "password", "", false);
            Assert.IsTrue(errors.Count > 0);
        }

        [TestMethod]
        public void ValidateCredentials_ShouldReturnNoErrorsForValidData()
        {
            var errors = Validator.ValidateCredentials("admin", "password123", "", false);
            Assert.AreEqual(0, errors.Count);
        }

        [TestMethod]
        public void ValidateCredentials_ShouldCheckPasswordMatch()
        {
            var errors = Validator.ValidateCredentials("admin", "password", "different", true);
            Assert.IsTrue(errors.Exists(e => e.Contains("совпадают")));
        }

        #endregion

        #region SecurityHelper Tests (2 теста)

        [TestMethod]
        public void ComputeSha256Hash_ShouldReturnSameHashForSameInput()
        {
            string password = "admin123";
            string hash1 = SecurityHelper.ComputeSha256Hash(password);
            string hash2 = SecurityHelper.ComputeSha256Hash(password);

            Assert.AreEqual(hash1, hash2);
            Assert.IsFalse(string.IsNullOrEmpty(hash1));
        }

        [TestMethod]
        public void ComputeSha256Hash_ShouldReturnDifferentHashForDifferentInput()
        {
            string hash1 = SecurityHelper.ComputeSha256Hash("admin123");
            string hash2 = SecurityHelper.ComputeSha256Hash("guest123");

            Assert.AreNotEqual(hash1, hash2);
        }

        #endregion

        #region DataFormatter Tests (2 теста)

        [TestMethod]
        public void FormatDatesInTable_ShouldFormatDateColumn()
        {
            var table = new DataTable();
            table.Columns.Add("Дата_рождения", typeof(DateTime));
            table.Rows.Add(new DateTime(2009, 5, 10));

            DataFormatter.FormatDatesInTable(table);

            Assert.AreEqual("10.05.2009", table.Rows[0]["Дата_рождения"].ToString());
        }

        [TestMethod]
        public void FormatValueForDisplay_ShouldFormatDateTime()
        {
            var dateTime = new DateTime(2009, 5, 10, 0, 0, 0);
            var result = DataFormatter.FormatValueForDisplay(dateTime);

            Assert.AreEqual("10.05.2009", result);
        }

        #endregion

        #region Logger Tests (1 тест)

        [TestMethod]
        public void LogError_ShouldCreateLogFile()
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            if (File.Exists(logFile)) File.Delete(logFile);

            Logger.LogError("Test error", null, "TestContext");

            Assert.IsTrue(File.Exists(logFile));
            string content = File.ReadAllText(logFile);
            Assert.IsTrue(content.Contains("Test error"));
        }

        #endregion

        #region TableConstants Tests (1 тест)

        [TestMethod]
        public void TableConstants_ShouldHaveCorrectValues()
        {
            Assert.AreEqual("Пользователи", TableConstants.TableUsers);
            Assert.AreEqual("Абитуриент", TableConstants.TableApplicants);
            Assert.AreEqual("Администратор", TableConstants.RoleAdmin);
            Assert.AreEqual("Гость", TableConstants.RoleGuest);
        }

        #endregion

        #region Database Tests (3 теста - требуют БД)

        [TestMethod]
        public void DatabaseHelper_ShouldConnectToDatabase()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Abiturient.accdb");

            // Пропускаем тест, если файла нет
            if (!File.Exists(dbPath))
            {
                Assert.Inconclusive("Файл БД не найден: " + dbPath);
                return;
            }

            var dbHelper = new DatabaseHelper();
            bool result = dbHelper.SetDatabaseConnection(dbPath);

            Assert.IsTrue(result);
            Assert.IsNotNull(dbHelper.DatabaseFileName);
        }

        [TestMethod]
        public void DatabaseHelper_ShouldGetTableNames()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Abiturient.accdb");

            if (!File.Exists(dbPath))
            {
                Assert.Inconclusive("Файл БД не найден");
                return;
            }

            var dbHelper = new DatabaseHelper();
            dbHelper.SetDatabaseConnection(dbPath);
            var tables = dbHelper.GetTableNames();

            Assert.IsTrue(tables.Count > 0);
            Assert.IsTrue(tables.Contains("Пользователи"));
        }

        [TestMethod]
        public void UserService_ShouldValidateAdminCredentials()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Abiturient.accdb");

            if (!File.Exists(dbPath))
            {
                Assert.Inconclusive("Файл БД не найден");
                return;
            }

            var userService = new UserService();
            userService.ConnectToDatabase(dbPath);

            string role = userService.ValidateUser("admin", "admin123", "Администратор");

            Assert.AreEqual("Администратор", role);
        }

        #endregion
    }
}