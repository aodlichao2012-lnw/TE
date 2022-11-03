using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Information_System.Models
{
    public class DocInfoModel
    {
        public string ID { get; set;}
        public string INFO_NO { get; set;}
        public string REQUEST_NO { get; set;}
        public string REQUEST_ID { get; set; }
        public string ISSUE_ID { get; set;}
        public string REQUIREMENT_ISSUE_ID { get; set; }
        public string ISSUE_NAME { get; set; }
        public DateTime ISSUE_DATE  { get; set;}
        public string DOC_TYPE { get; set;}
        public string PLANT { get; set;}
        public string DEPARTMENT { get; set;}
        public string APPROVED_FLAX { get; set;}
        public string ACKNOWLAGE_FLEX { get; set;}
        public string CC_GRP { get; set;}
        public string DOC_FLOW_ID { get; set;}
        public string DOC_FLOW_TEXT { get; set; }
        public string DOC_DETAIL { get; set;}
        public string EFFECTIVE { get; set;}
        public string ADD_ORDER_NO { get; set;}
        public string RELATION_ID { get; set;}
        public string RELATION_TEXT { get; set; }
        //public string STATUS_1 { get; set; }
        //public string STATUS_2 { get; set; } 
        public string PLANT_DEP { get; set; }
        
        public string STATUS { get; set; }
        public string STATUS_ALL { get; set; }

        public string DOC_SUBJECT { get; set; }
        public string REASON_EXPLAIN { get; set; }
        public string DETAIL_REF { get; set; }

        public HttpPostedFileBase PIC_REF_1 { get; set; }
        public HttpPostedFileBase PIC_REF_2 { get; set; }
        public HttpPostedFileBase ATT_DOC_PURCHASE { get; set; }
        public HttpPostedFileBase ATT_DOC_REQUIRE { get; set; }
        public HttpPostedFileBase ATT_DOC_OTHER { get; set; }

        public string txt_PIC_REF_1 { get; set; }
        public string txt_PIC_REF_2 { get; set; }
        public string txt_ATT_DOC_PURCHASE { get; set; }
        public string txt_ATT_DOC_REQUIRE { get; set; }
        public string txt_ATT_DOC_OTHER { get; set; }

        public string APPROVE_LIST { get; set; }
        public string REQUEST_NAME { get; set; }
        public List<string> REQ_FILE_LIST { get; set; }
        public List<DocInfoApprove> TE_APPROVE_LIST { get; set; }
        public List<MailGroup> MAIL_GRP { get; set; }
        public string APPROVE_NAME { get; set; }
        public string REVIEW_NAME { get; set; }

    }

    public class FilePatch
    {
        public string txt_PIC_REF_1 { get; set; }
        public string txt_PIC_REF_2 { get; set; }
        public string txt_ATT_DOC_PURCHASE { get; set; }
        public string txt_ATT_DOC_REQUIRE { get; set; }
        public string txt_ATT_DOC_OTHER { get; set; }
    }

    public class DocInfoApprove
    {
        // public string DOCINFO_ID { get; set; }
        public string APPROVE_NAME { get; set; }
        public string APPROVE_ID { get; set; }
        public string COMMENT { get; set; }
        public string STATUS { get; set; }
        public DateTime? APPROVE_DATE { get; set; }
        public string APPROVE_DEPT { get; set; }
    }
}