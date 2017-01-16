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
    /// Clasa care contine definitia obiectului ce mapeaza tabela cu Dosare din baza de date
    /// </summary>
    public class Drept
    {
        private int authenticatedUserId { get; set; }
        private string connectionString { get; set; }
        public int? ID {get;set;}
        public string DENUMIRE { get; set; }
        public string DETALII { get; set; }

        /// <summary>
        /// Constructorul default
        /// </summary>
        public Drept() { }

        public Drept(int _authenticatedUserId, string _connectionString)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
        }

        public Drept(int _authenticatedUserId, string _connectionString, int _ID)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "ACTIONSsp_GetById", new object[] { new MySqlParameter("_ID", _ID) });
            DbDataReader r = da.ExecuteSelectQuery();
            while (r.Read())
            {
                IDataRecord drept = (IDataRecord)r;
                DreptConstructor(drept);
                break;
            }
        }

        public Drept(int _authenticatedUserId, string _connectionString, IDataRecord drept)
        {
            authenticatedUserId = _authenticatedUserId;
            connectionString = _connectionString;
            DreptConstructor(drept);
        }

        public void DreptConstructor(IDataRecord drept)
        {
            try { this.ID = Convert.ToInt32(drept["ID"]); }
            catch { }
            try { this.DENUMIRE = drept["DENUMIRE"].ToString(); }
            catch { }
            try { this.DETALII = drept["DETALII"].ToString(); }
            catch { }
        }

        /// <summary>
        /// Metoda pentru inserarea Dreptului curent
        /// </summary>
        /// <returns>SOCISA.response = new object(bool = status, string = error message, int = id-ul cheie returnat)</returns>
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
                var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "drepturi");
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
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "DREPTURIsp_insert", _parameters.ToArray());
            toReturn = da.ExecuteInsertQuery();
            if (toReturn.Status) this.ID = toReturn.InsertedId;

            return toReturn;
        }

        /// <summary>
        /// Metoda pentru modificarea Dreptului curent
        /// </summary>
        /// <returns>SOCISA.response = new object(bool = status, string = error message, int = id-ul cheie returnat)</returns>
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
                var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "drepturi");
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
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "DREPTURIsp_update", _parameters.ToArray());
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
                        //var col = CommonFunctions.table_columns(authenticatedUserId, connectionString, "drepturi");
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

        /// <summary>
        /// Metoda pentru stergerea Dreptului curent
        /// </summary>
        /// <returns>SOCISA.response = new object(bool = status, string = error message, int = id-ul cheie returnat)</returns>
        public response Delete()
        {
            response toReturn = new response(false, "", null, new List<Error>());
            ArrayList _parameters = new ArrayList();
            _parameters.Add(new MySqlParameter("_ID", this.ID));
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "DREPTURIsp_delete", _parameters.ToArray());
            toReturn = da.ExecuteDeleteQuery();
            return toReturn;
        }

        /// <summary>
        /// Metoda pentru validarea Dreptului curent
        /// </summary>
        /// <returns>SOCISA.response = new object(bool = status, string = error message, int = id-ul cheie returnat)</returns>
        public response Validare()
        {
            response toReturn = new response(true, "", null, new List<Error>());;
            Error err = new Error();
            if (this.DENUMIRE == null || this.DENUMIRE.Trim() == "")
            {
                toReturn.Status = false;
                err = CommonFunctions.ErrorMessage("emptyDenumireDrept");
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

        /// <summary>
        /// Metoda pentru generarea filtrului de cautare/filtrare pe baza coloanelor si a valorilor acestora.
        /// Folosita la cautarea cu TypeAhead
        /// </summary>
        /// <returns>string cu filtrul ce va fi trimis ca parametru in procedura stocata din BD pentru filtrare</returns>
        public string GenerateFilterFromJsonObject()
        {
            return CommonFunctions.GenerateFilterFromJsonObject(this);
        }

        public bool HasChildrens(string tableName)
        {
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "TABLEsp_GetReferences", new object[] { new MySqlParameter("_PARENT_TABLE", "drepturi"), new MySqlParameter("_CHILD_TABLE", tableName) });
            DbDataReader r = da.ExecuteSelectQuery();
            while (r.Read())
            {
                if (r["REFERENCED_TABLE_NAME"].ToString().ToUpper() == tableName.ToUpper())
                {
                    PropertyInfo[] props = this.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        if (prop.Name.ToUpper() == r["COLUMN_NAME"].ToString().ToUpper())
                        {
                            da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "CHILDRENSsp_Get", new object[] { new MySqlParameter("_PRIMARY_KEY_VALUE", prop.GetValue(this)), new MySqlParameter("_EXTERNAL_ID", r["REFERENCED_COLUMN_NAME"].ToString()), new MySqlParameter("_EXTERNAL_TABLE", r["REFERENCED_TABLE_NAME"].ToString()) });
                            object counter = da.ExecuteScalarQuery();
                            try
                            {
                                if (Convert.ToInt32(counter) > 0)
                                    return true;
                            }
                            catch { }
                            break;
                        }
                    }
                }
                else
                {
                    da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "CHILDRENSsp_Get", new object[] { new MySqlParameter("_PRIMARY_KEY_VALUE", this.ID), new MySqlParameter("_EXTERNAL_ID", r["COLUMN_NAME"].ToString()), new MySqlParameter("_EXTERNAL_TABLE", r["TABLE_NAME"].ToString()) });
                    object counter = da.ExecuteScalarQuery();
                    try
                    {
                        return Convert.ToInt32(counter) > 0;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool HasChildren(string tableName, int childrenId)
        {
            DataAccess da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "TABLEsp_GetReferences", new object[] { new MySqlParameter("_PARENT_TABLE", "drepturi"), new MySqlParameter("_CHILD_TABLE", tableName) });
            DbDataReader r = da.ExecuteSelectQuery();
            while (r.Read())
            {
                if (r["REFERENCED_TABLE_NAME"].ToString().ToUpper() == tableName.ToUpper())
                {
                    PropertyInfo[] props = this.GetType().GetProperties();
                    foreach (PropertyInfo prop in props)
                    {
                        if (prop.Name.ToUpper() == r["COLUMN_NAME"].ToString().ToUpper())
                        {
                            da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "CHILDRENsp_Get", new object[] { new MySqlParameter("_PRIMARY_KEY_VALUE", prop.GetValue(this)), new MySqlParameter("_EXTERNAL_ID", r["REFERENCED_COLUMN_NAME"].ToString()), new MySqlParameter("_EXTERNAL_TABLE", r["REFERENCED_TABLE_NAME"].ToString()), new MySqlParameter("_CHILDREN_ID_FIELD", "1"), new MySqlParameter("_CHILDREN_ID_VALUE", "1") });
                            object counter = da.ExecuteScalarQuery();
                            try
                            {
                                if (Convert.ToInt32(counter) > 0)
                                    return true;
                            }
                            catch { }
                            break;
                        }
                    }
                }
                else
                {
                    da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "TABLEsp_GetReferences", new object[] { new MySqlParameter("_PARENT_TABLE", r["TABLE_NAME"].ToString()), new MySqlParameter("_CHILD_TABLE", tableName) });
                    DbDataReader rc = da.ExecuteSelectQuery();
                    while (rc.Read())
                    {
                        da = new DataAccess(authenticatedUserId, connectionString, CommandType.StoredProcedure, "CHILDRENsp_Get", new object[] { new MySqlParameter("_PRIMARY_KEY_VALUE", this.ID), new MySqlParameter("_EXTERNAL_ID", r["REFERENCED_COLUMN_NAME"].ToString()), new MySqlParameter("_EXTERNAL_TABLE", r["REFERENCED_TABLE_NAME"].ToString()), new MySqlParameter("_CHILDREN_ID_FIELD", rc["COLUMN_NAME"].ToString()), new MySqlParameter("_CHILDREN_ID_VALUE", childrenId) });
                        object counter = da.ExecuteScalarQuery();
                        try
                        {
                            return Convert.ToInt32(counter) > 0;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                }
            }
            return false;
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