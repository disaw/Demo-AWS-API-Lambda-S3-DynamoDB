using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWSLambdaWebAPI.DTOs;
using Domain.Interfaces;

namespace AWSLambdaWebAPI.Services
{
    public class MeterService : IMeterService
    {
        private readonly ILP _LP;
        private readonly ITOU _TOU;

        public MeterService(ILP lp, ITOU tou)
        {
            _LP = lp;
            _TOU = tou;
        }

        public async Task ClearData()
        {
            await _LP.ClearData();
            await _TOU.ClearData();
        }

        public async Task LoadData()
        {
            await _LP.LoadData();
            await _TOU.LoadData();
        }

        public async Task<Response> GetData(DateTime date, int meter, int dataType)
        {
            var lpMeterData = await _LP.GetMeterData(date, meter, dataType);
            var touMeterData = await _TOU.GetMeterData(date, meter, dataType);

            return new Response()
            {
                LP = lpMeterData,
                TOU = touMeterData
            };
        }
    }
}
