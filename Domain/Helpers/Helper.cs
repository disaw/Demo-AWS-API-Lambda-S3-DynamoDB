using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models;
using System.Linq;
using System.IO;
using System.Globalization;

namespace Domain.Helpers
{
    public static class Helper
    {
        private static char _dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern[0]; 

        public static int ConvertToInt(string value)
        {
            int result;

            int.TryParse(value, out result);

            return result;
        }

        public static decimal ConvertToDecimal(string value)
        {
            decimal result;

            decimal.TryParse(value, out result);

            return result;
        }

        public static bool ConvertToBool(string value)
        {
            bool result;

            bool.TryParse(value, out result);

            return result;
        }
       
        public static DataType ConvertToDataType(string value)
        {
            DataType result;

            Enum.TryParse(value.Replace(" ", "").ToUpper(), out result);

            return result;
        }

        public static Units ConvertToUnits(string value)
        {
            Units result;

            Enum.TryParse(value.Replace(" ", "").ToUpper(), out result);

            return result;
        }

        public static decimal GetMedian(IEnumerable<decimal> numbers)
        {
            decimal result;

            var sortedNumbers = numbers.OrderBy(x => x).Distinct().ToArray();

            int numbersCount = sortedNumbers.Count();

            if (numbersCount % 2 == 0)
            {
                result = (sortedNumbers[(numbersCount / 2) - 1] + sortedNumbers[numbersCount / 2]) / 2;
            }
            else
            {
                result = sortedNumbers[sortedNumbers.Count() / 2];
            }

            return result;
        }

        public static DateTime ConvertToDate(string date)
        {
            try
            {
                var dateValues = date.Split('/');

                return new DateTime(ConvertToInt(dateValues[0]), ConvertToInt(dateValues[1]), ConvertToInt(dateValues[2]));
            }
            catch (Exception e)
            {
                throw new FormatException(@$"Date string should be in 'yyyy/MM/dd' format. Input: {date}.", e);
            }
        }

        public static DateTime ConvertToDateTimeFromDB(string value)
        {
            var result = new DateTime();

            try
            {             
                var dateTimeValues = value.Split(' ');
                var dateValues = dateTimeValues[0].Split('/');
                var timeValues = dateTimeValues[1].Split(':');
                var hours = ConvertToInt(timeValues[0]);
                var hoursIn24 = dateTimeValues[2].ToUpper() == "PM" && hours != 12 ? hours + 12 : hours;
                var seconds = timeValues.Count() != 3 ? 0 : ConvertToInt(timeValues[2]);

                if (_dateTimeFormat == 'M')
                {
                    result = new DateTime(ConvertToInt(dateValues[2]), ConvertToInt(dateValues[0]), ConvertToInt(dateValues[1]),
                        hoursIn24, ConvertToInt(timeValues[1]), seconds);
                }
                else
                {
                    result = new DateTime(ConvertToInt(dateValues[2]), ConvertToInt(dateValues[1]), ConvertToInt(dateValues[0]),
                        hoursIn24, ConvertToInt(timeValues[1]), seconds);
                }

                return result;
            }
            catch (Exception e)
            {
                throw new FormatException(@$"Date string from DB should be in '{_dateTimeFormat} hh:mm:ss tt' format. Input: {value}.", e);
            }
        }

        public static DateTime ConvertToDateTimeFromCSV(string value)
        {
            try
            {
                var dateTimeValues = value.Split(' ');
                var dateValues = dateTimeValues[0].Split('/');
                var timeValues = dateTimeValues[1].Split(':');
                var seconds = timeValues.Count() != 3 ? 0 : ConvertToInt(timeValues[2]);

                return new DateTime(ConvertToInt(dateValues[2]), ConvertToInt(dateValues[1]), ConvertToInt(dateValues[0]),
                    ConvertToInt(timeValues[0]), ConvertToInt(timeValues[1]), seconds);
            }
            catch (Exception e)
            {
                throw new FormatException(@$"Date string from CSV should be in 'dd/MM/yyyy HH:mm(:ss)' format. Input: {value}.", e);
            }
        }
    }
}
