using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinalDownloader.Services.Update
{
    internal interface IUpdateService
    {
        Task<bool> UpdateToolAsync();
    }
}
