using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FramatechLayoutsApi.Models
{
    public class LayoutParams
    {
        public string DocCode { get; set; }
        public Dictionary<string, string> Pairs { get; set; }
    }
}