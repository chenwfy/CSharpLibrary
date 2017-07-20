using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB;
using MongoDB.Configuration;

namespace CSharpLib.Common.NoSql
{
    /// <summary>
    /// MonogDb操作辅助类
    /// </summary>
    public class MongoDbContext : IDisposable
    {
        private Mongo mongo;
        private IMongoDatabase db;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public MongoDbContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            MongoConfiguration config = new MongoConfiguration { ConnectionString = connectionString };
            MongoConnectionStringBuilder connBuilder = new MongoConnectionStringBuilder(connectionString);
            string dbName = connBuilder.Database;

            mongo = new Mongo(config);
            mongo.Connect();
            if (!string.IsNullOrEmpty(dbName))
                db = mongo.GetDatabase(dbName);
            else
                db = new MongoDatabase(config);
        }

        /// <summary>
        /// 获取当前所在的数据库
        /// </summary>
        public IMongoDatabase CurrentDatabase
        {
            get { return db; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IMongoCollection<T> Collection<T>() where T : class
        {
            return db.GetCollection<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public IMongoCollection<T> Collection<T>(string name) where T : class
        {
            return db.GetCollection<T>(name);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (mongo != null)
            {
                mongo.Dispose();
                mongo = null;
            }
        }
    }
}