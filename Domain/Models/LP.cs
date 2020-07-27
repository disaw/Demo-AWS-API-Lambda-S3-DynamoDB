using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Domain.Interfaces;
using Domain.Helpers;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class LP : ILP
    {
        private readonly ICSVRepository _CSVRepository;
        private readonly IDataRepository _DataRepository;
        private readonly ILogger<ILP> _logger;

        public int Id { get; set; }
        public int MeterPointCode { get; set; }
        public int SerialNumber { get; set; }
        public string PlantCode { get; set; }
        public DateTime DateTime { get; set; }
        public DataType DataType { get; set; }
        public decimal DataValue { get; set; }
        public Units Units { get; set; }
        public bool Status { get; set; }

        public LP(IServiceProvider serviceProvider)
        {
            _CSVRepository = serviceProvider.GetRequiredService<ICSVRepository>();
            _logger = serviceProvider.GetRequiredService<ILogger<ILP>>();
            _DataRepository = serviceProvider.GetRequiredService<IDataRepository>();
        }

        public LP() { }

        public async Task ClearData()
        {
            var existingRecordIds = await _DataRepository.ReadLPRecordIds();

            await _DataRepository.DeleteLPRecords(existingRecordIds);
        }

        public async Task LoadData()
        {
            var lpRecords = await _CSVRepository.ReadLPFiles();

            var log = lpRecords.Take(1);
            _logger.LogInformation(@$"Records: {log.Select(x => x.DateTime).FirstOrDefault()}, {log.Select(x => x.MeterPointCode).FirstOrDefault()}, 
                {log.Select(x => x.DataType).FirstOrDefault()}, {log.Select(x => x.DataValue).FirstOrDefault()}.\n");

            await _DataRepository.SaveLPRecords(lpRecords);
        }

        public async Task<IMeterData> GetMeterData(DateTime date, int meter, int dataType)
        {
            var meterData = new MeterData();

            _logger.LogInformation($"Parameters: {date}, {meter}, {dataType}.\n");

            var lpRecords = await _DataRepository.ReadLPRecords();

            var dataValues = lpRecords.Where(x => x.DateTime.Date == date.Date 
                                                && x.MeterPointCode == meter 
                                                && (int)x.DataType == dataType)
                                    .Select(x => x.DataValue);

            if (dataValues.Count() > 0)
            {
                meterData.Minimum = dataValues.Min();
                meterData.Median = Helper.GetMedian(dataValues);
                meterData.Maximum = dataValues.Max();
            }

            return meterData;
        } 
    }
}
