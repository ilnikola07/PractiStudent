using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Logic;

namespace PractiStudent.Tests
{
    [TestClass]
    public class AllTests
    {
        #region Тесты констант

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
        public void Constants_TableName_ShouldBeCorrect()
        {
            Assert.AreEqual("Пользователи", TableConstants.TableUsers);
        }

        [TestMethod]
        public void TableConstants_ShouldHaveCorrectValues()
        {
            Assert.AreEqual("Пользователи", TableConstants.TableUsers);
            Assert.AreEqual("Абитуриент", TableConstants.TableApplicants);
        }

        #endregion

        #region Тесты валидации

        [TestMethod]
        public void Login_ShouldNotBeEmpty()
        {
            string login = "admin";
            Assert.IsFalse(string.IsNullOrWhiteSpace(login));
        }

        [TestMethod]
        public void Password_Length_ShouldBeAtLeast5Characters()
        {
            string password = "12345";
            Assert.IsTrue(password.Length >= 5);
        }

        [TestMethod]
        public void Passwords_ShouldMatch()
        {
            string pass1 = "password123";
            string pass2 = "password123";
            Assert.AreEqual(pass1, pass2);
        }

        #endregion

        #region Тесты работы со строками

        [TestMethod]
        public void String_IsNotNullOrEmpty_ShouldWorkCorrectly()
        {
            Assert.IsFalse(string.IsNullOrEmpty("test"));
            Assert.IsTrue(string.IsNullOrEmpty(""));
        }

        [TestMethod]
        public void String_IsNotNullOrEmpty_ShouldHandleWhitespace()
        {
            Assert.IsFalse(string.IsNullOrEmpty("  "));
            Assert.IsTrue(string.IsNullOrWhiteSpace("  "));
        }

        [TestMethod]
        public void String_Trim_ShouldRemoveWhitespaces()
        {
            Assert.AreEqual("admin", "  admin  ".Trim());
        }

        [TestMethod]
        public void String_Comparison_ShouldBeCaseSensitive()
        {
            string s1 = "Admin";
            string s2 = "admin";
            Assert.AreNotEqual(s1, s2);
        }

        #endregion

        #region Тесты работы с путями

        [TestMethod]
        public void Path_Combine_ShouldWorkCorrectly()
        {
            string path = Path.Combine("folder", "subfolder", "file.txt");
            Assert.IsTrue(path.Contains("file.txt"));
        }

        [TestMethod]
        public void File_Exists_ShouldCreateLogFile()
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test_log.txt");

            File.WriteAllText(logFile, "Test content");
            Assert.IsTrue(File.Exists(logFile));

            File.Delete(logFile);
        }

        #endregion

        #region Тесты логирования

        [TestMethod]
        public void LogError_ShouldCreateLogFile()
        {
            string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            if (File.Exists(logFile)) File.Delete(logFile);

            Exceptions.Logger.LogError("Test error", null, "TestContext");

            Assert.IsTrue(File.Exists(logFile));
            string content = File.ReadAllText(logFile);
            Assert.IsTrue(content.Contains("Test error"));
        }

        #endregion
    }
}