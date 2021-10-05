using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using DnsClient;

namespace MXApp
{
    class Program
    {
        static async Task Main()
        {
            ClearFileReport();
            string option;
            do
            {
                ShowMenu();
                Console.Write("> ");
                option = Console.ReadLine();
                await MenuInteractionProcess(option);
            }
            while (option != "quit");
        }

        static void ShowMenu()
        {
            Console.WriteLine(
                "Type \"1\" to enter domain/domains names\n" +
                "Type \"2\" to read domains list from file\n" +
                "Type \"quit\" to quit application."
                );
        }

        static async Task MenuInteractionProcess(string choosenOption)
        {
            switch (choosenOption)
            {
                case "1":
                    await ResolveDomains(false);
                    break;
                case "2":
                    await ResolveDomains(true);
                    break;
                default:
                    Console.WriteLine("\nOption " + choosenOption + " doesn't exist\n");
                    break;
            }
        }

        static async Task Resolver(string[] domains, string ip)
        {
            IPEndPoint endPoint;
            LookupClient client;
            List<IDnsQueryResponse> MXresults = new List<IDnsQueryResponse>();
            List<string> domainsIPs = new List<string>();
            string separator = "--------------------------------------------------------------------";

            if (ip != "")
            {
                try
                {
                    endPoint = new IPEndPoint(IPAddress.Parse(ip), 53);
                }
                catch
                {
                    Console.Error.WriteLine("IP adress is incorrect\n");
                    return;
                }
                client = new LookupClient(endPoint);
            }
            else
            {
                client = new LookupClient();
            }


            for (int i = 0; i < domains.Length; i++)
            {
                if (domains[i] != "")
                {
                    domainsIPs.Add(GetIP(domains[i]));
                    IDnsQueryResponse res = await client.QueryAsync(domains[i], QueryType.MX);
                    MXresults.Add(res);
                }
            }

            for (int i = 0; i < MXresults.Count; i++)
            {
                if (domainsIPs[i] != "")
                {
                    Console.WriteLine("\nTrying resolve MX from domain of IP: " + domainsIPs[i]);
                    Console.WriteLine(separator);

                    for (int j = 0; j < MXresults[i].Answers.Count; j++)
                    {
                        Console.WriteLine(MXresults[i].Answers[j]);
                        SaveReportToFile(new string[] { MXresults[i].Answers[j].ToString() });
                    }

                    Console.WriteLine(separator);
                }
            }
        }

        static async Task ResolveDomains(bool isFile)
        {
            string[] DomainsNames;

            if (isFile)
            {
                Console.Write("Enter file path: ");
                try
                {
                    string path = Console.ReadLine();
                    string fileText = File.ReadAllText(path);
                    string ext = Path.GetExtension(path);
                    if (ext != ".txt")
                    {
                        throw new Exception();
                    }
                    DomainsNames = fileText.Split(new[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.None);
                }
                catch
                {
                    Console.Error.WriteLine("File not found or format is incorrect!");
                    return;
                }
            }
            else
            {
                Console.Write("Enter domain/domains names separated by space: ");
                string line = Console.ReadLine();
                DomainsNames = line.Split(' ');
            }
            Console.Write("Enter resolving server IP or leave it to use local server: ");
            string serverIP = Console.ReadLine();
            await Resolver(DomainsNames, serverIP);
        }
        static void SaveReportToFile(string[] reportText)
        {
            File.AppendAllLines("DnsAppReport.txt", reportText);
        }

        /// <summary>
        /// Clear file with report if exist at app start.
        /// </summary>
        static void ClearFileReport()
        {
            string fileName = "DnsAppReport.txt";

            if (File.Exists(fileName))
            {
                File.WriteAllText(fileName, string.Empty);
            }
        }

        /// <summary>
        /// Get IP adress of domain
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        static string GetIP(string host)
        {
            string ipAddress = "";

            try
            {
                IPHostEntry hostinfo = Dns.GetHostEntry(host);
                for (int i = 0; i < hostinfo.AddressList.Length; i++)
                {
                    ipAddress = hostinfo.AddressList[i].ToString();
                }

            }
            catch
            {
                Console.Error.WriteLine("\nOne or more invalid domain name.\n");
            }

            return ipAddress;
        }
    }
}
