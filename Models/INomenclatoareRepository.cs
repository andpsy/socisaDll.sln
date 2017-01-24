﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json;

namespace SOCISA.Models
{
    public interface INomenclatoareRepository
    {
        response GetAll(string tableName);
        response GetFiltered(string tableName, string _sort, string _order, string _filter, string _limit);
        response Find(string tableName, int _id);
        response Insert(Nomenclator item);
        response Update(Nomenclator item);
        response Update(string tableName, int id, string fieldValueCollection);
        response Update(string tableName, string fieldValueCollection);

        response Delete(Nomenclator item);
        response HasChildrens(Nomenclator item, string tableName);
        response HasChildren(Nomenclator item, string tableName, int childrenId);
        response GetChildrens(Nomenclator item, string tableName);
        response GetChildren(Nomenclator item, string tableName, int childrenId);
        int? GetIdByName(Nomenclator item, string tableName, string denumire);
        response Delete(string tableName, int _id);
        response HasChildrens(string tableName, int _id, string childTableName);
        response HasChildren(string tableName, int _id, string childTableName, int childrenId);
        response GetChildrens(string tableName, int _id, string childTableName);
        response GetChildren(string tableName, int _id, string childTableName, int childrenId);
    }

    public class NomenclatoareRepository : INomenclatoareRepository
    {
        private string connectionString;
        private int authenticatedUserId;

        public NomenclatoareRepository(int _authenticatedUserId, string _connectionString)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
        }

        public response GetAll(string tableName)
        {
            try
            {
                DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, String.Format("{0}sp_select", tableName.ToUpper()), new object[] {
                new MySqlParameter("_SORT", null),
                new MySqlParameter("_ORDER", null),
                new MySqlParameter("_FILTER", null),
                new MySqlParameter("_LIMIT", null) });
                ArrayList aList = new ArrayList();
                DbDataReader r = da.ExecuteSelectQuery();
                while (r.Read())
                {
                    Nomenclator a = new Nomenclator(authenticatedUserId, connectionString, tableName, (IDataRecord)r);
                    aList.Add(a);
                }
                Nomenclator[] toReturn = new Nomenclator[aList.Count];
                for (int i = 0; i < aList.Count; i++)
                    toReturn[i] = (Nomenclator)aList[i];
                return new response(true, JsonConvert.SerializeObject(toReturn), null, null); 
            }
            catch (Exception exp) { LogWriter.Log(exp); return new response(false, exp.ToString(), null, new System.Collections.Generic.List<Error>() { new Error(exp) }); }
        }

        public response GetFiltered(string tableName, string _sort, string _order, string _filter, string _limit)
        {
            try
            {
                try
                {
                    string newFilter = Filtering.GenerateFilterFromJsonObject(typeof(Nomenclator), _filter, authenticatedUserId, connectionString);
                    _filter = newFilter == null ? _filter : newFilter;
                }
                catch { }
                DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, String.Format("{0}sp_select", tableName.ToUpper()), new object[] {
                new MySqlParameter("_SORT", _sort),
                new MySqlParameter("_ORDER", _order),
                new MySqlParameter("_FILTER", _filter),
                new MySqlParameter("_LIMIT", _limit) });
                ArrayList aList = new ArrayList();
                DbDataReader r = da.ExecuteSelectQuery();
                while (r.Read())
                {
                    Nomenclator a = new Nomenclator(authenticatedUserId, connectionString, tableName, (IDataRecord)r);
                    aList.Add(a);
                }
                Nomenclator[] toReturn = new Nomenclator[aList.Count];
                for (int i = 0; i < aList.Count; i++)
                    toReturn[i] = (Nomenclator)aList[i];
                return new response(true, JsonConvert.SerializeObject(toReturn), null, null); 
            }
            catch (Exception exp) { LogWriter.Log(exp); return new response(false, exp.ToString(), null, new System.Collections.Generic.List<Error>() { new Error(exp) }); }
        }

        public response Find(string tableName, int _id)
        {
            try
            {
                Nomenclator item = new Nomenclator(authenticatedUserId, connectionString, tableName, _id);
                return new response(true, JsonConvert.SerializeObject(item), null, null); 
            }
            catch (Exception exp) { LogWriter.Log(exp); return new response(false, exp.ToString(), null, new System.Collections.Generic.List<Error>() { new Error(exp) }); }
        }

        public response Insert(Nomenclator item)
        {
            return item.Insert();
        }

        public response Update(Nomenclator item)
        {
            return item.Update();
        }

        public response Update(string tableName, int id, string fieldValueCollection)
        {
            Nomenclator item = JsonConvert.DeserializeObject<Nomenclator>(Find(tableName, id).Message);
            return item.Update(fieldValueCollection);
        }
        public response Update(string tableName, string fieldValueCollection)
        {
            Nomenclator tmpItem = JsonConvert.DeserializeObject<Nomenclator>(fieldValueCollection); // sa vedem daca merge asa sau trebuie cu JObject
            return JsonConvert.DeserializeObject<Nomenclator>(Find(tableName, Convert.ToInt32(tmpItem.ID)).Message).Update(fieldValueCollection);
        }

        public response Delete(Nomenclator item)
        {
            return item.Delete();
        }

        public response HasChildrens(Nomenclator item, string childTableName)
        {
            return item.HasChildrens(childTableName);
        }

        public response HasChildren(Nomenclator item, string childTableName, int childrenId)
        {
            return item.HasChildren(childTableName, childrenId);
        }

        public response GetChildrens(Nomenclator item, string childTableName)
        {
            return item.GetChildrens(childTableName);
        }

        public response GetChildren(Nomenclator item, string childTableName, int childrenId)
        {
            return item.GetChildren(childTableName, childrenId);
        }

        public int? GetIdByName(Nomenclator item, string tableName, string denumire)
        {
            return item.GetIdByName(tableName, denumire);
        }
        public response Delete(string tableName, int _id)
        {
            var obj = Find(tableName, _id);
            return JsonConvert.DeserializeObject<Nomenclator>(obj.Message).Delete();
        }

        public response HasChildrens(string tableName, int _id, string childTableName)
        {
            var obj = Find(tableName, _id);
            return JsonConvert.DeserializeObject<Nomenclator>(obj.Message).HasChildrens(childTableName);
        }
        public response HasChildren(string tableName, int _id, string childTableName, int childrenId)
        {
            var obj = Find(tableName, _id);
            return JsonConvert.DeserializeObject<Nomenclator>(obj.Message).HasChildren(childTableName, childrenId);
        }
        public response GetChildrens(string tableName, int _id, string childTableName)
        {
            var obj = Find(tableName, _id);
            return JsonConvert.DeserializeObject<Nomenclator>(obj.Message).GetChildrens(childTableName);
        }
        public response GetChildren(string tableName, int _id, string childTableName, int childrenId)
        {
            var obj = Find(tableName, _id);
            return JsonConvert.DeserializeObject<Nomenclator>(obj.Message).GetChildren(childTableName, childrenId);
        }
    }
}
