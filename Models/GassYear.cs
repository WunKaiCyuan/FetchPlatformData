using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Models
{
    public class GassDateTime
    {
        public DateTime GassBase { get; set; } = DateTime.Now;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateString">MM/dd</param>
        /// <returns></returns>
        public DateTime JustMMDD(String dateString)
        {
            var strs = dateString.Split('/');
            var gassM = int.Parse(strs[0]);
            var gassD = int.Parse(strs[1]);

            if (gassM > GassBase.Month || gassD > GassBase.Day)
            {
                GassBase.AddYears(-1);
            }
            return new DateTime(year: GassBase.Year, month: gassM, day: gassD);
        }
        /// <summary>
        /// 往後猜 不改基準
        /// </summary>
        /// <param name="dateString">MM/dd HH:mm</param>
        /// <returns></returns>
        public DateTime InPostFullTime(string dateString)
        {
            var strs = dateString.Split('/');
            var gassM = int.Parse(strs[0]);

            if (gassM < GassBase.Month)
            {
                return DateTime.Parse($"{GassBase.Year + 1}/{dateString}");
            }
            else
            {
                return DateTime.Parse($"{GassBase.Year}/{dateString}");
            }

        }
    }
}
