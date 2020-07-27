using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AWSLambdaWebAPI.DTOs;

namespace AWSLambdaWebAPI.Services
{
    public interface IMeterService
    {
        Task<Response> GetData(DateTime date, int meter, int dataType);
        Task LoadData();

        Task ClearData();
    }
}