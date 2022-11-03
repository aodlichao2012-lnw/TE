using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Text;
using Information_System.Models;

namespace Information_System.Controllers
{
    public class ApproveController : Controller
    {
        // GET: Approve
        InFunction Fn = new InFunction();
        public string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        public ActionResult ApproveList()
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

        [HttpGet]
        public JsonResult getApproveList(string appLevel, string status_option)
        {
            //AutoSendMail a = new AutoSendMail();
            //a.Autosend();
            List<DocInfoModel> dmList = new List<DocInfoModel>();
            using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
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
                    if (appLevel == "2" || appLevel =="1")
                    {
                        sql.AppendLine(" AND A.STATUS IS NULL ");
                        sql.AppendLine(" AND D.STATUS = 'CREATE' ");
                    }
                    else if (appLevel == "0")
                    {
                        if (status_option == "Wait Approve")
                        {
                            sql.AppendLine(" AND D.STATUS = 'IN-PROGRESS' ");
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
                    while(dr.Read())
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

        [HttpPost]
        public bool setApproveOrReject(string id, string action, string level, string comment)
        {
            try
            {
                using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        string actionStatus = "";
                        if (action == "REJECT")  {
                            actionStatus = "REJECT";
                        } else if (action == "Approve" || action == "Accept") {
                            actionStatus = "APPROVED";
                        } else if (action == "PENDING") {
                            actionStatus = "PENDING";
                        } else if (action == "CANCEL") {
                            actionStatus = "CANCEL";
                        } else if (action == "APPROVE_ALL") {
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
                                foreach (var m in mt)
                                {
                                    mailTo.Add(m.EMP_EMAIL);
                                }
                                
                                DocInfoModel dm = getDocInfoModelInfor(d, level);
                                if (action == "Approve" || action == "Accept")
                                {
                                    if (level == "1")
                                    {
                                        subject = "Information System: Information Document is waiting for your approval.";
                                        template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Information Document is waiting for your approval.</p>";
                                        hrefTo = " <a href='"+ url + "/TE-IS/Approve/ApproveList/" + dm.ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    }
                                    else
                                    {
                                        subject = "Information system: The information document has been approved.";
                                        template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Documents are waiting for you to save and send to approver list.</p>";
                                        hrefTo = " <a href='" + url + "/TE-IS/Create/CreateInfoDoc/" + dm.REQUEST_ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
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
                                    if (dm.txt_ATT_DOC_OTHER != null && dm.txt_ATT_DOC_OTHER != "")
                                    {
                                        att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_OTHER);
                                    }
                                    TempData["statusApprove"] = " successfully!";

                                }
                                else if (action == "REJECT")
                                {
                                    hrefTo = " <a href='" + url + "/TE-IS/Create/CreateInfoDoc/" + dm.REQUEST_ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "The Information document has been rejected.";
                                    template += "<p><b>Dear K." + mt[0].EMP_NAME + " </b></p><p>Documents has been rejected.</p>";
                                    TempData["statusReject"] = " successfully!";
                                }
                                else if (action == "CANCEL")
                                {
                                    hrefTo = " <a href='" + url + "/TE-IS/Create/CreateInfoDoc/" + dm.REQUEST_ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "The Information document has been cancel.";
                                    template += "<p><b>Dear All </b></p><p>Documents has been cancel.</p>";
                                    TempData["statusReject"] = " successfully!";
                                }

                                template += getEmailTemplate(dm);
                                template += hrefTo;
                                
                                Fn.sendMail(subject, mailTo, template, att);
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
                                    hrefTo = " <a href='" + url + "/TE-IS/Create/CreateInfoDoc/" + dm.REQUEST_ID + "'>Click link to Information System </a>" +
                                                 " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                                    subject = "The information document has already been approved by all approvers.";
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
                foreach(var l in model.TE_APPROVE_LIST)
                {
                    html += " <tr> <td> " + l.APPROVE_NAME + " </td><td>  "+ l.APPROVE_DATE+ " </td><td>  "+ l.STATUS + " </td></tr>";
                }
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
                        cmd.CommandText = " SELECT mg.NAME DOC_FLOW, DO.*, r.*, e.empTitleEng + ' ' +e.empNameEng ISSUE_NAME , CONCAT(p.plant_mark,'/',d.Dept_Remark) plant_dep, er.empTitleEng + ' ' +er.empNameEng request_name FROM TBL_TECH_IS_DOCINFO DO " +
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
                            dm.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();
                            dm.REQUEST_NAME = dr["request_name"].ToString();
                            dm.REQUEST_ID = dr["IS_ID"].ToString();
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
                        while(dr.Read())
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
                    catch(Exception ex)
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
                DocInfoModel model = new DocInfoModel();
                using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using(SqlCommand cmd = new SqlCommand(null, con))
                    {
                        StringBuilder sql = new StringBuilder();
                        sql.AppendLine(" SELECT DI.ID DIID, DI.INFO_NO, R.SUBJECT, CONCAT(P.plant_mark,'/',D.Dept_Remark) PD, R.REASON_EXPLAIN, R.IS_NO, R.IS_ID, DI.ISSUE_DATE, CONCAT(EMI.empTitleEng,' ',EMI.empNameEng) ISSUE_NAME ");
                        sql.AppendLine(" , CONCAT(EMR.empTitleEng,' ',EMR.empNameEng) CHECK_NAME, CONCAT(EMA.empTitleEng,' ',EMA.empNameEng) APP_NAME, DI.DOC_DETAIL ");
                        sql.AppendLine(" , DI.PIC_REF_1, DI.PIC_REF_2, ATT_DOC_PURCHASE, ATT_DOC_REQUIRE, ATT_DOC_OTHER, DI.EFFECTIVE, DI.ADD_ORDER_NO, RL.NAME REALATION_NAME,RL.DETAILS RELATION_DETAIL ");
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
                        sql.AppendLine(" WHERE DI.ID ="+Fn.getSQL(id));
                        cmd.CommandText = sql.ToString();
                        SqlDataReader dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            model.ID = dr["DIID"].ToString();
                            model.INFO_NO = dr["INFO_NO"].ToString() != "" ? dr["INFO_NO"].ToString() : "-";
                            model.REQUEST_ID = dr["IS_ID"].ToString();
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
                            model.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                            model.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();
                            model.EFFECTIVE = dr["EFFECTIVE"].ToString() != "" ? dr["EFFECTIVE"].ToString() : "-";
                            model.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString() != "" ? dr["ADD_ORDER_NO"].ToString() : "-";
                            model.RELATION_ID = dr["REALATION_NAME"].ToString();
                            model.RELATION_TEXT = dr["RELATION_DETAIL"].ToString() != "" ? dr["RELATION_DETAIL"].ToString() : "-";
                        }

                        dr.Close();
                        cmd.CommandText = " SELECT DA.APPROVE_ID,CONCAT(e.empTitleEng,' ',e.empNameEng) APPROVE_NAME, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE , d.Dept_Remark " +
                                          " FROM TBL_TECH_IS_DOCINFO_APPROVE DA JOIN db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                          " left JOIN db_employee.dbo.tbl_Dept d on e.deptId = d.Dept_ID " +
                                          " WHERE DOCINFO_ID = " + Fn.getSQL(id) + " AND APPROVE_LEVEL IS NULL ";
                        dr = cmd.ExecuteReader();

                        List<DocInfoApprove> m = new List<DocInfoApprove>();
                        while (dr.Read())
                        {
                            DocInfoApprove da = new DocInfoApprove();
                            da.APPROVE_NAME = dr["APPROVE_NAME"].ToString() != ""? dr["APPROVE_NAME"].ToString(): "-";
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
                                          " LEFT JOIN db_employee.dbo.tbl_employee emp on emp.empId = r.ISSUE_ID " +
                                          " JOIN db_employee.dbo.tbl_plant P ON P.plant_id = R.PLANT JOIN db_employee.dbo.tbl_Dept D ON D.Dept_ID = R.DEPARTMENT " +
                                          " WHERE APPROVE_STATUS = 'CREATE' " +
                                          " AND APPROVE_BY = " + Fn.getSQL(person_id);
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
    }
}