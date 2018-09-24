using System;
using PI.Data.Mongo;
using PI.Data.Mongo.Interfaces;
using PI.Data.Mongo.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using PI.Utilities.Interfaces;

namespace PI.Data.Mongo.Example
{
    class Program
    {
        public const string CollectionName = "TestItems";


        static void Main(string[] args)
        {
            MainAsync().Wait();
        }

        static async Task MainAsync()
        {
            try
            {
                Console.WriteLine("Pi & Mash.");
                Console.WriteLine("Mongo Data Example");

                //create repository
                string connectionString = "mongodb://localhost:27017/pimongotest?strict=false&poolsize=100&lifetime=1";
                string database = "pimongotest";

                Console.WriteLine("Connection String: {0}", connectionString);
                Console.WriteLine("Database: {0}", database);

                IMongoRepositorySettings repositorySettings = new MongoRepositorySettings(connectionString, database);
                IMongoDbRepository mongoDbRepository = new MongoDbRepository(repositorySettings);

                Boolean quitNow = false;
                while (!quitNow)
                {

                    string command = Console.ReadLine();
                    string[] commandParts = command.Split(' ');
                    string mainCommand = commandParts[0];
                    switch (mainCommand)
                    {
                        case "help":
                            ShowHelp();
                            break;
                        case "conn":
                            connectionString = commandParts[1];
                            database = commandParts[2];
                            repositorySettings = new MongoRepositorySettings(connectionString, database);
                            mongoDbRepository = new MongoDbRepository(repositorySettings);
                            Console.WriteLine("Connection String: {0}", connectionString);
                            Console.WriteLine("Database: {0}", database);
                            break;
                        case "add":
                            string name = (commandParts.Length > 1) ? commandParts[1] : "New Item: " + DateTime.Now.ToShortDateString();
                            //add item
                            await AddItem(mongoDbRepository, name);
                            break;
                        case "list":
                            //list items
                            await List(mongoDbRepository);
                            break;
                        case "clear":
                            Console.Clear();
                            Console.WriteLine("Pi & Mash.");
                            Console.WriteLine("Mongo Data Example");
                            break;
                        case "quit":
                            quitNow = true;
                            break;
                        default:
                            break;
                    }
                    Console.WriteLine("-------------------------------");
                }
            }
            catch(Exception ex)
            {
                Console.Write("Exception: {0}", ex.Message);
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine("Help");
            Console.WriteLine("help - shows the application help");
            Console.WriteLine("conn <connection string> <database> - creates a new connection with the connection string and database sent");
            Console.WriteLine("add <name> - creates a new test item with the name sent");
            Console.WriteLine("list - lists all the test items in the database");
            Console.WriteLine("clear - clears the console screen");
            Console.WriteLine("quit - closes application");
        }

        public static async Task AddItem(IMongoDbRepository mongoDbRepository, string name)
        {
            //create promotionId filter
            TestItem item = new TestItem()
            {
                Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
                Name = name
            };
            //audit item
            AuditManager.SetAuditFields(item as IAuditFieldsWithId, "test_user");
            //get the collection and do replace one
            var collection = mongoDbRepository.Database.GetCollection<TestItem>(CollectionName);
            var result = await collection.ReplaceOneAsync(x => x.Id == item.Id, item, new UpdateOptions
            {
                IsUpsert = true
            });
            
            Console.WriteLine("UpsertedId: {0}", result.UpsertedId);
        }

        public static async Task List(IMongoDbRepository mongoDbRepository)
        {
            //create promotionId filter
            FilterDefinitionBuilder<TestItem> filterBuilder = Builders<TestItem>.Filter;
            var filterDef = filterBuilder.Empty;
            var result = await SearchManager.Search<TestItem>(mongoDbRepository.Database, CollectionName);
            Console.WriteLine("TotalCount: {0}", result.TotalCount);
            foreach (var item in result.Results)
            {
                Console.WriteLine("-------------------------------");
                Console.WriteLine("Id: {0}", item.Id);
                Console.WriteLine("Name: {0}", item.Name);
                Console.WriteLine("Deleted: {0}", item.Deleted);
                Console.WriteLine("Archived: {0}", item.Archived);
                Console.WriteLine("CreatedBy: {0}", item.CreatedBy);
                Console.WriteLine("CreatedDate: {0}", item.CreatedDate);
                Console.WriteLine("UpdatedBy: {0}", item.UpdatedBy);
                Console.WriteLine("UpdatedDate: {0}", item.UpdatedDate);
                Console.WriteLine("Version: {0}", item.Version);
            }
        }
    }
}
