using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Text;
using Information_System.Models;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Information_System.Controllers
{
    public class ApproveController : Controller
    {
        // GET: Approve
        InFunction Fn = new InFunction();
        //public string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        public string url = "http://10.145.163.14/te-is";
        public ActionResult ApproveList()
        {
            try
            {
                if (Session["login_id"] != null)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
                    {
                        return View("../Home/Login");
                    }
                    else
                    {
                        return View();
                    }
                }

            }
            catch
            {
                return View("../Home/Login");
            }

            return View("../Home/Login");
        }

        [HttpGet]
        public ActionResult SendEmail(string id)
        {
            List<string> email = new List<string>();
            DocInfoModel model = new DocInfoModel();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine($@"SELECT TOP (1) 
                                       a1.*
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
                          where a3.empId = '{id}' ORDER BY a1.ISSUE_DATE DESC ");
                    cmd.CommandText = sql.ToString();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
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
                        //model.RELATION_ID = dr["REALATION_NAME"].ToString();
                        //model.RELATION_TEXT = dr["RELATION_DETAIL"].ToString() != "" ? dr["RELATION_DETAIL"].ToString() : "-";
                        if (dr["WARNING_DATE"].ToString() != "")
                        {
                            model.WARNNIG_DATE = Convert.ToDateTime(dr["WARNING_DATE"].ToString()) != null ? Convert.ToDateTime(dr["WARNING_DATE"].ToString()) : DateTime.Now;
                        }
                        else
                        {
                            model.WARNNIG_DATE = DateTime.UtcNow;
                        }
                     
                        //email.Add("aodlichao2012@hotmail.com");

                        email.Add(dr["empemail"].ToString());
                    }

                    dr.Close();
                    cmd.CommandText = " SELECT DISTINCT TOP(1) DA.APPROVE_ID,CONCAT(e.empTitleEng,' ',e.empNameEng) APPROVE_NAME, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE , d.Dept_Remark  , e.empEmail2 as Email" +
                                      " FROM TBL_TECH_IS_DOCINFO_APPROVE DA JOIN db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                      " left JOIN db_employee.dbo.tbl_Dept d on e.deptId = d.Dept_ID " +
                                      " WHERE DA.APPROVE_ID = " + Fn.getSQL(id) + " AND APPROVE_LEVEL IS NULL ";
                    dr = cmd.ExecuteReader();

                    List<DocInfoApprove> m = new List<DocInfoApprove>();

                    while (dr.Read())
                    {
                        DocInfoApprove da = new DocInfoApprove();
                        da.APPROVE_NAME = dr["APPROVE_NAME"].ToString() != "" ? dr["APPROVE_NAME"].ToString() : "-";
                        da.APPROVE_EMAIL = dr["Email"].ToString() != "" ? dr["Email"].ToString() : "-";
                        da.APPROVE_ID = dr["APPROVE_ID"].ToString() != "" ? dr["APPROVE_ID"].ToString() : "-";
                        da.COMMENT = dr["COMMENT"].ToString() != "" ? dr["COMMENT"].ToString() : "-";
                        da.STATUS = dr["STATUS"].ToString() != "" ? dr["STATUS"].ToString() : "-";
                        if (da.STATUS != "APPROVED")
                        {
                           
                       
                        }
                        da.APPROVE_DEPT = dr["Dept_Remark"].ToString() != "" ? dr["Dept_Remark"].ToString() : "-";
                        if (dr["APPROVE_DATE"].ToString() != null && dr["APPROVE_DATE"].ToString() != "")
                        {
                            da.APPROVE_DATE = Convert.ToDateTime(dr["APPROVE_DATE"].ToString());
                        }
                        m.Add(da);
                        checkonemorenonApprove_Emailtosend(model.WARNNIG_DATE, model.ID, model.REQUEST_NO, model, da.STATUS, email, da.APPROVE_EMAIL);
                    }
                    model.TE_APPROVE_LIST = m.ToList();

                    List<RevDoc> revDoc = new List<RevDoc>();
                    dr.Close();
                    cmd.CommandText = " SELECT INFO_NO, ID, STATUS from TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = " + Fn.getSQL(model.REQUEST_ID) +
                                      " AND ID <> " + Fn.getSQL(id) + " ORDER BY INFO_NO ASC ";
                    dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        RevDoc rev = new RevDoc();
                        rev.INFO_ID = dr["ID"].ToString();
                        rev.INFO_NAME = dr["INFO_NO"].ToString();
                        rev.STATUS = dr["STATUS"].ToString();
                        revDoc.Add(rev);
                    }
                    model.RevDoc = revDoc.ToList();

                }
                con.Close();
            }
            return RedirectToAction("ReportList"  , "Report");
        }
        [HttpGet]
        public JsonResult getApproveList(string appLevel, string status_option)
        {
            //AutoSendMail a = new AutoSendMail();
            //a.Execute();
            try
            {
                if (Session["emp_id"] != null)
                {
                    List<DocInfoModel> dmList = new List<DocInfoModel>();
                    using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(null, con))
                        {
                            StringBuilder sql = new StringBuilder();
                            sql.AppendLine(" SELECT DISTINCT D.ID, CONCAT( EI.empTitleEng,' ', EI.empNameEng) AS ISSUE_NAME, CONCAT( EC.empTitleEng,' ', EC.empNameEng) AS CREATE_NAME, ");
                            sql.AppendLine(" D.INFO_NO , R.SUBJECT, R.REASON_EXPLAIN, R.APPROVE_STATUS,  D.ISSUE_DATE ");
                            sql.AppendLine(" ,STUFF((SELECT  ',' + FL.PATCH FROM TBL_TECH_IS_FILES FL WHERE FL.IS_ID = R.IS_ID FOR XML PATH('')), 1, 1, '') AS REQ_FILE_LIST ");
                            sql.AppendLine(" FROM TBL_TECH_IS_DOCINFO D INNER JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID ");
                            sql.AppendLine(" INNER JOIN TBL_TECH_IS_DOCINFO_APPROVE A ON D.ID = A.DOCINFO_ID ");
                            sql.AppendLine(" INNER JOIN [db_employee].[dbo].[tbl_employee] EI ON R.ISSUE_ID = EI.empId ");
                            sql.AppendLine(" INNER JOIN [db_employee].[dbo].[tbl_employee] EC ON D.ISSUE_ID = EC.empId ");
                            sql.AppendLine(" WHERE 1=1 ");
                            if (appLevel == "2" || appLevel == "1")
                            {
                                sql.AppendLine(" AND A.STATUS IS NULL ");
                                sql.AppendLine(" AND D.STATUS = 'CREATE'  ");
                            }
                            else if (appLevel == "0")
                            {
                                if (status_option == "Wait Approve")
                                {
                                    //sql.AppendLine(" AND D.STATUS = 'IN-PROGRESS' ");
                                    sql.AppendLine(" AND D.STATUS = 'IN-PROGRESS'  ");
                                }
                            }
                            sql.AppendLine(" AND R.APPROVE_STATUS IN ('APPROVED','NO REQUEST') ");
                            sql.AppendLine(" AND A.APPROVE_ID = " + Fn.getSQL(Session["emp_id"].ToString()));
                            if (appLevel != null && appLevel != "")
                            {
                                if (appLevel == "2")
                                {
                                    sql.AppendLine(" AND EXISTS ( SELECT * FROM TBL_TECH_IS_DOCINFO_APPROVE DA WHERE DA.APPROVE_LEVEL = '1' AND DA.STATUS = 'APPROVED' AND DA.DOCINFO_ID = D.ID) ");
                                }
                                else if (appLevel == "0")
                                {
                                    //if (status_option == "Wait Approve")
                                    //{
                                    //    status_option = null;
                                    //}
                                    if (status_option == "Follow")
                                    {
                                        sql.AppendLine(" AND A.IS_FOLLOW = '1' ");
                                    }
                                    else if (status_option == "Wait Approve")
                                    {
                                        sql.AppendLine(" AND A.STATUS IS NULL ");
                                    }
                                    else
                                    {
                                        sql.AppendLine(" AND A.STATUS = " + Fn.getSQL(status_option));
                                    }
                                }
                            }

                            cmd.CommandText = sql.ToString();
                            SqlDataReader dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                DocInfoModel dm = new DocInfoModel();
                                dm.REQUEST_NAME = dr["ISSUE_NAME"].ToString();
                                dm.ID = dr["ID"].ToString();
                                dm.INFO_NO = dr["INFO_NO"].ToString();
                                dm.DOC_SUBJECT = dr["SUBJECT"].ToString();
                                dm.STATUS = dr["APPROVE_STATUS"].ToString();
                                dm.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                                dm.ISSUE_NAME = dr["CREATE_NAME"].ToString();
                                dm.ISSUE_DATE = DateTime.Parse(dr["ISSUE_DATE"].ToString());
                                dm.REQ_FILE_LIST = dr["REQ_FILE_LIST"].ToString().Split(',').ToList();
                                dmList.Add(dm);
                            }

                        }
                        con.Close();
                    }

                    return Json(dmList, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

            }
         
            return Json("");

        }

        public void checkonemorenonApprove_Emailtosend(DateTime iSSUE_DATE, string id, string req, DocInfoModel dm, string action, List<string> Email , string APPROVE_EMAIL)
        {
            //List<string> CC = new List<string>();
            //CC.Add(APPROVE_EMAIL);
            //DateTime dtst = iSSUE_DATE;
            //DateTime dtNow = DateTime.Now;
            //double sum = double.Parse((dtNow - dtst).TotalDays.ToString());
            //if (sum <= 0.99)
            //{
            //    Console.WriteLine("None");
            //}
            //else
            //{
                string mail = getEmailTemplate(dm);
                Fn.sendMail("Nofitication has item Approve Document non 'APPROVE' ", Email, mail, null, null, action);

            
        }        
        
        public void checkonemorenonApprove_Emailtosend_sceddue(DateTime iSSUE_DATE, string id, string req, DocInfoModel dm, string action, List<string> Email , string APPROVE_EMAIL)
        {
            List<string> CC = new List<string>();
            CC.Add(APPROVE_EMAIL);
            DateTime dtst = iSSUE_DATE;
            DateTime dtNow = DateTime.Now;
            double sum = double.Parse((dtNow - dtst).TotalDays.ToString());
            if (sum <= 0.99)
            {
                Console.WriteLine("None");
            }
            else
            {
                string mail = getEmailTemplate(dm);
                Fn.sendMail("Nofitication has item Approve Document non 'APPROVE' ", Email, mail, null, null, action);
                Console.WriteLine("Send Mail");
            }
            
        }
        [HttpPost]
        public bool setApproveOrReject(string id, string action, string level, string comment)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        string actionStatus = "";
                        if (action == "REJECT")
                        {
                            actionStatus = "REJECT";
                        }
                        else if (action == "Approve" || action == "Accept")
                        {
                            actionStatus = "APPROVED";
                        }
                        else if (action == "PENDING")
                        {
                            actionStatus = "PENDING";
                        }
                        else if (action == "CANCEL")
                        {
                            actionStatus = "CANCEL";
                        }
                        else if (action == "APPROVE_ALL")
                        {
                            actionStatus = "APPROVED";
                        }

                        StringBuilder sql = new StringBuilder();
                        sql.AppendLine(" UPDATE TBL_TECH_IS_DOCINFO_APPROVE SET ");
                        sql.AppendLine(" STATUS = " + Fn.getSQL(actionStatus));
                        sql.AppendLine(" , APPROVE_DATE = GETDATE() ");
                        sql.AppendLine(" , COMMENT = @COMMENT ");
                        sql.AppendLine(" WHERE DOCINFO_ID IN (" + id + ")");
                        sql.AppendLine(" AND APPROVE_ID = " + Fn.getSQL(Session["emp_id"].ToString()));

                        cmd.CommandText = sql.ToString();
                        cmd.Parameters.AddWithValue("@COMMENT", (comment == null) ? (object)DBNull.Value : (object)comment);
                        cmd.ExecuteNonQuery();

                        //Send Mails
                        string subject = "";
                        List<string> ids = id.Split(',').ToList();

                        if (action == "Approve" || action == "Accept" || action == "CANCEL" || action == "REJECT")
                        {
                            foreach (var d in ids)
                            {
                                string template = "";
                                if (action == "Approve" && level == "2")
                                {

                                    cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'TECH-APPROVE' " +
                                                     " , INFO_NO = (SELECT CASE WHEN (SELECT TOP 1 SUBSTRING(INFO_NO, 4, 4) AS INT FROM TBL_TECH_IS_DOCINFO ORDER BY INFO_NO DESC) = (SELECT YEAR(GETDATE())) THEN " +
                                                     " CASE WHEN (select COUNT(*)  from TBL_TECH_IS_DOCINFO) > 0  then (select TOP 1 CONCAT('TE-', YEAR(GETDATE()), FORMAT( CAST(right(INFO_NO,3) AS int)+1, '00#')) from TBL_TECH_IS_DOCINFO ORDER BY INFO_NO DESC) " +
                                                     " ELSE CONCAT('TE-', YEAR(GETDATE()), '001') END ELSE CONCAT('TE-', YEAR(GETDATE()), '001') END AS INFO_NO) " +
                                                     " WHERE ID = " + d;
                                    cmd.ExecuteNonQuery();
                                    //string info_no = "";
                                    //cmd.CommandText = " SELECT INFO_NO FROM TBL_TECH_IS_DOCINFO WHERE ID = " + d;
                                    //if (cmd.ExecuteScalar() != null)
                                    //{
                                    //    info_no = cmd.ExecuteScalar().ToString();
                                    //}

                                    //if (info_no != null && info_no != "")
                                    //{
                                    //    cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'TECH-APPROVE' " +
                                    //                       " WHERE ID = " + d;
                                    //    cmd.ExecuteNonQuery();
                                    //}
                                    //else
                                    //{
                                    //    cmd.CommandText = " DECLARE @no_max int; SELECT TOP 1 @no_max = SUBSTRING(INFO_NO, 8, 3) FROM TBL_TECH_IS_DOCINFO ORDER BY SUBSTRING(INFO_NO, 4, 7) DESC; " +
                                    //                     " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'TECH-APPROVE' " +
                                    //                     " , INFO_NO = (SELECT CASE WHEN (SELECT TOP 1 SUBSTRING(INFO_NO, 4, 4) AS INT FROM TBL_TECH_IS_DOCINFO ORDER BY INFO_NO DESC) = (SELECT YEAR(GETDATE())) THEN " +
                                    //                     " CASE WHEN (select COUNT(*)  from TBL_TECH_IS_DOCINFO) > 0  then (select TOP 1 CONCAT('TE-', YEAR(GETDATE()), FORMAT( CAST(@no_max AS int)+1, '00#')) from TBL_TECH_IS_DOCINFO ORDER BY INFO_NO DESC) " +
                                    //                     " ELSE CONCAT('TE-', YEAR(GETDATE()), '001') END ELSE CONCAT('TE-', YEAR(GETDATE()), '001') END AS INFO_NO) " +
                                    //                     " WHERE ID = " + d;
                                    //    cmd.ExecuteNonQuery();
                                    //}

                                }
                                else if (action == "REJECT")
                                {
                                    cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'TECH-REJECT' " +
                                                      " WHERE ID = " + d;
                                    cmd.ExecuteNonQuery();

                                }
                                else if (action == "CANCEL")
                                {
                                    cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'REJECT' " +
                                                      " WHERE ID = " + d;
                                    cmd.ExecuteNonQuery();
                                }

                                string hrefTo = "";
                                List<string> mailTo = new List<string>();
                                List<string> att = new List<string>();
                                List<MailTo> mt = getMailToList(cmd, actionStatus, level, d);
                                //เพิ่ม CC TE ชั่วคราวเพื่อให้ TE
                                List<string> mailCC_TE = new List<string>();
                             

                                foreach (var m in mt)
                                {
                                    mailTo.Add(m.EMP_EMAIL);
                                    mailTo.Add("aodlichao2012@hotmail.com");
                                }

                                DocInfoModel dm = getDocInfoModelInfor(d, level);
                                if (action == "Approve" || action == "Accept")
                                {
                                    if (level == "1")
                                    {
                                       
                                        subject = "Information System: Information Document is waiting for your approval.";
                                        template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Information Document is waiting for your approval.</p>";
                                        hrefTo = " <a href='" + "http://10.145.163.14/te-is" + "/Approve/ApproveList/" + dm.ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                        template += getEmailTemplate(dm);
                                        template += hrefTo;


                                    }
                                    else if (level == "2")
                                    {
                                        
                                        mailCC_TE.Add("wandeep@citizen.co.jp");
                                        mailCC_TE.Add("wiparats@citizen.co.jp");
                                        mailCC_TE.Add("jariyac@citizen.co.jp");
                                        subject = "Information system: The information document has been approved.";
                                        template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Documents are waiting for you to save and send to approver list.</p>";
                                        hrefTo = " <a href='" + "http://10.145.163.14/te-is" + "/Create/CreateInfoDoc/" + dm.REQUEST_ID + "?info_id=" + dm.ID + "''>Click link to Information System </a>" +
                                               " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                        template += getEmailTemplate(dm);
                                        template += hrefTo;


                                    }
                                    else
                                    {
                                        cmd.CommandText = $@"SELECT APPROVE_DATE
                                                  FROM  [db_rtc].[dbo].[TBL_TECH_IS_DOCINFO_APPROVE] a2
                                                   where a2.APPROVE_DATE is null AND a2.DOCINFO_ID = {id}";


                                        SqlDataReader reader = cmd.ExecuteReader();
                                        if (reader.Read())
                                        {
                                            while (reader.Read())
                                            {
                                                if (reader["APPROVE_DATE"].ToString() == "")
                                                {

                                                }
                                                else
                                                {
                                                    saveInformation_inprogress(id , dm);
                                                    TempData["statusApprove"] = " successfully!";
                                                    return true;
                                                }
                                            }
                                         
                                        }
                                        else
                                        {
                                            saveInformation_inprogress(id , dm);
                                            TempData["statusApprove"] = " successfully!";
                                            return true;
                                        }
                                        //}
                                        //try
                                        //{
                                        //    if (reader["APPROVE_DATE"] != null)
                                        //    {

                                        //    }
                                        //    else
                                        //    {
                                        //        saveInformation_inprogress(id);
                                        //    }
                                        //}
                                        //catch
                                        //{
                                        //    saveInformation_inprogress(id);
                                        //}

                                        //subject = "Information system: The information document has been approved.";
                                        //template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Documents are waiting for you to save and send to approver list.</p>";
                                        //hrefTo = " <a href='" +  "http://10.145.163.14/te-is" + "/Approve/ApproveList/" + dm.ID +  "'>Click link to Information System </a>" +
                                        //         " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                        //Fn.sendMail(subject, mailTo, template, att, null);
                                        TempData["statusApprove"] = " successfully!";
                                        return true;
                                    }

                                    if (dm.txt_PIC_REF_1 != null && dm.txt_PIC_REF_1 != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_1);
                                    }
                                    if (dm.txt_PIC_REF_2 != null && dm.txt_PIC_REF_2 != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_2);
                                    }
                                    if (dm.txt_ATT_DOC_PURCHASE != null && dm.txt_ATT_DOC_PURCHASE != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_PURCHASE);
                                    }
                                    if (dm.txt_ATT_DOC_REQUIRE != null && dm.txt_ATT_DOC_REQUIRE != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_REQUIRE);
                                    }
                                    if (dm.txt_DOC_IMPORTANT != null && dm.txt_DOC_IMPORTANT != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_DOC_IMPORTANT);
                                    }
                                    if (dm.txt_ATT_DOC_OTHER != null )
                                    {
                                        foreach (var item in dm.txt_ATT_DOC_OTHER)
                                        {
                                            if (item != null && item != "")
                                            {
                                                att.Add(Server.MapPath("~/Temp_File/").ToString() + item);
                                            }

                                        }
                                        Fn.sendMail(subject, mailTo, template, att, mailCC_TE);
                                    }
                                    else
                                    {
                                        Fn.sendMail(subject, mailTo, template, null, mailCC_TE);
                                    }
                              

                                    TempData["statusApprove"] = " successfully!";


                                }
                                else if (action == "REJECT")
                                {
                                    mailCC_TE.Add("wandeep@citizen.co.jp");
                                    mailCC_TE.Add("wiparats@citizen.co.jp");
                                    mailCC_TE.Add("jariyac@citizen.co.jp");


                                    if (dm.txt_PIC_REF_1 != null && dm.txt_PIC_REF_1 != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_1);
                                    }
                                    if (dm.txt_PIC_REF_2 != null && dm.txt_PIC_REF_2 != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_2);
                                    }
                                    if (dm.txt_ATT_DOC_PURCHASE != null && dm.txt_ATT_DOC_PURCHASE != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_PURCHASE);
                                    }
                                    if (dm.txt_ATT_DOC_REQUIRE != null && dm.txt_ATT_DOC_REQUIRE != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_REQUIRE);
                                    }
                                    if (dm.txt_DOC_IMPORTANT != null && dm.txt_DOC_IMPORTANT != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_DOC_IMPORTANT);
                                    }
                                    if (dm.txt_ATT_DOC_OTHER != null)
                                    {
                                        foreach (var item in dm.txt_ATT_DOC_OTHER)
                                        {
                                            if (item != null && item != "")
                                            {
                                                att.Add(Server.MapPath("~/Temp_File/").ToString() + item);
                                            }

                                        }
                                    }
                                    else
                                    {
                                    }

                                    hrefTo = " <a href='" + "http://10.145.163.14/te-is" + "/Create/CreateInfoDoc/" + dm.ID + "?info_id=" + dm.ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "Information system: The Information document has been rejected.";
                                    template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Documents has been rejected.</p>";
                                    template += getEmailTemplate(dm);
                                    template += hrefTo;
                                    TempData["statusReject"] = " successfully!";

                                    Fn.sendMail(subject, mailTo, template, att, mailCC_TE);
                                }
                                else if (action == "CANCEL")
                                {
                                    //เพิ่ม CC TE ชั่วคราวเพื่อให้ TE
                                    mailCC_TE.Add("wandeep@citizen.co.jp");
                                    mailCC_TE.Add("wiparats@citizen.co.jp");
                                    mailCC_TE.Add("jariyac@citizen.co.jp");

                                    hrefTo = " <a href='" + "http://10.145.163.14/te-is" + "/Create/CreateInfoDoc/" + dm.ID + "?info_id=" + dm.ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "Information system: The Information document has been cancel.";
                                    template += "<p><b>Dear All </b></p><p>Documents has been cancel.</p>";
                                    template += hrefTo;
                                    TempData["statusReject"] = " successfully!";
                                }
                             
                               
                                //template += getEmailTemplate(dm);
                                //template += hrefTo;

                                //Fn.sendMail(subject, mailTo, template, att, mailCC_TE);
                            }
                         
                        }
                        else if (action == "APPROVE_ALL")
                        {
                            foreach (var d in ids)
                            {
                                string Doc_info = null;
                                cmd.CommandText = " SELECT DOCINFO_ID FROM TBL_TECH_IS_DOCINFO_APPROVE WHERE DOCINFO_ID = " + d +
                                                  " AND (STATUS IN ('REJECT','PENDING') OR (STATUS IS NULL)) ";
                                if (cmd.ExecuteScalar() != null)
                                {
                                    Doc_info = cmd.ExecuteScalar().ToString();
                                }
                                if (Doc_info == null)
                                {
                                    cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'APPROVED' " +
                                                      " WHERE ID = " + d;
                                    cmd.ExecuteNonQuery();
                                    List<string> mailTo = new List<string>();
                                    List<MailTo> mt = getMailToList(cmd, actionStatus, level, d);
                                    DocInfoModel dm = getDocInfoModelInfor(d, level);
                                    string template = "";
                                    string hrefTo = "";
                                    foreach (var m in mt)
                                    {
                                        mailTo.Add(m.EMP_EMAIL);
                                    }
                                    hrefTo = " <a href='" + "http://10.145.163.14/te-is" + "/Approve/InformationFormat/" + dm.ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "Information system: The information document has already been approved by all approvers.";
                                    template += "<p><b>Dear All </b></p><p>Documents has been approved by all approvers.</p>";
                                    template += "<p> Information ID: <b>" + dm.INFO_NO + "</b><p>";
                                    template += getEmailTemplate(dm);
                                    template += hrefTo;

                                    Fn.sendMail(subject, mailTo, template);
                                }
                            }

                            TempData["statusAction"] = " successfully!";
                        }
                        else
                        {
                            TempData["statusAction"] = " successfully!";
                        }

                    }
                    con.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string getEmailTemplate(DocInfoModel model)
        {
            string html = null;
            try
            {
                html += $@"<p>Information ID : {model.INFO_NO} </p><br />";
                html += "<table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'>" +
                " <tr> <td height ='35' colspan='3' align='center' style='background-color:#65A4FF;'><b>Information System</b></td> </tr>" +
                " <tr><td width='20%'>Plant/Department</td> <td colspan='2'>" + model.PLANT_DEP + "</td></tr><tr><td width='20%'>Issue Name</td> <td colspan='2'>" + model.ISSUE_NAME + "</td></tr> <tr><td width='20%'>Issue Date</td> <td colspan='2'>" + model.ISSUE_DATE.ToString("dd-MM-yyyy") + "</td></tr>" +
                " <tr><td width='20%'>Document Type</td> <td colspan='2'>" + model.DOC_TYPE + "</td></tr>" +
                " <tr><td width='20%'>Document flow</td> <td colspan='2'>" + model.DOC_FLOW_TEXT + "</td></tr>" +
                " <tr><td width='20%'>Request By</td> <td colspan='2'>" + model.REQUEST_NAME + "</td></tr>" +
                " <tr><td width='20%'>Reson Explain</td> <td colspan='2'>" + model.REASON_EXPLAIN + "</td></tr> <tr><td width='20%'>Detail Reference</td> <td colspan='2'>" + model.DETAIL_REF + "</td></tr>" +
                " <tr><td width='20%'>Document Detail</td> <td colspan='2'>" + model.DOC_DETAIL + "</td></tr> <tr><td width='20%'>Effective</td> <td colspan='2'>" + model.EFFECTIVE + "</td></tr>" +
                " <tr><td width='20%'>Add order no </td> <td colspan='2'>" + model.ADD_ORDER_NO + "</td></tr>" +
                " </table><br/>" +
                " <table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'> " +
                " <thead style='background-color:#65A4FF;'><tr><th>Approve Name</th><th>Approve Date</th><th>Status</th></tr></thead> " +
                " <tbody class='table-body' id='tbody'>";
                if(model.TE_APPROVE_LIST != null)
                {
                    foreach (var l in model.TE_APPROVE_LIST)
                    {
                        html += " <tr> <td> " + l.APPROVE_NAME + " </td><td>  " + l.APPROVE_DATE + " </td><td>  " + l.STATUS + " </td></tr>";
                    }
                }
                else
                {
                    html += " <tr> <td>  </td><td>  </td><td>  </td></tr>";
                }

                //html += " <tr> <td> " + model.APPROVE_NAME + " </td><td>  " + model.ISSUE_DATE + " </td><td>  " + model.STATUS + " </td></tr>";
                html += "</tbody> " +
                " </table><br/>";
              

                return html;
            }
            catch (Exception ex)
            {

                return html;
            }
        }

        public DocInfoModel getDocInfoModelInfor(string id, string level)
        {
            DocInfoModel dm = new DocInfoModel();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    try
                    {
                        cmd.CommandText = " SELECT mg.NAME DOC_FLOW, DO.*, r.*, e.empTitleEng + ' ' +e.empNameEng ISSUE_NAME , CONCAT(p.plant_mark,'/',d.Dept_Remark) plant_dep, er.empTitleEng + ' ' +er.empNameEng request_name , DO.ATT_DOC_OTHER FROM TBL_TECH_IS_DOCINFO DO " +
                                          " join TBL_TECH_IS_REQUEST r on r.IS_ID = DO.REQUEST_NO left join db_employee.dbo.tbl_plant p on p.plant_id = DO.PLANT left join db_employee.dbo.tbl_Dept d on d.Dept_ID = DO.DEPARTMENT" +
                                          " left join db_employee.dbo.tbl_employee e on e.empId = DO.ISSUE_ID join db_employee.dbo.tbl_employee er on er.empId = r.ISSUE_ID join TBL_TECH_IS_MAILGRP mg on mg.ID = DOC_FLOW_ID " +
                                          " WHERE DO.ID = " + id;
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            dm.ID = dr["ID"].ToString();
                            dm.INFO_NO = dr["INFO_NO"].ToString();
                            dm.ISSUE_NAME = dr["ISSUE_NAME"].ToString();
                            dm.ISSUE_DATE = Convert.ToDateTime(dr["ISSUE_DATE"].ToString());
                            dm.DOC_TYPE = dr["DOC_TYPE"].ToString();
                            dm.DOC_FLOW_TEXT = dr["DOC_FLOW"].ToString();
                            dm.DETAIL_REF = dr["IS_NO"].ToString();
                            dm.APPROVED_FLAX = dr["APPROVED_FLAX"].ToString();
                            dm.ACKNOWLAGE_FLEX = dr["ACKNOWLAGE_FLEX"].ToString();
                            dm.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                            dm.DOC_DETAIL = dr["DOC_DETAIL"].ToString();
                            dm.EFFECTIVE = dr["EFFECTIVE"].ToString();
                            dm.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString();
                            dm.RELATION_ID = dr["RELATION_ID"].ToString();
                            dm.PLANT_DEP = dr["plant_dep"].ToString();
                            dm.CC_GRP = dr["CC_GRP"].ToString();
                            dm.DOC_FLOW_ID = dr["DOC_FLOW_ID"].ToString();
                            dm.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                            dm.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                            dm.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                            dm.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                            dm.txt_DOC_IMPORTANT = dr["ATT_DOC_IMPORTANT"].ToString();
                            dm.REQUEST_NAME = dr["request_name"].ToString();
                            dm.REQUEST_ID = dr["IS_ID"].ToString();

                        }
                        dr.Close();
                        int count = 0;
                        cmd.CommandText = $@"SELECT  [PATCH]
                                      FROM [db_rtc].[dbo].[TBL_TECH_IS_FILES] where [IS_ID] = {id} AND [PATCH] is not null ";
                        dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            dm.txt_ATT_DOC_OTHER = new string[5];
                            while (dr.Read())
                            {

                                dm.txt_ATT_DOC_OTHER[count] = dr[0].ToString();
                                count++;
                            }
                        }

                        dr.Close();
                        string queryLevel = "";
                        if (level == "1")
                        {
                            queryLevel = " and DA.APPROVE_LEVEL IN ('1') ";
                        }
                        else if (level == "2")
                        {
                            queryLevel = " and DA.APPROVE_LEVEL IN ('1', '2') ";
                        }
                        //else
                        //{
                        //    queryLevel = " and DA.APPROVE_LEVEL IN ('1', '2') ";
                        //}
                        cmd.CommandText = " SELECT DA.APPROVE_ID,CONCAT(e.empTitleEng,' ',e.empNameEng) APPROVE_NAME, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE " +
                                          " FROM TBL_TECH_IS_DOCINFO_APPROVE DA JOIN db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                          " WHERE DOCINFO_ID = " + id + queryLevel;
                        dr = cmd.ExecuteReader();
                        List<DocInfoApprove> m = new List<DocInfoApprove>();
                        while (dr.Read())
                        {
                            DocInfoApprove da = new DocInfoApprove();
                            da.APPROVE_NAME = dr["APPROVE_NAME"].ToString();
                            da.APPROVE_ID = dr["APPROVE_ID"].ToString();
                            da.COMMENT = dr["COMMENT"].ToString();
                            da.STATUS = dr["STATUS"].ToString();
                            if (dr["APPROVE_DATE"].ToString() != null && dr["APPROVE_DATE"].ToString() != "")
                            {
                                da.APPROVE_DATE = Convert.ToDateTime(dr["APPROVE_DATE"].ToString());
                            }

                            m.Add(da);
                        }
                        dm.TE_APPROVE_LIST = m.ToList();



                    }
                    catch (Exception ex)
                    {
                        Response.Write(ex.Message);
                        con.Close();
                    }
                }
                con.Close();
            }
            return dm;
        }

        [HttpGet]
        public List<MailTo> getMailToList(SqlCommand cmds, string action, string level, string id)
        {
            List<MailTo> mtList = new List<MailTo>();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {



                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    if (action == "APPROVED" && level == "1")
                    {   //send to Kobayashi
                        cmd.CommandText = " SELECT e.empNameEng, e.empEmail, e.empId FROM TBL_TECH_IS_DOCINFO_APPROVE DA " +
                                          " join db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                          " WHERE DOCINFO_ID = " + id +
                                          " AND DA.APPROVE_LEVEL = '2'";
                    }
                    else if (action == "APPROVED" || action == "REJECT")
                    {  //send to Wiparat
                        cmd.CommandText = " SELECT e.empNameEng, e.empEmail, e.empId FROM TBL_TECH_IS_DOCINFO DO join db_employee.dbo.tbl_employee e on e.empId = DO.ISSUE_ID " +
                                          " WHERE ID = " + id;
                    }
                    else if (action == "CANCEL")
                    {
                        cmd.CommandText = " SELECT e.empNameEng, e.empEmail, e.empId FROM TBL_TECH_IS_DOCINFO_APPROVE DA " +
                                          " join db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                          " WHERE DOCINFO_ID = " + id;
                    }

                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MailTo mt = new MailTo();
                        mt.EMP_ID = dr["empId"].ToString();
                        mt.EMP_NAME = dr["empNameEng"].ToString();
                        mt.EMP_EMAIL = dr["empEmail"].ToString();
                        mtList.Add(mt);
                    }
                    con.Close();

                   
             
                    return mtList;
                }
                //con.Close();
            }

        }

        public ActionResult InformationFormat(string id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                List<string> email = new List<string>();
                DocInfoModel model = new DocInfoModel();
                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        StringBuilder sql = new StringBuilder();
                        sql.AppendLine(" SELECT DI.ID DIID, DI.INFO_NO, R.SUBJECT, DI.STATUS, CONCAT(P.plant_mark,'/',D.Dept_Remark) PD, R.REASON_EXPLAIN, R.IS_NO, R.IS_ID, DI.ISSUE_DATE, CONCAT(EMI.empTitleEng,' ',EMI.empNameEng) ISSUE_NAME ");
                        sql.AppendLine(" , CONCAT(EMR.empTitleEng,' ',EMR.empNameEng) CHECK_NAME, CONCAT(EMA.empTitleEng,' ',EMA.empNameEng) APP_NAME, DI.DOC_DETAIL ");
                        sql.AppendLine(" , DI.PIC_REF_1, DI.PIC_REF_2, ATT_DOC_PURCHASE, ATT_DOC_REQUIRE, ATT_DOC_OTHER, ATT_DOC_IMPORTANT , DI.EFFECTIVE, DI.ADD_ORDER_NO, RL.NAME REALATION_NAME,RL.DETAILS RELATION_DETAIL  , EMI.EmpEmail as empemail");
                        sql.AppendLine(" FROM TBL_TECH_IS_DOCINFO DI ");
                        sql.AppendLine(" JOIN TBL_TECH_IS_REQUEST R ON DI.REQUEST_NO = R.IS_ID ");
                        sql.AppendLine(" JOIN TBL_TECH_IS_DOCINFO_APPROVE DA ON DI.ID = DA.DOCINFO_ID ");
                        sql.AppendLine(" JOIN TBL_TECH_IS_DOCINFO_APPROVE DA2 ON DI.ID = DA2.DOCINFO_ID ");
                        sql.AppendLine(" JOIN TBL_TECH_IS_RELATION RL ON DI.RELATION_ID = RL.ID ");
                        sql.AppendLine(" JOIN db_employee.dbo.tbl_employee EMI ON DI.ISSUE_ID = EMI.empId ");
                        sql.AppendLine(" JOIN db_employee.dbo.tbl_employee EMR ON DA.APPROVE_ID = EMR.empId AND DA.APPROVE_LEVEL = '1' ");
                        sql.AppendLine(" JOIN db_employee.dbo.tbl_employee EMA ON DA2.APPROVE_ID = EMA.empId AND DA2.APPROVE_LEVEL = '2' ");
                        sql.AppendLine(" JOIN db_employee.dbo.tbl_plant P ON P.plant_id = DI.PLANT ");
                        sql.AppendLine(" JOIN db_employee.dbo.tbl_Dept D ON D.Dept_ID = DI.DEPARTMENT ");
                        sql.AppendLine(" WHERE DI.ID =" + Fn.getSQL(id));
                        cmd.CommandText = sql.ToString();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            model.ID = dr["DIID"].ToString();
                            model.INFO_NO = dr["INFO_NO"].ToString() != "" ? dr["INFO_NO"].ToString() : "-";
                            model.REQUEST_ID = dr["IS_ID"].ToString();
                            model.STATUS_ALL = dr["STATUS"].ToString();
                            model.DOC_SUBJECT = dr["SUBJECT"].ToString() != "" ? dr["SUBJECT"].ToString() : "-";
                            model.PLANT_DEP = dr["PD"].ToString() != "" ? dr["PD"].ToString() : "-";
                            model.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString() != "" ? dr["REASON_EXPLAIN"].ToString() : "-";
                            model.DETAIL_REF = dr["IS_NO"].ToString() != "" ? dr["IS_NO"].ToString() : "-";
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
                            model.EFFECTIVE = dr["EFFECTIVE"].ToString() != "" ? dr["EFFECTIVE"].ToString() : "-";
                            model.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString() != "" ? dr["ADD_ORDER_NO"].ToString() : "-";
                            model.RELATION_ID = dr["REALATION_NAME"].ToString();
                            model.RELATION_TEXT = dr["RELATION_DETAIL"].ToString() != "" ? dr["RELATION_DETAIL"].ToString() : "-";
                            //email.Add("aodlichao2012@hotmail.com");
                            email.Add(dr["empemail"].ToString());
                            int count = 0;
                            dr.Close();
                            cmd.CommandText = $@"SELECT TOP (1000) [IS_ID]
                                                              ,[PATCH]
                                                          FROM [db_rtc].[dbo].[TBL_TECH_IS_FILES]
                                                          where IS_ID  =  '{id}'  ";
                            dr = cmd.ExecuteReader();
                            if (dr.HasRows)
                            {
                                model.txt_ATT_DOC_OTHER = new string[5];
                                while (dr.Read())
                                {

                                    if (count < 5)
                                    {

                                        model.txt_ATT_DOC_OTHER[count] = dr[1].ToString();
                                        count++;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                }

                            }
                            dr.Close();
                       
                        }

                        dr.Close();
                        cmd.CommandText = " SELECT DA.APPROVE_ID,CONCAT(e.empTitleEng,' ',e.empNameEng) APPROVE_NAME, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE , d.Dept_Remark  , e.empEmail2 as Email" +
                                          " FROM TBL_TECH_IS_DOCINFO_APPROVE DA JOIN db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                          " left JOIN db_employee.dbo.tbl_Dept d on e.deptId = d.Dept_ID " +
                                          " WHERE DOCINFO_ID = " + Fn.getSQL(id) + " AND APPROVE_LEVEL IS NULL ";
                        dr = cmd.ExecuteReader();

                        List<DocInfoApprove> m = new List<DocInfoApprove>();
                      
                        while (dr.Read())
                        {
                            DocInfoApprove da = new DocInfoApprove();
                            da.APPROVE_NAME = dr["APPROVE_NAME"].ToString() != "" ? dr["APPROVE_NAME"].ToString() : "-";
                            da.APPROVE_EMAIL = dr["Email"].ToString() != "" ? dr["Email"].ToString() : "-";
                            da.APPROVE_ID = dr["APPROVE_ID"].ToString() != "" ? dr["APPROVE_ID"].ToString() : "-";
                            da.COMMENT = dr["COMMENT"].ToString() != "" ? dr["COMMENT"].ToString() : "-";
                            da.STATUS = dr["STATUS"].ToString() != "" ? dr["STATUS"].ToString() : "-";
                  
                            da.APPROVE_DEPT = dr["Dept_Remark"].ToString() != "" ? dr["Dept_Remark"].ToString() : "-";
                            if (dr["APPROVE_DATE"].ToString() != null && dr["APPROVE_DATE"].ToString() != "")
                            {
                                da.APPROVE_DATE = Convert.ToDateTime(dr["APPROVE_DATE"].ToString());
                            }
                            m.Add(da);
                        }
                        model.TE_APPROVE_LIST = m.ToList();

                        List<RevDoc> revDoc = new List<RevDoc>();
                        dr.Close();
                        cmd.CommandText = " SELECT INFO_NO, ID, STATUS from TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = " + Fn.getSQL(model.REQUEST_ID) +
                                          " AND ID <> " + Fn.getSQL(id) + " ORDER BY INFO_NO ASC ";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            RevDoc rev = new RevDoc();
                            rev.INFO_ID = dr["ID"].ToString();
                            rev.INFO_NAME = dr["INFO_NO"].ToString();
                            rev.STATUS = dr["STATUS"].ToString();
                            revDoc.Add(rev);
                        }
                        model.RevDoc = revDoc.ToList();

                    }
                    con.Close();
                }

                return View(model);
            }
        }

        public bool setFollowUnfollow(string id, string action)
        {
            List<string> ids = id.Split(',').ToList();
            string isFollow;
            if (action == "Follow") { isFollow = "1"; } else { isFollow = "0"; }
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                try
                {
                    foreach (var l in ids)
                    {
                        SqlCommand cmd = new SqlCommand(null, con);
                        cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO_APPROVE " +
                                          " SET IS_FOLLOW = " + Fn.getSQL(isFollow) +
                                          " WHERE APPROVE_ID = " + Fn.getSQL(Session["emp_id"].ToString()) +
                                          " AND DOCINFO_ID = " + l;
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    con.Close();
                    return false;
                }
                //con.Close();
            }
        }

        public ActionResult ApproveRequirement()
        {
            try
            {
                if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
                {
                    return View("../Home/Login");
                }
                else
                {
                    List<RequestDocModel> model_list = new List<RequestDocModel>();
                    string person_id = Session["emp_id"].ToString();
                    using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(null, con))
                        {
                            SqlDataReader reader = null;
                            cmd.CommandText = " SELECT r.*, CONCAT(EMP.empTitleEng,' ', emp.empNameEng) AS ISSUE_NAME, EMP.empEmail, EMP.empEmail, CONCAT(P.plant_mark,'/',D.Dept_Remark) DEP FROM TBL_TECH_IS_REQUEST r " +
                                              " JOIN TBL_TECH_IS_REQUEST_APPROVE RA ON R.IS_ID = RA.REQUEST_ID LEFT JOIN db_employee.dbo.tbl_employee emp on emp.empId = r.ISSUE_ID " +
                                              " JOIN db_employee.dbo.tbl_plant P ON P.plant_id = R.PLANT JOIN db_employee.dbo.tbl_Dept D ON D.Dept_ID = R.DEPARTMENT " +
                                              " WHERE APPROVE_STATUS = 'CREATE' " +
                                              " AND RA.APPROVE_ID = " + Fn.getSQL(person_id) +
                                              " AND RA.STATUS = 'WAITING' " +
                                              " ORDER BY ISSUE_DATE ASC ";
                            reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                RequestDocModel model = new RequestDocModel();
                                model.IS_ID = reader["IS_ID"].ToString();
                                model.IS_NO = reader["IS_NO"].ToString();
                                model.DEPARTMENT = reader["DEP"].ToString();
                                model.SUBJECT = reader["SUBJECT"].ToString();
                                model.REASON_EXPLAIN = reader["REASON_EXPLAIN"].ToString();
                                model.START_PROD_MONTH = reader["START_PROD_MONTH"].ToString();
                                model.DOCUMENT_DATA_OF_CHANGE = reader["DOCUMENT_DATA_OF_CHANGE"].ToString();
                                model.PARTS_CODE = reader["PARTS_CODE"].ToString();
                                model.WATCH_CODE = reader["WATCH_CODE"].ToString();
                                model.CLASSIFICATION = reader["CLASSIFICATION"].ToString();
                                model.ISSUE_ID = reader["ISSUE_ID"].ToString();
                                model.ISSUE_DATE = Convert.ToDateTime(reader["ISSUE_DATE"]);
                                model.ISSUE_NAME = reader["ISSUE_NAME"].ToString();
                                model.APPROVE_STATUS = reader["APPROVE_STATUS"].ToString();
                                model.ISSUE_EMAIL = reader["empEmail"].ToString();
                                model_list.Add(model);
                            }
                            reader.Close();
                        }
                        con.Close();
                    }
                    if (model_list.Count != 0)
                    {
                        ViewData["RequirementList"] = model_list;
                    }

                    return View();
                }
            }
            catch
            {
                return View("../Home/Login");
            }
         

        }

        [HttpGet]
        public JsonResult getComment(string id)
        {
            List<CommentModel> cmList = new List<CommentModel>();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    cmd.CommandText = " SELECT C.ID, C.IS_ID, C.COMMENT, C.FILES, format(C.CREATED_DATE,'dd MMM yyyy HH:mm') CREATED_DATE , D.Dept_Remark " +
                                      " , CONCAT(UPPER(LEFT(EMPNAMEENG,1))+LOWER(SUBSTRING(EMPNAMEENG,2,LEN(EMPNAMEENG))),' ',UPPER(LEFT(EMPLNAMEENG,1))+LOWER(SUBSTRING(EMPLNAMEENG,2,LEN(EMPLNAMEENG)))) NAME " +
                                      " FROM TBL_TECH_IS_COMMENT C JOIN db_employee.DBO.TBL_EMPLOYEE E ON C.CREATED_BY = E.EMPID " +
                                      " JOIN db_employee.DBO.tbl_Dept D ON C.DEPT = D.Dept_ID " +
                                      " WHERE IS_ID = " + Fn.getSQL(id) +
                                      " ORDER BY CREATED_DATE ASC ";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        CommentModel cm = new CommentModel();
                        cm.COMMENT_ID = dr["ID"].ToString();
                        cm.IS_ID = dr["IS_ID"].ToString();
                        cm.COMMENT = dr["COMMENT"].ToString();
                        cm.CREATED_DATE = dr["CREATED_DATE"].ToString();
                        cm.DEPT = dr["Dept_Remark"].ToString();
                        cm.CREATED_BY = dr["NAME"].ToString();
                        cm.FILES = dr["FILES"].ToString().Split(',').ToList();
                        cmList.Add(cm);
                    }
                }
                con.Close();
            }
            return Json(cmList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public bool setComment(CommentModel cm, string IS_NO)
        {
            try
            {
                string txtfiles = "";
                string mailToName = "";
                string sendto, sendCC;
                List<string> mailTo = new List<string>();
                List<string> mailCC = new List<string>();
                AccessRightModel p = Session["permission"] as AccessRightModel;

                Random generator = new Random();
                string random = generator.Next(0, 1000000).ToString("D6");
                HttpFileCollectionBase files = Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    string filePart = Server.MapPath("~/Temp_Comment/").ToString();
                    HttpPostedFileBase file = files[i];
                    string fname;
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string tempFilename = file.FileName.Replace(" ", "_");
                        string[] IEfiles = tempFilename.Split(new char[] { '\\' });
                        fname = IEfiles[IEfiles.Length - 1];
                    }
                    else
                    {
                        fname = random + "_" + file.FileName.Replace(" ", "_");
                    }
                    file.SaveAs(filePart + fname);

                    string ftp = "ftp://10.145.163.10/";
                    string ftpFolder = "IS/IS_COMMENT/";
                    using (var client = new WebClient())
                    {
                        string uriDestination = ftp + ftpFolder + fname;
                        string uriSource = filePart + fname;
                        client.Credentials = new NetworkCredential("sodick", "Rtc0000", "rtc");
                        client.UploadFile(uriDestination, WebRequestMethods.Ftp.UploadFile, uriSource);
                    }

                    if ((System.IO.File.Exists(filePart + fname)))
                    {
                        System.IO.File.Delete(filePart + fname);
                    }

                    if (txtfiles != "")
                    {
                        txtfiles += "," + fname;
                    }
                    else
                    {
                        txtfiles += fname;
                    }
                }

                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        cmd.CommandText = " INSERT INTO TBL_TECH_IS_COMMENT (ID, IS_ID, CREATED_BY, CREATED_DATE, COMMENT, FILES, DEPT) " +
                                          " VALUES (NEWID(),'" + cm.IS_ID + "','" + Session["emp_id"].ToString() + "',GETDATE(), @COMMENT, @txtfiles " +
                                          " , '" + Session["dep_id"].ToString() + "')";
                        cmd.Parameters.AddWithValue("@COMMENT", cm.COMMENT);
                        cmd.Parameters.AddWithValue("@txtfiles", txtfiles);
                        cmd.ExecuteNonQuery();

                        if (p.IS_TE)
                        {
                            sendto = "IS_IT";
                            sendCC = "IS_TE";
                        }
                        else
                        {
                            sendto = "IS_TE";
                            sendCC = "IS_IT";
                        }

                        StringBuilder sql = new StringBuilder();
                        SqlDataReader dr;
                        sql.Clear();
                        sql.AppendLine(" SELECT empEmail, CONCAT(empTitleEng,empNameEng) name FROM db_employee.dbo.tbl_employee e ");
                        sql.AppendLine(" join db_employee.dbo.tbl_access_right a on e.empID = a.empID ");
                        sql.AppendLine(" where " + sendto + " = 1 ");
                        cmd.CommandText = sql.ToString();
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            mailTo.Add(dr["empEmail"].ToString());
                            if (mailToName != "")
                            {
                                mailToName += ", " + dr["name"].ToString();
                            }
                            else
                            {
                                mailToName += dr["name"].ToString();
                            }
                        }

                        dr.Close();
                        sql.Clear();
                        sql.AppendLine(" SELECT empEmail FROM db_employee.dbo.tbl_employee e ");
                        sql.AppendLine(" join db_employee.dbo.tbl_access_right a on e.empID = a.empID ");
                        sql.AppendLine(" where " + sendCC + " = 1 ");
                        cmd.CommandText = sql.ToString();
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            mailCC.Add(dr["empEmail"].ToString());
                        }
                    }
                    con.Close();
                }

                string subject = "Information System: [" + IS_NO + "] " + sendCC.Substring(3, 2).ToString() + " commented in document";
                string template = "";
                template += "<div style='border: 1px solid #999999; width: 50%; margin: 0px auto;'><p style='background-color: lightcoral; font-size:24px;'><b>INFORMATION SYSTEM</b></p> ";
                template += "<p><b>Dear " + mailToName + "</b></p><p>" + sendCC.Substring(3, 2).ToString() + " commenting in Information System</p> <p>" + sendCC.Substring(3, 2).ToString() + " Say: <b> " + cm.COMMENT + " </b></p> ";
                template += "<a href='" + url + "/TE-IS/Approve/InformationFormat/" + cm.IS_ID + "'> Click the link to view more comment </a><p></p></div>"; ;

                mailCC.Add("ssimsystem@citizen.co.jp");
                Fn.sendMail(subject, mailTo, template, null, mailCC);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("The deletion failed: {0}", ex.Message);
                return false;
            }
        }

        public bool saveInformation_inprogress(string id , DocInfoModel doc)
        {
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    SqlDataReader dr = null;
                    try
                    {
                        List<string> mailTo = new List<string>();
                        List<string> mailToName = new List<string>();
                        List<string> att = new List<string>();
                        List<string> mailCC = new List<string>();
                        string mail_cc_list = "";
                        //List<string> approveList = doc.APPROVE_LIST.Split(',').ToList();
                        string subject = "";
                        string template = "";
                        string sendToN = null;

                        //CC Mail
                        string subject_cc = "";
                        string template_cc = "";

                        cmd.CommandText = $" UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'APPROVED' WHERE ID = {id}";
                        cmd.ExecuteNonQuery();

                        DocInfoModel dm = new DocInfoModel();
                        cmd.CommandText = " SELECT DO.*, EMP.empTitleEng + ' ' +EMP.empNameEng ISSUE_NAME ,MG.NAME DOC_FLOW, R.REASON_EXPLAIN, R.IS_NO, R.SUBJECT, R.ISSUE_ID REQ_ISSUE_ID, R.APPROVE_STATUS REQ_STATUS, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, RL.NAME RELATION_NAME " +
                                          " FROM TBL_TECH_IS_DOCINFO DO JOIN TBL_TECH_IS_REQUEST R ON DO.REQUEST_NO = R.IS_ID JOIN [db_employee].[dbo].[tbl_employee] EMP ON DO.ISSUE_ID = EMP.empId " +
                                          " JOIN TBL_TECH_IS_MAILGRP MG ON DO.DOC_FLOW_ID = MG.ID JOIN [db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID " +
                                          " JOIN [db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT JOIN TBL_TECH_IS_RELATION RL ON RL.ID = DO.RELATION_ID " +
                                          " WHERE DO.ID = " + id + " ";
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            dm.ID = dr["ID"].ToString();
                            dm.ISSUE_NAME = dr["ISSUE_NAME"].ToString();
                            dm.DOC_TYPE = dr["DOC_TYPE"].ToString();
                            dm.APPROVED_FLAX = dr["APPROVED_FLAX"].ToString();
                            dm.ACKNOWLAGE_FLEX = dr["ACKNOWLAGE_FLEX"].ToString();
                            dm.DOC_DETAIL = dr["DOC_DETAIL"].ToString();
                            dm.EFFECTIVE = dr["EFFECTIVE"].ToString();
                            dm.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString();
                            dm.RELATION_ID = dr["RELATION_ID"].ToString();
                            dm.CC_GRP = dr["CC_GRP"].ToString();
                            dm.DOC_FLOW_ID = dr["DOC_FLOW_ID"].ToString();
                            dm.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                            dm.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                            dm.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                            dm.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                            //dm.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();
                            dm.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                            dm.DOC_FLOW_TEXT = dr["DOC_FLOW"].ToString();
                            dm.REQUEST_NO = dr["IS_NO"].ToString();
                            dm.DETAIL_REF = dr["IS_NO"].ToString();
                            dm.PLANT_DEP = dr["DEP"].ToString();
                            dm.RELATION_TEXT = dr["RELATION_NAME"].ToString();
                            dm.DOC_SUBJECT = dr["SUBJECT"].ToString();
                            dm.INFO_NO = dr["INFO_NO"].ToString();
                            if (dr["REQ_STATUS"].ToString() != "NO REQUEST")
                            {
                                dm.REQUIREMENT_ISSUE_ID = dr["REQ_ISSUE_ID"].ToString();
                            }


                        }

                        ////////////Mail CC
                        dr.Close();
                        dm.txt_ATT_DOC_OTHER = new string[5];
                        int count = 0;
                        cmd.CommandText = $@"SELECT TOP (5) [PATCH]
                                      FROM [db_rtc].[dbo].[TBL_TECH_IS_FILES] where [IS_ID] = {id}";
                        dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                dm.txt_ATT_DOC_OTHER[count] = dr[0].ToString();
                                count++;
                            }
                        }
                        dr.Close();
                        cmd.CommandText = " SELECT * FROM TBL_TECH_IS_MAILGRP_CC " +
                                          " WHERE ID =  " + dm.CC_GRP + "";
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            mail_cc_list = dr["USERID"].ToString();

                            mailCC.Add(dr["TE_MAIL"].ToString());
                            mailCC.Add(dr["TE_MAIL"].ToString());
                        }

                        if (mail_cc_list != "" && mail_cc_list != null)
                        {
                            dr.Close();
                            cmd.CommandText = " SELECT empEmail EMAIL FROM db_employee.dbo.tbl_employee WHERE empId IN (" + mail_cc_list + ")";
                            dr = cmd.ExecuteReader();
                            while (dr.Read())
                            {
                                mailCC.Add(dr["EMAIL"].ToString());
                            }
                        }

                        ////////////Mail Approve
                        //เพิ่ม CC TE ชั่วคราวเพื่อให้ TE รับทราบตอนแก้ไข SAP
                        List<string> mailCC_TE = new List<string>();
                        mailCC_TE.Add("wandeep@citizen.co.jp");
                        mailCC_TE.Add("wiparats@citizen.co.jp");
                        mailCC_TE.Add("jariyac@citizen.co.jp");

                        string mailGrp = "";
                        string req_issue_id = "";
                        if (dm.REQUIREMENT_ISSUE_ID != null && dm.REQUIREMENT_ISSUE_ID != "")
                        {
                            req_issue_id = ",'" + dm.REQUIREMENT_ISSUE_ID + "'";
                        }
                        dr.Close();

                        cmd.CommandText = " SELECT USERID FROM TBL_TECH_IS_MAILGRP " +
                                          " WHERE ID =  " + dm.DOC_FLOW_ID + "";
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            mailGrp = dr["USERID"].ToString();
                        }
                        dr.Close();
                        cmd.CommandText = " SELECT empEmail EMAIL FROM db_employee.dbo.tbl_employee WHERE empId IN (" + mailGrp + req_issue_id + ")";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            mailTo.Add(dr["EMAIL"].ToString());
                        }

                        //Attachment
                        if (dm.txt_PIC_REF_1 != null && dm.txt_PIC_REF_1 != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_1);
                        }
                        if (dm.txt_PIC_REF_2 != null && dm.txt_PIC_REF_2 != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_PIC_REF_2);
                        }
                        if (dm.txt_ATT_DOC_PURCHASE != null && dm.txt_ATT_DOC_PURCHASE != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_PURCHASE);
                        }
                        if (dm.txt_DOC_IMPORTANT != null && dm.txt_DOC_IMPORTANT != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_DOC_IMPORTANT);
                        }
                        if (dm.txt_ATT_DOC_REQUIRE != null && dm.txt_ATT_DOC_REQUIRE != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_REQUIRE);
                        }
                        foreach (var ITEMFILE in dm.txt_ATT_DOC_OTHER)
                        {
                            if (dm.txt_ATT_DOC_OTHER != null && ITEMFILE != "")
                            {
                                att.Add(Server.MapPath("~/Temp_File/").ToString() + ITEMFILE);
                            }
                        }


                        //send mail to approve list
                        subject = "Information system: The information document has already been approved by all approvers.";
                        template += "<p><b>Dear Approver </b></p><p>Information Document is waiting for your approval.</p>";
                        template += getEmailTemplate(doc);
                        TempData["CreateSucess"] = "Successfully save and send email the information document.";

                        Fn.sendMail(subject, mailTo, template, null, mailCC_TE);

                        //send mail to CC group
                        if (dm.ACKNOWLAGE_FLEX == "True")
                        {
                            subject_cc =  "Information system: The information document has already been approved by all approvers.";
                            template_cc += "<p><b>Dear Acknowledge person</b></p><p>Information Document is for your acknowledgement.</p>";
                            template_cc += getEmailTemplate(dm);
                            Fn.sendMail(subject_cc, mailCC, template_cc, null);
                        }

                        con.Close();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return false;
                    }
                }
                //con.Close();
            }

        }

  

         [HttpPost]
        public JsonResult SendValues_inHttpMail2(DocInfoModel models)
        {
          checkonemorenonApprove_Emailtosend(models.ISSUE_DATE , models.ID , null, models , null,null,"");
            return null;
        }

    }
}