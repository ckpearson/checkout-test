using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IDataStore
    {
        Task<IEnumerable<T>> GetAll<T>() where T : IDataItem;
        Task<Option<IEnumerable<T>>> GetAllWhere<T>(Func<T,bool> predicate) where T : IDataItem;
        Task<Option<T>> GetById<T>(int id) where T : IDataItem;
    }
}