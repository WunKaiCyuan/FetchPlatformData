using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FetchPlatformData.Services
{
    public interface IFetchData<TModel, TConditions>
    {
        Task<IEnumerable<TModel>> FetchDataAsync(TConditions conditions);
    }
}
