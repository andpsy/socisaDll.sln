﻿using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace SOCISA.Models
{
    /// <summary>
    /// Clasa statica cu diferite metode pentru selectia Asiguratilor din baza de date
    /// </summary>
    public class Asigurat
    {
        private int authenticatedUserId { get; set; }
        private string connectionString { get; set; }
        public int? ID { get; set; }
        public string DENUMIRE { get; set; }
        public string DETALII { get; set; }

        public Asigurat() { }

        public Asigurat(int _authenticatedUserId, string _connectionString)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
        }

        public Asigurat(int _authenticatedUserId, string _connectionString, int _ID)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ASIGURATIsp_GetById", new object[] { new MySqlParameter("_ID", _ID) });
            DbDataReader r = da.ExecuteSelectQuery();
            while (r.Read())
            {
                IDataRecord asigurat = (IDataRecord)r;
                AsiguratConstructor(asigurat);
                break;
            }
        }

        public Asigurat(int _authenticatedUserId, string _connectionString, string _DENUMIRE)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ASIGURATIsp_GetByDenumire", new object[] { new MySqlParameter("_DENUMIRE", _DENUMIRE) });
            DbDataReader r = da.ExecuteSelectQuery();
            while (r.Read())
            {
                IDataRecord asigurat = (IDataRecord)r;
                AsiguratConstructor(asigurat);
                break;
            }
        }

        public Asigurat(int _authenticatedUserId, string _connectionString, IDataRecord asigurat)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
            AsiguratConstructor(asigurat);
        }

        public void AsiguratConstructor(IDataRecord asigurat)
        {
            try { this.ID = Convert.ToInt32(asigurat["ID"]); }
            catch { }
            try { this.DENUMIRE = asigurat["DENUMIRE"].ToString(); }
            catch { }
            try { this.DETALII = asigurat["DETALII"].ToString(); }
            catch { }
        }

        public response Insert()
        {
            response toReturn = Validare();
            if (!toReturn.Status)
            {
                return toReturn;
            }
            PropertyInfo[] props = this.GetType().GetProperties();
            ArrayList _parameters = new ArrayList();

            foreach (PropertyInfo prop in props)
            {
                var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "asigurati");
                if (col != null && col.ToUpper().IndexOf(prop.Name.ToUpper()) > -1) // ca sa includem in Array-ul de parametri doar coloanele tabelei, nu si campurile externe si/sau alte proprietati
                {
                    string propName = prop.Name;
                    string propType = prop.PropertyType.ToString();
                    object propValue = prop.GetValue(this, null);
                    propValue = propValue == null ? DBNull.Value : propValue;
                    if (propType != null)
                    {
                        if (propName.ToUpper() != "ID") // il vom folosi doar la Edit!
                            _parameters.Add(new MySqlParameter(String.Format("_{0}", propName.ToUpper()), propValue));
                    }
                }
            }
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ASIGURATIsp_insert", _parameters.ToArray());
            toReturn = da.ExecuteInsertQuery();
            if (toReturn.Status) this.ID = toReturn.InsertedId;

            return toReturn;
        }

        public response Update()
        {
            response toReturn = Validare();
            if (!toReturn.Status)
            {
                return toReturn;
            }
            PropertyInfo[] props = this.GetType().GetProperties();
            ArrayList _parameters = new ArrayList();
            foreach (PropertyInfo prop in props)
            {
                var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "asigurati");
                if (col != null && col.ToUpper().IndexOf(prop.Name.ToUpper()) > -1) // ca sa includem in Array-ul de parametri doar coloanele tabelei, nu si campurile externe si/sau alte proprietati
                {
                    string propName = prop.Name;
                    string propType = prop.PropertyType.ToString();
                    object propValue = prop.GetValue(this, null);
                    propValue = propValue == null ? DBNull.Value : propValue;
                    if (propType != null)
                    {
                        _parameters.Add(new MySqlParameter(String.Format("_{0}", propName.ToUpper()), propValue));
                    }
                }
            }
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ASIGURATIsp_update", _parameters.ToArray());
            toReturn = da.ExecuteUpdateQuery();

            return toReturn;
        }

        public response Update(string fieldValueCollection)
        {
            response r = ValidareColoane(fieldValueCollection);
            if (!r.Status)
            {
                return r;
            }
            else
            {
                Dictionary<string, string> changes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(fieldValueCollection);
                foreach (string fieldName in changes.Keys)
                {
                    PropertyInfo[] props = this.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        //var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "asigurati");
                        //if (col != null && col.ToUpper().IndexOf(prop.Name.ToUpper()) > -1 && fieldName.ToUpper() == prop.Name.ToUpper()) // ca sa includem in Array-ul de parametri doar coloanele tabelei, nu si campurile externe si/sau alte proprietati
                        if (fieldName.ToUpper() == prop.Name.ToUpper())
                        {
                            var tmpVal = prop.PropertyType.FullName.IndexOf("System.String") > -1 ? changes[fieldName] : Newtonsoft.Json.JsonConvert.DeserializeObject(changes[fieldName], prop.PropertyType);
                            prop.SetValue(this, tmpVal);
                            break;
                        }
                    }

                }
                return this.Update();
            }
        }

        public response Delete()
        {
            response toReturn = new response(false, "", null, new List<Error>()); ;
            ArrayList _parameters = new ArrayList();
            _parameters.Add(new MySqlParameter("_ID", this.ID));
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ASIGURATIsp_delete", _parameters.ToArray());
            toReturn = da.ExecuteDeleteQuery();
            return toReturn;
        }

        public response Validare()
        {
            response toReturn = new response(true, "", null, new List<Error>()); ;
            Error err = new Error();
            if (this.DENUMIRE == null || this.DENUMIRE.Trim() == "")
            {
                toReturn.Status = false;
                err = CommonFunctions.ErrorMessage("emptyDenumireAsigurat");
                toReturn.Message = string.Format("{0}{1};", toReturn.Message == null ? "" : toReturn.Message, err.ERROR_MESSAGE);
                toReturn.InsertedId = null;
                toReturn.Error.Add(err);
            }

            return toReturn;
        }

        public response ValidareColoane(string fieldValueCollection)
        {
            return CommonFunctions.ValidareColoane(this, fieldValueCollection);
        }

        public string GenerateFilterFromJsonObject()
        {
            return CommonFunctions.GenerateFilterFromJsonObject(this);
        }

        public bool HasChildrens(string tableName)
        {
            return CommonFunctions.HasChildrens(authenticatedUserId, connectionString, this, "asigurati", tableName);
        }

        public bool HasChildren(string tableName, int childrenId)
        {
            return CommonFunctions.HasChildren(authenticatedUserId, connectionString, this, "asigurati", tableName, childrenId);
        }

        public object[] GetChildrens(string tableName)
        {
            return null;
        }

        public object GetChildren(string tableName, int childrenId)
        {
            return null;
        }
    }
}