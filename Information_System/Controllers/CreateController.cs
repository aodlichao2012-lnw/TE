using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Information_System.Models;
using System.Data.SqlClient;
using System.Text;
using System.IO;
using System.Net;
using System.Web.Mvc;

namespace Information_System.Controllers
{
    public class CreateController : Controller
    {
        InFunction Fn = new InFunction();
        public string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        public ActionResult CreateRequirementDoc(string id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                if (id != null)
                {
                    RequestDocModel reqModel = getDocInfo(id);
                    return View(reqModel);
                }
                else
                {
                    //using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
                    //{
                    //    conn.Open();
                    //    using (SqlCommand cmd = new SqlCommand(null, conn))
                    //    {
                    //        StringBuilder sql = new StringBuilder();
                    //        sql.Clear();
                    //        sql.AppendLine(" SELECT approve_id, EMP.empEmail empEmail, EMP.empNameEng+' '+ EMP.empLnameEng as empName FROM tbl_Routing_Mst M JOIN tbl_Routing_Oper O ON M.routing_id = O.routing_id ");
                    //        sql.AppendLine(" JOIN [db_employee].[DBO].[tbl_employee] EMP ON EMP.empId = O.approve_id ");
                    //        sql.AppendLine(" WHERE system_id = '5' AND M.dept_id =" + Fn.getSQL(Session["dep_id"].ToString()));
                    //        cmd.CommandText = sql.ToString();
                    //        SqlDataReader read = cmd.ExecuteReader();
                    //        if (read.Read())
                    //        {
                    //            ViewData["APPROVE_ID"] = read["approve_id"].ToString();
                    //            ViewData["APPROVE_EMAIL"] = read["empEmail"].ToString();
                    //            ViewData["APPROVE_NAME"] = read["empName"].ToString();
                    //        }

                    //    }
                    //}

                    ViewData["NEWID"] = Fn.newID();
                    return View();
                }
            }
        }

        [HttpGet]
        public RequestDocModel getDocInfo(string id, string from = null)
        {
            var model = new RequestDocModel();
            using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    try
                    {
                        SqlDataReader reader = null;
                        string query = "";
                        query += " SELECT TOP 1 r.*, CONCAT(EMP.empTitleEng,' ', emp.empNameEng) AS ISSUE_NAME, EMP.empEmail , CONCAT(EMPA.empTitleEng,' ', EMPA.empNameEng) AS APPROVE_NAME, EMPA.empEmail as APPROVE_EMAIL  FROM TBL_TECH_IS_REQUEST r " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee emp on emp.empId = r.ISSUE_ID " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee empa on empa.empId = r.APPROVE_BY  " +
                                          " WHERE IS_ID = " + Fn.getSQL(id);
                        if (from != "notIssueID")
                        {
                            query += " AND ISSUE_ID = " + Fn.getSQL(Session["emp_id"].ToString());
                        }                                          

                        cmd.CommandText = query.ToString();
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            model.IS_ID = reader["IS_ID"].ToString();
                            model.IS_NO = reader["IS_NO"].ToString();
                            model.PLANT = reader["PLANT"].ToString();
                            model.DEPARTMENT = reader["DEPARTMENT"].ToString();
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
                            model.APPROVE_ID = reader["APPROVE_BY"].ToString();
                            model.APPROVE_NAME = reader["APPROVE_NAME"].ToString();
                            model.APPROVE_EMAIL = reader["APPROVE_EMAIL"].ToString();
                            model.APPROVE_COMMENT = reader["APPROVE_COMMENT"].ToString();
                        }
                        reader.Close();

                        cmd.CommandText = " SELECT PATCH FROM TBL_TECH_IS_FILES " +
                                          " WHERE IS_ID = " + Fn.getSQL(id);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            model.GET_FILE_LIST.Add(reader["PATCH"].ToString());
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Response.Write(ex.Message);
                    }
                }
                conn.Close();
            }

            return model;
        }

        public ActionResult CreateInfoDoc(string id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                List<ApproveGroup> apList = new List<ApproveGroup>();
                List<ApproveGroup> CCList = new List<ApproveGroup>();
                List<Relation> relation = new List<Relation>();
                //RequestDocModel rd = new RequestDocModel();
                DocInfoModel dm = new DocInfoModel();
                FilePatch fp = new FilePatch();
                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using(SqlCommand cmd = new SqlCommand(null, con))
                    {
                        SqlDataReader dr = null;
                        cmd.CommandText = " SELECT ID, NAME FROM TBL_TECH_IS_MAILGRP ORDER BY ID ASC ";
                        dr = cmd.ExecuteReader();
                        while(dr.Read())
                        {
                            ApproveGroup ap = new ApproveGroup();
                            ap.ID = dr["ID"].ToString();
                            ap.NAME = dr["NAME"].ToString();
                            apList.Add(ap);
                        }

                        dr.Close();
                        cmd.CommandText = " SELECT ID, CC_GRP FROM TBL_TECH_IS_MAILGRP_CC ORDER BY ID ASC ";
                        dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            ApproveGroup CC = new ApproveGroup();
                            CC.ID = dr["ID"].ToString();
                            CC.NAME = dr["CC_GRP"].ToString();
                            CCList.Add(CC);
                        }

                        dr.Close();
                        cmd.CommandText = " SELECT * FROM TBL_TECH_IS_RELATION ";
                        dr = cmd.ExecuteReader();
                        while(dr.Read())
                        {
                            Relation rl = new Relation();
                            rl.ID = dr["ID"].ToString();
                            rl.NAME = dr["NAME"].ToString();
                            rl.DETAIL = dr["DETAILS"].ToString();
                            relation.Add(rl);
                        }

                        dr.Close();
                        if (id != null)
                        {
                            cmd.CommandText = " SELECT * FROM TBL_TECH_IS_REQUEST WHERE IS_ID = " + Fn.getSQL(id);
                            dr = cmd.ExecuteReader();
                            while(dr.Read())
                            {
                                //rd.SUBJECT = dr["SUBJECT"].ToString();
                                //rd.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                                //rd.IS_NO = dr["IS_NO"].ToString();
                                //rd.IS_ID = dr["IS_ID"].ToString();
                                if (dr["APPROVE_STATUS"].ToString() != "NO REQUEST")
                                {
                                    dm.REQUIREMENT_ISSUE_ID = dr["ISSUE_ID"].ToString();
                                }                                
                                dm.DOC_SUBJECT = dr["SUBJECT"].ToString();
                                dm.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                                dm.DETAIL_REF = dr["IS_NO"].ToString();
                                dm.REQUEST_NO = dr["IS_ID"].ToString();

                            }

                            dr.Close();
                            cmd.CommandText = " SELECT DO.*, e.empTitleEng + ' ' +e.empNameEng name, dp.STATUS STATUS_TE FROM TBL_TECH_IS_DOCINFO DO join db_employee.dbo.tbl_employee e on e.empId = DO.ISSUE_ID" +
                                              " join TBL_TECH_IS_DOCINFO_APPROVE dp on do.ID = dp.DOCINFO_ID and dp.APPROVE_LEVEL = '2' " +
                                              " WHERE REQUEST_NO = " + Fn.getSQL(id);
                            dr = cmd.ExecuteReader();
                            if (dr.Read())
                            {
                                dm.INFO_NO = dr["INFO_NO"].ToString();
                                dm.ID = dr["ID"].ToString();
                                dm.ISSUE_NAME = dr["name"].ToString();
                                dm.DOC_TYPE = dr["DOC_TYPE"].ToString();
                                dm.APPROVED_FLAX = dr["APPROVED_FLAX"].ToString();
                                dm.ACKNOWLAGE_FLEX = dr["ACKNOWLAGE_FLEX"].ToString();
                                dm.DOC_DETAIL = dr["DOC_DETAIL"].ToString();
                                dm.EFFECTIVE = dr["EFFECTIVE"].ToString();
                                dm.ADD_ORDER_NO = dr["ADD_ORDER_NO"].ToString();
                                dm.RELATION_ID = dr["RELATION_ID"].ToString();
                                dm.CC_GRP = dr["CC_GRP"].ToString();
                                dm.DOC_FLOW_ID = dr["DOC_FLOW_ID"].ToString();
                                dm.STATUS = dr["STATUS_TE"].ToString();
                                dm.STATUS_ALL = dr["STATUS"].ToString();
                                //dm.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                                //dm.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                                //dm.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                                //dm.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_OTHER"].ToString();
                                //dm.txt_ATT_DOC_OTHER = dr["CC_GRP"].ToString();

                                fp.txt_PIC_REF_1 = dr["PIC_REF_1"].ToString();
                                fp.txt_PIC_REF_2 = dr["PIC_REF_2"].ToString();
                                fp.txt_ATT_DOC_PURCHASE = dr["ATT_DOC_PURCHASE"].ToString();
                                fp.txt_ATT_DOC_REQUIRE = dr["ATT_DOC_REQUIRE"].ToString();
                                fp.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();
                            } 
                            ViewData["requestInfo"] = dm;
                            ViewData["fp"] = fp;
                        }

                    }
                    con.Close();
                }
                
                ViewData["relation"] = relation;
                ViewData["app_list"] = apList;
                ViewData["CC_list"] = CCList;

                return View();
                    
            }
            
        }

        //Create Imformation Document
        [HttpPost]
        public bool createInfoDoc(DocInfoModel doc, string action, string update_app_flow)
        {
            List<string> mailTo = new List<string>();
            List<string> mailToName = new List<string>();
            List<string> att = new List<string>();
            List<string> mailCC = new List<string>();
            List<string> approveList = doc.APPROVE_LIST.Split(',').ToList();
            string subject = "";
            string approve_id_1 = "";
            string approve_id_2 = "";
            string template = "";
            string sendToN = null;
            //List<string> fileList = new List<string>();
            HttpFileCollectionBase files = Request.Files;
            Random generator = new Random();
            string random = generator.Next(0, 1000000).ToString("D6");
            for (int i = 0; i < files.Count; i++)
            {
                string filePart = Server.MapPath("~/Temp_File/").ToString();
                HttpPostedFileBase file = files[i];
                string fname;
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = random + "_" + file.FileName;
                }

                file.SaveAs(filePart + fname);
                att.Add(Server.MapPath("~/Temp_File/").ToString() + fname);
            }

            doc.ISSUE_NAME = Session["login_name"].ToString();
            using (SqlConnection conEmp = new SqlConnection(Fn.connString))
            {
                conEmp.Open();
                SqlCommand cmd = new SqlCommand(null, conEmp);
                cmd.CommandText = " SELECT R.empId EMP_ID FROM tbl_access_right R WHERE IS_APP_LEVEL2 = 1 ";
                approve_id_2 = cmd.ExecuteScalar().ToString();

                cmd.CommandText = " SELECT R.empId , E.empEmail, E.empTitleEng+ ' '+ e.empNameEng as EMP_NAME FROM tbl_access_right R JOIN tbl_employee e ON R.empId = E.empId WHERE IS_APP_LEVEL1 = 1 ";
                SqlDataReader DR = cmd.ExecuteReader();
                if (DR.Read())
                {
                    approve_id_1 = DR["empId"].ToString();
                    mailTo.Add(DR["empEmail"].ToString());
                    mailToName.Add(DR["EMP_NAME"].ToString());
                }
                conEmp.Close();
            }

            try
            {
                if (action == "Temporary")
                {
                    using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(null, con))
                        {
                            StringBuilder sql = new StringBuilder();
                            if (doc.REQUEST_NO == null)
                            {
                                cmd.CommandText = " SELECT NEWID(); ";
                                string newIDRequest = cmd.ExecuteScalar().ToString();
                                doc.REQUEST_NO = newIDRequest;
                                sql.Clear();
                                sql.AppendLine(" INSERT INTO TBL_TECH_IS_REQUEST ");
                                sql.AppendLine(" (IS_ID, PLANT, DEPARTMENT, SUBJECT, REASON_EXPLAIN, ISSUE_ID, ISSUE_DATE, APPROVE_STATUS) ");
                                sql.AppendLine(" VALUES ( ");
                                sql.AppendLine(Fn.getSQL(newIDRequest));
                                sql.AppendLine("," + Fn.getSQL(Session["plant_id"].ToString()));
                                sql.AppendLine("," + Fn.getSQL(Session["dep_id"].ToString()));
                                sql.AppendLine(", @DOC_SUBJECT ");
                                sql.AppendLine(", @REASON_EXPLAIN ");
                                sql.AppendLine("," + Fn.getSQL(Session["emp_id"].ToString()));
                                sql.AppendLine(", getdate() ");
                                sql.AppendLine(", 'NO REQUEST') ");
                                cmd.CommandText = sql.ToString();
                                cmd.Parameters.AddWithValue("@DOC_SUBJECT", (doc.DOC_SUBJECT == null) ? (object)DBNull.Value : (object)doc.DOC_SUBJECT);
                                cmd.Parameters.AddWithValue("@REASON_EXPLAIN", (doc.REASON_EXPLAIN == null) ? (object)DBNull.Value : (object)doc.REASON_EXPLAIN);
                                cmd.ExecuteNonQuery();

                            }

                            cmd.CommandText = " SELECT NEWID(); ";
                            string newID = cmd.ExecuteScalar().ToString();

                            sql.Clear();
                            sql.AppendLine(" INSERT INTO TBL_TECH_IS_DOCINFO (ID ,REQUEST_NO ,ISSUE_ID ,ISSUE_DATE ,DOC_TYPE ,PLANT ,DEPARTMENT ,ACKNOWLAGE_FLEX ,CC_GRP ,DOC_FLOW_ID ,DOC_DETAIL , EFFECTIVE , ADD_ORDER_NO , RELATION_ID, STATUS ");
                            if (doc.PIC_REF_1 != null)
                            {
                                sql.AppendLine(" , PIC_REF_1 ");
                            }
                            if (doc.PIC_REF_2 != null)
                            {
                                sql.AppendLine(" , PIC_REF_2 ");
                            }
                            if (doc.ATT_DOC_PURCHASE != null)
                            {
                                sql.AppendLine(" , ATT_DOC_PURCHASE ");
                            }
                            if (doc.ATT_DOC_REQUIRE != null)
                            {
                                sql.AppendLine(" , ATT_DOC_REQUIRE ");
                            }
                            if (doc.ATT_DOC_OTHER != null)
                            {
                                sql.AppendLine(" , ATT_DOC_OTHER ");
                            }
                            sql.AppendLine(" )  VALUES ( " + Fn.getSQL(newID) + "," + Fn.getSQL(doc.REQUEST_NO) + ", " + Fn.getSQL(Session["emp_id"].ToString()) + ", GETDATE() " + ", " + Fn.getSQL(doc.DOC_TYPE));
                            sql.AppendLine(", " + Fn.getSQL(Session["plant_id"].ToString()) + ", " + Fn.getSQL(Session["dep_id"].ToString()) + ", " + Fn.getSQL(doc.ACKNOWLAGE_FLEX));
                            sql.AppendLine(", " + Fn.getSQL(doc.CC_GRP) + ", " + Fn.getSQL(doc.DOC_FLOW_ID) + ", @DOC_DETAIL, @EFFECTIVE , @ADD_ORDER_NO , " + Fn.getSQL(doc.RELATION_ID) + ",'CREATE' ");
                            if (doc.PIC_REF_1 != null)
                            {
                                //sql.AppendLine(", " + Fn.getSQL(random + "_" + doc.PIC_REF_1.FileName));
                                sql.AppendLine(", @PIC_REF_1 ");
                            }
                            if (doc.PIC_REF_2 != null)
                            {
                                //sql.AppendLine(", " + Fn.getSQL(random + "_" + doc.PIC_REF_2.FileName));
                                sql.AppendLine(", @PIC_REF_2 ");
                            }
                            if (doc.ATT_DOC_PURCHASE != null)
                            {
                                //sql.AppendLine(", " + Fn.getSQL(random + "_" + doc.ATT_DOC_PURCHASE.FileName));
                                sql.AppendLine(", @ATT_DOC_PURCHASE ");
                            }
                            if (doc.ATT_DOC_REQUIRE != null)
                            {
                                //sql.AppendLine(", " + Fn.getSQL(random + "_" + doc.ATT_DOC_REQUIRE.FileName));
                                sql.AppendLine(", @ATT_DOC_REQUIRE ");
                            }
                            if (doc.ATT_DOC_OTHER != null)
                            {
                                //sql.AppendLine(", " + Fn.getSQL(random + "_" + doc.ATT_DOC_OTHER.FileName));
                                sql.AppendLine(", @ATT_DOC_OTHER ");
                            }
                            sql.Append(" )");

                            cmd.CommandText = sql.ToString();
                            cmd.Parameters.AddWithValue("@DOC_DETAIL", (doc.DOC_DETAIL == null) ? (object)DBNull.Value : (object)doc.DOC_DETAIL);
                            cmd.Parameters.AddWithValue("@EFFECTIVE", (doc.EFFECTIVE == null) ? (object)DBNull.Value : (object)doc.EFFECTIVE);
                            cmd.Parameters.AddWithValue("@ADD_ORDER_NO", (doc.ADD_ORDER_NO == null) ? (object)DBNull.Value : (object)doc.ADD_ORDER_NO);

                            cmd.Parameters.AddWithValue("@PIC_REF_1", (doc.PIC_REF_1 == null) ? (object)DBNull.Value : (object)random + "_" + doc.PIC_REF_1.FileName);
                            cmd.Parameters.AddWithValue("@PIC_REF_2", (doc.PIC_REF_2 == null) ? (object)DBNull.Value : (object)random + "_" + doc.PIC_REF_2.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_PURCHASE", (doc.ATT_DOC_PURCHASE == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_PURCHASE.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_REQUIRE", (doc.ATT_DOC_REQUIRE == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_REQUIRE.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_OTHER", (doc.ATT_DOC_OTHER == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_OTHER.FileName);

                            cmd.ExecuteNonQuery();

                            string approveQueryList = "";
                            foreach (var list in approveList)
                            {
                                approveQueryList += "INSERT INTO TBL_TECH_IS_DOCINFO_APPROVE(DOCINFO_ID, APPROVE_ID) VALUES(" +
                                              Fn.getSQL(newID) + ", " + Fn.getSQL(list) + " ) ";
                            }

                            cmd.CommandText = " INSERT INTO TBL_TECH_IS_DOCINFO_APPROVE (DOCINFO_ID, APPROVE_ID,APPROVE_LEVEL) VALUES ( " +
                                              Fn.getSQL(newID) + ", " + Fn.getSQL(approve_id_1) + " , 1) " +
                                              " INSERT INTO TBL_TECH_IS_DOCINFO_APPROVE (DOCINFO_ID, APPROVE_ID,APPROVE_LEVEL) VALUES ( " +
                                              Fn.getSQL(newID) + ", " + Fn.getSQL(approve_id_2) + " , 2) " +
                                              approveQueryList;
                            cmd.ExecuteNonQuery();

                            if (mailToName != null)
                            {
                                foreach (var s in mailToName)
                                {
                                    if (sendToN == null)
                                    { sendToN += s; }
                                    else { sendToN += "," + s; }
                                }
                            }

                            subject = " Information System: Information Document is waiting for your approval.";
                            template += "<p><b>Dear K'" + sendToN + "</b></p><p>Information Document is waiting for your approval.</p>";
                            template += getTemplateEmailCreate(doc, action);
                            TempData["CreateSucess"] = "Successfully created the information document.";
                        }
                        con.Close();
                    }

                }
                else if (action == "Edit")
                {
                    using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                    {
                        con.Open();
                        using (SqlCommand cmd = new SqlCommand(null, con))
                        {
                            StringBuilder sql = new StringBuilder();
                            sql.Clear();
                            sql.AppendLine(" UPDATE TBL_TECH_IS_DOCINFO SET DOC_TYPE = " + Fn.getSQL(doc.DOC_TYPE) + ", APPROVED_FLAX = " + Fn.getSQL(doc.APPROVED_FLAX));
                            sql.AppendLine(", ACKNOWLAGE_FLEX = " + Fn.getSQL(doc.ACKNOWLAGE_FLEX) + ", CC_GRP = " + Fn.getSQL(doc.CC_GRP) + ", DOC_FLOW_ID = " + Fn.getSQL(doc.DOC_FLOW_ID));
                            sql.AppendLine(", DOC_DETAIL = @DOC_DETAIL, EFFECTIVE = @EFFECTIVE, ADD_ORDER_NO = @ADD_ORDER_NO");
                            sql.AppendLine(", RELATION_ID = " + Fn.getSQL(doc.RELATION_ID));
                            if (doc.PIC_REF_1 != null)
                            {
                                sql.AppendLine(" , PIC_REF_1 = @PIC_REF_1 ");
                            }
                            if (doc.PIC_REF_2 != null)
                            {
                                sql.AppendLine(" , PIC_REF_2 = @PIC_REF_2 ");
                            }
                            if (doc.ATT_DOC_PURCHASE != null)
                            {
                                sql.AppendLine(" , ATT_DOC_PURCHASE = @ATT_DOC_PURCHASE ");
                            }
                            if (doc.ATT_DOC_REQUIRE != null)
                            {
                                sql.AppendLine(" , ATT_DOC_REQUIRE = @ATT_DOC_REQUIRE ");
                            }
                            if (doc.ATT_DOC_OTHER != null)
                            {
                                sql.AppendLine(" , ATT_DOC_OTHER = @ATT_DOC_OTHER");
                            }
                            sql.AppendLine(" , STATUS = 'CREATE' ");
                            sql.AppendLine(" WHERE ID = " + Fn.getSQL(doc.ID));
                            cmd.CommandText = sql.ToString();
                            cmd.Parameters.AddWithValue("@DOC_DETAIL", (doc.DOC_DETAIL == null) ? (object)DBNull.Value : (object)doc.DOC_DETAIL);
                            cmd.Parameters.AddWithValue("@EFFECTIVE", (doc.EFFECTIVE == null) ? (object)DBNull.Value : (object)doc.EFFECTIVE);
                            cmd.Parameters.AddWithValue("@ADD_ORDER_NO", (doc.ADD_ORDER_NO == null) ? (object)DBNull.Value : (object)doc.ADD_ORDER_NO);
                            cmd.Parameters.AddWithValue("@PIC_REF_1", (doc.PIC_REF_1 == null) ? (object)DBNull.Value : (object)random + "_" + doc.PIC_REF_1.FileName);
                            cmd.Parameters.AddWithValue("@PIC_REF_2", (doc.PIC_REF_2 == null) ? (object)DBNull.Value : (object)random + "_" + doc.PIC_REF_2.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_PURCHASE", (doc.ATT_DOC_PURCHASE == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_PURCHASE.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_REQUIRE", (doc.ATT_DOC_REQUIRE == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_REQUIRE.FileName);
                            cmd.Parameters.AddWithValue("@ATT_DOC_OTHER", (doc.ATT_DOC_OTHER == null) ? (object)DBNull.Value : (object)random + "_" + doc.ATT_DOC_OTHER.FileName);

                            cmd.ExecuteNonQuery();

                            sql.Clear();
                            sql.AppendLine(" UPDATE TBL_TECH_IS_DOCINFO_APPROVE SET STATUS = NULL WHERE DOCINFO_ID = " + Fn.getSQL(doc.ID));
                            cmd.CommandText = sql.ToString();
                            cmd.ExecuteNonQuery();

                            if (update_app_flow == "Y")
                            {
                                sql.Clear();
                                sql.AppendLine(" DELETE FROM TBL_TECH_IS_DOCINFO_APPROVE WHERE DOCINFO_ID =" + Fn.getSQL(doc.ID) + " AND APPROVE_LEVEL IS NULL ");
                                cmd.CommandText = sql.ToString();
                                cmd.ExecuteNonQuery();

                                string approveQueryList = "";
                                foreach (var list in approveList)
                                {
                                    approveQueryList += "INSERT INTO TBL_TECH_IS_DOCINFO_APPROVE(DOCINFO_ID, APPROVE_ID) VALUES(" + Fn.getSQL(doc.ID) + ", " + Fn.getSQL(list) + " ) ";
                                }
                                cmd.CommandText = approveQueryList;
                                cmd.ExecuteNonQuery();
                            }

                            if (mailToName != null)
                            {
                                foreach (var s in mailToName)
                                {
                                    if (sendToN == null)
                                    { sendToN += s; }
                                    else { sendToN += "," + s; }
                                }
                            }

                            subject = "[Revised] Information System: Information Document is waiting for your approval.";
                            template += "<p><b>Dear K'" + sendToN + "</b></p><p>[Revised] Information Document is waiting for your approval.</p>";
                            template += getTemplateEmailCreate(doc, action);
                            TempData["CreateSucess"] = "Successfully edit the information document.";
                        }
                        con.Close();
                    }


                }

                //send email
                Fn.sendMail(subject, mailTo, template, att, mailCC);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public ActionResult RequirementInfoList()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                List<Department> res = new List<Department>();
                using (SqlConnection con = new SqlConnection(Fn.connString))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        cmd.CommandText = " SELECT Dept_ID, Dept_Name FROM tbl_Dept where Dept_Status = 'T' ";
                        SqlDataReader reader = cmd.ExecuteReader();
                        while(reader.Read())
                        {
                            Department model = new Department();
                            model.DEP_ID = reader["Dept_ID"].ToString();
                            model.DEP_NAME = reader["Dept_Name"].ToString();
                            res.Add(model);
                        }
                        ViewData["DepList"] = res;
                    }
                    con.Close();
                }

                return View();
            }
        }

        public ActionResult ApproveRequest(string id)
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                RequestDocModel model = new RequestDocModel();
                using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        SqlDataReader reader = null;
                        cmd.CommandText = " SELECT TOP 1 r.*, CONCAT(EMP.empTitleEng,' ', emp.empNameEng) AS ISSUE_NAME, EMP.empEmail , CONCAT(EMPA.empTitleEng,' ', EMPA.empNameEng) AS APPROVE_NAME FROM TBL_TECH_IS_REQUEST r " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee emp on emp.empId = r.ISSUE_ID " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee empa on empa.empId = r.APPROVE_BY  " +
                                          " WHERE IS_ID = " + Fn.getSQL(id) +
                                          " AND APPROVE_BY = " + Fn.getSQL(Session["emp_id"].ToString());
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            model.IS_ID = reader["IS_ID"].ToString();
                            model.IS_NO = reader["IS_NO"].ToString();
                            model.PLANT = reader["PLANT"].ToString();
                            model.DEPARTMENT = reader["DEPARTMENT"].ToString();
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
                            model.APPROVE_NAME = reader["APPROVE_NAME"].ToString();
                            model.APPROVE_ID = reader["APPROVE_BY"].ToString();
                        }
                        reader.Close();
                        
                        cmd.CommandText = " SELECT PATCH FROM TBL_TECH_IS_FILES " +
                                          " WHERE IS_ID = " + Fn.getSQL(id);
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                             model.GET_FILE_LIST.Add(reader["PATCH"].ToString());
                        }

                    }
                    con.Close();
                }

                return View(model);
            }
            
        }

        public ActionResult ReceivedRequirment(string id)
        {
            
            if (string.IsNullOrEmpty(Convert.ToString(Session["login_id"])))
            {
                return View("../Home/Login");
            }
            else
            {
                RequestDocModel model = new RequestDocModel();
                using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(null, con))
                    {
                        SqlDataReader reader = null;
                        cmd.CommandText = " SELECT TOP 1 DO.STATUS INFO_STATUS, r.*, CONCAT(EMP.empTitleEng,' ', emp.empNameEng) AS ISSUE_NAME, EMP.empEmail , CONCAT(EMPA.empTitleEng,' ', EMPA.empNameEng) AS APPROVE_NAME FROM TBL_TECH_IS_REQUEST r " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee emp on emp.empId = r.ISSUE_ID " +
                                          " LEFT JOIN db_employee.dbo.tbl_employee empa on empa.empId = r.APPROVE_BY  " +
                                          " LEFT JOIN TBL_TECH_IS_DOCINFO DO ON r.IS_ID = DO.REQUEST_NO  " +
                                          " WHERE IS_ID = " + Fn.getSQL(id) +
                                          " AND APPROVE_STATUS IN ('APPROVED','REJECT','TE-REJECT') ";
                        reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            model.IS_ID = reader["IS_ID"].ToString();
                            model.IS_NO = reader["IS_NO"].ToString();
                            model.PLANT = reader["PLANT"].ToString();
                            model.DEPARTMENT = reader["DEPARTMENT"].ToString();
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
                            model.APPROVE_NAME = reader["APPROVE_NAME"].ToString();
                            model.INFO_STATUS = reader["INFO_STATUS"].ToString();
                            model.APPROVE_COMMENT = reader["APPROVE_COMMENT"].ToString();
                        }
                        reader.Close();

                        cmd.CommandText = " SELECT PATCH FROM TBL_TECH_IS_FILES F " +
                                          " JOIN TBL_TECH_IS_REQUEST R ON R.IS_ID = F.IS_ID " +
                                          " WHERE F.IS_ID = " + Fn.getSQL(id) +
                                          " AND R.APPROVE_STATUS = 'APPROVED' ";
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            model.GET_FILE_LIST.Add(reader["PATCH"].ToString());
                        }

                    }
                    con.Close();
                }
                return View(model);
            }
            
        }

        [HttpPost]
        public string AddRequirementDoc(RequestDocModel model, string action)
        { 
            using(SqlConnection conn = new SqlConnection(Fn.conRTCStr))
            {
                conn.Open();
                using(SqlCommand cmd2 = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Clear();
                    //sql.AppendLine(" SELECT approve_id, EMP.empEmail empEmail, EMP.empNameEng+' '+ EMP.empLnameEng as empName FROM tbl_Routing_Mst M JOIN tbl_Routing_Oper O ON M.routing_id = O.routing_id ");
                    //sql.AppendLine(" JOIN [db_employee].[DBO].[tbl_employee] EMP ON EMP.empId = O.approve_id ");
                    sql.AppendLine(" SELECT empId, empEmail empEmail, empNameEng+' '+ empLnameEng as empName FROM [db_employee].[DBO].[tbl_employee] ");
                    sql.AppendLine(" WHERE empId = @empId ");
                    cmd2.CommandText = sql.ToString();
                    cmd2.Parameters.AddWithValue("@empId", model.APPROVE_ID);
                    SqlDataReader read = cmd2.ExecuteReader();
                    if (read.Read())
                    {
                        model.APPROVE_EMAIL = read["empEmail"].ToString();
                        model.APPROVE_NAME = read["empName"].ToString();
                    }
                    else
                    {
                        return "errFindApprover";
                    }
                }
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    string emp_id = Session["emp_id"].ToString();
                    try
                    {
                        string dep_id = Session["dep_id"].ToString();
                                      
                        sql.Clear();
                        if (action == "Edit")
                        {
                            //sql.AppendLine(" UPDATE TBL_TECH_IS_REQUEST SET ");
                            //sql.AppendLine(" SUBJECT = " + Fn.getSQL(model.SUBJECT));
                            //sql.AppendLine(" ,REASON_EXPLAIN = " + Fn.getSQL(model.REASON_EXPLAIN));
                            //sql.AppendLine(" ,START_PROD_MONTH = " + Fn.getSQL(model.START_PROD_MONTH));
                            //sql.AppendLine(" ,DOCUMENT_DATA_OF_CHANGE = " + Fn.getSQL(model.DOCUMENT_DATA_OF_CHANGE));
                            //sql.AppendLine(" ,PARTS_CODE = " + Fn.getSQL(model.PARTS_CODE));
                            //sql.AppendLine(" ,WATCH_CODE = " + Fn.getSQL(model.WATCH_CODE));
                            //sql.AppendLine(" ,CLASSIFICATION = " + Fn.getSQL(model.CLASSIFICATION));
                            //sql.AppendLine(" ,APPROVE_STATUS = 'CREATE' ");
                            //sql.AppendLine(" WHERE IS_ID = " + Fn.getSQL(model.IS_ID));

                            sql.AppendLine(" UPDATE TBL_TECH_IS_REQUEST SET ");
                            sql.AppendLine(" SUBJECT = @SUBJECT ");
                            sql.AppendLine(" ,REASON_EXPLAIN = @REASON_EXPLAIN ");
                            sql.AppendLine(" ,START_PROD_MONTH = @START_PROD_MONTH ");
                            sql.AppendLine(" ,DOCUMENT_DATA_OF_CHANGE = @DOCUMENT_DATA_OF_CHANGE ");
                            sql.AppendLine(" ,PARTS_CODE = @PARTS_CODE ");
                            sql.AppendLine(" ,WATCH_CODE = @WATCH_CODE ");
                            sql.AppendLine(" ,CLASSIFICATION = @CLASSIFICATION");
                            sql.AppendLine(" ,APPROVE_STATUS = 'CREATE' ");
                            sql.AppendLine(" WHERE IS_ID = " + Fn.getSQL(model.IS_ID));
                            cmd.CommandText = sql.ToString();
                            cmd.Parameters.AddWithValue("@SUBJECT", (model.SUBJECT == null) ? (object)DBNull.Value : (object)model.SUBJECT);
                            cmd.Parameters.AddWithValue("@REASON_EXPLAIN", (model.REASON_EXPLAIN == null) ? (object)DBNull.Value : (object)model.REASON_EXPLAIN);
                            cmd.Parameters.AddWithValue("@START_PROD_MONTH", (model.START_PROD_MONTH == null) ? (object)DBNull.Value : (object)model.START_PROD_MONTH);
                            cmd.Parameters.AddWithValue("@DOCUMENT_DATA_OF_CHANGE", (model.DOCUMENT_DATA_OF_CHANGE == null) ? (object)DBNull.Value : (object)model.DOCUMENT_DATA_OF_CHANGE);
                            cmd.Parameters.AddWithValue("@PARTS_CODE", (model.PARTS_CODE == null) ? (object)DBNull.Value : (object)model.PARTS_CODE);
                            cmd.Parameters.AddWithValue("@WATCH_CODE", (model.WATCH_CODE == null) ? (object)DBNull.Value : (object)model.WATCH_CODE);
                            cmd.Parameters.AddWithValue("@CLASSIFICATION", (model.CLASSIFICATION == null) ? (object)DBNull.Value : (object)model.CLASSIFICATION);
                        }
                        else
                        {
                            //sql.AppendLine(" INSERT INTO TBL_TECH_IS_REQUEST ");
                            //sql.AppendLine(" (IS_ID, PLANT, DEPARTMENT, SUBJECT, REASON_EXPLAIN, START_PROD_MONTH, DOCUMENT_DATA_OF_CHANGE, PARTS_CODE, WATCH_CODE, CLASSIFICATION, ISSUE_ID, ISSUE_DATE, APPROVE_BY, APPROVE_STATUS) ");
                            //sql.AppendLine(" VALUES ( ");
                            //sql.AppendLine(Fn.getSQL(model.IS_ID));
                            //sql.AppendLine("," + Fn.getSQL(Session["plant_id"].ToString()));
                            //sql.AppendLine("," + Fn.getSQL(dep_id));
                            //sql.AppendLine("," + Fn.getSQL(model.SUBJECT));
                            //sql.AppendLine("," + Fn.getSQL(model.REASON_EXPLAIN));
                            //sql.AppendLine("," + Fn.getSQL(model.START_PROD_MONTH));
                            //sql.AppendLine("," + Fn.getSQL(model.DOCUMENT_DATA_OF_CHANGE));
                            //sql.AppendLine("," + Fn.getSQL(model.PARTS_CODE));
                            //sql.AppendLine("," + Fn.getSQL(model.WATCH_CODE));
                            //sql.AppendLine("," + Fn.getSQL(model.CLASSIFICATION));
                            //sql.AppendLine("," + Fn.getSQL(emp_id));
                            //sql.AppendLine(", getdate() ");
                            //sql.AppendLine("," + Fn.getSQL(model.APPROVE_ID));
                            //sql.AppendLine(", 'CREATE' ) ");

                            cmd.CommandText = " INSERT INTO TBL_TECH_IS_REQUEST " +
                                              " (IS_ID, PLANT, DEPARTMENT, SUBJECT, REASON_EXPLAIN, START_PROD_MONTH, DOCUMENT_DATA_OF_CHANGE, PARTS_CODE, WATCH_CODE, CLASSIFICATION, ISSUE_ID, ISSUE_DATE, APPROVE_BY, APPROVE_STATUS) values " +
                                              " (@IS_ID, @PLANT, @DEPARTMENT, @SUBJECT, @REASON_EXPLAIN, @START_PROD_MONTH, @DOCUMENT_DATA_OF_CHANGE, @PARTS_CODE, @WATCH_CODE, @CLASSIFICATION, @ISSUE_ID, getdate(), @APPROVE_BY, 'CREATE') ";

                            cmd.Parameters.AddWithValue("@IS_ID", model.IS_ID);
                            cmd.Parameters.AddWithValue("@PLANT", Session["plant_id"].ToString());
                            cmd.Parameters.AddWithValue("@DEPARTMENT", dep_id);
                            cmd.Parameters.AddWithValue("@SUBJECT", (model.SUBJECT == null) ? (object)DBNull.Value : (object)model.SUBJECT);
                            cmd.Parameters.AddWithValue("@REASON_EXPLAIN", (model.REASON_EXPLAIN == null) ? (object)DBNull.Value : (object)model.REASON_EXPLAIN);
                            cmd.Parameters.AddWithValue("@START_PROD_MONTH", (model.START_PROD_MONTH == null) ? (object)DBNull.Value : (object)model.START_PROD_MONTH);
                            cmd.Parameters.AddWithValue("@DOCUMENT_DATA_OF_CHANGE", (model.DOCUMENT_DATA_OF_CHANGE == null) ? (object)DBNull.Value : (object)model.DOCUMENT_DATA_OF_CHANGE);
                            cmd.Parameters.AddWithValue("@PARTS_CODE", (model.PARTS_CODE == null) ? (object)DBNull.Value : (object)model.PARTS_CODE);
                            cmd.Parameters.AddWithValue("@WATCH_CODE", (model.WATCH_CODE == null) ? (object)DBNull.Value : (object)model.WATCH_CODE);
                            cmd.Parameters.AddWithValue("@CLASSIFICATION", (model.CLASSIFICATION == null) ? (object)DBNull.Value : (object)model.CLASSIFICATION);
                            cmd.Parameters.AddWithValue("@ISSUE_ID", emp_id);
                            cmd.Parameters.AddWithValue("@APPROVE_BY", model.APPROVE_ID);

                        }
                        
                        cmd.ExecuteNonQuery();
                        
                        //send Email
                        List<string> sendTo = new List<string>();
                        List<string> attch = new List<string>();
                        sendTo.Add(model.APPROVE_EMAIL);
                        if (model.FILE_LIST != null)
                        {
                            foreach (string list in model.FILE_LIST)
                            {
                                attch.Add(Server.MapPath("~/Temp_File/").ToString() + list);
                            }
                        }                        
                      
                        TemplateEmail(model, sendTo, attch, "CREATE", null);
                        
                        TempData["createRequestDoc"] = "Save and send Email completed!";
                        conn.Close();
                        return "True";
                    }
                    catch (Exception ex)
                    {
                        sql.Clear();
                        sql.AppendLine(" INSERT INTO tbl_Error_Data ");
                        sql.AppendLine(" (system_id, error_code, error_desc, error_user, error_date) ");
                        sql.AppendLine(" VALUES ('0000000006', "+ Fn.getSQL(ex.Message) + ", " + Fn.getSQL(ex.Message) + ", " + Fn.getSQL(emp_id) + ", getdate() )");
                        cmd.CommandText = sql.ToString();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                        return "False";
                    }
                    
                }
            }
            
        }


        [HttpPost]
        public JsonResult UploadFile()
        {

            string NEWID; 
            if (Request.QueryString["id"] != null)
            {
                NEWID = Request.QueryString["id"].ToString();
            }
            else
            {
                NEWID = System.Web.HttpContext.Current.Request.Params["id"];
            }

            Random generator = new Random();
            string random = generator.Next(0, 1000000).ToString("D6");

            List<string> fileList = new List<string>();
            HttpFileCollectionBase files = Request.Files;
            for (int i = 0; i < files.Count; i++)
            {
                string filePart = Server.MapPath("~/Temp_File/").ToString();
                HttpPostedFileBase file = files[i];
                string fname;
                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = random +"_"+ file.FileName;
                }

                file.SaveAs(filePart + fname);

                using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(null, conn))
                    {
                        StringBuilder sql = new StringBuilder();
                        sql.Clear();
                        sql.AppendLine(" INSERT INTO TBL_TECH_IS_FILES ");
                        sql.AppendLine(" (IS_ID, PATCH) ");
                        sql.AppendLine(" VALUES ( ");
                        sql.AppendLine(Fn.getSQL(NEWID));
                        sql.AppendLine(", @fname )");
                        //sql.AppendLine("," + Fn.getSQL(fname) + ")");
                        cmd.CommandText = sql.ToString();
                        cmd.Parameters.AddWithValue("@fname", fname);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

            }

            using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Clear();
                    sql.AppendLine(" SELECT PATCH FROM TBL_TECH_IS_FILES ");
                    sql.AppendLine(" WHERE IS_ID =" + Fn.getSQL(NEWID));
                    cmd.CommandText = sql.ToString();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        fileList.Add(reader["PATCH"].ToString());
                    }

                }
                conn.Close();
            }
            return Json(fileList, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult deleteFile(string patch, string id)
        {
            List<string> fileList = new List<string>();
            string filePart = Server.MapPath("~/Temp_File/").ToString();
            string fileName = patch;

            try
            {
                FileInfo file = new FileInfo(filePart + fileName);
                file.Delete();
                using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(null, conn))
                    {
                        StringBuilder sql = new StringBuilder();
                        cmd.CommandText = " DELETE FROM TBL_TECH_IS_FILES WHERE IS_ID = " + Fn.getSQL(id) +
                                          //" AND PATCH = " + Fn.getSQL(patch);
                                          " AND PATCH = @PATCH ";
                        cmd.Parameters.AddWithValue("@PATCH", patch);
                        cmd.ExecuteNonQuery();

                        sql.Clear();
                        sql.AppendLine(" SELECT PATCH FROM TBL_TECH_IS_FILES ");
                        sql.AppendLine(" WHERE IS_ID =" + Fn.getSQL(id));
                        cmd.CommandText = sql.ToString();
                        SqlDataReader reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            fileList.Add(reader["PATCH"].ToString());
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The deletion failed: {0}", e.Message);
            }

            return Json(fileList, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getRequirementInfoList(string IS_NO, string ISSUEID, string APPROVEID, string MAXRESULT, string DEP, string STATUS)
        {
            List<RequestDocModel> res = new List<RequestDocModel>();
            string statusCmd = "";
            if (STATUS == "CREATED")
            {
                statusCmd = " AND EXISTS ( SELECT * FROM TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = R.IS_ID) ";
            } else if (STATUS == "WATTING")
            {
                statusCmd = " AND NOT EXISTS ( SELECT * FROM TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = R.IS_ID) ";
            }
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(" SELECT TOP " + MAXRESULT + " R.*, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, CONCAT(EMP.empTitleEng,' ',EMP.empNameEng) AS APPROVE_NAME, CONCAT(EMPI.empTitleEng,' ',EMPI.empNameEng) AS ISSUE_NAME ");
                    sql.AppendLine(" , case when (SELECT REQUEST_NO FROM TBL_TECH_IS_DOCINFO WHERE REQUEST_NO = R.IS_ID) = R.IS_ID then 'Create' else 'Wait Create' end STATUS FROM TBL_TECH_IS_REQUEST R JOIN [db_employee].[dbo].[tbl_employee] EMP ON R.APPROVE_BY = EMP.empId ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_employee] EMPI ON R.ISSUE_ID = EMPI.empId ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID ");
                    sql.AppendLine(" JOIN [db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT ");
                    sql.AppendLine(" WHERE APPROVE_STATUS IN ('APPROVED') ");
                    sql.AppendLine(" AND R.PLANT = " + Fn.getSQL(Session["plant_id"].ToString()));
                    if (ISSUEID != "" && ISSUEID != null)
                    {
                        sql.AppendLine(" AND ISSUE_ID = " + Fn.getSQL(ISSUEID));
                    }
                    if (APPROVEID != "" && APPROVEID != null)
                    {
                        sql.AppendLine(" AND APPROVE_BY = " + Fn.getSQL(APPROVEID));
                    }
                    if (DEP != "" && DEP != null)
                    {
                        sql.AppendLine(" AND DEPARTMENT = " + Fn.getSQL(DEP));
                    }
                    if (IS_NO != "" && IS_NO != null)
                    {
                        sql.AppendLine(" AND IS_NO like '%" + IS_NO + "%'");
                    }

                    if (STATUS != "" && STATUS != null)
                    {
                        sql.AppendLine(statusCmd);
                    }

                    sql.AppendLine(" order by IS_NO desc ");
                    
                    cmd.CommandText = sql.ToString();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while(reader.Read())
                    {
                        RequestDocModel model = new RequestDocModel();
                        model.IS_ID = reader["IS_ID"].ToString();
                        model.IS_NO = reader["IS_NO"].ToString();
                        model.SUBJECT = reader["SUBJECT"].ToString();
                        model.ISSUE_NAME = reader["ISSUE_NAME"].ToString();
                        model.APPROVE_NAME = reader["APPROVE_NAME"].ToString();
                        model.DEPARTMENT = reader["DEP"].ToString();
                        model.APPROVE_STATUS = reader["STATUS"].ToString();
                        model.ISSUE_DATE = Convert.ToDateTime(reader["ISSUE_DATE"].ToString());
                        res.Add(model);
                    }
                }
                con.Close();
            }
            
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getInfoList(string NO, string RequestID, string RequestBy, string MAXRESULT, string DEP, string STATUS)
        {
            List<DocInfoModel> res = new List<DocInfoModel>();
            using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.AppendLine(" SELECT TOP "+ MAXRESULT  + " R.ISSUE_DATE, R.IS_NO, R.IS_ID, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, CONCAT(EMP.empTitleEng,' ', UPPER(LEFT(EMP.empNameEng,1))+LOWER(SUBSTRING(EMP.empNameEng,2,LEN(EMP.empNameEng)))) AS ISSUE_NAME ");
                    sql.AppendLine(" , D.INFO_NO, UPPER(LEFT(D.STATUS,1))+LOWER(SUBSTRING(D.STATUS,2,LEN(D.STATUS))) STATUS ");
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
                        sql.AppendLine(" AND D.INFO_NO = " + Fn.getSQL(NO));
                    }
                    if (RequestID != "" && RequestID != null)
                    {
                        sql.AppendLine(" AND R.IS_NO LIKE '%" +RequestID + "%'");
                    }
                    if (DEP != "" && DEP != null)
                    {
                        sql.AppendLine(" AND D.DEPARTMENT = " + Fn.getSQL(DEP));
                    }
                    if (RequestBy != "" && RequestBy != null)
                    {
                        sql.AppendLine(" AND R.ISSUE_ID = '" + RequestBy + "'");
                    }
                    if (STATUS != "" && STATUS != null)
                    {
                        sql.AppendLine("AND D.STATUS =" + Fn.getSQL(STATUS));
                    }
                    sql.AppendLine(" order by INFO_NO desc ");
                    cmd.CommandText = sql.ToString();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while(reader.Read())
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
                        res.Add(doc);
                    }

                }
                con.Close();
            }

            return Json(res, JsonRequestBehavior.AllowGet);
        }
  
        [HttpPost]
        public bool deleteAllFile(RequestDocModel model)
        {
            try
            {
                string filePart = Server.MapPath("~/Temp_File/").ToString();
                string fileall = "";
                string param = "";

                int s = 1;
                foreach(string list in model.FILE_LIST)
                {
                    string fileName = list;
                    FileInfo file = new FileInfo(filePart + fileName);
                    file.Delete();
                    if (fileall != "")
                    {
                        fileall += ",'" + list + "'";
                        param += ", @p_" + s.ToString();
                    }
                    else
                    {
                        fileall += "'" + list + "'";
                        param += " @p_" + s.ToString();
                    }
                    s++;

                }

                using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(null, conn))
                    {
                        StringBuilder sql = new StringBuilder();
                        cmd.CommandText = " DELETE FROM TBL_TECH_IS_FILES WHERE IS_ID = " + Fn.getSQL(model.IS_ID) +
                                           //" AND PATCH in (" + fileall + ")";
                                           " AND PATCH in (" + param + ")";

                        int i = 1;
                        foreach (string list in model.FILE_LIST)
                        {
                            cmd.Parameters.AddWithValue("@p_"+ i.ToString(), list);
                            i++;
                        }

                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpPost]
        public bool ApproveRequest(RequestDocModel model)
        {
            List<string> sendTo = new List<string>();
            List<string> sendToName = new List<string>();
            if (model.APPROVE_STATUS == "APPROVED")
            {
                using (SqlConnection conn = new SqlConnection(Fn.connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(null, conn);
                    cmd.CommandText = " SELECT empEmail, CONCAT(E.empTitleEng,' ',E.empNameEng) AS NAME FROM tbl_access_right R " +
                                              " JOIN tbl_employee E ON E.empId = R.empId WHERE IS_RECEIVE LIKE '%R%' ";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        sendTo.Add(dr["empEmail"].ToString());
                        sendToName.Add(dr["NAME"].ToString());
                    }
                    conn.Close();
                }
            }
            else
            {
                sendToName.Add(model.ISSUE_NAME);
                sendTo.Add(model.ISSUE_EMAIL);
            }

            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    try
                    {
                        string IS_NO = null;
                        if (model.APPROVE_STATUS == "APPROVED")
                        {
                            cmd.CommandText = " SELECT CASE WHEN (SELECT TOP 1SUBSTRING(IS_NO, 5, 4) AS INT FROM TBL_TECH_IS_REQUEST ORDER BY IS_NO DESC) = (SELECT YEAR(GETDATE())) THEN " +
                                              "  CASE WHEN (select COUNT(*)  from TBL_TECH_IS_REQUEST) > 0  then (select TOP 1 CONCAT('REG-', YEAR(GETDATE()), FORMAT( CAST(right(IS_NO,3) AS int)+1, '00#')) from TBL_TECH_IS_REQUEST ORDER BY IS_NO DESC) " +
                                              " ELSE CONCAT('REG-', YEAR(GETDATE()), '001') END ELSE CONCAT('REG-', YEAR(GETDATE()), '001') END AS IS_NO ";
                            IS_NO = cmd.ExecuteScalar().ToString();
                        }
                        
                        cmd.CommandText = " UPDATE TBL_TECH_IS_REQUEST SET APPROVE_STATUS = " + Fn.getSQL(model.APPROVE_STATUS) +
                                          " , IS_NO = " + Fn.getSQL(IS_NO) +
                                          " , APPROVE_DATE = getdate() " +
                                          " , APPROVE_COMMENT = @APPROVE_COMMENT" + //Fn.getSQL(model.APPROVE_COMMENT) +
                                          " WHERE IS_ID = " + Fn.getSQL(model.IS_ID);
                        cmd.Parameters.AddWithValue("@APPROVE_COMMENT", (model.APPROVE_COMMENT == null) ? (object)DBNull.Value : (object)model.APPROVE_COMMENT);
                        cmd.ExecuteNonQuery();
                        
                        string patch = Server.MapPath("~/Temp_File/");
                        List<string> attch = new List<string>();
                        if (model.APPROVE_STATUS == "APPROVED" && model.FILE_LIST != null)
                        {
                            //Upload file into SERVER//
                            try
                            {
                                string ftp = "ftp://10.145.163.10/";
                                string ftpFolder = "IS/";
                                foreach (string list in model.FILE_LIST)
                                {
                                    string filePart = Server.MapPath("~/Temp_File/").ToString();
                                    string tempPath = filePart + list;
                                    using (var client = new WebClient())
                                    {
                                        string uriDestination = ftp + ftpFolder + list;
                                        string uriSource = tempPath;
                                        client.Credentials = new NetworkCredential("sodick", "Rtc0000", "rtc");
                                        client.UploadFile(uriDestination, WebRequestMethods.Ftp.UploadFile, uriSource);
                                    }

                                    if ((System.IO.File.Exists(tempPath)))
                                    {
                                        System.IO.File.Delete(tempPath);
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                con.Close();
                                return false;
                            }

                            foreach (string list in model.FILE_LIST)
                            {
                                attch.Add(list);
                            }
                            
                        }
                        //else
                        //{
                        //    sendToName.Add(model.ISSUE_NAME);
                        //    sendTo.Add(model.ISSUE_EMAIL);
                        //    //foreach (string list in model.FILE_LIST)
                        //    //{
                        //    //    attch.Add(patch + list);
                        //    //}
                        //}

                        TemplateEmail(model, sendTo, attch, model.APPROVE_STATUS, sendToName);
                        con.Close();
                        TempData["successApprove"] = "show";
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

        [HttpGet]
        public JsonResult getMailGrp(string id, string issueID)
        {
            List<MailGroup> grp = new List<MailGroup>();
            if (id == null) { id = "1"; }
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    //string current_userid = Session["emp_id"].ToString();
                    cmd.CommandText = " select USERID from TBL_TECH_IS_MAILGRP where ID = "+Fn.getSQL(id);
                    string grp_id = cmd.ExecuteScalar().ToString();
                    cmd.CommandText = " select empId , empEmail, CONCAT(empTitleEng,' ', empNameEng) EMP_NAME from [db_employee].[dbo].[tbl_employee] where empId in (" + Fn.getSQL(issueID) +"," + grp_id + ")";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MailGroup res = new MailGroup();
                        res.EMP_ID = dr["empId"].ToString();
                        res.EMP_EMAIL = dr["empEmail"].ToString();
                        res.EMP_NAME = dr["EMP_NAME"].ToString();
                        grp.Add(res);
                    }
                }
                con.Close();
            }

            return Json(grp.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult getMailGrpCC(string id)
        {
            List<MailGroup> grp = new List<MailGroup>();
            if (id == null) { id = "1"; }
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    string current_userid = Session["emp_id"].ToString();
                    cmd.CommandText = " select USERID from TBL_TECH_IS_MAILGRP_CC where ID = " + Fn.getSQL(id);
                    string grp_id = cmd.ExecuteScalar().ToString();
                    string query = "";
                    if (grp_id != "" && grp_id != null)
                    {
                        query += " select empId , empEmail, CONCAT(empTitleEng,' ', empNameEng) EMP_NAME from [db_employee].[dbo].[tbl_employee] where empId in (" + grp_id + ") UNION ";
                    }
                    query += " select '0000' empId, TE_MAIL empEmail, TE_NAME EMP_NAME from TBL_TECH_IS_MAILGRP_CC where ID = " + Fn.getSQL(id);
                    cmd.CommandText = query.ToString();
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MailGroup res = new MailGroup();
                        res.EMP_ID = dr["empId"].ToString();
                        res.EMP_EMAIL = dr["empEmail"].ToString();
                        res.EMP_NAME = dr["EMP_NAME"].ToString();
                        grp.Add(res);
                    }
                }
                con.Close();
            }

            return Json(grp.ToList(), JsonRequestBehavior.AllowGet); ;
        }
        
        public bool TemplateEmail(RequestDocModel model, List<string>sendTo,List<string>att, string action, List<string> sendToName)
        {
            DateTime dateNow = Convert.ToDateTime(DateTime.Now);
            string date = null;
            string subject = null;
            string html = null;
            string sendToN = null;
            string activity = null;
            string approveColumn = null;
            if (sendToName != null)
            {
                foreach (var s in sendToName)
                {
                    if (sendToN == null)
                    {
                        sendToN += s;
                    }
                    else
                    {
                        sendToN += "," + s;
                    }
                }
            }

            if (action == "APPROVED")
            {
                html += "<p><b>Dear K'" + sendToN + "</b></p><p>REQUIREMENT AND INFORMATION DOCUMENT is waiting for your receive. </p>";
                subject = "Information System: REQUIREMENT AND INFORMATION DOCUMENT is waiting for your receive.";
                date = model.ISSUE_DATE.ToString("yyyy-MM-dd");
                activity = "<a href='"+ url + "/TE-IS/Create/ReceivedRequirment/" + model.IS_ID + "'> Click link to view/received requirement </a>";
                approveColumn = "<tr><td width='20%'> Information By :</td> <td colspan='2'>" + model.ISSUE_NAME + "</td></tr> <tr><td width='20%'> Approve By :</td> <td colspan='2'>" + model.APPROVE_NAME + "</td></tr>";
            }
            else if (action == "REJECT")
            {
                html += "<p><b>Dear K'" + sendToN + "</b></p><p>Your document has been canceled. Please modify the requirements.</p>";
                subject = "Information System: Requirement Document reject";
                date = model.ISSUE_DATE.ToString("yyyy-MM-dd");
                activity = "<a href='" + url + "/TE-IS/Create/CreateRequirementDoc/" + model.IS_ID + "'> Click link to edit requirement </a>";
                approveColumn = "<tr><td width='20%'> Information By :</td> <td colspan='2'>" + model.ISSUE_NAME + "</td></tr> <tr><td width='20%'> Reject By :</td> <td colspan='2'>" + model.APPROVE_NAME + "</td></tr>";
            }
            else if (action == "CREATE")
            {
                html += "<p><b>Dear K'" + model.APPROVE_NAME + "</b></p><p>The information system would like to request that you approve the documents as detailed below.</p>";
                date = dateNow.ToString("yyyy-MM-dd");
                subject = "Information System: Requirement Document create";
                activity = "<a href='" + url + "/TE-IS/Create/ApproveRequest/" + model.IS_ID + "'> Click link to approve requirement </a>";
                approveColumn = "<tr><td width='20%'> Information By :</td> <td colspan='2'>" + Session["login_name"].ToString() + "</td></tr>";
            }

            html += "<table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'>" +
                " <tr> <td height ='35' colspan='3' align='center' style='background-color:#65A4FF;'><b>Information System</b></td> </tr>" +
                " <tr><td width='20%'>Subjet :</td> <td colspan='2'>" + model.SUBJECT + "</td></tr><tr><td width='20%'>Issue Date</td> <td colspan='2'>" + date + "</td></tr> <tr><td width='20%'>Data Of Change</td> <td colspan='2'>" + model.DOCUMENT_DATA_OF_CHANGE + "</td></tr>" +
                " <tr><td width='20%'>Parts Code :</td> <td colspan='2'>" + model.PARTS_CODE + "</td></tr> <tr><td width='20%'>Watch Code :</td> <td colspan='2'>" + model.WATCH_CODE + "</td></tr>" +
                " <tr><td width='20%'>START PROD.MONTH</td> <td colspan='2'>" + model.START_PROD_MONTH + "</td></tr> <tr><td width='20%'>Classification for changing :</td> <td colspan='2'>" + model.CLASSIFICATION + "</td></tr>";

            if (action == "REJECT" || action == "APPROVED")
            {
                html += " <tr><td width='20%'>Approve Comment :</td> <td colspan='2'>" + model.APPROVE_COMMENT + "</td></tr> ";
            }
            html += approveColumn +
                    " </table><br/> <div>"+ activity +"</div>" +
                    " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards, ^-^</div><div>IT developer team (363)</div>";
            
            Fn.sendMail(subject, sendTo, html, att, null, action);
            return true;
        }
        


        [HttpPost]
        public bool saveInformation(string id)
        {
            using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
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

                        cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'IN-PROGRESS' WHERE ID = " + Fn.getSQL(id);
                        cmd.ExecuteNonQuery();

                        DocInfoModel dm = new DocInfoModel();
                        cmd.CommandText = " SELECT DO.*, EMP.empTitleEng + ' ' +EMP.empNameEng ISSUE_NAME ,MG.NAME DOC_FLOW, R.REASON_EXPLAIN, R.IS_NO, R.ISSUE_ID REQ_ISSUE_ID, R.APPROVE_STATUS REQ_STATUS, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) DEP, RL.NAME RELATION_NAME " +
                                          " FROM TBL_TECH_IS_DOCINFO DO JOIN TBL_TECH_IS_REQUEST R ON DO.REQUEST_NO = R.IS_ID JOIN [db_employee].[dbo].[tbl_employee] EMP ON DO.ISSUE_ID = EMP.empId " +
                                          " JOIN TBL_TECH_IS_MAILGRP MG ON DO.DOC_FLOW_ID = MG.ID JOIN [db_employee].[dbo].[tbl_Dept] DEP ON R.DEPARTMENT = DEP.Dept_ID " +
                                          " JOIN [db_employee].[dbo].[tbl_plant] P ON P.plant_id = R.PLANT JOIN TBL_TECH_IS_RELATION RL ON RL.ID = DO.RELATION_ID " +
                                          " WHERE DO.ID = " + Fn.getSQL(id);
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
                            dm.txt_ATT_DOC_OTHER = dr["ATT_DOC_OTHER"].ToString();
                            dm.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString();
                            dm.DOC_FLOW_TEXT = dr["DOC_FLOW"].ToString();
                            dm.REQUEST_NO = dr["IS_NO"].ToString();
                            dm.DETAIL_REF = dr["IS_NO"].ToString();
                            dm.PLANT_DEP = dr["DEP"].ToString();
                            dm.RELATION_TEXT = dr["RELATION_NAME"].ToString();
                            if (dr["REQ_STATUS"].ToString() != "NO REQUEST")
                            {
                                dm.REQUIREMENT_ISSUE_ID = dr["REQ_ISSUE_ID"].ToString();
                            }
                        }

                        ////////////Mail CC
                        dr.Close();
                        cmd.CommandText = " SELECT * FROM TBL_TECH_IS_MAILGRP_CC " +
                                          " WHERE ID =  " + Fn.getSQL(dm.CC_GRP);
                        dr = cmd.ExecuteReader();
                        if (dr.Read())
                        {
                            mail_cc_list = dr["USERID"].ToString();
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
                        string mailGrp = "";
                        string req_issue_id = "";
                        if (dm.REQUIREMENT_ISSUE_ID != null && dm.REQUIREMENT_ISSUE_ID != "")
                        {
                            req_issue_id = ",'" + dm.REQUIREMENT_ISSUE_ID + "'";
                        }
                        dr.Close();


                        cmd.CommandText = " SELECT USERID FROM TBL_TECH_IS_MAILGRP " +
                                          " WHERE ID =  " + Fn.getSQL(dm.DOC_FLOW_ID);
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
                        if (dm.txt_ATT_DOC_REQUIRE != null && dm.txt_ATT_DOC_REQUIRE != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_REQUIRE);
                        }
                        if (dm.txt_ATT_DOC_OTHER != null && dm.txt_ATT_DOC_OTHER != "")
                        {
                            att.Add(Server.MapPath("~/Temp_File/").ToString() + dm.txt_ATT_DOC_OTHER);
                        }

                        //send mail to approve list
                        subject = " Information System: Information Document is waiting for your approval.";
                        template += "<p><b>Dear Approver </b></p><p>Information Document is waiting for your approval.</p>";
                        template += getTemplateEmailCreate(dm, "save");
                        TempData["CreateSucess"] = "Successfully save and send email the information document.";
                        
                        Fn.sendMail(subject, mailTo, template, att);
                        
                        //send mail to CC group
                        if (dm.ACKNOWLAGE_FLEX == "True")
                        {
                            subject_cc = " Information System: Information Document is for your acknowledgement.";
                            template_cc += "<p><b>Dear Acknowledge person</b></p><p>Information Document is for your acknowledgement.</p>";
                            template_cc += getTemplateEmailCreate(dm, "cc_grp");
                            Fn.sendMail(subject_cc, mailCC, template_cc, att);
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

        public string getTemplateEmailCreate(DocInfoModel model, string action)
        {
            string html = null;
            try
            {
                html += "<table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'>" +
                        "<tr> <td height ='35' colspan='3' align='center' style='background-color:#65A4FF;'><b>Information System</b></td> </tr>" +
                        "<tr><td width='20%'>Plant/Department</td> <td colspan='2'>" + model.PLANT_DEP + "</td></tr><tr><td width='20%'>Issue Name</td> <td colspan='2'>" + model.ISSUE_NAME + "</td></tr> <tr><td width='20%'>Document Type</td> <td colspan='2'>" + model.DOC_TYPE + "</td></tr>" +
                        "<tr><td width='20%'>Document flow</td> <td colspan='2'>" + model.DOC_FLOW_TEXT + "</td></tr>";
                if (action != "cc_grp")
                {
                    html += " <tr><td width='20%'>Reson Explain</td> <td colspan='2'>" + model.REASON_EXPLAIN + "</td></tr> <tr><td width='20%'>Detail Reference</td> <td colspan='2'>" + model.DETAIL_REF + "</td></tr>";
                }

                html += " <tr><td width='20%'>Document Detail</td> <td colspan='2'>" + model.DOC_DETAIL + "</td></tr> <tr><td width='20%'>Effective</td> <td colspan='2'>" + model.EFFECTIVE + "</td></tr>" +
                        " <tr><td width='20%'>Add order no </td> <td colspan='2'>" + model.ADD_ORDER_NO + "</td></tr>";
                if (action != "cc_grp")
                {
                    html += "<tr><td width='20%'>Relation system</td> <td colspan='2'>" + model.RELATION_TEXT + "</td></tr>";
                }

                html += " </table><br/>";
                if (action != "cc_grp")
                {
                    html += "<a href='" + url + "/TE-IS/Approve/ApproveList/" + model.ID + "'>Click link to approve information document </a>";
                }
                        
                html += "<div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>";
                
                return html;
            }
            catch(Exception ex)
            {
                return html;
            }
        }

        [HttpPost]
        public bool deleteOneFile(string patch, string id, string field)
        {
            //List<string> fileList = new List<string>();
            string filePart = Server.MapPath("~/Temp_File/").ToString();
            string fileName = patch;
            string fieldDelete = "";
            if (field == "PIC_REF_1")
            {
                fieldDelete = " PIC_REF_1 = NULL ";
            }
            else if (field == "PIC_REF_2")
            {
                fieldDelete = " PIC_REF_2 = NULL ";
            }
            else if (field == "ATT_DOC_PURCHASE")
            {
                fieldDelete = " ATT_DOC_PURCHASE = NULL ";
            }
            else if (field == "ATT_DOC_REQUIRE")
            {
                fieldDelete = " ATT_DOC_REQUIRE = NULL ";
            }
            else
            {
                fieldDelete = " ATT_DOC_OTHER = NULL ";
            }
            try
            {
                FileInfo file = new FileInfo(filePart + fileName);
                file.Delete();
                using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(null, conn))
                    {
                        StringBuilder sql = new StringBuilder();
                        cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET " + fieldDelete +
                                          " WHERE ID = " + Fn.getSQL(id);
                        cmd.ExecuteNonQuery();
                        
                    }
                    conn.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("The deletion failed: {0}", e.Message);
                return false;
            }
            
        }

        [HttpPost]
        public bool cancel(string id)
        {
            try
            {
                using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
                {
                    con.Open();
                    using(SqlCommand cmd = new SqlCommand(null, con))
                    {
                        cmd.CommandText = " UPDATE TBL_TECH_IS_DOCINFO SET STATUS = 'REJECT' WHERE ID = " + Fn.getSQL(id);
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = " UPDATE TBL_TECH_IS_REQUEST SET APPROVE_STATUS = 'REJECT' WHERE IS_ID = (" +
                                          " SELECT REQUEST_NO FROM TBL_TECH_IS_DOCINFO WHERE ID = " + Fn.getSQL(id) + ")";
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            } 
            catch(Exception ex)
            {
                return false;
            }
        }

        [HttpGet]
        public JsonResult getApproveLists(string id)
        {
            List<MailGroup> grp = new List<MailGroup>();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                    cmd.CommandText = " SELECT DA.APPROVE_ID, CONCAT(EMP.empTitleEng,' ', EMP.empNameEng) APPROVE_NAME, EMP.empEmail APPROVE_EMAIL, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE " +
                                      " FROM TBL_TECH_IS_DOCINFO_APPROVE DA " +
                                      " JOIN [db_employee].[dbo].[tbl_employee] EMP ON DA.APPROVE_ID = EMP.empId " +
                                      " WHERE DOCINFO_ID = " + Fn.getSQL(id) +
                                      " AND APPROVE_LEVEL IS NULL ";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        MailGroup res = new MailGroup();
                        res.EMP_ID = dr["APPROVE_ID"].ToString();
                        res.EMP_EMAIL = dr["APPROVE_EMAIL"].ToString();
                        res.EMP_NAME = dr["APPROVE_NAME"].ToString();
                        res.EMP_STATUS = dr["STATUS"].ToString();
                        res.EMP_COMMENT = dr["COMMENT"].ToString();
                        if (dr["APPROVE_DATE"].ToString() != "" && dr["APPROVE_DATE"].ToString() != null)
                        {
                            res.EMP_DATE = DateTime.Parse(dr["APPROVE_DATE"].ToString());
                        }
                        else
                        {
                            res.EMP_DATE = null;
                        }

                        grp.Add(res);
                    }
                }
                con.Close();
            }


            return Json(grp.ToList(), JsonRequestBehavior.AllowGet);
        }


        public bool rejectRequirement(string id, string comment)
        {
            string url = System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                    try
                    {
                        cmd.CommandText = " UPDATE TBL_TECH_IS_REQUEST " +
                                          " SET APPROVE_STATUS = 'TE-REJECT' " +
                                          " , APPROVE_COMMENT = @APPROVE_COMMENT " + //Fn.getSQL(comment) +
                                          " WHERE IS_ID = " + Fn.getSQL(id);
                        cmd.Parameters.AddWithValue("@APPROVE_COMMENT", (comment == null) ? (object)DBNull.Value : (object)comment);
                        cmd.ExecuteNonQuery();
                        
                        RequestDocModel reqModel = getDocInfo(id, "notIssueID");
                        string subject = " Information System: Requirement No. " + reqModel.IS_NO;
                        List<string> sendTo = new List<string>();
                        sendTo.Add(reqModel.ISSUE_EMAIL);
                        string html = "<p>Dear " + reqModel.ISSUE_NAME + " </p><p>Requirement No. <b> " + reqModel.IS_NO + "</b> has been rejected from Technical department.</p> <p>Please check the details and try creating a new requirement document again.</p>"
                                    + "<p>Sorry for the inconvenience.</p>";
                        html += "<p><a href='"+ url + "/TE-IS/Create/ReceivedRequirment/" + reqModel.IS_ID +"'>Click link to view requirement</a></p>" +
                                "<p>If you have any question please do not hesitate to contact me. Thank you & Best Regards, ^-^</p> " +
                                "<p>IT developer team (363)</p>";
                        Fn.sendMail(subject, sendTo, html);

                        con.Close();
                        return true;
                    } 
                    catch(Exception ex)
                    {
                        con.Close();
                        return false;
                    }
                }
            }
        }


    }
}