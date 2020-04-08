using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Coyote.Tasks;

namespace TinyService
{
    using Document = Dictionary<string, string>;
    using Collection = Dictionary<string, Dictionary<string, string>>;

    public class DatabaseProvider
    {
        public static Dictionary<string, Collection> collections = new Dictionary<string, Collection>()
        {
            { Constants.UserCollection, new Collection() }
        };

        private Logger logger;

        public DatabaseProvider(string context, Logger parentLogger)
        {
            logger = new Logger($"{nameof(DatabaseProvider)} - {context}", parentLogger.FlowId);
        }

        public async Task AddDocument(string collectionName, string rowKey, Document doc)
        {
            await Task.Run(() =>
            {
                logger.Write($"AddDocument {rowKey}, {DocToStr(doc)} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var collection = collections[collectionName];

                if (collection.ContainsKey(rowKey))
                {
                    throw new DatabaseException($"AddRow: {rowKey} already exists");
                }
                else
                {
                    collection[rowKey] = doc;
                }
            });
        }

        public async Task UpdateDocument(string collectionName, string rowKey, Document doc)
        {
            await Task.Run(() =>
            {
                logger.Write($"UpdateDocument {rowKey}, {DocToStr(doc)} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var collection = collections[collectionName];

                if (!collection.ContainsKey(rowKey))
                {
                    throw new DatabaseException($"UpdateRow: {rowKey} does not exist");
                }

                collection[rowKey] = doc;
            });
        }

        public async Task UpdateKeyInDocument(string collectionName, string rowKey, string docKey, string docValue)
        {
            await Task.Run(() =>
            {
                logger.Write($"UpdateKeyInDocument for row {rowKey}, {docKey}: {docValue} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var collection = collections[collectionName];

                if (!collection.ContainsKey(rowKey))
                {
                    throw new DatabaseException($"UpdateKeyInDocument: {rowKey} does not exist");
                }

                var doc = collection[rowKey];

                if (!doc.ContainsKey(docKey))
                {
                    throw new DatabaseException($"UpdateKeyInDocument: {docKey} does not exist in document");
                }

                doc[docKey] = docValue;
            });
        }

        public async Task DeleteDocument(string collectionName, string rowKey)
        {
            await Task.Run(() =>
            {
                logger.Write($"DeleteDocument {rowKey} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var collection = collections[collectionName];

                if (!collection.ContainsKey(rowKey))
                {
                    throw new DatabaseException($"DeleteRow: {rowKey} does not exist");
                }
                else
                {
                    collection.Remove(rowKey);
                }
            });
        }

        public async Task<Document> GetDocument(string collectionName, string rowKey)
        {
            return await Task.Run(() =>
            {
                logger.Write($"GetDocument {rowKey} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var collection = collections[collectionName];

                if (!collection.ContainsKey(rowKey))
                {
                    throw new DatabaseException($"GetValue: {rowKey} does not exit");
                }

                return collection[rowKey];
            });
        }

        public async Task<bool> DoesDocumentExist(string collectionName, string rowKey)
        {
            return await Task.Run(() =>
            {
                logger.Write($"DoesDocumentExist {rowKey} in collection {collectionName}");

                if (!collections.ContainsKey(collectionName))
                {
                    throw new DatabaseException($"Collection {collectionName} does not exist");
                }

                var table = collections[collectionName];

                return table.ContainsKey(rowKey);
            });
        }
        
        // My implementations 
        // Impl 1 - passed tests
        public async Task<bool> AddDocumentIfNotExists(string collectionName, string rowKey, Document doc)
        {
            logger.Write($"AddDocumentIfExists {rowKey}, {DocToStr(doc)} in collection {collectionName}");
            try {
                await AddDocument(collectionName, rowKey, doc);
                return true;
            } catch (DatabaseException e) {
                Console.WriteLine(e.StackTrace);
                return false;
            }

        }
    
        public async Task<bool> UpdateDocumentIfExists(string collectionName, string rowKey, Document doc)
        {
            logger.Write($"UpdateDocumentIfExists {rowKey}, {DocToStr(doc)} in collection {collectionName}");
            try {
                await UpdateDocument(collectionName, rowKey, doc);
                return true;
            } catch (DatabaseException e) {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<bool> UpdateKeyInDocumentIfExists(string collectionName, string rowKey, string docKey, string docValue)
        {
            logger.Write($"UpdateKeyInDocumentIfExists for row {rowKey}, {docKey}: {docValue} in collection {collectionName}");
            try {
                await UpdateKeyInDocument(collectionName, rowKey, docKey, docValue);
                return true;
            } catch (DatabaseException e) {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        public async Task<Document> GetDocumentIfExists(string collectionName, string rowKey) 
        {
            try {      
                var doc = await GetDocument(collectionName, rowKey);
                return doc;
            } catch (DatabaseException e) {
                Console.WriteLine(e.StackTrace);
                return null;
            }
        }

        public async Task<bool> DeleteDocumentIfExists(string collectionName, string rowKey)
        {
            logger.Write($"DeleteDocumentIfExists {rowKey} in collection {collectionName}");
            try {
                await DeleteDocument(collectionName, rowKey);
                return true;
            } catch (DatabaseException e) {
                Console.WriteLine(e.StackTrace);
                return false;
            }
        }

        // Impl 2 - failed tests
        public async Task<bool> AddDocumentIfNotExists(string collectionName, string rowKey, Document doc)
        {
            logger.Write($"AddDocumentIfExists {rowKey}, {DocToStr(doc)} in collection {collectionName}");
            return await Task.Run(() =>
            {
                // Collection is not existed or document is already added
                if (!collections.ContainsKey(collectionName) || IsDocumentExist(collectionName, rowKey)) return false;
                    
                var collection = collections[collectionName];
                collection[rowKey] = doc;
                return true;
            });
        }
    
        public async Task<bool> UpdateDocumentIfExists(string collectionName, string rowKey, Document doc)
        {
            return await Task.Run(() =>
            {
                logger.Write($"UpdateDocumentIfExists {rowKey}, {DocToStr(doc)} in collection {collectionName}");
                if (!IsDocumentExist(collectionName, rowKey)) return false;

                var collection = collections[collectionName];
                collection[rowKey] = doc;
                return true;
            });
        }

        public async Task<bool> UpdateKeyInDocumentIfExists(string collectionName, string rowKey, string docKey, string docValue)
        {
            return await Task.Run(() =>
            {
                logger.Write($"UpdateKeyInDocumentIfExists for row {rowKey}, {docKey}: {docValue} in collection {collectionName}");
                if (!IsDocumentExist(collectionName, rowKey)) return false;

                var collection = collections[collectionName];
                var doc = collection[rowKey];
                if (!doc.ContainsKey(docKey)) return false;
                
                doc[docKey] = docValue;
                return true;
            });
        }

        public async Task<Document> GetDocumentIfExists(string collectionName, string rowKey) 
        {
            return await Task.Run(() =>
            {
                logger.Write($"GetDocumentIfExists {rowKey} in collection {collectionName}");
                if (!IsDocumentExist(collectionName, rowKey)) return null;

                var collection = collections[collectionName];
                return collection[rowKey];
            });
        }

        public async Task<bool> DeleteDocumentIfExists(string collectionName, string rowKey)
        {
            return await Task.Run(() =>
            {
                logger.Write($"DeleteDocumentIfExists {rowKey} in collection {collectionName}");
                if (!IsDocumentExist(collectionName, rowKey)) return false;

                var collection = collections[collectionName];
                collection.Remove(rowKey);
                return true;
            });
        }

        private bool IsDocumentExist(string collectionName, string rowKey)
        {
            logger.Write($"IsDocumentExist {rowKey} in collection {collectionName}");
            if (!collections.ContainsKey(collectionName)) return false;

            var table = collections[collectionName];
            return table.ContainsKey(rowKey);
        }
        // My implementations end
        
        public static void Cleanup()
        {
            collections[Constants.UserCollection].Clear();
        }

        private static string DocToStr(Document doc)
        {
            List<string> kvPairs = new List<string>();
            foreach (var key in doc.Keys)
            {
                kvPairs.Add($"{key}: {doc[key]}");
            }
            return "{ " + string.Join(", ", kvPairs) + " }";
        }
    }

    public class DatabaseException : Exception
    {
        public DatabaseException(string msg) : base(msg)
        {
        }
    }
}
