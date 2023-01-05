using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;

namespace Scedue_Warnning
{
    class Program
    {
        public static string connString = "data source=10.145.163.14; initial catalog=db_employee; user id=sa; password=p@ssw0rd002;MultipleActiveResultSets=True;";
        public static string conRTCStr = "data source=10.145.163.14; initial catalog=db_rtc; user id=sa; password=p@ssw0rd002;MultipleActiveResultSets=True;";
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
                Keepapi();
        }

        public static  void Keepapi()
        {

            List<string> email = new List<string>();
            DocInfoModel model = new DocInfoModel();
            List<DocInfoModel> modelList = new List<DocInfoModel>();
            using (SqlConnection con = new SqlConnection(Program.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine($@"SELECT  a1.*
	                                  ,a2. *
                                      , a1.ISSUE_DATE as ISSUE_DATE
                                       , a4.ISSUE_DATE as ISSUE_DATE2
	                                  ,a3. *
	                                  , a4.*
                                       , a5 .*
                                    ,  CONCAT(a3.empTitleEng,' ',a3.empNameEng) ISSUE_NAME
                                    , CONCAT(a3.empTitleEng,' ',a3.empNameEng) CHECK_NAME
                                   , a3.empNameEng as APP_NAME
                                  FROM [db_rtc].[dbo].[TBL_TECH_IS_DOCINFO] a1
                                  INNER JOIN [db_rtc].[dbo].[TBL_TECH_IS_DOCINFO_APPROVE] a2 ON a1.ID = a2.DOCINFO_ID
                                    INNER JOIN [db_employee].[dbo].[tbl_employee] a3 ON a3.empId = a2.APPROVE_ID
	                                INNER JOIN [db_rtc].[dbo].TBL_TECH_IS_REQUEST a4 ON a4.IS_ID = a1.REQUEST_NO
	                                INNER JOIN [db_rtc].[dbo].[TBL_TECH_IS_MAILGRP] a5 ON a5.ID = a1.DOC_FLOW_ID
                          where a1.WARNING_DATE >= GETDATE() ORDER BY a1.ISSUE_DATE DESC  ");
                    cmd.CommandText = sql.ToString();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {   model = new DocInfoModel();
                        model.ID = dr["ID"].ToString();
                        model.INFO_NO = dr["INFO_NO"].ToString() != "" ? dr["INFO_NO"].ToString() : "-";
                        model.REQUEST_ID = dr["REQUEST_NO"].ToString();
                        model.STATUS_ALL = dr["STATUS_2"].ToString();
                        model.DOC_SUBJECT = dr["SUBJECT"].ToString() != "" ? dr["SUBJECT"].ToString() : "-";
                        model.PLANT_DEP = dr["PLANT"].ToString() != "" ? dr["PLANT"].ToString() : "-";
                        model.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString() != "" ? dr["REASON_EXPLAIN"].ToString() : "-";
                        model.DETAIL_REF = dr["DETAILS"].ToString() != "" ? dr["DETAILS"].ToString() : "-";
                        model.ISSUE_NAME = dr["ISSUE_NAME"].ToString() != "" ? dr["ISSUE_NAME"].ToString() : "-";
                        model.REVIEW_NAME = dr["CHECK_NAME"].ToString() != "" ? dr["CHECK_NAME"].ToString() : "-";
                        model.APPROVE_NAME = dr["APP_NAME"].ToString() != "" ? dr["APP_NAME"].ToString() : "-";
                        model.ISSUE_DATE = Convert.ToDateTime(dr["ISSUE_DATE"].ToString());
                        model.DOC_DETAIL = dr["DOC_DETAIL"].ToString() != "" ? dr["DOC_DETAIL"].ToString() : "-";
                        model.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                        model.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                        model.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                        model.txt_DOC_IMPORTANT = dr["ATT_DOC_IMPORTANT"].ToString();
                        model.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                        model.DOC_TYPE = dr["DOC_TYPE"].ToString();
                        model.REQUEST_NAME = dr["ISSUE_NAME"].ToString();
                        model.DOC_FLOW_TEXT = dr["NAME"].ToString();
                        model.EFFECTIVE = dr["EFFECTIVE"].ToString() != "" ? dr["EFFECTIVE"].ToString() : "-";
                        model.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString() != "" ? dr["ADD_ORDER_NO"].ToString() : "-";

                        var json = JsonConvert.SerializeObject(model);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");

                        var url = "http://10.145.163.14/Approve/SendValues_inHttpMail2";
                        using var client = new HttpClient();

                        var response = client.PostAsync(url, data).Result;

                        var result = response.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(result);

                    }
                }
                 
                con.Close();
            }

        }
    }

    public class DocInfoModel
    {
        public string ID { get; set; }
        public string INFO_NO { get; set; }
        public string REQUEST_NO { get; set; }
        public string REQUEST_ID { get; set; }
        public string ISSUE_ID { get; set; }
        public string REQUIREMENT_ISSUE_ID { get; set; }
        public string ISSUE_NAME { get; set; }
        public DateTime ISSUE_DATE { get; set; }
        public string DOC_TYPE { get; set; }
        public string PLANT { get; set; }
        public string DEPARTMENT { get; set; }
        public string APPROVED_FLAX { get; set; }
        public string ACKNOWLAGE_FLEX { get; set; }
        public string CC_GRP { get; set; }
        public string DOC_FLOW_ID { get; set; }
        public string DOC_FLOW_TEXT { get; set; }
        public string DOC_DETAIL { get; set; }
        public string EFFECTIVE { get; set; }
        public string ADD_ORDER_NO { get; set; }
        public string RELATION_ID { get; set; }
        public string RELATION_TEXT { get; set; }
        //public string STATUS_1 { get; set; }
        //public string STATUS_2 { get; set; } 
        public string PLANT_DEP { get; set; }

        public string STATUS { get; set; }
        public string STATUS_ALL { get; set; }

        public string DOC_SUBJECT { get; set; }
        public string REASON_EXPLAIN { get; set; }
        public string DETAIL_REF { get; set; }
        public DateTime WARNNIG_DATE { get; set; }



        public string txt_PIC_REF_1 { get; set; }
        public string txt_PIC_REF_2 { get; set; }
        public string txt_ATT_DOC_PURCHASE { get; set; }
        public string txt_ATT_DOC_REQUIRE { get; set; }
        public string txt_DOC_IMPORTANT { get; set; }
        public string[] txt_ATT_DOC_OTHER { get; set; }

        public string APPROVE_LIST { get; set; }
        public string REQUEST_NAME { get; set; }

        public string APPROVE_NAME { get; set; }
        public string REVIEW_NAME { get; set; }

    }

    public class FilePatch
    {
        public string txt_PIC_REF_1 { get; set; }
        public string txt_PIC_REF_2 { get; set; }
        public string txt_ATT_DOC_PURCHASE { get; set; }
        public string txt_ATT_DOC_REQUIRE { get; set; }
        public string[] txt_ATT_DOC_OTHER { get; set; }
        public string txt_ATT_DOC_IMPORTANT { get; set; }
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
        public string APPROVE_EMAIL { get; set; }
    }
}
