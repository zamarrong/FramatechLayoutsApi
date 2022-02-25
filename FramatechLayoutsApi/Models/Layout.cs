using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FramatechLayoutsApi.Models
{
    public class Layout
    {
        public string DocCode { get; set; }
        public string TypeCode { get; set; }
        public string DocName { get; set; }
        public LayoutTemplate Template { get; set; }
        public string Status { get; set; }
    }

    public class LayoutTemplate
    {
        public string type { get; set; }
        public byte[] data { get; set; }
    }

    public class LayoutSearchResult
    {
        public Layout searchResult { get; set; }
    } 
}