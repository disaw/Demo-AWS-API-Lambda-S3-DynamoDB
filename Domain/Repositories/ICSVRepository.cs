using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.Repositories
{
    public interface ICSVRepository
    {
        Task<List<LP>> ReadLPFiles();

        Task<List<TOU>> ReadTOUFiles();
    }
}
