using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Information_System.Models
{
    public class MailGroup
    {
        public string EMP_ID { get; set; }
        public string EMP_EMAIL { get; set; }
        public string EMP_NAME { get; set; }
        public string EMP_STATUS { get; set; }
        public string EMP_COMMENT { get; set; }
        public DateTime? EMP_DATE { get; set; }
    }

    public class ApproveGroup
    {
        public string ID { get; set; }
        public string NAME { get; set; }
    }

    public class Relation
    {
        public string ID { get; set; }
        public string NAME { get; set; }
        public string DETAIL { get; set; }
    }

    public class MailTo
    {
        public string EMP_ID { get; set; }
        public string EMP_EMAIL { get; set; }
        public string EMP_NAME { get; set; }
    }

    public class RevDoc
    {
        public string INFO_NAME { get; set; }
        public string INFO_ID { get; set; }
        public string STATUS { get; set; }
        public string REQ_ID { get; set; }
        public string WARNNINGDATE { get; set; }
    }
}