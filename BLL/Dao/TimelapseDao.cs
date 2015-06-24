using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using BLL.Entities;
using BLL.Common;

namespace BLL.Dao
{
    public class TimelapseDao
    {
        public static Timelapse Get(int id)
        {
            var timelapse = new Timelapse();
            try
            {
                const string sql = "Select * FROM Timelapses WHERE Id=@Id AND IsDeleted=0 ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@Id", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());

                if (dr.Count > 0) timelapse = dr.FirstOrDefault();

                Connection.CloseConnection();
                return timelapse;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao Get(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao Get(int id) Id={0}<br />{1}", id, ex.Message));
                return timelapse;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static Timelapse Get(string code)
        {
            var timelapse = new Timelapse();
            try
            {
                const string sql = "Select * FROM Timelapses WHERE Code=@Code AND IsDeleted=0 ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@Code", code);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());

                if (dr.Count > 0) timelapse = dr.FirstOrDefault();

                Connection.CloseConnection();
                return timelapse;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao Get(string code) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao Get(string code) code={0}<br />{1}", code, ex.Message));
                return null;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static Timelapse GetExists(int id)
        {
            var timelapse = new Timelapse();
            try
            {
                const string sql = "Select * FROM Timelapses WHERE Id=@Id ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@Id", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());

                if (dr.Count > 0) timelapse = dr.FirstOrDefault();

                Connection.CloseConnection();
                return timelapse;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao Get(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao Get(int id) Id={0}<br />{1}", id, ex.Message));
                return timelapse;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetList(TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE IsDeleted=0" + s + p + " ";
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetList() " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetList() <br />{1}", ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListByUserId(int id, TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE UserId=@UserId AND IsDeleted=0 " + s + p + " ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@UserId", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetListByUserId(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetListByUserId(int id) Id={0}<br />{1}", id, ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListByEvercamId(string evercamId, TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE UserId=@UserId AND IsDeleted=0 " + s + p + " ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@UserId", evercamId);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetListByEvercamId(string evercamId, TimelapsePrivacy? privacy, TimelapseStatus? status) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetListByEvercamId(string evercamId, TimelapsePrivacy? privacy, TimelapseStatus? status) evercamId={0}<br />{1}", evercamId, ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListByCameraId(string id, TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE CameraId=@CameraId AND IsDeleted=0 " + s + p + " ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@CameraId", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetListByCameraId(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetListByCameraId(int id) Id={0}<br />{1}", id, ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListByServerIP(string serverIp, TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE ServerIP=@ServerIP AND IsDeleted=0 " + s + p + " ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@ServerIP", serverIp);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetListByServerIP(string serverIp) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetListByServerIP(string serverIp) serverIp={0}<br />{1}", serverIp, ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListByRecording(bool isRecording, TimelapsePrivacy? privacy, TimelapseStatus? status)
        {
            try
            {
                string s = (status.HasValue ? " AND Status = " + ((int)status.Value) : "");
                string p = (privacy.HasValue ? " AND Privacy = " + ((int)privacy.Value) : "");
                string sql = "Select * FROM Timelapses WHERE IsRecording=@IsRecording AND IsDeleted=0 " + s + p + " ORDER BY CreatedDT DESC";
                var p1 = new SqlParameter("@IsRecording", isRecording);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetListByType(bool isRecording) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetListByType(bool isRecording) isRecording={0}<br />{1}", isRecording, ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static List<Timelapse> GetListForDeletion(string deleted, string status)
        {
            try
            {
                string sql = "Select * FROM Timelapses WHERE " + deleted + " AND " + status + " ORDER BY CreatedDT DESC";
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Connection = Connection.DbConnection;
                Connection.OpenConnection();
                var dr = GetListFromDataReader(cmd.ExecuteReader());
                Connection.CloseConnection();
                return dr;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetList() " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetList() <br />{1}", ex.Message));
                return new List<Timelapse>();
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static int Insert(Timelapse timelapse)
        {
            string query = @"INSERT INTO [dbo].[Timelapses] " +
                           "([UserId],[CameraId],[OauthToken],[Code],[Title],[Status],[Privacy],[FromDT],[ToDT],[DateAlways],[TimeAlways],[ServerIP],[TzId],[TimeZone],[SnapsInterval],[ModifiedDT],[EnableMD],[MDThreshold],[ExcludeDark],[DarkThreshold],[FPS],[IsRecording],[IsDeleted],[CreatedDT],[WatermarkImage],[WatermarkPosition]) " +
                           "VALUES " +
                           "(@UserId,@CameraId,@OauthToken,@Code,@Title,@Status,@Privacy,@FromDT,@ToDT,@DateAlways,@TimeAlways,@ServerIP,@TzId,@TimeZone,@SnapsInterval,@ModifiedDT,@EnableMD,@MDThreshold,@ExcludeDark,@DarkThreshold, @FPS,@IsRecording,@IsDeleted,@CreatedDT,@WatermarkImage,@WatermarkPosition) " +
                           "SELECT CAST(scope_identity() AS int)";
            try
            {
                var p1 = new SqlParameter("@CameraId", timelapse.CameraId);
                var p2 = new SqlParameter("@UserId", timelapse.UserId);
                var p3 = new SqlParameter("@Code", timelapse.Code);
                var p4 = new SqlParameter("@Title", timelapse.Title);
                var p5 = new SqlParameter("@Status", timelapse.Status);
                var p6 = new SqlParameter("@Privacy", timelapse.Privacy);
                var p7 = new SqlParameter("@FromDT", (timelapse.FromDT == null ? Utils.SQLMinDate : timelapse.FromDT));
                var p8 = new SqlParameter("@ToDT", (timelapse.ToDT == null ? Utils.SQLMaxDate : timelapse.ToDT));
                var p9 = new SqlParameter("@ServerIP", timelapse.ServerIP);
                var p10 = new SqlParameter("@EnableMD", timelapse.EnableMD);
                var p11 = new SqlParameter("@MDThreshold", timelapse.MDThreshold);
                var p12 = new SqlParameter("@ExcludeDark", timelapse.ExcludeDark);
                var p13 = new SqlParameter("@DarkThreshold", timelapse.DarkThreshold);
                var p14 = new SqlParameter("@IsRecording", timelapse.IsRecording);
                var p15 = new SqlParameter("@IsDeleted", timelapse.IsDeleted);
                var p16 = new SqlParameter("@ModifiedDT", Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone));
                var p17 = new SqlParameter("@SnapsInterval", timelapse.SnapsInterval);
                var p18 = new SqlParameter("@TimeZone", timelapse.TimeZone);
                var p19 = new SqlParameter("@DateAlways", timelapse.DateAlways);
                var p20 = new SqlParameter("@TimeAlways", timelapse.TimeAlways);
                var p21 = new SqlParameter("@CreatedDT", Utils.ConvertFromUtc(DateTime.UtcNow, timelapse.TimeZone));
                var p22 = new SqlParameter("@TzId", timelapse.TzId);
                var p23 = new SqlParameter("@FPS", timelapse.FPS);
                var p24 = new SqlParameter("@OauthToken", timelapse.OauthToken);
                var p25 = new SqlParameter("@WatermarkImage", (timelapse.WatermarkImage.Equals("-")? "" : timelapse.WatermarkImage));
                var p26 = new SqlParameter("@WatermarkPosition", timelapse.WatermarkPosition);

                var list = new[] { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21, p22, p23, p24, p25, p26};
                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.AddRange(list);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                int result = (int)cmd.ExecuteScalar();
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao Insert(Timelapse timelapse) " + ex.Message);
                return 0;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static int GetSnapsCount(int id)
        {
            try
            {
                const string sql = "Select SnapsCount FROM Timelapses WHERE Id=@Id AND IsDeleted=0";
                var p1 = new SqlParameter("@Id", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;
                
                Connection.OpenConnection();
                int count = (int)cmd.ExecuteScalar();
                Connection.CloseConnection();
                return count;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetSnapsCount(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetSnapsCount(int id) Id={0}<br />{1}", id, ex.Message));
                return 0;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static int GetSnapsCount(string code)
        {
            try
            {
                const string sql = "Select SnapsCount FROM Timelapses WHERE Code=@Code AND IsDeleted=0";
                var p1 = new SqlParameter("@Code", code);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;

                Connection.OpenConnection();
                int count = (int)cmd.ExecuteScalar();
                Connection.CloseConnection();
                return count;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetSnapsCount(int code) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetSnapsCount(int code) Code={0}<br />{1}", code, ex.Message));
                return 0;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static double GetFileSize(int id)
        {
            try
            {
                const string sql = "Select FileSize FROM Timelapses WHERE Id=@Id AND IsDeleted=0";
                var p1 = new SqlParameter("@Id", id);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;

                Connection.OpenConnection();
                double size = (double)cmd.ExecuteScalar();
                Connection.CloseConnection();
                return size;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetFileSize(int id) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetFileSize(int id) Id={0}<br />{1}", id, ex.Message));
                return 0;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static double GetFileSize(string code)
        {
            try
            {
                const string sql = "Select FileSize FROM Timelapses WHERE Code=@Code AND IsDeleted=0";
                var p1 = new SqlParameter("@Code", code);
                var cmd = new SqlCommand { CommandText = sql, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Connection = Connection.DbConnection;

                Connection.OpenConnection();
                double size = (double)cmd.ExecuteScalar();
                Connection.CloseConnection();
                return size;
            }
            catch (Exception ex)
            {
                if (Connection.DbConnection.State == ConnectionState.Closed)
                    Utils.FileLog("TimelapseDao GetFileSize(string code) " + ex.Message);
                else
                    Utils.FileLog(string.Format("TimelapseDao GetFileSize(string code) Id={0}<br />{1}", code, ex.Message));
                return 0;
            }
            finally
            {
                Connection.CloseConnection();
            }
        }

        public static bool Update(Timelapse timelapse)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [OauthToken]=@OauthToken,[Title]=@Title, [Privacy]=@Privacy, [SnapsInterval]=@SnapsInterval, [FromDT]=@FromDT, [ToDT]=@ToDT,[DateAlways]=@DateAlways,[TimeAlways]=@TimeAlways,[TzId]=@TzId,[Timezone]=@Timezone, [EnableMD]=@EnableMD, [MDThreshold]=@MDThreshold, [ExcludeDark]=@ExcludeDark, [DarkThreshold]=@DarkThreshold, [IsRecording]=@IsRecording, [FPS]=@FPS, [WatermarkImage]=@WatermarkImage, [WatermarkPosition]=@WatermarkPosition " +
                           "WHERE (Code = '" + timelapse.Code + "')";
            try
            {
                var p3 = new SqlParameter("@OauthToken", timelapse.OauthToken);
                var p4 = new SqlParameter("@Title", timelapse.Title);
                var p6 = new SqlParameter("@Privacy", timelapse.Privacy);
                var p7 = new SqlParameter("@FromDT", (timelapse.FromDT == null) ? Utils.SQLMinDate : timelapse.FromDT);
                var p8 = new SqlParameter("@ToDT", (timelapse.ToDT == null) ? Utils.SQLMinDate : timelapse.ToDT);
                var p9 = new SqlParameter("@SnapsInterval", timelapse.SnapsInterval);
                var p10 = new SqlParameter("@EnableMD", timelapse.EnableMD);
                var p11 = new SqlParameter("@MDThreshold", timelapse.MDThreshold);
                var p12 = new SqlParameter("@ExcludeDark", timelapse.ExcludeDark);
                var p13 = new SqlParameter("@DarkThreshold", timelapse.DarkThreshold);
                var p14 = new SqlParameter("@IsRecording", timelapse.IsRecording);
                var p15 = new SqlParameter("@DateAlways", timelapse.DateAlways);
                var p16 = new SqlParameter("@TimeAlways", timelapse.TimeAlways);
                var p17 = new SqlParameter("@Timezone", timelapse.TimeZone);
                var p18 = new SqlParameter("@TzId", timelapse.TzId);
                var p19 = new SqlParameter("@FPS", timelapse.FPS);
                var p20 = new SqlParameter("@WatermarkImage", (timelapse.WatermarkImage.Equals("-") ? "" : timelapse.WatermarkImage));
                var p21 = new SqlParameter("@WatermarkPosition", timelapse.WatermarkPosition);

                var list = new[] { p3, p4, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16, p17, p18, p19, p20, p21 };
                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.AddRange(list);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = (cmd.ExecuteNonQuery() > 0);
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao Update(Timelapse timelapse) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool UpdateStatus(string code, TimelapseStatus status, string tag, string timezone)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [Status]=@Status, [StatusTag]=@StatusTag, [ModifiedDT]=@ModifiedDT " +
                           "WHERE (Code = '" + code + "')";
            try
            {
                var p1 = new SqlParameter("@Status", (int)status);
                var p2 = new SqlParameter("@StatusTag", tag);
                var p3 = new SqlParameter("@ModifiedDT", Utils.ConvertFromUtc(DateTime.UtcNow, timezone));

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = (cmd.ExecuteNonQuery() > 0);
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao UpdateStatus(string code, TimelapseStatus status, string tag) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool UpdateWatermark(string code, string watermark, WatermarkPosition position, string timezone)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [WatermarkImage]=@WatermarkImage, [WatermarkPosition]=@WatermarkPosition, [ModifiedDT]=@ModifiedDT " +
                           "WHERE (Code = '" + code + "')";
            try
            {
                var p1 = new SqlParameter("@WatermarkImage", watermark);
                var p2 = new SqlParameter("@WatermarkPosition", (int)position);
                var p3 = new SqlParameter("@ModifiedDT", Utils.ConvertFromUtc(DateTime.UtcNow, timezone));

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                cmd.Parameters.Add(p2);
                cmd.Parameters.Add(p3);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = (cmd.ExecuteNonQuery() > 0);
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao UpdateWatermark(string code, string watermark, WatermarkPosition position) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static long UpdateSnapsCount(string code, long snaps)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [SnapsCount] = [SnapsCount] + (@Snaps) " +
                           "OUTPUT INSERTED.SnapsCount " +
                           "WHERE (Code = '" + code + "')";
            if (snaps > 1)
                query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [SnapsCount] = @Snaps " +
                           "OUTPUT INSERTED.SnapsCount " +
                           "WHERE (Code = '" + code + "')";

            try
            {
                var p1 = new SqlParameter("@Snaps", (int)snaps);

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                long result = (long)cmd.ExecuteScalar();
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao UpdateSnapsCount(string code, int snaps) " + ex.Message);
                return 0;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static long UpdateFileSize(string code, long size)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [FileSize] = @FileSize " +
                           "OUTPUT INSERTED.FileSize " +
                           "WHERE (Code = '" + code + "')";

            try
            {
                var p1 = new SqlParameter("@FileSize", (double)size);

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                long result = (long)cmd.ExecuteScalar();
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao long UpdateFileSize(string code, double size) " + ex.Message);
                return 0;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool UpdateLastSnapshot(string code, DateTime lastSnapDate)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [LastSnapDT] = @LastSnapDT " +
                           "WHERE (Code = '" + code + "')";

            try
            {
                var p1 = new SqlParameter("@LastSnapDT", lastSnapDate);

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = cmd.ExecuteNonQuery() > 0 ? true : false;
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao bool UpdateLastSnapshot(string code, DateTime lastSnapDate) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool UpdateFileInfo(string code, TimelapseVideoInfo info)
        {
            if (info.Duration == "" && info.FileSize == 0 && info.Resolution == "" && info.SnapsCount == 0)
                return false;   // empty info posted

            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [SnapsCount] = @SnapsCount, [FileSize] = @FileSize, [Resolution] = @Resolution, [Duration] = @Duration " +
                           "WHERE (Code = '" + code + "')";
            try
            {
                var p1 = new SqlParameter("@SnapsCount", info.SnapsCount);
                var p2 = new SqlParameter("@FileSize", info.FileSize);
                var p3 = new SqlParameter("@Resolution", info.Resolution);
                var p4 = new SqlParameter("@Duration", info.Duration);
                //var p5 = new SqlParameter("@LastSnapDT", info.SnapshotDate);
                var list = new[] { p1, p2, p3, p4 };
                
                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.AddRange(list);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                int result = cmd.ExecuteNonQuery();
                Connection.CloseConnection();
                cmd.Dispose();
                return result > 0;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao long UpdateFileInfo(string code, TimelapseVideoInfo info) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static void UpdateUserToken(string user, string access_token)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [OauthToken] = (@Token) " +
                           "WHERE (UserId = '" + user + "')";
            try
            {
                var p1 = new SqlParameter("@OauthToken", access_token);

                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                cmd.Parameters.Add(p1);
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                long result = (long)cmd.ExecuteScalar();
                Connection.CloseConnection();
                cmd.Dispose();
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao UpdateUserToken(string user, string access_token) " + ex.Message);
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool Delete(int id)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [IsDeleted] = 1 " +
                           "WHERE (Id = " + id + ")";
            try
            {
                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = (cmd.ExecuteNonQuery() > 0);
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao Delete(int id) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        public static bool Delete(string code)
        {
            string query = @"UPDATE [dbo].[Timelapses] " +
                           "SET [IsDeleted] = 1 " +
                           "WHERE (Code = '" + code + "')";
            try
            {
                var cmd = new SqlCommand { CommandText = query, CommandType = CommandType.Text };
                Connection.OpenConnection();
                cmd.Connection = Connection.DbConnection;
                bool result = (cmd.ExecuteNonQuery() > 0);
                Connection.CloseConnection();
                cmd.Dispose();
                return result;
            }
            catch (Exception ex)
            {
                Utils.FileLog("TimelapseDao Delete(string code) " + ex.Message);
                return false;
            }
            finally
            { Connection.CloseConnection(); }
        }

        private static List<Timelapse> GetListFromDataReader(SqlDataReader dr)
        {
            List<Timelapse> timelapses = new List<Timelapse>();
            while (dr.Read())
            {
                var timelapse = new Timelapse();
                if (!dr.IsDBNull(dr.GetOrdinal("Id")))
                    timelapse.ID = dr.GetInt32(dr.GetOrdinal("Id"));
                
                if (!dr.IsDBNull(dr.GetOrdinal("UserId")))
                    timelapse.UserId = dr["UserId"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("CameraId")))
                    timelapse.CameraId = dr["CameraId"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("OauthToken")))
                    timelapse.OauthToken = dr["OauthToken"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("StatusTag")))
                    timelapse.StatusTag = dr["StatusTag"].ToString();

                if (!dr.IsDBNull(dr.GetOrdinal("Code")))
                    timelapse.Code = dr["Code"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("Title")))
                    timelapse.Title = dr["Title"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("Status")))
                    timelapse.Status = dr.GetInt32(dr.GetOrdinal("Status"));
                if (!dr.IsDBNull(dr.GetOrdinal("Privacy")))
                    timelapse.Privacy = dr.GetInt32(dr.GetOrdinal("Privacy"));
                if (!dr.IsDBNull(dr.GetOrdinal("FromDT")))
                    timelapse.FromDT = dr.GetDateTime(dr.GetOrdinal("FromDT"));
                if (!dr.IsDBNull(dr.GetOrdinal("ToDT")))
                    timelapse.ToDT = dr.GetDateTime(dr.GetOrdinal("ToDT"));
                if (!dr.IsDBNull(dr.GetOrdinal("SnapsInterval")))
                    timelapse.SnapsInterval = dr.GetInt32(dr.GetOrdinal("SnapsInterval"));
                if (!dr.IsDBNull(dr.GetOrdinal("SnapsCount")))
                    timelapse.SnapsCount = dr.GetInt32(dr.GetOrdinal("SnapsCount"));
                if (!dr.IsDBNull(dr.GetOrdinal("FileSize")))
                    timelapse.FileSize = dr.GetInt64(dr.GetOrdinal("FileSize"));
                if (!dr.IsDBNull(dr.GetOrdinal("Duration")))
                    timelapse.Duration = dr["Duration"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("Resolution")))
                    timelapse.Resolution = dr["Resolution"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("MaxResolution")))
                    timelapse.MaxResolution = dr.GetBoolean(dr.GetOrdinal("MaxResolution"));
                if (!dr.IsDBNull(dr.GetOrdinal("ServerIP")))
                    timelapse.ServerIP = dr["ServerIP"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("TzId")))
                    timelapse.TzId = dr["TzId"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("TimeZone")))
                    timelapse.TimeZone = dr["TimeZone"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("EnableMD")))
                    timelapse.EnableMD = dr.GetBoolean(dr.GetOrdinal("EnableMD"));
                if (!dr.IsDBNull(dr.GetOrdinal("MDThreshold")))
                    timelapse.MDThreshold = dr.GetInt32(dr.GetOrdinal("MDThreshold"));
                if (!dr.IsDBNull(dr.GetOrdinal("ExcludeDark")))
                    timelapse.ExcludeDark = dr.GetBoolean(dr.GetOrdinal("ExcludeDark"));
                if (!dr.IsDBNull(dr.GetOrdinal("DarkThreshold")))
                    timelapse.DarkThreshold = dr.GetInt32(dr.GetOrdinal("DarkThreshold"));
                if (!dr.IsDBNull(dr.GetOrdinal("DateAlways")))
                    timelapse.DateAlways = dr.GetBoolean(dr.GetOrdinal("DateAlways"));
                if (!dr.IsDBNull(dr.GetOrdinal("TimeAlways")))
                    timelapse.TimeAlways = dr.GetBoolean(dr.GetOrdinal("TimeAlways"));
                if (!dr.IsDBNull(dr.GetOrdinal("FPS")))
                    timelapse.FPS = dr.GetInt32(dr.GetOrdinal("FPS"));
                if (!dr.IsDBNull(dr.GetOrdinal("WatermarkImage")))
                    timelapse.WatermarkImage = dr["WatermarkImage"].ToString();
                if (!dr.IsDBNull(dr.GetOrdinal("WatermarkPosition")))
                    timelapse.WatermarkPosition = int.Parse(dr["WatermarkPosition"].ToString());
                if (!dr.IsDBNull(dr.GetOrdinal("IsRecording")))
                    timelapse.IsRecording = dr.GetBoolean(dr.GetOrdinal("IsRecording"));
                if (!dr.IsDBNull(dr.GetOrdinal("IsDeleted")))
                    timelapse.IsDeleted = dr.GetBoolean(dr.GetOrdinal("IsDeleted"));
                if (!dr.IsDBNull(dr.GetOrdinal("LastSnapDT")))
                    timelapse.LastSnapDT = dr.GetDateTime(dr.GetOrdinal("LastSnapDT"));
                if (!dr.IsDBNull(dr.GetOrdinal("ModifiedDT")))
                    timelapse.ModifiedDT = dr.GetDateTime(dr.GetOrdinal("ModifiedDT"));
                if (!dr.IsDBNull(dr.GetOrdinal("CreatedDT")))
                    timelapse.CreatedDT = dr.GetDateTime(dr.GetOrdinal("CreatedDT"));

                timelapses.Add(timelapse);
            }
            dr.Close();
            dr.Dispose();
            return timelapses;
        }
    }
}
