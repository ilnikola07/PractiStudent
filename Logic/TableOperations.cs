using PractiStudent.Data;
using System;
using System.Collections.Generic;
using System.Data;
using Logic;

namespace Logic
{
    public class TableOperations
    {
        private readonly DatabaseHelper _dbHelper;

        public TableOperations(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public DataTable Search(string tableName, string columnName, string searchText)
        {
            return _dbHelper.SearchData(tableName, columnName, searchText);
        }

        public DataTable Filter(string tableName, string columnName, string filterValue, bool exactMatch = true)
        {
            return _dbHelper.FilterData(tableName, columnName, filterValue);
        }

        public DataTable Sort(string tableName, string columnName, bool ascending)
        {
            return _dbHelper.SortData(tableName, columnName, ascending);
        }

        public void AddRecord(string tableName, Dictionary<string, object> values, List<string> autoNumberColumns)
        {
            var filteredValues = new Dictionary<string, object>();
            foreach (var kvp in values)
            {
                if (!autoNumberColumns.Contains(kvp.Key))
                {
                    filteredValues[kvp.Key] = kvp.Value;
                }
            }
            _dbHelper.InsertRecord(tableName, filteredValues);
        }

        public void UpdateRecord(string tableName, Dictionary<string, object> values,
            string keyColumn, object keyValue, string primaryKeyColumn)
        {
            values.Remove(primaryKeyColumn);
            _dbHelper.UpdateRecord(tableName, values, keyColumn, keyValue);
        }

        public void DeleteRecord(string tableName, string keyColumn, object keyValue)
        {
            _dbHelper.DeleteRecord(tableName, keyColumn, keyValue);
        }

        public List<string> GetAutoNumberColumns(string tableName)
        {
            var autoNumberColumns = new List<string>();

            switch (tableName)
            {
                case TableConstants.TableUsers:
                    autoNumberColumns.Add(TableConstants.FieldUserId);
                    break;
                case TableConstants.TableApplicants:
                    autoNumberColumns.Add(TableConstants.FieldRegNumber);
                    break;
                case TableConstants.TableFaculties:
                    autoNumberColumns.Add("Код_факультета");
                    break;
                case TableConstants.TableSpecialties:
                    autoNumberColumns.Add("Код_специальности");
                    break;
                case TableConstants.TableSpecializations:
                    autoNumberColumns.Add("Код_специализации");
                    break;
            }

            return autoNumberColumns;
        }


        public Dictionary<string, string> GetForeignKeyMappings(string tableName)
        {
            var fkMappings = new Dictionary<string, string>();

            if (tableName == TableConstants.TableApplicants)
            {
                fkMappings[TableConstants.FieldSchoolCode] = TableConstants.TableSchools;
                fkMappings[TableConstants.FieldSpecializationCode] = TableConstants.TableSpecializations;
            }
            else if (tableName == TableConstants.TableSpecializations)
            {
                fkMappings["Код_специальности"] = TableConstants.TableSpecialties;
            }
            else if (tableName == TableConstants.TableSpecialties)
            {
                fkMappings["Код_факультета"] = TableConstants.TableFaculties;
            }

            return fkMappings;
        }

        public void DeleteWithCascade(string tableName, string keyColumn, object keyValue)
        {
            // Определяем связанные таблицы на основе удаляемой таблицы
            var relatedTables = GetRelatedTablesForCascade(tableName);

            // Вызываем метод из DatabaseHelper
            _dbHelper.DeleteWithCascade(tableName, keyColumn, keyValue, relatedTables);
        }

        private Dictionary<string, string> GetRelatedTablesForCascade(string tableName)
        {
            // Возвращаем словарь связанных таблиц для каскадного удаления
            return tableName switch
            {
                TableConstants.TableFaculties => new Dictionary<string, string>
        {
            { "Специальность", "Код_факультета" }
        },
                TableConstants.TableSpecialties => new Dictionary<string, string>
        {
            { "Специализация", "Код_специальности" }
        },
                TableConstants.TableSpecializations => new Dictionary<string, string>
        {
            { "Абитуриент", "Код_специализации" }
        },
                TableConstants.TableSchools => new Dictionary<string, string>
        {
            { "Абитуриент", "Код_учебного_заведения" }
        },
                _ => new Dictionary<string, string>()  // Пустой словарь для остальных таблиц
            };
        }
    }
}