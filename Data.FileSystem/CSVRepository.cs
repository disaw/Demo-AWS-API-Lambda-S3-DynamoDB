using System;
using System.Collections.Generic;
using System.Text;
using Domain.Repositories;
using Domain.Helpers;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Domain.Interfaces;
using Domain.Models;
using System.Threading.Tasks;

namespace Data.FileSystem
{
    public class CSVRepository : ICSVRepository
    {
        private const string FOLDER_PATH = @"c:\temp\";
        private const string LP_FILE_PREFIX = "LP_";
        private const string TOU_FILE_PREFIX = "TOU_";

        private readonly ILogger<ICSVRepository> _logger;

        public CSVRepository(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<ICSVRepository>>();
        }

        public async Task<List<LP>> ReadLPFiles()
        {
            var allRecords = new List<LP>();

            var fileList = GetFileList(FOLDER_PATH, LP_FILE_PREFIX);

            foreach (var file in fileList)
            {
                allRecords.AddRange(ReadLPFile(file));
            }

            return allRecords;
        }

        public async Task<List<TOU>> ReadTOUFiles()
        {
            var allRecords = new List<TOU>();

            var fileList = GetFileList(FOLDER_PATH, TOU_FILE_PREFIX);

            foreach (var file in fileList)
            {
                allRecords.AddRange(ReadTOUFile(file));
            }

            return allRecords;
        }

        private List<string> GetFileList(string directoryPath, string filePrefix)
        {
            var fileList = new List<string>();

            try
            {
                foreach (string filename in Directory.GetFiles(directoryPath))
                {
                    if (filename.StartsWith($@"{directoryPath}{filePrefix}"))
                    {
                        fileList.Add(filename);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return fileList;
        }

        private List<LP> ReadLPFile(string filePath)
        {
            var records = new List<LP>();
            var trace = new StringBuilder();
            trace.Append($"Start reading {filePath}.");

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    int lineCount = 0;

                    while (!parser.EndOfData)
                    {
                        lineCount++;
                        trace.Append($"Start reading line: {lineCount}.");

                        try
                        {
                            var record = new LP();

                            string[] fields = parser.ReadFields();

                            trace.Append($"Reading field: MeterPointCode.");
                            record.MeterPointCode = Helper.ConvertToInt(fields[0]);

                            trace.Append($"Reading field: SerialNumber.");
                            record.SerialNumber = Helper.ConvertToInt(fields[1]);

                            trace.Append($"Reading field: PlantCode.");
                            record.PlantCode = fields[2];

                            trace.Append($"Reading field: DateTime.");
                            record.DateTime = Helper.ConvertToDateTimeFromDB(fields[3]);

                            trace.Append($"Reading field: DataType.");
                            record.DataType = Helper.ConvertToDataType(fields[4]);

                            trace.Append($"Reading field: DataValue.");
                            record.DataValue = Helper.ConvertToDecimal(fields[5]);

                            trace.Append($"Reading field: Units.");
                            record.Units = Helper.ConvertToUnits(fields[6]);

                            trace.Append($"Reading field: Status.");
                            record.Status = Helper.ConvertToBool(fields[7]);

                            records.Add(record);
                        }
                        catch (Exception e)
                        {
                            trace.Append($"Error reading line: {lineCount}. Error: {e}.");
                        }

                        trace.Append($"End reading line: {lineCount}.");
                    }

                    trace.Append($"{records.Count} lines found.");
                }
            }
            catch (Exception e)
            {
                trace.Append($"Error reading {filePath}. Error: {e}.");
                _logger.LogError(trace.ToString(), e);
            }

            trace.Append($"{records.Count} lines converted.");

            trace.Append($"End reading {filePath}.");

            Console.Write(trace.ToString());
            _logger.LogInformation(trace.ToString());

            return records;
        }

        private List<TOU> ReadTOUFile(string filePath)
        {
            var records = new List<TOU>();
            var trace = new StringBuilder();
            trace.Append($"Start reading {filePath}.");

            try
            {
                using (TextFieldParser parser = new TextFieldParser(filePath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    int lineCount = 0;

                    while (!parser.EndOfData)
                    {
                        lineCount++;
                        trace.Append($"Start reading line: {lineCount}.");

                        try
                        {
                            var record = new TOU();

                            string[] fields = parser.ReadFields();

                            trace.Append($"Reading field: MeterCode.");
                            record.MeterCode = Helper.ConvertToInt(fields[0]);

                            trace.Append($"Reading field: Serial.");
                            record.Serial = Helper.ConvertToInt(fields[1]);

                            trace.Append($"Reading field: PlantCode.");
                            record.PlantCode = fields[2];

                            trace.Append($"Reading field: DateTime.");
                            record.DateTime = Helper.ConvertToDateTimeFromDB(fields[3]);

                            trace.Append($"Reading field: Quality.");
                            record.Quality = fields[4];

                            trace.Append($"Reading field: Stream.");
                            record.Stream = fields[5];

                            trace.Append($"Reading field: DataType.");
                            record.DataType = Helper.ConvertToDataType(fields[6]);

                            trace.Append($"Reading field: Energy.");
                            record.Energy = Helper.ConvertToDecimal(fields[7]);

                            trace.Append($"Reading field: Units.");
                            record.Units = Helper.ConvertToUnits(fields[8]);

                            records.Add(record);
                        }
                        catch (Exception e)
                        {
                            trace.Append($"Error reading line: {lineCount}. Error: {e}.");
                        }

                        trace.Append($"End reading line: {lineCount}.");
                    }

                    trace.Append($"{records.Count} lines found.");
                }
            }
            catch (Exception e)
            {
                trace.Append($"Error reading {filePath}. Error: {e}.");
                _logger.LogError(trace.ToString(), e);
            }

            trace.Append($"{records.Count} lines converted.");

            trace.Append($"End reading {filePath}.");

            Console.Write(trace.ToString());
            _logger.LogInformation(trace.ToString());

            return records;
        }       
    }
}
