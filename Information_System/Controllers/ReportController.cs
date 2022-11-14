using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Text;
using Information_System.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace Information_System.Controllers
{
    public class ReportController : Controller
    {
        static string pagglistsql = null;
        InFunction Fn = new InFunction();
        // GET: Report
        public ActionResult ReportList()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                List<ApproveGroup> apList = new List<ApproveGroup>();
                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        SqlDataReader dr = null;
                        cmd.CommandText = " SELECT ID, NAME FROM TBL_TECH_IS_MAILGRP ORDER BY ID ASC ";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            ApproveGroup ap = new ApproveGroup();
                            ap.ID = dr["ID"].ToString();
                            ap.NAME = dr["NAME"].ToString();
                            apList.Add(ap);
                        }
                    }
                }
                pagging("5");
                ViewData["app_list"] = apList;

                return View();
            }
        }

        [HttpGet]
        public void pagging(string max)
        {
            int[] count10;
            int count = 0;
            int sum = 0;
            int maxs = int.Parse(max);
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    SqlDataReader dr = null;
                    cmd.CommandText = $@" SELECT COUNT(D.ID) FROM TBL_TECH_IS_DOCINFO D   JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID 
                         JOIN[db_employee].[dbo].[tbl_employee] EMP ON D.ISSUE_ID = EMP.empId
                         JOIN[db_employee].[dbo].[tbl_employee] EMPI ON R.ISSUE_ID = EMPI.empId
                         JOIN[db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID
                         JOIN[db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT group by D.REQUEST_NO";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        count += int.Parse( dr[0].ToString());
                    }
                    count10 = new int[count / maxs];
                    for(int i =0; i < count / maxs; i += 1)
                    {
                        count10[i] += maxs + sum;
                        sum += maxs;
                    }
                }
            }
            
           TempData["Countpag"] = JsonConvert.SerializeObject(count10);
           ViewData["Countpagtotal"] = JsonConvert.SerializeObject(count);
        }

        [HttpGet]
        public string pagginto_list(string page , string max)
        {
            string sql = string.Empty;
            if (pagglistsql == null)
            {
                sql = "";
            }
            else
            {
                sql = pagglistsql;
            }
         
            sql += $@"OFFSET {page} ROWS FETCH NEXT { max} ROWS ONLY";
            List<DocInfoModel> res = new List<DocInfoModel>();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            DocInfoModel doc = new DocInfoModel();
                            doc.ISSUE_NAME = reader["ISSUE_NAME"].ToString();
                            doc.ISSUE_DATE = Convert.ToDateTime(reader["ISSUE_DATE"].ToString() , CultureInfo.InvariantCulture);
                            doc.REQUEST_NO = reader["IS_NO"].ToString();
                            doc.REQUEST_ID = reader["IS_ID"].ToString();
                            doc.REQUEST_NAME = reader["REQUESY_NAME"].ToString();
                            doc.STATUS = reader["STATUS"].ToString();
                            doc.INFO_NO = reader["INFO_NO"].ToString();
                            doc.PLANT_DEP = reader["DEP"].ToString();
                            doc.ID = reader["DOC_ID"].ToString();
                            res.Add(doc);
                        }
                    }
                }
            }
            pagging(max);
            return JsonConvert.SerializeObject(res);
        }

        [HttpGet]
        public JsonResult getInfoList(string NO, string RequestID, string RequestBy, string MAXRESULT, string STATUS, string ID, string DocumentFlow, string Details)
        {
            List<DocInfoModel> res = new List<DocInfoModel>();

            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    StringBuilder sql = new StringBuilder();
                    string sqlnontop = $@"TOP(" + MAXRESULT + ")";
                    TempData["top"] = sqlnontop;
                    sql.AppendLine(" SELECT "+ sqlnontop + "  R.IS_NO, R.IS_ID, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, CONCAT(EMP.empTitleEng,' ', UPPER(LEFT(EMP.empNameEng,1))+LOWER(SUBSTRING(EMP.empNameEng,2,LEN(EMP.empNameEng)))) AS ISSUE_NAME ");
                    sql.AppendLine($@" ,D.ID DOC_ID, D.INFO_NO , CASE
                                 WHEN
                                     D.INFO_NO IS NULL THEN  R.ISSUE_DATE
                                ELSE D.ISSUE_DATE
                                END as ISSUE_DATE, UPPER(LEFT(D.STATUS,1))+LOWER(SUBSTRING(D.STATUS,2,LEN(D.STATUS))) STATUS ");
                    sql.AppendLine(" , CONCAT(EMPI.empTitleEng,' ',  UPPER(LEFT(EMPI.empNameEng,1))+LOWER(SUBSTRING(EMPI.empNameEng,2,LEN(EMPI.empNameEng)))) AS REQUESY_NAME ");
                    sql.AppendLine(" FROM TBL_TECH_IS_DOCINFO D ");
                    sql.AppendLine(" JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_employee] EMP ON D.ISSUE_ID = EMP.empId ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_employee] EMPI ON R.ISSUE_ID = EMPI.empId ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT");
                    sql.AppendLine(" WHERE D.ID IS NOT NULL ");
                    sql.AppendLine(" AND D.PLANT = " + Fn.getSQL(Session["plant_id"].ToString()));
                    if (NO != "" && NO != null)
                    {
                        sql.AppendLine(" AND D.INFO_NO LIKE '%" + NO + "%'");
                    }
                    if (RequestID != "" && RequestID != null)
                    {
                        sql.AppendLine(" AND R.IS_NO LIKE '%" + RequestID + "%'");
                    }
                    if (RequestBy != "" && RequestBy != null)
                    {
                        sql.AppendLine(" AND R.ISSUE_ID = '" + RequestBy + "'");
                    }
                    if (STATUS != "" && STATUS != null && STATUS != "null")
                    {
                        if (STATUS == "WATTING")
                        {
                            sql.AppendLine(" AND STATUS NOT IN ('APPROVED','REJECT','REVISE') ");
                        }
                        else
                        {
                            sql.AppendLine(" AND D.STATUS =" + Fn.getSQL(STATUS));
                        }
                    }
                    if (DocumentFlow !="" && DocumentFlow != null)
                    {
                        sql.AppendLine(" AND DOC_FLOW_ID =" + Fn.getSQL(DocumentFlow));
                    }

                    if(Details != "" && Details != null)
                    {
                        sql.AppendLine(" AND (R.PARTS_CODE like '%" + Details + "%' or R.WATCH_CODE like '%" + Details + "%' or R.REASON_EXPLAIN like '%" + Details + "%' or R.DETAILS like '%" + Details + "%') ");
                    }

                    sql.AppendLine("  order by D.ISSUE_DATE DESC");
              
                    cmd.CommandText = sql.ToString();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        DocInfoModel doc = new DocInfoModel();
                        doc.ISSUE_NAME = reader["ISSUE_NAME"].ToString();
                        doc.ISSUE_DATE = DateTime.Parse(reader["ISSUE_DATE"].ToString());
                        doc.REQUEST_NO = reader["IS_NO"].ToString();
                        doc.REQUEST_ID = reader["IS_ID"].ToString();
                        doc.REQUEST_NAME = reader["REQUESY_NAME"].ToString();
                        doc.STATUS = reader["STATUS"].ToString();
                        doc.INFO_NO = reader["INFO_NO"].ToString();
                        doc.PLANT_DEP = reader["DEP"].ToString();
                        doc.ID = reader["DOC_ID"].ToString();
                        res.Add(doc);
                    }
                    pagglistsql = sql.Remove(sql.ToString().IndexOf(sqlnontop), sqlnontop.Length).ToString();
                    TempData["MAXLEGHT"] = MAXRESULT;

                }
                con.Close();
                pagging(MAXRESULT);
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getInformation(string id)
        {
            RevDoc rev = new RevDoc();
            DocInfoModel res = new DocInfoModel();
            string REQUEST_ID = "";
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                     StringBuilder sql = new StringBuilder();
                    sql.Clear();
                    sql.AppendLine(" SELECT TOP 1 D.STATUS STATUS_ALL, D.CC_GRP, D.ID, D.INFO_NO, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, ");
                    sql.AppendLine(" CONCAT(EMP.empTitleEng,' ',UPPER(LEFT(EMP.empNameEng,1))+LOWER(SUBSTRING(EMP.empNameEng,2,LEN(EMP.empNameEng)))) AS ISSUE_NAME ");
                    sql.AppendLine(" , D.DOC_TYPE, MG.NAME DOC_FLOW, R.IS_ID RID, R.SUBJECT, R.REASON_EXPLAIN, R.IS_NO, D.DOC_DETAIL,  D.EFFECTIVE, D.ADD_ORDER_NO, RL.DETAILS RELATION_TEXT ");
                    sql.AppendLine(" , D.PIC_REF_1, D.PIC_REF_2, ATT_DOC_PURCHASE, ATT_DOC_REQUIRE, ATT_DOC_OTHER , ATT_DOC_IMPORTANT");
                    sql.AppendLine(" FROM TBL_TECH_IS_DOCINFO D ");
                    sql.AppendLine(" JOIN TBL_TECH_IS_MAILGRP MG ON MG.ID = D.DOC_FLOW_ID ");
                    sql.AppendLine(" JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID ");
                    sql.AppendLine(" JOIN TBL_TECH_IS_RELATION RL ON RL.ID = D.RELATION_ID ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_employee] EMP ON D.ISSUE_ID = EMP.empId ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT ");
                    sql.AppendLine(" WHERE D.ID = " + Fn.getSQL(id));
                    cmd.CommandText = sql.ToString();

                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        res.ID = dr["ID"].ToString();
                        res.INFO_NO = dr["INFO_NO"].ToString();
                        res.PLANT_DEP = dr["DEP"].ToString();
                        res.ISSUE_NAME = dr["ISSUE_NAME"].ToString();
                        res.DOC_TYPE = dr["DOC_TYPE"].ToString();
                        res.DOC_FLOW_TEXT = dr["DOC_FLOW"].ToString();
                        res.DOC_SUBJECT = dr["SUBJECT"].ToString();
                        res.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                        res.REQUEST_NO = dr["IS_NO"].ToString();
                        res.DOC_DETAIL = dr["DOC_DETAIL"].ToString();
                        res.EFFECTIVE = dr["EFFECTIVE"].ToString();
                        res.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString();
                        res.RELATION_TEXT = dr["RELATION_TEXT"].ToString();
                        res.CC_GRP = dr["CC_GRP"].ToString();
                        res.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                        res.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                        res.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                        res.txt_DOC_IMPORTANT = dr["ATT_DOC_IMPORTANT"].ToString();
                        res.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                        res.txt_DOC_IMPORTANT = dr["ATT_DOC_IMPORTANT"].ToString();
                  
                        REQUEST_ID = dr["RID"].ToString();
                        res.STATUS_ALL = dr["STATUS_ALL"].ToString();

                        //res.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();

                   
                    }

                    dr.Close();

                    List<DocInfoApprove> doc = new List<DocInfoApprove>();
                    sql.Clear();
                    //sql.AppendLine(" SELECT A.*, UPPER(LEFT(EMP.empNameEng,1))+LOWER(SUBSTRING(EMP.empNameEng,2,LEN(EMP.empNameEng))) NAME FROM TBL_TECH_IS_DOCINFO_APPROVE A ");
                    sql.AppendLine(" SELECT A.*, CONCAT(EMP.empTitleEng,' ',UPPER(LEFT(EMP.empNameEng,1))+LOWER(SUBSTRING(EMP.empNameEng,2,LEN(EMP.empNameEng)))) NAME FROM TBL_TECH_IS_DOCINFO_APPROVE A ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_employee] EMP ON EMP.empId = A.APPROVE_ID ");
                    sql.AppendLine(" WHERE DOCINFO_ID = " + Fn.getSQL(id));
                    cmd.CommandText = sql.ToString();
                    dr = cmd.ExecuteReader();
                    while(dr.Read())
                    {
                        DocInfoApprove d = new DocInfoApprove();
                        d.APPROVE_ID = dr["APPROVE_ID"].ToString();
                        d.APPROVE_NAME = dr["NAME"].ToString();
                        d.STATUS = dr["STATUS"].ToString();
                        d.COMMENT = dr["COMMENT"].ToString();
                        if (dr["APPROVE_DATE"].ToString() != "" && dr["APPROVE_DATE"].ToString() != "")
                        {
                            d.APPROVE_DATE = DateTime.Parse(dr["APPROVE_DATE"].ToString());
                        }
                        doc.Add(d);
                    }
                    res.TE_APPROVE_LIST = doc.ToList();

                    dr.Close();
                    if (res.CC_GRP != "" && res.CC_GRP != null && res.CC_GRP != "0")
                    {
                        List<MailGroup> cc_list = new List<MailGroup>();
                        cmd.CommandText = " select USERID from TBL_TECH_IS_MAILGRP_CC where ID = " + Fn.getSQL(res.CC_GRP);
                        string cc_grp = cmd.ExecuteScalar().ToString();
                        sql.Clear();
                        sql.AppendLine(" select '0000' empId, TE_MAIL empEmail, TE_NAME EMP_NAME from TBL_TECH_IS_MAILGRP_CC where ID = " + Fn.getSQL(res.CC_GRP));
                        if (cc_grp != null && cc_grp != "")
                        {
                            sql.AppendLine(" UNION select empId , empEmail, CONCAT(empTitleEng,' ', empNameEng) EMP_NAME from [db_employee].[dbo].[tbl_employee] where empId in (" + cc_grp + ")");
                        }                        
                        cmd.CommandText = sql.ToString();
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            MailGroup m = new MailGroup();
                            m.EMP_NAME = dr["EMP_NAME"].ToString();
                            m.EMP_EMAIL = dr["empEmail"].ToString();
                            cc_list.Add(m);
                        }
                        res.MAIL_GRP = cc_list.ToList();
                    }

                    //if (res.STATUS_ALL == "REVISE")
                    //{ RevDoc rev 
               
                        List<RevDoc> revDoc = new List<RevDoc>();
                        dr.Close();
                        cmd.CommandText = " SELECT INFO_NO, ID, STATUS from TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = " + Fn.getSQL(REQUEST_ID) +
                                          " AND ID <> " + Fn.getSQL(id) + " ORDER BY INFO_NO ASC ";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                             rev = new RevDoc();
                            rev.INFO_ID = dr["ID"].ToString();
                            rev.INFO_NAME = dr["INFO_NO"].ToString();
                            rev.STATUS = dr["STATUS"].ToString();
                            revDoc.Add(rev);
                        }
                        res.RevDoc = revDoc.ToList();

                    dr.Close();
                  
                    int count = 0;
                    cmd.CommandText = $@"SELECT TOP (4) [PATCH]
                                      FROM [db_rtc].[dbo].[TBL_TECH_IS_FILES] where [IS_ID] = '{id}' ";
                    dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        res.txt_ATT_DOC_OTHER = new string[5];
                        while (dr.Read())
                        {

                            if (count < 5)
                            {

                                res.txt_ATT_DOC_OTHER[count] = dr[0].ToString();
                                count++;
                            }
                            else
                            {
                                continue;
                            }

                        }

                    }
                    dr.Close();
                    //}

                }
                con.Close();
            }
            return Json(res, JsonRequestBehavior.AllowGet);
        }

    }
}