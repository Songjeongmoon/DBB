using DBB.Repo;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBB.Cls
{
    class BackUp
    {
        private string DBBConnectionString = ConfigurationManager.ConnectionStrings["DBBConnection"].ConnectionString;
        private string HACSConnectionString = ConfigurationManager.ConnectionStrings["HACSConnection"].ConnectionString;
        public Dictionary<int, List<TR_DataLog>> GQData = new Dictionary<int, List<TR_DataLog>>();
        private List<TR_DataLog> tr_DataLog = new List<TR_DataLog>();
        int dataCnt = 0;

        public int LoadDataLog(Dictionary<string, string> loadTime)
        {
            using (SqlConnection connection = new SqlConnection(HACSConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"USE {loadTime["db"]};" +
                        $" WITH RankedResults AS (" +
                        $" SELECT DATEADD(SECOND, 1, L.LogDT) AS LogDT, L.Data, L.AmrId, L.LogNo, L.Cmd, L.TRMode, L.Mode, L.MissionId, L.JobId, L.Succ," +
                        $" ROW_NUMBER() OVER (PARTITION BY L.AmrId, CONVERT(VARCHAR, L.LogDT, 120) ORDER BY L.LogDT DESC) AS RowNum" +
                        $" FROM TR_DataLog AS L JOIN TD_AMR AS O ON L.AmrId = O.AmrId" +
                        $" WHERE L.Cmd = 'GQ' AND L.LogDT BETWEEN '{loadTime["startDay"]} {loadTime["startHour"]}:{loadTime["startMin"]}:00' AND" +
                        $" '{loadTime["startDay"]} {loadTime["lastHour"]}:{loadTime["lastMin"]}:00')" +
                        $" SELECT R.LogNo, FORMAT(R.LogDT, 'yyyy-MM-dd HH:mm:ss') AS LogDT, R.AmrId, R.Cmd, R.TRMode, R.Mode," +
                        $" R.MissionId, R.JobId, R.Succ, R.Data" +
                        $" FROM RankedResults R" +
                        $" WHERE R.RowNum = 1 ORDER BY LogDT;";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TR_DataLog data = new TR_DataLog();
                                
                                data.LogNo = Convert.ToInt32(reader[0]);
                                data.LogDT = Convert.ToDateTime(reader[1]);
                                data.AmrId = Convert.ToInt32(reader[2]);
                                data.Cmd = Convert.ToString(reader[3]);
                                data.TRMode = Convert.ToInt32(reader[4]);
                                data.Mode = Convert.ToInt32(reader[5]);
                                data.MissionId = Convert.ToInt32(reader[6]);
                                data.JobId = Convert.ToInt32(reader[7]);
                                data.Succ = Convert.ToByte(reader[8]);
                                data.Data = Convert.ToString(reader[9]);
                                tr_DataLog.Add(data);
                            }
                        }
                    }
                    GQData.Add(dataCnt, new List<TR_DataLog>(tr_DataLog));
                    tr_DataLog = new List<TR_DataLog>();
                    return dataCnt++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"연결 오류: {ex.Message}");
                    return -1;
                }
            }
        }

        public int saveDataLog(int dataIdx)
        {
            string query = "";
            int result = 0;
            int cnt = 0;
             List<TR_DataLog> dataLogs = GQData[dataIdx];
            using (MySqlConnection connection = new MySqlConnection(DBBConnectionString))
            {
                try
                {
                    connection.Open();
                    for(int i = 0; i < dataLogs.Count; i++)
                    {
                        query = $"INSERT INTO TR_DataLog (LogNo, LogDT, AmrId, Cmd, TRMode, Mode, MissionId, JobId, Succ, Data) VALUES({dataLogs[i].LogNo}, '{dataLogs[i].LogDT.ToString("yyyy-MM-dd HH:mm:ss")}', {dataLogs[i].AmrId}, '{dataLogs[i].Cmd}', {dataLogs[i].TRMode}," +
                                $"{dataLogs[i].Mode}, {dataLogs[i].MissionId}, {dataLogs[i].JobId}, {dataLogs[i].Succ}, '{dataLogs[i].Data}')" +
                                $" ON DUPLICATE KEY UPDATE LogDT = '{dataLogs[i].LogDT.ToString("yyyy-MM-dd HH:mm:ss")}', AmrId = {dataLogs[i].AmrId}, Cmd = '{dataLogs[i].Cmd}', TRMode = {dataLogs[i].TRMode}," +
                                $"Mode = {dataLogs[i].Mode}, MissionId = {dataLogs[i].MissionId}, JobId = {dataLogs[i].JobId}, Succ = {dataLogs[i].Succ}, Data = '{dataLogs[i].Data}';";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            result = cmd.ExecuteNonQuery();
                            cnt += result == 1 ? 0 : 1;
                            result = 0;
                        }
                    }
                    return cnt;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.GetType().Name);
                    return -1;
                }
            }
        }
    }
}
