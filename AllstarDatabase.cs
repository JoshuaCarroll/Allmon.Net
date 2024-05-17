using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AllmonNet
{
    public static class AllstarDatabase
    {
        public static Dictionary<string, AllstarDatabaseRecord> Records = new Dictionary<string, AllstarDatabaseRecord>();

        public static void Update()
        {
            string rawDb = string.Empty;
            bool updateOnline = false;
            
            if (File.Exists(AppConfig.Get("Allmon:publicNodesList")))
            {
                DateTime dtLastDbUpdate = File.GetLastWriteTimeUtc(AppConfig.Get("Allmon:publicNodesList"));
                if (DateTime.Now.Subtract(dtLastDbUpdate).TotalDays > 1)
                {
                    rawDb = File.ReadAllText(AppConfig.Get("Allmon:publicNodesList"));
                }
                else
                {
                    updateOnline = true;
                }
            }
            else
            {
                updateOnline = true;
            }

            if (updateOnline)
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(60);
                client.BaseAddress = new Uri(AppConfig.Get("Allmon:nodeListUrl"));
                HttpResponseMessage response = client.GetAsync("/").Result;

                if (response.IsSuccessStatusCode)
                {
                    rawDb = response.Content.ReadAsStringAsync().Result;

                    // Save it locally.
                    File.WriteAllText(AppConfig.Get("Allmon:publicNodesList"), rawDb);
                }
            }

            Parse(rawDb);
        }

        public static void Parse(string rawFlatFile)
        {
            string[] lines = rawFlatFile.Split(Environment.NewLine.ToCharArray());
            foreach (string line in lines)
            {
                if (line.Trim() != string.Empty && !line.StartsWith(';'))
                {
                    AllstarDatabaseRecord record = new AllstarDatabaseRecord(line);
                    if (!Records.TryAdd(record.NodeNumber, record))
                    {
                        Debug.WriteLine($"* Unable to add node: {line}");
                    }
                }
            }
        }
    }

    public class AllstarDatabaseRecord
    {
        public string NodeNumber;
        public string Callsign;
        public string Frequency;
        public string Location;

        public AllstarDatabaseRecord(string nodeNumber, string callsign, string frequency, string location)
        {
            NodeNumber = nodeNumber;
            Callsign = callsign;
            Frequency = frequency;
            Location = location;
        }

        public AllstarDatabaseRecord(string individualLine)
        {
            string[] arrLine = individualLine.Split('|', 4);
            if (arrLine.Length == 4)
            {
                NodeNumber = arrLine[0];
                Callsign = arrLine[1];
                Frequency = arrLine[2];
                Location = arrLine[3];
            }
            else
            {
                NodeNumber = "";
                Callsign = "";
                Frequency = "";
                Location = "";
            }
        }

        public AllstarDatabaseRecord()
        {
            NodeNumber = "";
            Callsign = "";
            Frequency = "";
            Location = "";
        }
    }
}