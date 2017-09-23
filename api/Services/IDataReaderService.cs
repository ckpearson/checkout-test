using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using api.Models;
using api.Utils;

namespace api.Services
{
    public interface IDataReader<T> where T : IDataItem
    {
        Task<IEnumerable<T>> GetAll();
        Task<Option<T>> GetById(int id);
    }
}