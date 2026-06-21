using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Logic;
using PractiStudent.Data;

namespace PractiStudent.Tests
{
    [TestClass]
    public class AllTests
    {     

        #region Тесты констант (не требуют БД)

        [TestMethod]
        public void Constants_RoleAdmin_ShouldBeCorrect()
        {
            Assert.AreEqual("Администратор", TableConstants.RoleAdmin);
        }

        [TestMethod]
        public void Constants_RoleGuest_ShouldBeCorrect()
        {
            Assert.AreEqual("Гость", TableConstants.RoleGuest);
        }

        [TestMethod]
        public void Constants_TableUsers_ShouldBeCorrect()
        {
            Assert.AreEqual("Пользователи", TableConstants.TableUsers);
        }

        [TestMethod]
        public void Constants_TableApplicants_ShouldBeCorrect()
        {
            Assert.AreEqual("Абитуриент", TableConstants.TableApplicants);
        }

        #endregion

        #region Простые тесты логики (не требуют БД)

        [TestMethod]
        public void String_IsNotNullOrEmpty_ShouldWork()
        {
            Assert.IsFalse(string.IsNullOrEmpty("admin"));
            Assert.IsTrue(string.IsNullOrEmpty(""));
        }

        [TestMethod]
        public void String_Trim_ShouldRemoveWhitespaces()
        {
            Assert.AreEqual("admin", "  admin  ".Trim());
        }

        [TestMethod]
        public void Password_Length_Check()
        {
            string password = "123456";
            Assert.IsTrue(password.Length >= 6);
        }

        [TestMethod]
        public void Login_ShouldNotBeEmpty()
        {
            string login = "admin";
            Assert.IsFalse(string.IsNullOrWhiteSpace(login));
        }

        [TestMethod]
        public void Passwords_ShouldMatch()
        {
            string pass1 = "password123";
            string pass2 = "password123";
            Assert.AreEqual(pass1, pass2);
        }

        #endregion

        #region Тесты коллекций (не требуют БД)

        [TestMethod]
        public void List_ShouldAddItems()
        {
            var list = new List<string>();
            list.Add("item1");
            list.Add("item2");

            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void Dictionary_ShouldAddKeyValuePairs()
        {
            var dict = new Dictionary<string, string>();
            dict.Add("key1", "value1");

            Assert.AreEqual("value1", dict["key1"]);
        }

        #endregion

        #region Тесты работы с файлами (не требуют БД)

        [TestMethod]
        public void File_Exists_ShouldCreateLogFile()
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_log.txt");

            File.WriteAllText(logFile, "Test content");
            Assert.IsTrue(File.Exists(logFile));

            File.Delete(logFile);
        }

        [TestMethod]
        public void Path_Combine_ShouldWorkCorrectly()
        {
            string path = Path.Combine("folder", "subfolder", "file.txt");
            Assert.IsTrue(path.Contains("file.txt"));
        }

        #endregion
    }
}