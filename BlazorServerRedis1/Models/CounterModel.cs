using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorServerRedis1.Models
{
    [Serializable]
    public class CounterModel
    {
        public int AddCounter { get; set; }
    }
}
