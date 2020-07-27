using Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Repositories
{
    public interface IDataRepository
    {
        Task<List<int>> ReadLPRecordIds();

        Task DeleteLPRecords(List<int> ids);

        Task SaveLPRecords(List<LP> records);

        Task<List<LP>> ReadLPRecords();

        Task<List<int>> ReadTOURecordIds();

        Task DeleteTOURecords(List<int> ids);

        Task SaveTOURecords(List<TOU> records);

        Task<List<TOU>> ReadTOURecords();
    }
}
