using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Domain.Repositories;
using Domain.Models;
using System.Linq;
using System.Text;
using Domain.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Data.AWS.S3
{
    public class CSVRepository : ICSVRepository
    {
        private readonly ILogger<ICSVRepository> _logger;
        private readonly IAmazonS3 _s3Client;
        private readonly Configuration _configuration;

        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _csvBucket;
        private readonly string _lpFilePrefix;
        private readonly string _touFilePrefix;
        private readonly char _newLine;
        private readonly char _separator;

        public CSVRepository(IServiceProvider serviceProvider)
        {
            _accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            _secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            _s3Client = new AmazonS3Client(_accessKey, _secretKey);

            _configuration = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
            _csvBucket = _configuration.CSVBucket;
            _lpFilePrefix = _configuration.LPFilePrefix;
            _touFilePrefix = _configuration.TOUFilePrefix;
            _newLine = _configuration.NewLine;
            _separator = _configuration.Separator;

            _logger = serviceProvider.GetRequiredService<ILogger<ICSVRepository>>();
        }

        public async Task<List<LP>> ReadLPFiles()
        {
            var allRecords = new List<LP>();

            var fileList = await GetFileList(_lpFilePrefix);

            foreach (var file in fileList)
            {
                allRecords.AddRange(await ReadLPFile(file));
            }

            return allRecords;
        }

        public async Task<List<TOU>> ReadTOUFiles()
        {
            var allRecords = new List<TOU>();

            var fileList = await GetFileList(_touFilePrefix);

            foreach (var file in fileList)
            {
                allRecords.AddRange(await ReadTOUFile(file));
            }

            return allRecords;
        }

        private async Task<List<string>> GetFileList(string filePrefix)
        {
            var fileList = new List<string>();

            try
            {
                ListObjectsRequest request = new ListObjectsRequest
                {
                    BucketName = _csvBucket
                };

                ListObjectsResponse response = await _s3Client.ListObjectsAsync(request);

                fileList.AddRange(response.S3Objects.Select(x => x.Key));

                fileList.RemoveAll(x => !x.StartsWith(filePrefix));
            }
            catch (AmazonS3Exception e)
            {
                _logger.LogError(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            return fileList;
        }

        private async Task<List<LP>> ReadLPFile(string fileName)
        {
            var records = new List<LP>();
            var trace = new StringBuilder();
            string fileContent;
            _logger.LogInformation($"Start reading {fileName}.");

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _csvBucket,
                    Key = fileName
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    fileContent = reader.ReadToEnd();
                }

                var lines = fileContent.Split('\n').Skip(1);
                int lineCount = 0;

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lineCount++;
                        trace = new StringBuilder();
                        trace.Append($"Start reading line: {lineCount}.");

                        try
                        {
                            var record = MapLPRecord(line, trace);

                            records.Add(record);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(@$"Error reading line: {lineCount}.{_newLine} Values: {line}.{_newLine} Trace: {trace}.{_newLine} Error: {e}.{_newLine}");
                        }
                    }
                }

                _logger.LogInformation($"{lineCount} lines found.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error reading {fileName}. Error: {e}.");
            }

            _logger.LogInformation($"{records.Count} lines converted.");

            _logger.LogInformation($"End reading {fileName}.");

            return records;
        }

        private LP MapLPRecord(string line, StringBuilder trace)
        {
            var record = new LP();

            string[] fields = line.Split(_separator);

            trace.Append($"Reading field: MeterPointCode.{_newLine}");
            record.MeterPointCode = Helper.ConvertToInt(fields[0]);

            trace.Append($"Reading field: SerialNumber.{_newLine}");
            record.SerialNumber = Helper.ConvertToInt(fields[1]);

            trace.Append($"Reading field: PlantCode.{_newLine}");
            record.PlantCode = fields[2];

            trace.Append($"Reading field: DateTime.{_newLine}");
            record.DateTime = Helper.ConvertToDateTimeFromCSV(fields[3]);

            trace.Append($"Reading field: DataType.{_newLine}");
            record.DataType = Helper.ConvertToDataType(fields[4]);

            trace.Append($"Reading field: DataValue.{_newLine}");
            record.DataValue = Helper.ConvertToDecimal(fields[5]);

            trace.Append($"Reading field: Units.{_newLine}");
            record.Units = Helper.ConvertToUnits(fields[6]);

            trace.Append($"Reading field: Status.{_newLine}");
            record.Status = Helper.ConvertToBool(fields[7]);

            return record;
        }

        private async Task<List<TOU>> ReadTOUFile(string fileName)
        {
            var records = new List<TOU>();
            var trace = new StringBuilder();
            string fileContent;
            _logger.LogInformation($"Start reading {fileName}.");

            try
            {
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _csvBucket,
                    Key = fileName
                };

                using (GetObjectResponse response = await _s3Client.GetObjectAsync(request))
                using (Stream responseStream = response.ResponseStream)
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    fileContent = reader.ReadToEnd();
                }

                var lines = fileContent.Split('\n').Skip(1);
                int lineCount = 0;

                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lineCount++;
                        trace = new StringBuilder();
                        trace.Append($"Start reading line: {lineCount}.");

                        try
                        {
                            var record = MapTOURecord(line, trace);

                            records.Add(record);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(@$"Error reading line: {lineCount}.{_newLine} Values: {line}.{_newLine} Trace: {trace}.{_newLine} Error: {e}.{_newLine}");
                        }
                    }
                }

                _logger.LogInformation($"{lineCount} lines found.");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error reading {fileName}. Error: {e}.");
            }

            _logger.LogInformation($"{records.Count} lines converted.");

            _logger.LogInformation($"End reading {fileName}.");

            return records;
        }

        private TOU MapTOURecord(string line, StringBuilder trace)
        {
            var record = new TOU();

            string[] fields = line.Split(_separator);

            trace.Append($"Reading field: MeterCode.{_newLine}");
            record.MeterCode = Helper.ConvertToInt(fields[0]);

            trace.Append($"Reading field: Serial.{_newLine}");
            record.Serial = Helper.ConvertToInt(fields[1]);

            trace.Append($"Reading field: PlantCode.{_newLine}");
            record.PlantCode = fields[2];

            trace.Append($"Reading field: DateTime.{_newLine}");
            record.DateTime = Helper.ConvertToDateTimeFromCSV(fields[3]);

            trace.Append($"Reading field: Quality.{_newLine}");
            record.Quality = fields[4];

            trace.Append($"Reading field: Stream.{_newLine}");
            record.Stream = fields[5];

            trace.Append($"Reading field: DataType.{_newLine}");
            record.DataType = Helper.ConvertToDataType(fields[6]);

            trace.Append($"Reading field: Energy.{_newLine}");
            record.Energy = Helper.ConvertToDecimal(fields[7]);

            trace.Append($"Reading field: Units.{_newLine}");
            record.Units = Helper.ConvertToUnits(fields[8]);

            return record;
        }
    }
}
