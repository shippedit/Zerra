﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZerraDemo.Domain.Weather.Models;
using ZerraDemo.Common;

namespace ZerraDemo.Domain.Weather
{
    public class WeatherQueryProvider : IWeatherQueryProvider
    {
        public async Task<WeatherModel> GetWeather()
        {
            Access.CheckRole("Admin");

            var weatherType = await WeatherServer.GetWeather();
            return new WeatherModel
            {
                Date = DateTime.UtcNow,
                WeatherType = weatherType
            };
        }

        public Task<Stream> TestStreams()
        {
            Access.CheckRole("Admin");

            var sb = new StringBuilder();
            for (var i = 0; i < 100000; i++)
                sb.Append("0123456789");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return Task.FromResult<Stream>(stream);
        }
    }
}
