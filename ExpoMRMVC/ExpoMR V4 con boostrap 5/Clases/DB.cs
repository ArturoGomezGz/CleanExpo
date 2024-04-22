using ExpoMR.Clases;
using ExpoMR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Diagnostics;


namespace ExpoMR.Clases
{
    public class DB
    {
        private IConfiguration _configurationDB;
		private readonly SqlConnection con = new();
		private readonly SqlCommand cmd = new();

		public DB( IConfiguration configurationDB)
        {
            _configurationDB = configurationDB;
        }

        public DB()
        {
            cmd.Connection = con;
        }

        private void SetConnection(string dbConnection)
        {
			if (con.State == ConnectionState.Open)
				con.Close();

            con.ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")[dbConnection.ToString()];

		}

        private void SetCommand(string command, ref SqlParameter[] parameters, CommandType commandType)
        {
            cmd.CommandType = commandType;
            cmd.CommandText = command;
            cmd.Parameters.AddRange(parameters);
        }

        public void ExecuteNonQuery(string command, ref SqlParameter[] parameters, CommandType commandType = CommandType.StoredProcedure, string dbConnection = "ExpoMRCS")
        {
            try
            {
                SetConnection(dbConnection);
                SetCommand(command, ref parameters, commandType);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception) { }
            finally
            {
                con.Close();
            }
        }

        public object ExecuteScalar(string command, ref SqlParameter[] parameters, CommandType commandType = CommandType.StoredProcedure, string dbConnection = "ExpoMRCS")
        {
            object ret = null;
            try
            {
                SetConnection(dbConnection);
                SetCommand(command, ref parameters, commandType);
                con.Open();
                ret = cmd.ExecuteScalar();
            }
            catch (Exception) 
            { 
            
            }
            finally
            {
                con.Close();
            }
            return ret;
        }

        public int ExecuteReturnValue(string command, ref SqlParameter[] parameters, string dbConnection = "ExpoMRCS")
        {

            int ret = 0;
            CommandType commandType = CommandType.StoredProcedure;

            try
            {
                SetConnection(dbConnection);
                SetCommand(command, ref parameters, commandType);
                cmd.Parameters.Add("@retValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;
                con.Open();
                cmd.ExecuteNonQuery();
                ret = (int)cmd.Parameters["@retValue"].Value;
            }
            catch (Exception) 
            { 
            
            }
            finally
            {
                con.Close();
            }

            return ret;
        }

        public SqlDataReader ExecuteReader(string command, ref SqlParameter[] parameters, CommandType commandType = CommandType.StoredProcedure, string dbConnection = "ExpoMRCS")
        {
            try
            {
                SetConnection(dbConnection);
                SetCommand(command, ref parameters, commandType);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                return dr;
            }
            catch (Exception)
            {
                con.Close();
                return null;
            }
        }

        public void Close()
        {
            try
            {
                con.Close();
            }
            catch (Exception)
            {
            
            }
        }
    }
}