using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using LiteDB;

namespace EeveexModManager.Classes.DatabaseClasses
{
    public class DatabaseContextBase : LiteDatabase
    {
        public DatabaseContextBase(string connecString) : base(connecString)
        { }

        public IEnumerable<T> GetAll<T>(string tableName) where T : class
            => GetCollection<T>(tableName).FindAll();

        public int FindIndex<T>(string tableName, Predicate<T> pred) where T : class
            => GetAll<T>(tableName).ToList().FindIndex(pred);

        public IEnumerable<T> GetWhere<T>(string tableName, Expression<Func<T, bool>> pred) where T : class
            => GetCollection<T>(tableName).Find(pred);
    }
}
