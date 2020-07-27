using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models;
using Domain.Repositories;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Amazon;
using System.Linq;
using Domain.Helpers;
using Microsoft.Extensions.Options;

namespace Data.AWS.DynamoDB
{
    public class DataRepository : IDataRepository
    {
        private readonly IAmazonDynamoDB _dynamoDBClient;
        private readonly ILogger<IDataRepository> _logger;
        private readonly Configuration _configuration;

        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _lpTableName;
        private readonly string _touTableName;
        private readonly char _newLine;

        public DataRepository(IServiceProvider serviceProvider)
        {
            _accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            _secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            _dynamoDBClient = new AmazonDynamoDBClient(_accessKey, _secretKey);
 
            _configuration = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
            _lpTableName = _configuration.LPTableName;
            _touTableName = _configuration.TOUTableName;
            _newLine = _configuration.NewLine;

            _logger = serviceProvider.GetRequiredService<ILogger<IDataRepository>>();

        }

        public async Task<List<int>> ReadLPRecordIds()
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = _lpTableName,
                    ProjectionExpression = "Id"
                };

                var responce = await _dynamoDBClient.ScanAsync(request);

                return responce.Items.Select(x => Helper.ConvertToInt(x["Id"].N)).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(@$"Error reading record ids: {_lpTableName} table.{_newLine} Error: {e}.{_newLine}");
                throw e;
            }
        }

        public async Task DeleteLPRecords(List<int> ids)
        {
            await DeleteRecords(_lpTableName, ids);
        }

        public async Task SaveLPRecords(List<LP> records)
        {
            int id = 0;
            foreach (var record in records)
            {
                id++;

                try
                {
                    var request = BuildLPPutRequest(id, record);

                    await _dynamoDBClient.PutItemAsync(request);
                }
                catch (Exception e)
                {
                    _logger.LogError(@$"Error saving record id: {id}. MeterPointCode: {record.MeterPointCode}.{_newLine} Error: {e}.{_newLine}");
                }
            }           
        }

        public async Task<List<LP>> ReadLPRecords()
        {
            var result = new List<LP>();

            try
            {
                var request = BuildLPScanRequest(null);

                var responce = await _dynamoDBClient.ScanAsync(request);

                foreach (var item in responce.Items)
                {
                    try
                    {
                        result.Add(MapLP(item));
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(@$"Failed reading record Id: {item["Id"].N} from {_lpTableName} table.{_newLine} Error: {e}.{_newLine}");
                        throw e;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(@$"Error scanning: {_lpTableName} table.{_newLine} Error: {e}.{_newLine}");
                throw e;
            }
        }



        public async Task<List<int>> ReadTOURecordIds()
        {
            try
            {
                var request = new ScanRequest
                {
                    TableName = _touTableName,
                    ProjectionExpression = "Id"
                };

                var responce = await _dynamoDBClient.ScanAsync(request);

                return responce.Items.Select(x => Helper.ConvertToInt(x["Id"].N)).ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(@$"Error reading record ids: {_touTableName} table.{_newLine} Error: {e}.{_newLine}");
                throw e;
            }
        }

        public async Task DeleteTOURecords(List<int> ids)
        {
            await DeleteRecords(_touTableName, ids);
        }

        public async Task SaveTOURecords(List<TOU> records)
        {
            int id = 0;
            foreach (var record in records)
            {
                id++;

                try
                {
                    var request = BuildTOUPutRequest(id, record);

                    await _dynamoDBClient.PutItemAsync(request);
                }
                catch (Exception e)
                {
                    _logger.LogError(@$"Error saving record id: {id}. MeterPointCode: {record.MeterCode}.{_newLine} Error: {e}.{_newLine}");
                }
            }
        }

        public async Task<List<TOU>> ReadTOURecords()
        {
            var result = new List<TOU>();

            try
            {
                var request = new ScanRequest
                {
                    TableName = _touTableName
                };

                var responce = await _dynamoDBClient.ScanAsync(request);

                foreach(var item in responce.Items)
                {
                    try
                    {
                        result.Add(MapTOU(item));
                    }
                    catch (Exception e)
                    {
                        _logger.LogInformation(@$"Failed reading record Id: {item["Id"].N} from {_touTableName} table.{_newLine} Error: {e}.{_newLine}");
                        throw e;
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                _logger.LogError(@$"Error scanning: {_touTableName} table.{_newLine} Error: {e}.{_newLine}");
                throw e;
            }
        }

        private async Task DeleteRecords(string table, List<int> ids)
        {
            var writeRequests = new List<WriteRequest>();

            var writeRequestsCollection = new List<List<WriteRequest>>();

            int count = 0;

            try
            {
                foreach (var id in ids)
                {
                    count++;

                    var key = new Dictionary<string, AttributeValue> { { "Id", new AttributeValue { N = id.ToString() } } };

                    writeRequests.Add(new WriteRequest() { DeleteRequest = new DeleteRequest { Key = key } });

                    if (count == 25)
                    {
                        writeRequestsCollection.Add(writeRequests);

                        writeRequests = new List<WriteRequest>();

                        count = 0;
                    }
                }

                if (count > 0)
                {
                    writeRequestsCollection.Add(writeRequests);
                }

                foreach (var requests in writeRequestsCollection)
                {
                    var requestItems = new Dictionary<string, List<WriteRequest>> { { table, requests } };

                    var batchRequest = new BatchWriteItemRequest { RequestItems = requestItems };

                    await _dynamoDBClient.BatchWriteItemAsync(batchRequest);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(@$"Error deleting records from: {table} table.{_newLine} Error: {e}.{_newLine}");
                throw e;
            }
        }

        private PutItemRequest BuildLPPutRequest(int id, LP record)
        {
            var row = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { N = id.ToString() } },
                { "MeterPointCode", new AttributeValue { N = record.MeterPointCode.ToString() } },
                { "SerialNumber", new AttributeValue { N = record.SerialNumber.ToString() } },
                { "PlantCode", new AttributeValue { S = record.PlantCode } },
                { "DateTime", new AttributeValue { S = record.DateTime.ToString() } },
                { "DataType", new AttributeValue { N = ((int)record.DataType).ToString() } },
                { "DataValue", new AttributeValue { N = record.DataValue.ToString() } },
                { "Units", new AttributeValue { N = ((int)record.Units).ToString() } },
                { "Status", new AttributeValue { BOOL = record.Status } },
            };

            return new PutItemRequest
            {
                TableName = _lpTableName,
                Item = row
            };
        }

        private ScanRequest BuildLPScanRequest(int? id)
        {
            var request = new ScanRequest
            {
                TableName = _lpTableName
            };

            if(id.HasValue)
            {
                request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":v_Id", new AttributeValue { N = id.ToString() } }
                };

                request.FilterExpression = "Id = :v_Id";

                request.ProjectionExpression = "Id, MeterPointCode, SerialNumber, PlantCode, DateTime, DataType, DataValue, Units, Status";
            }

            return request;
        }

        private LP MapLP(Dictionary<string, AttributeValue> responce)
        {
            return new LP
            {
                Id = Helper.ConvertToInt(responce["Id"].N),
                MeterPointCode = Helper.ConvertToInt(responce["MeterPointCode"].N),
                SerialNumber = Helper.ConvertToInt(responce["SerialNumber"].N),
                PlantCode = responce["PlantCode"].S,
                DateTime = Helper.ConvertToDateTimeFromDB(responce["DateTime"].S),
                DataType = Helper.ConvertToDataType(responce["DataType"].N),
                DataValue = Helper.ConvertToDecimal(responce["DataValue"].N),
                Units = Helper.ConvertToUnits(responce["Units"].N),
                Status = responce["Status"].BOOL
            };
        }

        private PutItemRequest BuildTOUPutRequest(int id, TOU record)
        {
            var row = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { N = id.ToString() } },
                { "MeterCode", new AttributeValue { N = record.MeterCode.ToString() } },
                { "Serial", new AttributeValue { N = record.Serial.ToString() } },
                { "PlantCode", new AttributeValue { S = record.PlantCode } },
                { "DateTime", new AttributeValue { S = record.DateTime.ToString() } },
                { "Quality", new AttributeValue { S = record.Quality } },
                { "Stream", new AttributeValue { S = record.Stream } },
                { "DataType", new AttributeValue { N = ((int)record.DataType).ToString() } },
                { "Energy", new AttributeValue { N = record.Energy.ToString() } },
                { "Units", new AttributeValue { N = ((int)record.Units).ToString() } },
            };

            return new PutItemRequest
            {
                TableName = _touTableName,
                Item = row
            };
        }

        private TOU MapTOU(Dictionary<string, AttributeValue> responce)
        {
            return new TOU
            {
                Id = Helper.ConvertToInt(responce["Id"].N),
                MeterCode = Helper.ConvertToInt(responce["MeterCode"].N),
                Serial = Helper.ConvertToInt(responce["Serial"].N),
                PlantCode = responce["PlantCode"].S,
                DateTime = Helper.ConvertToDateTimeFromDB(responce["DateTime"].S),
                Quality = responce["Quality"].S,
                Stream = responce["Stream"].S,
                DataType = Helper.ConvertToDataType(responce["DataType"].N),
                Energy = Helper.ConvertToDecimal(responce["Energy"].N),
                Units = Helper.ConvertToUnits(responce["Units"].N)
            };
        }
    }
}
