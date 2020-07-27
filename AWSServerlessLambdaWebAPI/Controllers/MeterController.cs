using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AWSLambdaWebAPI.Services;
using AWSLambdaWebAPI.DTOs;
using System.Globalization;
using Domain.Helpers;

namespace AWSLambdaWebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MeterController : ControllerBase
    {
        private readonly IMeterService _meterService;

        public MeterController(IMeterService meterService)
        {
            _meterService = meterService;
        }

        [HttpGet]
        [Route("ClearData")]
        public async Task<IActionResult> ClearData()
        {
            try
            {
                await _meterService.ClearData();

                return Ok("Data Cleared.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet]
        [Route("LoadData")]
        public async Task<IActionResult> LoadData()
        {
            try
            {
                await _meterService.LoadData();

                return Ok("Data Loaded.");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("GetData")]
        public async Task<IActionResult> GetData([FromBody] Request request)
        {
            try
            {
                var date = Helper.ConvertToDate(request.Date);

                var result = await _meterService.GetData(date, request.Meter, request.DataType);

                return Ok(result);
            }
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}