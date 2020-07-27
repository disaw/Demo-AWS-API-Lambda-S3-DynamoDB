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
    public class TOU : ITOU
    {
        private readonly ICSVRepository _CSVRepository;
        private readonly IDataRepository _DataRepository;
        private readonly ILogger<ITOU> _logger;

        public int Id { get; set; }
        public int MeterCode { get; set; }
        public int Serial { get; set; }
        public string PlantCode { get; set; }
        public DateTime DateTime { get; set; }
        public string Quality { get; set; }
        public string Stream { get; set; }
        public DataType DataType { get; set; }
        public decimal Energy { get; set; }
        public Units Units { get; set; }

        public TOU(IServiceProvider serviceProvider)
        {
            _CSVRepository = serviceProvider.GetRequiredService<ICSVRepository>();
            _logger = serviceProvider.GetRequiredService<ILogger<ITOU>>();
            _DataRepository = serviceProvider.GetRequiredService<IDataRepository>();
        }

        public TOU() { }

        public async Task ClearData()
        {
            var existingRecordIds = await _DataRepository.ReadTOURecordIds();

            await _DataRepository.DeleteTOURecords(existingRecordIds);
        }

        public async Task LoadData()
        {
            var touRecords = await _CSVRepository.ReadTOUFiles();

            var log = touRecords.Take(1);
            _logger.LogInformation(@$"Records: {log.Select(x => x.DateTime).FirstOrDefault()}, {log.Select(x => x.MeterCode).FirstOrDefault()}, 
                {log.Select(x => x.DataType).FirstOrDefault()}, {log.Select(x => x.Energy).FirstOrDefault()}.\n");

            await _DataRepository.SaveTOURecords(touRecords);
        }

        public async Task<IMeterData> GetMeterData(DateTime date, int meter, int dataType)
        {
            var meterData = new MeterData();

            _logger.LogInformation($"Parameters: {date}, {meter}, {dataType}.\n");
         
            var touRecords = await _DataRepository.ReadTOURecords();

            var dataValues = touRecords.Where(x => x.DateTime.Date == date.Date
                                                && x.MeterCode == meter
                                                && (int)x.DataType == dataType)
                                    .Select(x => x.Energy);

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
