using PractiStudent.Data;
using System;
using System.Collections.Generic;
using System.Data;

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
        public DataTable Filter(string tableName, string columnName, string filterValue)
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
        public void DeleteWithCascade(string tableName, string keyColumn, object keyValue)
        {
            var relatedTables = GetRelatedTablesForCascade(tableName);
            _dbHelper.DeleteWithCascade(tableName, keyColumn, keyValue, relatedTables);
        }
        public List<string> GetAutoNumberColumns(string tableName)
        {
            switch (tableName)
            {
                case TableConstants.TableUsers:
                    return new List<string> { TableConstants.FieldUserId };
                case TableConstants.TableApplicants:
                    return new List<string> { TableConstants.FieldRegNumber };
                case TableConstants.TableFaculties:
                    return new List<string> { "Код_факультета" };
                case TableConstants.TableSpecialties:
                    return new List<string> { "Код_специальности" };
                case TableConstants.TableSpecializations:
                    return new List<string> { "Код_специализации" };
                case TableConstants.TableSchools:
                    return new List<string> { "Код_учебного_заведения" };
                default:
                    return new List<string>();
            }
        }
        public Dictionary<string, string> GetForeignKeyMappings(string tableName)
        {
            if (tableName == TableConstants.TableApplicants)
            {
                return new Dictionary<string, string>
                {
                    { "Код_учебного_заведения", TableConstants.TableSchools },
                    { "Код_специализации", TableConstants.TableSpecializations }
                };
            }
            else if (tableName == TableConstants.TableSpecializations)
            {
                return new Dictionary<string, string>
                {
                    { "Код_специальности", TableConstants.TableSpecialties }
                };
            }
            else if (tableName == TableConstants.TableSpecialties)
            {
                return new Dictionary<string, string>
                {
                    { "Код_факультета", TableConstants.TableFaculties }
                };
            }

            return new Dictionary<string, string>();
        }
        private Dictionary<string, string> GetRelatedTablesForCascade(string tableName)
        {
            return tableName switch
            {
                TableConstants.TableFaculties => new Dictionary<string, string>
                {
                    { TableConstants.TableSpecialties, "Код_факультета" }
                },
                TableConstants.TableSpecialties => new Dictionary<string, string>
                {
                    { TableConstants.TableSpecializations, "Код_специальности" }
                },
                TableConstants.TableSpecializations => new Dictionary<string, string>
                {
                    { TableConstants.TableApplicants, "Код_специализации" }
                },
                TableConstants.TableSchools => new Dictionary<string, string>
                {
                    { TableConstants.TableApplicants, "Код_учебного_заведения" }
                },
                _ => new Dictionary<string, string>()
            };
        }
    }
}