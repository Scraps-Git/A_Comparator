using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data;
using System.Data.Sql;

namespace AzureComparator
{
    class Program
    {
        const string ACCOUNT_NAME = "khovexternalstorage";
        const string ACCOUNT_KEY = "g0AcY4nI3vamhq/s97UKd7KfqJqFY/C6TG9SjiUeRGQ0okaQwRyg29jmkftq31W1CEBN2czeIcS6ModqPYXvFQ==";
        const string AZURE_CONNECTION_STRING = "" + "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";

        static void Main(string[] args)
        {
            string date = DateTime.Now.ToString("MM_dd");            
            string az_tdy = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage"+date+".txt";            
            string az_dels_tdy = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_dels_db"+date+".txt";
            string az_ups_tdy  = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_ups_db"+date+".txt";            
            string ste_delta = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_delta_db" + date + ".txt";
            string ste_dels = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_dels_db" + date + ".txt";
            string ste_tdy = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_db"+date+".txt";
            string ste_inc_tdy = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_inc_db" + date + ".txt";
            string ste_exc_tdy = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_exc_db" + date + ".txt";
            
            var storageAccount = CloudStorageAccount.Parse(string.Format(AZURE_CONNECTION_STRING, ACCOUNT_NAME, ACCOUNT_KEY));
            var client = storageAccount.CreateCloudBlobClient();
            string indent = "";            
             
            ConsoleKeyInfo inf;

            do
            {
                Console.Write("Azure Comapator top Menu: " + '\n' + "A.)Aquire Today's Azure Storage Directory List" + '\n' + "B.)Aquire Today's Sitefinity Database Listing " + '\n' +
                    "C.)Detail Azure Storage Changes " + '\n' + "D.)Detail Sitefinity Changes " + '\n' + "E.)Compare Azure Storage to Sitefinity " + '\n' + "X.) Exit ");
               inf = Console.ReadKey();
                switch (inf.Key)
                {
                    case ConsoleKey.A://F)Update ALL!!
                        Console.WriteLine("  Preparing update Azure List");
                        Console.WriteLine("  Are you sure Y/N");
                        inf = Console.ReadKey();
                        switch (inf.Key)
                        {

                            case ConsoleKey.Y:
                                try
                                {
                                    
                                    getAzureDirectories(client.ListContainers(), az_tdy);
                                    Console.WriteLine("OK!");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Un-handled Program Failure ! " + e);
                                }
                                break;
                                case ConsoleKey.N:
                                break;
                        }
                        break;
                    case ConsoleKey.B://F)Update ALL!!
                        Console.WriteLine("  Preparing update Sitefinity List");
                        Console.WriteLine("  Are you sure Y/N");
                        inf = Console.ReadKey();
                        switch (inf.Key)
                        {

                            case ConsoleKey.Y:
                                try
                                {
                                    
                                    Console.WriteLine("OK!");
                                    getAzureDB(ste_tdy);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Un-handled Program Failure ! " + e);
                                }
                                break;
                            case ConsoleKey.N:
                                break;
                        }
                        break;
                    case ConsoleKey.C:
                        Console.WriteLine("  Preparing to Detail Azure  Changes");
                        Console.WriteLine("  Are you sure Y/N");
                        inf = Console.ReadKey();
                        switch (inf.Key)
                        {

                            case ConsoleKey.Y:
                                try
                                {
                                    CompareAzureStr(az_dels_tdy);
                                    Console.WriteLine("OK!");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Un-handled Program Failure ! " + e);
                                }
                                break;
                            case ConsoleKey.N:
                                break;
                        }
                        break;
                    case ConsoleKey.D://F)Update ALL!!
                        Console.WriteLine("  Preparing Detail DB Changes");
                        Console.WriteLine("  Are you sure Y/N");
                        inf = Console.ReadKey();
                        switch (inf.Key)
                        {

                            case ConsoleKey.Y:
                                try
                                {
                                    CompareDB(ste_dels);
                                    Console.WriteLine("OK!");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Un-handled Program Failure ! " + e);
                                }
                                break;
                            case ConsoleKey.N:
                                break;
                        }
                        break;
                    case ConsoleKey.E://F)Update ALL!!
                        Console.WriteLine("  Preparing to Compare DB to Cloud Storage");
                        Console.WriteLine("  Are you sure Y/N");
                        inf = Console.ReadKey();
                        switch (inf.Key)
                        {

                            case ConsoleKey.Y:
                                try
                                {
                                    CompareStrToDB(ste_tdy, az_tdy);
                                    Console.WriteLine("OK!");
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("Un-handled Program Failure ! " + e);
                                }
                                break;
                            case ConsoleKey.N:
                                break;
                        }
                        break;                    
                }
            } while (inf.Key != ConsoleKey.X);            
//
        }
//end main
        static SqlConnection GetKhovProd()
        {
            SqlConnection conn;

            string connStr = "Data Source=hts7j6xf2y.database.windows.net;Initial Catalog=KHAUTHPROD;User ID=khovweb;Password=Khoversm1n";
            conn = new SqlConnection(connStr);

            try
            {
                conn.Open();
                Console.WriteLine("kHovProd Connection Open ! ");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not open connection ! " + ex);
            }

            return conn;
        }
//
        static string FileLoc(string type)
        {   
            string date = "";
            string file = "";                        
            while (file == "")
            {
                Console.Write("Enter The date (MM\\dd)");
                date = Console.ReadLine();
                switch (type){
                    case "az_tdy": 
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage" + date + ".txt";                    
                        break;

                    case "az_dels_tdy":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_dels_db" + date + ".txt";
                        break;

                    case "az_ups_tdy":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_ups_db" + date + ".txt";
                        break;

                    case "ste_delta":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_delta_db" + date + ".txt";
                        break;

                    case "ste_tdy":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_db" + date + ".txt";
                        break;

                    case "ste_inc_tdy":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_inc_db" + date + ".txt";
                        break;

                    case "ste_exc_tdy":
                        file = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovprod_ste_exc_db" + date;
                        break;
                }
               
                if (!(File.Exists(file)))
                {
                    Console.WriteLine("File does not exist");
                    file = "";
                }
                else
                    return file;
            }            
        return file;
        }
//
        static void getAzureDB(string path1)
        {
            Console.WriteLine("Start ! ");
            SqlConnection cnn = new SqlConnection();
            SqlCommand azcom2 = new SqlCommand();
            SqlTransaction upTrans = null;
            SqlDataReader azdata;

            cnn = GetKhovProd();
            upTrans = cnn.BeginTransaction("Trans");

            string url = null;
            string data = null;
            int numrow = 0;
            int count = 0;
            int i = 0;
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            string sql_img = "SELECT * FROM sf_media_content where blob_storage = 'Azure External Storage'";
            string input = "0";

            List<string> Url = new List<string>();
            StreamWriter kazstr1 = new StreamWriter(path1);

            try
            {
                azcom2.Transaction = upTrans;
                azcom2.Connection = cnn;
                azcom2.CommandText = sql_img;
                azdata = azcom2.ExecuteReader();

                while (azdata.Read())
                {
                    Url.Add(azdata["file_path"].ToString());
                    input = Url[i];
                    Console.SetCursorPosition(x + 1, y + 1);
                    Console.WriteLine(input);
                    kazstr1.WriteLine(input);
                    i++;
                }

                azcom2.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Un-handled Program Failure ! " + ex);
            }

        }
//
        static void getAzureDirectories(IEnumerable<CloudBlobContainer> containers, string path)
        {
            string dir = "0";
            string name = "";
            int x = Console.CursorLeft;
            int y = Console.CursorTop;
            List<string> names = new List<string>();
            File.WriteAllText(path, "");
            StreamWriter kazstr = new StreamWriter(path);//dels
            kazstr.AutoFlush = true;
            int i = 0;
            foreach (var container in containers)
            {
                string indent = "";
                // Pass Ienumerable to recursive function to get "subdirectories":
                name = "Container: " + container.Name;
                Console.Write(new string(' ', Console.WindowWidth)); 
                Console.SetCursorPosition(x + 1, y + 1);
                Console.WriteLine(name);
                kazstr.WriteLine(name);
                dir = getContainerDirectories1(container.ListBlobs(), indent, kazstr, x,y);
                // Console.WriteLine(dir);
                if (dir != "")
                {
                    kazstr.WriteLine(dir);//names.Add(dir);
                    i++;
                }

            }
            kazstr.Close();

        }
        //
        static string getContainerDirectories1(IEnumerable<IListBlobItem> blobList, string indent, StreamWriter kazstr, int x, int y)
        {
            
            string name = "";
            StringBuilder sb = new StringBuilder("");
            // First list all the actual FILES within 
            // the current blob list. No recursion needed:
            foreach (var item in blobList.Where
            ((blobItem, type) => blobItem is CloudBlockBlob))
            {
                var blobFile = item as CloudBlockBlob;
                sb.AppendLine(indent + blobFile.Name);
            }

            // List all additional subdirectories 
            // in the current directory, and call recursively:
            foreach (var item in blobList.Where
            ((blobItem, type) => blobItem is CloudBlobDirectory))
            {

                var directory = item as CloudBlobDirectory;
                sb.AppendLine(indent + directory.Prefix.ToUpper());
                // Call this method recursively to retrieve subdirectories within the current:
                name = directory.Prefix;
                Console.Write(new string(' ', Console.WindowWidth)); 
                Console.SetCursorPosition(x + 1, y + 1);               
                Console.WriteLine(name);
                

                sb.AppendLine(getContainerDirectories1(directory.ListBlobs(), indent, kazstr,x,y));

            }
            return sb.ToString();
        }
//
        static void CompareAzureStr(string path3) // Check for missing files
        {
            int i = 3;
            string line2 = "0";
            string line = "0";
            string path1 = "";
            string path2 = "";
            do
            {
                Console.WriteLine("today?");
                 path1 = FileLoc("az_tdy");//today
                 Console.WriteLine("yesterday?");
                 path2 = FileLoc("az_tdy");
                 if (path1 == path2) {
                 Console.WriteLine("cannot be the same");
                 }

            } while (path1 == path2);//previous day
            
            
            if(!(File.Exists(path1)))
            File.CreateText(path1).Close();
            if (!(File.Exists(path3)))
            File.CreateText(path3).Close();
            

            StreamReader kazstr1 = new StreamReader(path1);//current
            StreamReader kazstr2 = new StreamReader(path2);//yesterday
            StreamWriter kazstr3 = new StreamWriter(path3);//dels
            kazstr3.AutoFlush = true;
            
            Console.WriteLine("Start");
            int sl = Console.CursorLeft;
            int st = Console.CursorTop;
            Console.WriteLine("checking...");
            int cl = Console.CursorLeft;
            int ct = Console.CursorTop;
            Console.WriteLine("found! ");
            int fl = Console.CursorLeft;
            int ft = Console.CursorTop;
            Console.WriteLine("missing! ");
            int ml = Console.CursorLeft;
            int mt = Console.CursorTop;
            while ((line2 = kazstr1.ReadLine()) != null)
            {
                if (line2 != "")
                {
                    Console.SetCursorPosition(cl + 14, ct - 1);
                    Console.Write(new string(' ', Console.WindowWidth)); 
                    Console.SetCursorPosition(cl+14, ct-1);
                    Console.Write(line2);
                    while (line != null && (line2 != line))
                    {
                        line = kazstr2.ReadLine();
                    }
                    if ((line2 == line))
                    {
                        Console.SetCursorPosition(fl + 8, ft - 1);
                        Console.Write(new string(' ', Console.WindowWidth)); 
                        Console.SetCursorPosition(fl+8, ft-1);
                        Console.Write(" " + line2);
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                    }
                    else if ((line2 != line))
                    {                        
                        Console.SetCursorPosition(ml+9, mt+i-1);
                        Console.Write(" " + line2);
                        kazstr3.WriteLine(line2);
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                        line = "0";
                        i++;
                    }
                    else
                    {
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                    }
                }
            }
            Console.SetCursorPosition(0, mt + i + 1);
            Console.Write(line2);
            Console.SetCursorPosition(0, mt + i + 2);
            Console.Write(line);
            kazstr1.Close();
            kazstr2.Close();            
            kazstr2.Close();


        }
//
        static void CompareDB(string path3) // Check for missing files
        {
            int i = 3;
            string line = "0";
            string line2 = "0";
            string path1 = "";
            string path2 = "";
            do
            {
                Console.WriteLine("today?");
                path1 = FileLoc("ste_tdy");//today
                Console.WriteLine("yesterday?");
                path2 = FileLoc("ste_tdy");
                if (path1 == path2)
                {
                    Console.WriteLine("cannot be the same");
                }

            } while (path1 == path2);//previous day


            if (!(File.Exists(path1)))
                File.CreateText(path1).Close();
            if (!(File.Exists(path3)))
                File.CreateText(path3).Close();


            StreamReader kazstr1 = new StreamReader(path1);//current
            StreamReader kazstr2 = new StreamReader(path2);//yesterday
            StreamWriter kazstr3 = new StreamWriter(path3);//dels
            kazstr3.AutoFlush = true;

            Console.WriteLine("Start");
            int sl = Console.CursorLeft;
            int st = Console.CursorTop;
            Console.WriteLine("checking...");
            int cl = Console.CursorLeft;
            int ct = Console.CursorTop;
            Console.WriteLine("found! ");
            int fl = Console.CursorLeft;
            int ft = Console.CursorTop;
            Console.WriteLine("missing! ");
            int ml = Console.CursorLeft;
            int mt = Console.CursorTop;
            while ((line = kazstr1.ReadLine()) != null)
            {
                if (line != "")
                {
                    Console.SetCursorPosition(cl + 14, ct - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(cl + 14, ct - 1);
                    Console.Write(line);
                    while (line2 != null && (line != line2))
                    {
                        line2 = kazstr2.ReadLine();
                    }
                    if ((line == line2))
                    {
                        Console.SetCursorPosition(fl + 8, ft - 1);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(fl + 8, ft - 1);
                        Console.Write(" " + line);
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                    }
                    else if ((line != line2))
                    {
                        Console.SetCursorPosition(ml + 9, mt + i - 1);
                        Console.Write(" " + line);
                        kazstr3.WriteLine(line);
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                        line2 = "0";
                        i++;
                    }
                    else
                    {
                        kazstr2.DiscardBufferedData();
                        kazstr2.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr2.BaseStream.Position = 0;
                    }
                }
            }
            Console.SetCursorPosition(0, mt + i + 1);
            Console.Write(line);
            Console.SetCursorPosition(0, mt + i + 2);
            Console.Write(line2);
            kazstr1.Close();
            kazstr2.Close();
            kazstr3
                .Close();


        }
//
        static void CompareStrToDB(string path1, string path4)
        {   
            string input = "0"; 
            Regex reg = new Regex(@"images/default-source/external/*");           
            Match match = reg.Match(input);             
            Console.WriteLine("Start ! ");
            string date = DateTime.Now.ToString("MM_dd");  
            int  i = 4;
            int j = 0;

            int x = Console.CursorLeft;
            int y = Console.CursorTop;

            string[] changes = null;
            bool hit = true;
            string line = "0";
            string line2 = "0";
            string[] redux;
            
            string path2 = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_exc_db"+date+".txt";
            string path3 = "C:\\Users\\jastaylor\\Desktop\\Logs\\khovexternalstorage_inc_db" + date + ".txt";

            if (!(File.Exists(path2)))
                File.CreateText(path2).Close();
            if (!(File.Exists(path3)))
                File.CreateText(path3).Close();
            

            StreamReader kazstr1 = new StreamReader(path1);
            StreamWriter kazstr2 = new StreamWriter(path2);
            StreamWriter kazstr3 = new StreamWriter(path3);
            StreamReader kazstr4 = new StreamReader(path4);

            kazstr2.AutoFlush = true;
            kazstr3.AutoFlush = true;

            Console.SetCursorPosition(x, y+3);
            Console.WriteLine("Start");
            Console.SetCursorPosition(x, y+4);
            Console.WriteLine("checking...");
            Console.SetCursorPosition(x, y+5);
            Console.WriteLine("found! ");
            Console.SetCursorPosition(x, y+6);
            Console.WriteLine("missing! ");
            i = y + 6;
            while ((line = kazstr1.ReadLine()) != null)
            {
                if (line != "")
                {
                    match = reg.Match(line);
                    if (!(match.Success || line.Contains("tmb-0") || char.IsUpper(line[0])||!(line.Contains("."))))
                    {                                                           
                    Console.SetCursorPosition(x+14, y+4);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.SetCursorPosition(x + 14, y + 4);
                    Console.Write(line);
                    while (line2 != null && (line != line2))
                    {

                        line2 = kazstr4.ReadLine();
                        j++;                    
                    }
                    if ((line == line2))
                    {
                        Console.SetCursorPosition(x+8, y+5);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(x + 8, y + 5);
                        Console.Write(" " + line2);
                        kazstr3.WriteLine(line); 
                        j = 0;
                        kazstr4.DiscardBufferedData();
                        kazstr4.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr4.BaseStream.Position = 0;
                    }
                    else if ((line != line2))
                    {
                        Console.SetCursorPosition(x+9, i);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(x + 9, i);
                        Console.Write(" " + line);
                        kazstr2.WriteLine(line);                    
                        line2 = "0";
                        i++;
                        j = 0;
                        kazstr4.DiscardBufferedData();
                        kazstr4.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr4.BaseStream.Position = 0;
                    }
                    else
                    {                    
                        j = 0;
                        kazstr4.DiscardBufferedData();
                        kazstr4.BaseStream.Seek(0, SeekOrigin.Begin);
                        kazstr4.BaseStream.Position = 0;
                    }
                }
            }
         }

            Console.SetCursorPosition(0, y+7);
            Console.Write(line);
            Console.SetCursorPosition(0, y+8);
            Console.Write(line2);

            Console.WriteLine("Finished !");
            }
//
        }
        //
    }
    

