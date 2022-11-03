using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Text;
using System.Web.Mvc;
using Information_System.Models;
using System.IO;
using ClosedXML.Excel;
using System.Data;

namespace Information_System.Controllers
{
    public class HomeController : Controller
    {
        InFunction Fn = new InFunction();
        string constr = new InFunction().connString;

        public ActionResult Index()
       {
            if (Session["login_id"] == null)
            {
                return View("Login");
            }
            else
            {
                DashboardModel dbModel = new DashboardModel();
                SqlConnection con = new SqlConnection(Fn.conRTCStr);
                con.Open();
                SqlCommand cmd = new SqlCommand(null, con);
                cmd.CommandText = " SELECT (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO ) TOTAL, (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS NOT IN ('APPROVED') ) IN_PROGRESS," +
                                  " (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS = 'APPROVED') APPROVED ";
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    dbModel.total = Int32.Parse(dr["TOTAL"].ToString());
                    dbModel.waiting = Int32.Parse(dr["IN_PROGRESS"].ToString());
                    dbModel.finished = Int32.Parse(dr["APPROVED"].ToString());
                }

                con.Close();               
               
                ViewData["DB"] = dbModel;

                return View();
            }
        }

        public ActionResult Login()
        {

            if (Session["login_id"] == null)
            {
                return View();
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost]
        public string checkLogin()
        {
            System.Uri UrlReferrer = System.Web.HttpContext.Current.Request.UrlReferrer;
            string returnURL = UrlReferrer.ToString();
            var user = System.Web.HttpContext.Current.Request.Form["username"];
            var password = System.Web.HttpContext.Current.Request.Params["password"];
            string status = "";
            using (SqlConnection conn = new SqlConnection(constr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    StringBuilder sql = new StringBuilder();
                    sql.Append(" SELECT USR.userId userId, USR.userName userName, USR.Emp_ID EmpID, EMP.deptId deptId, EMP.PlantID , AC.*,P.plant_mark, D.Dept_Remark ");
                    sql.Append(" FROM tbl_user USR INNER JOIN tbl_employee EMP ON USR.Emp_ID = EMP.empId LEFT JOIN tbl_access_right AC ON AC.empId = EMP.empId ");
                    sql.Append(" LEFT JOIN tbl_Dept D ON EMP.deptId = D.Dept_ID LEFT JOIN tbl_plant P ON EMP.PlantID = P.plant_id");
                    sql.Append(" WHERE userId = ").Append(Fn.getSQL(user));
                    sql.Append(" AND userPass = ").Append(Fn.getSQL(Fn.GetSHA1(password)));
                    cmd.CommandText = sql.ToString();
                    SqlDataReader dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        AccessRightModel ac = new AccessRightModel();
                        if (dr["IS_ADMIN"].ToString() != null && (dr["IS_ADMIN"].ToString() != "")) {
                            ac.IS_ADMIN = dr.GetBoolean(dr.GetOrdinal("IS_ADMIN"));
                        }
                        ac.IS_RECEIVE = dr["IS_RECEIVE"].ToString();
                        if (dr["IS_APP_LEVEL1"].ToString() != null && dr["IS_APP_LEVEL1"].ToString() != "")
                        {
                            ac.IS_APP_LEVEL1 = dr.GetBoolean(dr.GetOrdinal("IS_APP_LEVEL1"));
                        }
                        if (dr["IS_APP_LEVEL2"].ToString() != null && dr["IS_APP_LEVEL2"].ToString() != "")
                        {
                            ac.IS_APP_LEVEL2 = dr.GetBoolean(dr.GetOrdinal("IS_APP_LEVEL2"));
                        }
                        if (dr["IS_TE"].ToString() != null && dr["IS_TE"].ToString() != "")
                        {
                            ac.IS_TE = dr.GetBoolean(dr.GetOrdinal("IS_TE"));
                        }
                        if (dr["IS_IT"].ToString() != null && dr["IS_IT"].ToString() != "")
                        {
                            ac.IS_IT = dr.GetBoolean(dr.GetOrdinal("IS_IT"));
                        }
                        //if (dr["IS_APPROVE"].ToString() != null && dr["IS_APPROVE"].ToString() != "")
                        //{
                        //    ac.IS_APPROVE = dr.GetBoolean(dr.GetOrdinal("IS_APPROVE"));
                        //}

                        Session["permission"] = ac;
                        Session["login_id"] = dr["userId"];
                        Session["login_name"] = dr["userName"];
                        Session["emp_id"] = dr["empid"];
                        Session["dep_id"] = dr["deptId"];
                        Session["plant_id"] = dr["PlantID"];
                        Session["plantRemark"] = dr["plant_mark"] + "/" + dr["Dept_Remark"];
                        status = "Y";

                    }
                    else
                    {
                        status = "N";
                    }
                }
                conn.Close();
            }

            return status;
        }

        public ActionResult Logout()
        {
            Session["login_id"] = null;
            Session["login_name"] = null;
            Session["emp_id"] = null;
            Session["dep_id"] = null;
            Session["plant_id"] = null;
            return View("Login");
        }
        
        [HttpPost]
        public FileResult exportExcel(string type)
        {
            DataTable dt = new DataTable("Information_Report");
            string file_name;
            using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
            {
                conn.Open();
                string sql = @" SELECT INFO_NO, D.STATUS, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) PLANT, CONCAT(EMP.empTitleEng,' ', EMP.empNameEng) ISSUE_NAME, convert(varchar(10), D.ISSUE_DATE, 120) ISSUE_DATE " +
                              " , DOC_TYPE, M.NAME DOC_FLOW, R.SUBJECT , R.REASON_EXPLAIN, D.DOC_DETAIL, EMP.empNameEng + ' '+empLnameEng as RequestName , R.IS_NO DETAIL_REFERNCE, D.EFFECTIVE, D.ADD_ORDER_NO, D.PIC_REF_1 " +
                              " , D.PIC_REF_2,D.ATT_DOC_PURCHASE, D.ATT_DOC_REQUIRE, D.ATT_DOC_OTHER " +
                              " FROM TBL_TECH_IS_DOCINFO D " +
                              " JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID " +
                              " JOIN [db_employee].[dbo].[tbl_employee] EMP ON D.ISSUE_ID = EMP.empId " +
                              " JOIN TBL_TECH_IS_MAILGRP M ON D.DOC_FLOW_ID = M.ID " +
                              " JOIN [db_employee].[dbo].[tbl_Dept] DEP ON D.DEPARTMENT = DEP.Dept_ID " +
                              " JOIN [db_employee].[dbo].[tbl_plant] P ON D.PLANT = P.plant_id ";
                if (type == "yearly")
                {
                    sql += " WHERE D.PLANT = " + Fn.getSQL(Session["plant_id"].ToString()) + " AND YEAR(D.ISSUE_DATE) = YEAR(GETDATE())";
                    file_name = "Information_Report_Yearly.xlsx";
                } else if (type == "monthly")
                {
                    sql += " WHERE D.PLANT = " + Fn.getSQL(Session["plant_id"].ToString()) + " AND  MONTH(D.ISSUE_DATE) = MONTH(GETDATE())";
                    file_name = "Information_Report_Monthly.xlsx";
                } else
                {
                    file_name = "Information_Report.xlsx";
                }

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            dt.Load(dr);
                        }
                    }
                }
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", file_name);
                }
            }
        }

        public JsonResult MyRequirementList()
        {
            List<RequestDocModel> model = new List<RequestDocModel>();
            using (SqlConnection conn = new SqlConnection(Fn.conRTCStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(null, conn))
                {
                    cmd.CommandText = $@" SELECT * , a2.APPROVE_ID as APPROVE_ID , a2.STATUS as sta  FROM TBL_TECH_IS_REQUEST  a1
                         LEFT JOIN[db_test].[dbo].[TBL_TECH_IS_REQUEST_APPROVE] a2
                        ON a1.IS_ID = a2.[REQUEST_ID] WHERE ISSUE_ID = " + Fn.getSQL(Session["emp_id"].ToString());
                    SqlDataReader dr = cmd.ExecuteReader();
                    while(dr.Read())
                    {
                        RequestDocModel res = new RequestDocModel();
                        res.IS_ID = dr["IS_ID"].ToString();
                        res.IS_NO = dr["IS_NO"].ToString();
                        res.ISSUE_DATE = DateTime.Parse(dr["ISSUE_DATE"].ToString());
                        res.SUBJECT = dr["SUBJECT"].ToString();
                        res.APPROVE_STATUS = dr["APPROVE_STATUS"].ToString();
                        if(res.APPROVE_STATUS == "CREATE")
                        {
                            res.APPROVE_ID = dr["APPROVE_ID"].ToString();
                        }
                        else
                        {
                            res.APPROVE_ID = "";
                        }
                       
                        model.Add(res);
                    }
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getInforTotal()
        {
            DashboardModel dbModel = new DashboardModel();
            SqlConnection con = new SqlConnection(Fn.conRTCStr);
            con.Open();
            SqlCommand cmd = new SqlCommand(null, con);
            string plant = Session["plant_id"].ToString();
            cmd.CommandText = " SELECT (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS NOT IN ('REVISE') AND PLANT = " + Fn.getSQL(plant) +" ) TOTAL" +
                              " , (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS NOT IN ('APPROVED','REJECT','REVISE') AND PLANT = " + Fn.getSQL(plant) + ") IN_PROGRESS " +
                              " , (SELECT COUNT(ID) FROM TBL_TECH_IS_DOCINFO WHERE STATUS = 'APPROVED' AND PLANT = " + Fn.getSQL(plant) + ") APPROVED " +
                              " , (SELECT COUNT(ID) FROM TBL_TECH_IS_DOCINFO WHERE STATUS = 'REJECT' AND PLANT = " + Fn.getSQL(plant) + ") REJECT ";
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                dbModel.total = Int32.Parse(dr["TOTAL"].ToString());
                dbModel.waiting = Int32.Parse(dr["IN_PROGRESS"].ToString());
                dbModel.finished = Int32.Parse(dr["APPROVED"].ToString());
                dbModel.reject = Int32.Parse(dr["REJECT"].ToString());
            }

            con.Close();
            return Json(dbModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getInforYear(string year, string month)
        {
            string condition_month = "";
            if (month != null && month != "")
            {
                condition_month = " AND DATEPART(month, ISSUE_DATE) = " + month;
            }
            DashboardModel dbModel = new DashboardModel();
            SqlConnection con = new SqlConnection(Fn.conRTCStr);
            con.Open();
            SqlCommand cmd = new SqlCommand(null, con);
            string plant = Session["plant_id"].ToString();
            cmd.CommandText = " SELECT * , TOTAL.IN_PROGRESS *100 / NULLIF(TOTAL.TOTAL, 0) as P_IN_PROGRESS, TOTAL.APPROVED *100 / NULLIF(TOTAL.TOTAL, 0) as P_APPROVED, TOTAL.REJECT *100 / NULLIF(TOTAL.TOTAL, 0) as P_REJECT FROM ( " +
                              " SELECT (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS NOT IN ('REVISE') AND PLANT = " + Fn.getSQL(plant) + " AND YEAR(ISSUE_DATE) = "+ year + condition_month +" ) TOTAL" +
                              " , (SELECT COUNT(ID)  FROM TBL_TECH_IS_DOCINFO WHERE STATUS NOT IN ('APPROVED','REJECT','REVISE') AND PLANT = " + Fn.getSQL(plant) + " AND YEAR(ISSUE_DATE) = " + year + condition_month + " ) IN_PROGRESS " +
                              " , (SELECT COUNT(ID) FROM TBL_TECH_IS_DOCINFO WHERE STATUS = 'APPROVED' AND PLANT = " + Fn.getSQL(plant) + " AND YEAR(ISSUE_DATE) = " + year + condition_month + " ) APPROVED " +
                              " , (SELECT COUNT(ID) FROM TBL_TECH_IS_DOCINFO WHERE STATUS = 'REJECT' AND PLANT = " + Fn.getSQL(plant) + " AND YEAR(ISSUE_DATE) = " + year + condition_month + " ) REJECT " +
                              " ) AS TOTAL ";
            SqlDataReader dr = cmd.ExecuteReader();
            if (dr.Read())
            {
                dbModel.total = Int32.Parse(dr["TOTAL"].ToString());
                dbModel.waiting = Int32.Parse(dr["IN_PROGRESS"].ToString());
                dbModel.finished = Int32.Parse(dr["APPROVED"].ToString());
                dbModel.reject = Int32.Parse(dr["REJECT"].ToString());
                dbModel.p_waiting = dr["P_IN_PROGRESS"].ToString() == ""? 0 : Int32.Parse(dr["P_IN_PROGRESS"].ToString());
                dbModel.p_finished = dr["P_APPROVED"].ToString() == "" ? 0 : Int32.Parse(dr["P_APPROVED"].ToString());
                dbModel.p_reject = dr["P_REJECT"].ToString() == "" ? 0 : Int32.Parse(dr["P_REJECT"].ToString());
            }

            con.Close();
            return Json(dbModel, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getDocFlow()
        {
            List<DBDocFlowModel> res = new List<DBDocFlowModel>();
            SqlConnection con = new SqlConnection(Fn.conRTCStr);
            con.Open();
            SqlCommand cmd = new SqlCommand(null, con);
            string plant = Session["plant_id"].ToString();
            cmd.CommandText = " SELECT DISTINCT NAME, D.DOC_FLOW_ID , COUNT(D.DOC_FLOW_ID) COUNT_DB, (COUNT(D.DOC_FLOW_ID)*100/(select COUNT(ID) from TBL_TECH_IS_DOCINFO)) as PERCENTAGE FROM TBL_TECH_IS_MAILGRP M " +
                              " LEFT JOIN TBL_TECH_IS_DOCINFO D ON M.ID = D.DOC_FLOW_ID AND D.PLANT = " + Fn.getSQL(plant) + "  GROUP BY NAME, DOC_FLOW_ID ORDER BY COUNT_DB DESC ";

            SqlDataReader dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                DBDocFlowModel dbModel = new DBDocFlowModel();
                dbModel.NAME = dr["NAME"].ToString();
                dbModel.COUNT = Int32.Parse(dr["COUNT_DB"].ToString());
                dbModel.DOC_FLOW_ID = dr["DOC_FLOW_ID"].ToString();
                dbModel.PERCENTAGE = Int32.Parse(dr["PERCENTAGE"].ToString());
                res.Add(dbModel);
            }

            con.Close();
            return Json(res, JsonRequestBehavior.AllowGet);
        }

    }
}