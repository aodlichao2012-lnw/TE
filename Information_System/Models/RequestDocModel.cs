using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Information_System.Models
{
    public class RequestDocModel
    {
        public string IS_ID { get; set; }
        public string IS_NO { get; set; }
        public string PLANT { get; set; }
        public string DEPARTMENT { get; set; }
        public string SUBJECT { get; set; }
        public string REASON_EXPLAIN { get; set; }
        public string START_PROD_MONTH { get; set; }
        public string DOCUMENT_DATA_OF_CHANGE { get; set; }
        public string PARTS_CODE { get; set; }
        public string WATCH_CODE { get; set; }
        public string CLASSIFICATION { get; set; }
        public string ISSUE_ID { get; set; }
        public string ISSUE_NAME { get; set; }
        public DateTime ISSUE_DATE { get; set; } 
        public string ISSUE_EMAIL { get; set; }
        public string APPROVE_ID { get; set; }
        public string APPROVE_NAME { get; set; }
        public string APPROVE_EMAIL { get; set; }
        public DateTime APPROVE_DATE { get; set; }
        public string APPROVE_STATUS { get; set; }
        public string APPROVE_COMMENT { get; set; }
        public HttpPostedFileBase fileUpload { get; set; }
        public string INFO_STATUS { get; set; }

        //FileList
        public List<string> FILE_LIST { get; set; }
        public List<string> GET_FILE_LIST = new List<string>(); // { get; set; }
    }
}