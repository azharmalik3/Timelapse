using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using BLL.Common;

namespace BLL.Common
{
    public class Connection
    {
        [ThreadStatic]
        private static String _connectionString;
        [ThreadStatic]
        private static SqlConnection _connection;
        [ThreadStatic]
        private static int _transLocks;
        [ThreadStatic]
        private static int _cnLocks;

        public static SqlConnection DbConnection
        {
            get
            {
                if (_connection == null)
                {
                    _connectionString = Settings.ConnectionString;
                    _connection = new SqlConnection(_connectionString);
                    return _connection;
                }
                return _connection;

            }
        }

        public static void OpenConnection()
        {
            if (_cnLocks == 0)
                DbConnection.Open();
            if (DbConnection.State != ConnectionState.Open)
                throw new ApplicationException("Assert: Connection is not open");
            _cnLocks++;
            if (_cnLocks >= (int.MaxValue - 100))
                _cnLocks = 1;
        }

        public static void CloseConnection()
        {
            if (_cnLocks > 0)
            {
                DbConnection.Close();
                DbConnection.Dispose();
                _connection = null;
                _cnLocks--;
            }
        }
    }
}
