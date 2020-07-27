using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILP
    {
        public Task ClearData();
        public Task LoadData();
        public Task<IMeterData> GetMeterData(DateTime date, int meter, int dataType);
    }
}
