using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Information_System.Models
{
    public class CommentModel
    {
        public string COMMENT_ID { get; set; }
        public string IS_ID { get; set; }
        public string CREATED_DATE { get; set; }
        public string CREATED_BY { get; set; }
        public string CREATED_NAME { get; set; }
        public string COMMENT { get; set; }
        public string DEPT { get; set; }
        public List<string> FILES { get; set; }
    }
}