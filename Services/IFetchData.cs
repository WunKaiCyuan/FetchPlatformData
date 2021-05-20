using FetchPlatformData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Services
{
    public interface IFetchData<TConditions, Saver>
    {
        Task FetchDataAsync(TConditions conditions, Saver saver);
    }
}
