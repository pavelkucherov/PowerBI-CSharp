﻿using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.PowerBI.Api.Beta;
using Microsoft.PowerBI.Api.Beta.Models;
using Microsoft.Rest;
using Console = System.Console;
using Microsoft.Threading;
using ApiHost.Models;
using System.IO;
using Microsoft.PowerBI.Security;
using System.Threading;
using Microsoft.Rest.Serialization;
using System.Net.Http.Headers;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Net;
using System.Collections;
using System.Collections.Generic;

namespace ProvisionSample
{
    class Program
    {
        const string version = "?api-version=2016-01-29";

        static string apiEndpointUri = ConfigurationManager.AppSettings["pbiApiEndpoint"];
        static string azureEndpointUri = ConfigurationManager.AppSettings["azureApiEndpoint"];
        static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
        static string resourceGroup = ConfigurationManager.AppSettings["resourceGroup"];
        static string workspaceCollectionName = ConfigurationManager.AppSettings["workspaceCollectionName"];
        static string username = ConfigurationManager.AppSettings["username"];
        static string password = ConfigurationManager.AppSettings["password"];
        static string clientId = ConfigurationManager.AppSettings["clientId"];
        static string thumbprint = ConfigurationManager.AppSettings["thumbprint"];
        static bool useCertificate = bool.Parse(ConfigurationManager.AppSettings["useCertificate"]);
        static string signingKey = ConfigurationManager.AppSettings["signingKey"];

        static WorkspaceCollectionKeys signingKeys = null;
        static Guid workspaceId = Guid.Empty;
        static string importId = null;

        static void Main(string[] args)
        {
            if (!string.IsNullOrWhiteSpace(signingKey))
            {
                signingKeys = new WorkspaceCollectionKeys
                {
                    Key1 = signingKey
                };
            }

            AsyncPump.Run(async delegate
            {
                await Run();
            });

            Console.ReadKey(true);
        }

        static async Task Run()
        {
            Console.ResetColor();

            try
            {
                Console.WriteLine();
                Console.WriteLine("What do you want to do?");
                Console.WriteLine("=================================================================");
                Console.WriteLine("1. Provision a new workspace collection");
                Console.WriteLine("2. Get workspace collection metadata");
                Console.WriteLine("3. Retrieve a workspace collection's API keys");
                Console.WriteLine("4. Get list of workspaces within a collection");
                Console.WriteLine("5. Provision a new workspace in an existing workspace collection");
                Console.WriteLine("6. Import PBIX Desktop file into an existing workspace");
                Console.WriteLine("7. Update connection string info for an existing dataset");
                Console.WriteLine("8. Get embed url and token for existing report");
                Console.WriteLine();

                var key = Console.ReadKey(true);

                switch (key.KeyChar)
                {
                    case '0':
                        await ProvisionWorkspaceCollections();
                        await Run();
                        break;
                    case '1':
                        if (string.IsNullOrWhiteSpace(subscriptionId))
                        {
                            Console.Write("Azure Subscription ID:");
                            subscriptionId = Console.ReadLine();
                            Console.WriteLine();
                        }
                        if (string.IsNullOrWhiteSpace(resourceGroup))
                        {
                            Console.Write("Azure Resource Group:");
                            resourceGroup = Console.ReadLine();
                            Console.WriteLine();
                        }
                        Console.Write("Workspace Collection Name:");
                        workspaceCollectionName = Console.ReadLine();
                        Console.WriteLine();

                        await CreateWorkspaceCollection(subscriptionId, resourceGroup, workspaceCollectionName);
                        signingKeys = await ListWorkspaceCollectionKeys(subscriptionId, resourceGroup, workspaceCollectionName);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Workspace collection created successfully");

                        await Run();
                        break;
                    case '2':
                        if (string.IsNullOrWhiteSpace(subscriptionId))
                        {
                            Console.Write("Azure Subscription ID:");
                            subscriptionId = Console.ReadLine();
                            Console.WriteLine();
                        }
                        if (string.IsNullOrWhiteSpace(resourceGroup))
                        {
                            Console.Write("Azure Resource Group:");
                            resourceGroup = Console.ReadLine();
                            Console.WriteLine();
                        }
                        Console.Write("Workspace Collection Name:");
                        workspaceCollectionName = Console.ReadLine();
                        Console.WriteLine();

                        var metadata = await GetWorkspaceCollectionMetadata(subscriptionId, resourceGroup, workspaceCollectionName);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(metadata);

                        await Run();
                        break;
                    case '3':
                        if (string.IsNullOrWhiteSpace(subscriptionId))
                        {
                            Console.Write("Azure Subscription ID:");
                            subscriptionId = Console.ReadLine();
                            Console.WriteLine();
                        }
                        if (string.IsNullOrWhiteSpace(resourceGroup))
                        {
                            Console.Write("Azure Resource Group:");
                            resourceGroup = Console.ReadLine();
                            Console.WriteLine();
                        }
                        Console.Write("Workspace Collection Name:");
                        workspaceCollectionName = Console.ReadLine();
                        Console.WriteLine();

                        signingKeys = await ListWorkspaceCollectionKeys(subscriptionId, resourceGroup, workspaceCollectionName);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Key1: {0}", signingKeys.Key1);
                        Console.WriteLine("===============================");
                        Console.WriteLine("Key2: {0}", signingKeys.Key2);

                        await Run();
                        break;
                    case '4':
                        Console.Write("Workspace Collection Name:");
                        workspaceCollectionName = Console.ReadLine();
                        Console.WriteLine();

                        var workspaces = await GetWorkspaces(workspaceCollectionName);

                        foreach (var instance in workspaces)
                        {
                            Console.WriteLine("Collection: {0}, ID: {1}", instance.WorkspaceCollectionName, instance.WorkspaceId);
                        }

                        await Run();
                        break;
                    case '5':
                        if (string.IsNullOrWhiteSpace(subscriptionId))
                        {
                            Console.Write("Azure Subscription ID:");
                            subscriptionId = Console.ReadLine();
                            Console.WriteLine();
                        }
                        if (string.IsNullOrWhiteSpace(resourceGroup))
                        {
                            Console.Write("Azure Resource Group:");
                            resourceGroup = Console.ReadLine();
                            Console.WriteLine();
                        }
                        if (string.IsNullOrWhiteSpace(workspaceCollectionName))
                        {
                            Console.Write("Workspace Collection Name:");
                            workspaceCollectionName = Console.ReadLine();
                            Console.WriteLine();
                        }

                        var workspace = await CreateWorkspace(workspaceCollectionName);
                        workspaceId = Guid.Parse(workspace.WorkspaceId);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Workspace ID: {0}", workspaceId);

                        await Run();
                        break;
                    case '6':
                        if (string.IsNullOrWhiteSpace(workspaceCollectionName))
                        {
                            Console.Write("Workspace Collection Name:");
                            workspaceCollectionName = Console.ReadLine();
                            Console.WriteLine();
                        }

                        if (workspaceId == Guid.Empty)
                        {
                            Console.Write("Workspace ID:");
                            workspaceId = Guid.Parse(Console.ReadLine());
                            Console.WriteLine();
                        }

                        Console.Write("Dataset Name:");
                        var datasetName = Console.ReadLine();
                        Console.WriteLine();

                        Console.Write("File Path:");
                        var filePath = Console.ReadLine();
                        Console.WriteLine();

                        var import = await ImportPbix(workspaceCollectionName, workspaceId, datasetName, filePath);
                        importId = import.Id;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Import: {0}", import.Id);

                        await Run();
                        break;
                    case '7':
                        if (string.IsNullOrWhiteSpace(workspaceCollectionName))
                        {
                            Console.Write("Workspace Collection Name:");
                            workspaceCollectionName = Console.ReadLine();
                            Console.WriteLine();
                        }

                        if (workspaceId == Guid.Empty)
                        {
                            Console.Write("Workspace ID:");
                            workspaceId = Guid.Parse(Console.ReadLine());
                            Console.WriteLine();
                        }

                        await UpdateConnection(workspaceCollectionName, workspaceId);

                        await Run();
                        break;
                    case '8':
                        if (string.IsNullOrWhiteSpace(workspaceCollectionName))
                        {
                            Console.Write("Workspace Collection Name:");
                            workspaceCollectionName = Console.ReadLine();
                            Console.WriteLine();
                        }

                        if (workspaceId == Guid.Empty)
                        {
                            Console.Write("Workspace ID:");
                            workspaceId = Guid.Parse(Console.ReadLine());
                            Console.WriteLine();
                        }

                        var report = await GetReport(workspaceCollectionName, workspaceId);
                        var embedToken = PowerBIToken.CreateReportEmbedToken(workspaceCollectionName, workspaceId, Guid.Parse(report.Id));
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Embed Url: {0}", report.EmbedUrl);
                        Console.WriteLine("Embed Token: {0}", embedToken.Generate(signingKeys.Key1));

                        await Run();
                        break;
                    default:
                        Console.WriteLine("Press any key to exit..");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Ooops, something broke: {0}", ex.Message);
                Console.WriteLine();
                await Run();
            }
        }

        static async Task ProvisionWorkspaceCollections()
        {
            var subscriptionsValue = ConfigurationManager.AppSettings["subscriptions"];
            var subscriptionIds = subscriptionsValue.Split(',');
            var counter = 1;

            var filePath = string.Concat("ProvisionWorkspaceCollections-", DateTime.UtcNow.ToFileTimeUtc(), ".log");
            using (var writer = File.CreateText(filePath))
            {
                foreach (var subId in subscriptionIds)
                {
                    var workspaceCollectionName = string.Concat("WC-", counter);
                    try
                    {
                        await CreateWorkspaceCollection(subId, "PowerBI", workspaceCollectionName);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        var message = string.Format("Successfully created workspace collection '{0}' in Subscription ID: '{1}'", workspaceCollectionName, subId);
                        Console.WriteLine(message);
                        writer.WriteLine(message);
                    }
                    catch (Exception ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        var message = string.Format("Failed creating workspace collection '{0}' in Subscription ID: '{1}', Exception: {2}", workspaceCollectionName, subId, ex.Message);
                        Console.WriteLine(message);
                        writer.WriteLine(message);
                    }
                    counter++;
                }
            }
        }

        static async Task CreateWorkspaceCollection(string subscriptionId, string resourceGroup, string workspaceCollectionName)
        {
            var url = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections/{3}{4}", azureEndpointUri, subscriptionId, resourceGroup, workspaceCollectionName, version);

            HttpClient client = new HttpClient();

            if (useCertificate)
            {
                var handler = new WebRequestHandler();
                var certificate = GetCertificate(thumbprint);
                handler.ClientCertificates.Add(certificate);
                client = new HttpClient(handler);
            }

            using (client)
            {
                var content = new StringContent(@"{
                                                ""location"": ""southcentralus"",
                                                ""tags"": {},
                                                ""sku"": {
                                                    ""name"": ""S1"",
                                                    ""tier"": ""Standard""
                                                }
                                            }", Encoding.UTF8);
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                var request = new HttpRequestMessage(HttpMethod.Put, url);
                // Set authorization header from you acquired Azure AD token
                if (!useCertificate)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetAzureAccessToken());
                }
                request.Content = content;

                var response = await client.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var message = string.Format("Status: {0}, Reason: {1}, Message: {2}", response.StatusCode, response.ReasonPhrase, responseText);
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return;
            }
        }

        static async Task<WorkspaceCollectionKeys> ListWorkspaceCollectionKeys(string subscriptionId, string resourceGroup, string workspaceCollectionName)
        {
            var url = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections/{3}/listkeys{4}", azureEndpointUri, subscriptionId, resourceGroup, workspaceCollectionName, version);

            HttpClient client = new HttpClient();

            if (useCertificate)
            {
                var handler = new WebRequestHandler();
                var certificate = GetCertificate(thumbprint);
                handler.ClientCertificates.Add(certificate);
                client = new HttpClient(handler);
            }

            using (client)
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                // Set authorization header from you acquired Azure AD token
                if (!useCertificate)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetAzureAccessToken());
                }
                request.Content = new StringContent(string.Empty);
                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var responseText = await response.Content.ReadAsStringAsync();
                    var message = string.Format("Status: {0}, Reason: {1}, Message: {2}", response.StatusCode, response.ReasonPhrase, responseText);
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return SafeJsonConvert.DeserializeObject<WorkspaceCollectionKeys>(json);
            }
        }

        static async Task<string> GetWorkspaceCollectionMetadata(string subscriptionId, string resourceGroup, string workspaceCollectionName)
        {
            var url = string.Format("{0}/subscriptions/{1}/resourceGroups/{2}/providers/Microsoft.PowerBI/workspaceCollections/{3}{4}", azureEndpointUri, subscriptionId, resourceGroup, workspaceCollectionName, version);
            HttpClient client = new HttpClient();

            if (useCertificate)
            {
                var handler = new WebRequestHandler();
                var certificate = GetCertificate(thumbprint);
                handler.ClientCertificates.Add(certificate);
                client = new HttpClient(handler);
            }

            using (client)
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                // Set authorization header from you acquired Azure AD token
                if (!useCertificate)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetAzureAccessToken());
                }
                var response = await client.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    throw new Exception(message);
                }

                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
        }

        static async Task<Workspace> CreateWorkspace(string workspaceCollectionName)
        {
            // Create a provision token required to create a new workspace within your collection
            var provisionToken = PowerBIToken.CreateProvisionToken(workspaceCollectionName);
            using (var client = await CreateClient(provisionToken))
            {
                // Create a new workspace witin the specified collection
                return await client.Workspaces.PostWorkspaceAsync(workspaceCollectionName);
            }
        }

        static async Task<IEnumerable<Workspace>> GetWorkspaces(string workspaceCollectionName)
        {
            var provisionToken = PowerBIToken.CreateProvisionToken(workspaceCollectionName);
            using (var client = await CreateClient(provisionToken))
            {
                var response = await client.Workspaces.GetWorkspacesByCollectionNameAsync(workspaceCollectionName);
                return response.Value;
            }
        }

        static async Task<Import> ImportPbix(string workspaceCollectionName, Guid workspaceId, string datasetName, string filePath)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                // Create a dev token for import
                var devToken = PowerBIToken.CreateDevToken(workspaceCollectionName, workspaceId);
                using (var client = await CreateClient(devToken))
                {

                    // Import PBIX file from the file stream
                    var import = await client.Imports.PostImportWithFileAsync(workspaceCollectionName, workspaceId.ToString(), fileStream, datasetName);

                    // Example of polling the import to check when the import has succeeded.
                    while (import.ImportState != "Succeeded" && import.ImportState != "Failed")
                    {
                        import = await client.Imports.GetImportByIdAsync(workspaceCollectionName, workspaceId.ToString(), import.Id);
                        Console.WriteLine("Checking import state... {0}", import.ImportState);
                        Thread.Sleep(1000);
                    }

                    return import;
                }
            }
        }

        static async Task UpdateConnection(string workspaceCollectionName, Guid workspaceId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.Write("Username: ");
                username = Console.ReadLine();
                Console.WriteLine();
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.Write("Password: ");
                password = Console.ReadLine();
                Console.WriteLine();
            }

            var devToken = PowerBIToken.CreateDevToken(workspaceCollectionName, workspaceId);
            using (var client = await CreateClient(devToken))
            {
                // Get the newly created dataset from the previous import process
                var datasets = await client.Datasets.GetDatasetsAsync(workspaceCollectionName, workspaceId.ToString());

                // Get the datasources from the dataset
                var datasources = await client.Datasets.GetGatewayDatasourcesAsync(workspaceCollectionName, workspaceId.ToString(), datasets.Value[datasets.Value.Count - 1].Id);

                // Reset your connection credentials
                var delta = new GatewayDatasource
                {
                    CredentialType = "Basic",
                    BasicCredentials = new BasicCredentials
                    {
                        Username = username,
                        Password = password
                    }
                };

                // Update the datasource with the specified credentials
                await client.Gateways.PatchDatasourceAsync(workspaceCollectionName, workspaceId.ToString(), datasources.Value[datasources.Value.Count - 1].GatewayId, datasources.Value[datasources.Value.Count - 1].Id, delta);
            }
        }

        static async Task<Report> GetReport(string workspaceCollectionName, Guid workspaceId)
        {
            // Create a dev token to access the reports within your workspace
            var devToken = PowerBIToken.CreateDevToken(workspaceCollectionName, workspaceId);
            using (var client = await CreateClient(devToken))
            {
                var reports = await client.Reports.GetReportsAsync(workspaceCollectionName, workspaceId.ToString());
                return reports.Value[reports.Value.Count - 1];
            }
        }

        static async Task<IPowerBIClient> CreateClient(PowerBIToken token)
        {
            if (signingKeys == null)
            {
                Console.Write("Signing Key: ");
                signingKey = Console.ReadLine();
                Console.WriteLine();

                signingKeys = new WorkspaceCollectionKeys()
                {
                    Key1 = signingKey
                };
            }

            if (signingKeys == null)
            {
                signingKeys = await ListWorkspaceCollectionKeys(subscriptionId, resourceGroup, workspaceCollectionName);
            }

            // Generate a JWT token used when accessing the REST APIs
            var jwt = token.Generate(signingKeys.Key1);

            // Create a token credentials with "AppToken" type
            var credentials = new TokenCredentials(jwt, "AppToken");

            // Instantiate your Power BI client passing in the required credentials
            var client = new PowerBIClient(credentials);

            // Override the api endpoint base URL.  Default value is https://api.powerbi.com
            client.BaseUri = new Uri(apiEndpointUri);

            return client;
        }

        static string GetAzureAccessToken()
        {
            return "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSIsImtpZCI6Ik1uQ19WWmNBVGZNNXBPWWlKSE1iYTlnb0VLWSJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuY29yZS53aW5kb3dzLm5ldC8iLCJpc3MiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC83MmY5ODhiZi04NmYxLTQxYWYtOTFhYi0yZDdjZDAxMWRiNDcvIiwiaWF0IjoxNDU4MDcwNTM5LCJuYmYiOjE0NTgwNzA1MzksImV4cCI6MTQ1ODA3NDQzOSwiX2NsYWltX25hbWVzIjp7Imdyb3VwcyI6InNyYzEifSwiX2NsYWltX3NvdXJjZXMiOnsic3JjMSI6eyJlbmRwb2ludCI6Imh0dHBzOi8vZ3JhcGgud2luZG93cy5uZXQvNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3L3VzZXJzL2M1NDlmMWY2LTZlOWMtNGNkYi1iZTM4LWZlYjczMjNhNjcyMS9nZXRNZW1iZXJPYmplY3RzIn19LCJhY3IiOiIxIiwiYW1yIjpbInB3ZCIsIm1mYSJdLCJhcHBpZCI6ImM0NGI0MDgzLTNiYjAtNDljMS1iNDdkLTk3NGU1M2NiZGYzYyIsImFwcGlkYWNyIjoiMiIsImZhbWlseV9uYW1lIjoiQnJlemEiLCJnaXZlbl9uYW1lIjoiV2FsbGFjZSIsImluX2NvcnAiOiJ0cnVlIiwiaXBhZGRyIjoiMTA0LjQzLjI1NC4xNjQiLCJuYW1lIjoiV2FsbGFjZSBCcmV6YSIsIm9pZCI6ImM1NDlmMWY2LTZlOWMtNGNkYi1iZTM4LWZlYjczMjNhNjcyMSIsIm9ucHJlbV9zaWQiOiJTLTEtNS0yMS0yMTI3NTIxMTg0LTE2MDQwMTI5MjAtMTg4NzkyNzUyNy02NzQ0NjUyIiwicHVpZCI6IjEwMDMzRkZGODAyRTM2MkUiLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiJOM3RjODBMYlZSck0yZ3ZuS0dGZE1LX1NHSXhuLW5YRDRFUlU5NjJyNnhBIiwidGlkIjoiNzJmOTg4YmYtODZmMS00MWFmLTkxYWItMmQ3Y2QwMTFkYjQ3IiwidW5pcXVlX25hbWUiOiJ3YWJyZXphQG1pY3Jvc29mdC5jb20iLCJ1cG4iOiJ3YWJyZXphQG1pY3Jvc29mdC5jb20iLCJ2ZXIiOiIxLjAifQ.J4lbI9QUJ0WjIWJnSPu5VTGmGSmcphQ0pcFxWeOTFmoI3DBaphPl_uawttvFiidgSZfkQkJgs2A1shE4ScEhDEASKg6LQQzkdbz1Vo-vwCBNNtKNHoWsg2ToMXPi3Z1nNAYFpeuE7BOWQ5d5eTDqsMpizyag6Ai6hitaKBfBYb9aVpbk_cunXIUN14z-gpduxBZ4MucQ5uyrxk8-dmmpcDwunAyq2k2vHV4bdrsNUpgrtAydcaZ0vRMqaKNdwk6qYI6z90XGi9Z1Czm_09F8H0VB9SeNKpaCwh7y4DeVbUDFMjyN_y6Ex3EuyjSYleavnE5V85CumXlWvbLLscXW3Q";
        }

        static X509Certificate2 GetCertificate(string thumbprint)
        {
            X509Certificate2 cert = null;

            var certStore = new X509Store(StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly);

            var certificates = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (certificates.Count > 0)
            {
                cert = certificates[0];
            }
            else
            {
                throw new CryptographicException("Cannot find the certificate with the thumbprint: {0}", thumbprint);
            }

            certStore.Close();

            return cert;
        }
    }
}