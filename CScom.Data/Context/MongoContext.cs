﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CScom.Data.Context
{
    public class MongoContext: IMongoContext
    {
        private IMongoDatabase Database { get; set; }
        private readonly List<Func<Task>> _commands;
        public MongoContext(IConfiguration configuration)
        {
            BsonDefaults.GuidRepresentation = GuidRepresentation.CSharpLegacy;
            
            _commands = new List<Func<Task>>();

            RegisterConventions();

            var mongoClient = new MongoClient(configuration.GetSection("MongoSettings").GetSection("ConnectionString").Value);

            Database = mongoClient.GetDatabase(configuration.GetSection("MongoSettings").GetSection("DatabaseName").Value);

        }

        private void RegisterConventions()
        {
            var pack = new ConventionPack
            {
                new IgnoreExtraElementsConvention(true),
                new IgnoreIfDefaultConvention(true)
            };
            ConventionRegistry.Register("My Solution Conventions", pack, t => true);
        }

        public async Task<int> SaveChanges()
        {
            var commandTasks = _commands.Select(c => c());

            await Task.WhenAll(commandTasks);

            return _commands.Count;
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return Database.GetCollection<T>(name);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void AddCommand(Func<Task> func)
        {
            _commands.Add(func);
        }
    }
}
