## Solution Diagram

![Design](https://github.com/disaw/Demo-AWS-API-Lambda-S3-DynamoDB/blob/master/System%20Diagram.png?raw=true)


## Technologies Used

- .NET Core 3.1
- C#
- Web API
- AWS Lambda Serverless
- AWS S3 Service
- AWS DynamoDB
- Domain Driven Design
- Repository Pattern
- Dependency Injection
- XUnit
- Postman


## How to Invoke Solution

1. API Endpoint: GET https://localhost:44331/api/v1/Meter/ClearData

2. API Endpoint: GET https://localhost:44331/api/v1/Meter/LoadData

3. API Endpoint: POST https://localhost:44331/api/v1/Meter/GetData


## Sample Input:
```
{
  "Date": "2018/08/31",
  "Meter": 210095893,
  "DataType": 3
}
```

## Sample Output:
```
{
  "lp": {
    "minimum": 2.5,
    "maximum": 3.3,
    "median": 2.9
},
  "tou": {
    "minimum": 0,
    "maximum": 0,
    "median": 0
  }
}
```
