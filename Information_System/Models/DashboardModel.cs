using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Information_System.Models
{
    public class DashboardModel
    {
        public int total { get; set; }
        public int waiting { get; set; }
        public int finished { get; set; }
        public int reject { get; set; }
        //public int percent { get; set; }
        public int p_waiting { get; set; }
        public int p_finished { get; set; }
        public int p_reject { get; set; }
    }

    public class DBDocFlowModel
    {
        public string NAME { get; set; }
        public string DOC_FLOW_ID { get; set; }
        public int COUNT { get; set; }
        public int PERCENTAGE { get; set; }
    }

}