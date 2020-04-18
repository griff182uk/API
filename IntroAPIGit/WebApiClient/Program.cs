using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
//using System.Text.Json;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Azure.Storage;
using Azure;

namespace WebAPIClient
{
    class Program
    {    
        




        private static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {


            await ProcessRepositories();
        }

        private static async Task ProcessRepositories()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            // var stringTask = client.GetStringAsync("https://api.github.com/orgs/dotnet/repos");

            var streamTask = client.GetStreamAsync("https://api.github.com/orgs/dotnet/repos");
            var repositories = await System.Text.Json.JsonSerializer.DeserializeAsync<List<Repository>>(await streamTask);

            //  var msg = await stringTask;
            // Console.Write(msg);

            // foreach (var repo in repositories)
            //  Console.WriteLine(repo.Name);

           // string jsonString;

           
                    string accountName = "griffapitest";
            string accountKey = "IB8IBJlSpcOveCC1KsiUEKsKn58FhIwvQ8RjPS47H7egROPKqsVcA+S/0Y120c5DiexZyJ7YFLzFt5tQl+jrjg==";
       
      StorageSharedKeyCredential sharedKeyCredential =
        new StorageSharedKeyCredential(accountName, accountKey);
        
               string dfsUri = "https://" + accountName + ".dfs.core.windows.net";

            DataLakeServiceClient dataLakeServiceClient = new DataLakeServiceClient
                (new Uri(dfsUri), sharedKeyCredential);

            DataLakeFileSystemClient fileSystemClient = dataLakeServiceClient.GetFileSystemClient("lake");
           // DataLakeDirectoryClient directoryClient = fileSystemClient.GetDirectoryClient("t2");



            foreach (var repo in repositories)
            {
                Console.WriteLine(repo.Name);
                Console.WriteLine(repo.Description);
                Console.WriteLine(repo.GitHubHomeUrl);
                Console.WriteLine(repo.Homepage);
                Console.WriteLine(repo.Watchers);
                Console.WriteLine(repo.LastPush);
                Console.WriteLine();

/*
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                };
                jsonString = System.Text.Json.JsonSerializer.Serialize(repo, options);

                       Console.WriteLine(jsonString);
                
                                using (StreamWriter file = File.CreateText(@"E:\Downloads\" + repo.Name+".json"))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, jsonString);
                }
                */
            }
                using (StreamWriter file = File.CreateText(@".\DotNetGitHub.json"))
                {
                    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, repositories);
                }

                DataLakeDirectoryClient directoryClient =
                    fileSystemClient.GetDirectoryClient("thisplace");

                DataLakeFileClient fileClient = await directoryClient.CreateFileAsync("DotNetGitHub-file.json");

                FileStream fileStream = 
                    File.OpenRead(@".\DotNetGitHub.json");

                long fileSize = fileStream.Length;

                await fileClient.AppendAsync(fileStream, offset: 0);

                await fileClient.FlushAsync(position: fileSize);


         //use the code below to upload a file
        //    DataLakeFileClient fileClient = directoryClient.CreateFile("22.txt");
            //FileStream fileStream = File.OpenRead("d:\\foo2.txt");

          //  long fileSize = fileStream.Length;
           // fileClient.Append(fileStream, offset: 0);
            //fileClient.Flush(position: fileSize);
        }

         public async Task UploadFile(DataLakeFileSystemClient fileSystemClient)
            {
                DataLakeDirectoryClient directoryClient =
                    fileSystemClient.GetDirectoryClient("thisplace");

                DataLakeFileClient fileClient = await directoryClient.CreateFileAsync("DotNetGitHub-file.json");

                FileStream fileStream = 
                    File.OpenRead(@".\DotNetGitHub.json");

                long fileSize = fileStream.Length;

                await fileClient.AppendAsync(fileStream, offset: 0);

                await fileClient.FlushAsync(position: fileSize);

            }

/*
            public void GetDataLakeServiceClient(ref DataLakeServiceClient dataLakeServiceClient,
                    string accountName, string accountKey)
                {
                    StorageSharedKeyCredential sharedKeyCredential =
                        new StorageSharedKeyCredential(accountName, accountKey);

                    string dfsUri = "https://" + accountName + ".dfs.core.windows.net";

                    dataLakeServiceClient = new DataLakeServiceClient
                        (new Uri(dfsUri), sharedKeyCredential);
                }
                */
    }
}
