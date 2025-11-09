using SalesAnalytics.Application.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalytics.Application.Interfaces
{
    public interface ISalesDataHandlerService
    {
        Task<ServiceResult> ProcessSalesDataAsync();
    }
}
