using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Data.SqlClient;
using System.Text;
using Information_System.Models;
using Quartz;
using Quartz.Impl;

namespace Information_System
{
    public class AutoSendMail : IJob
    {
        InFunction Fn = new InFunction();
        public string url = "http://10.145.163.10/TE-IS"; //System.Web.HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        public void Execute(IJobExecutionContext context)
        {
            List<DocInfoModel> docList = new List<DocInfoModel>();
            List<string> mailto = new List<string>();
            using (SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(null, con))
                {
                    cmd.CommandText = " SELECT D.ID, CONCAT(P.plant_mark,'/', DEP.Dept_Remark) PLANT, INFO_NO, CONCAT(EMP.empTitleEng,' ', EMP.empNameEng) ISSUE_NAME, DOC_TYPE, M.NAME DOC_FLOW, R.SUBJECT, R.REASON_EXPLAIN, D.DOC_DETAIL " +
                                      " FROM TBL_TECH_IS_DOCINFO D JOIN TBL_TECH_IS_REQUEST R ON D.REQUEST_NO = R.IS_ID " +
                                      " JOIN [db_employee].[dbo].[tbl_employee] EMP ON D.ISSUE_ID = EMP.empId JOIN TBL_TECH_IS_MAILGRP M ON D.DOC_FLOW_ID = M.ID " +
                                      " JOIN [db_employee].[dbo].[tbl_Dept] DEP ON D.DEPARTMENT = DEP.Dept_ID JOIN [db_employee].[dbo].[tbl_plant] P ON D.PLANT = P.plant_id " +
                                      " WHERE STATUS NOT IN ('REJECT') " +
                                      " AND EXISTS ( SELECT DOCINFO_ID FROM TBL_TECH_IS_DOCINFO_APPROVE " +
                                      " WHERE IS_FOLLOW = 1 AND D.ID = DOCINFO_ID ) ";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        DocInfoModel model = new DocInfoModel();
                        model.ID = dr["ID"].ToString();
                        model.INFO_NO = dr["INFO_NO"].ToString();
                        model.ISSUE_NAME = dr["ISSUE_NAME"].ToString() != "" ? dr["ISSUE_NAME"].ToString() : "-";
                        model.DOC_SUBJECT = dr["SUBJECT"].ToString() != "" ? dr["SUBJECT"].ToString() : "-";
                        model.DOC_DETAIL = dr["DOC_DETAIL"].ToString() != "" ? dr["DOC_DETAIL"].ToString() : "-";
                        model.REASON_EXPLAIN = dr["REASON_EXPLAIN"].ToString() != "" ? dr["REASON_EXPLAIN"].ToString() : "-";
                        model.DOC_TYPE = dr["DOC_TYPE"].ToString() != "" ? dr["DOC_TYPE"].ToString() : "-";
                        model.DOC_FLOW_TEXT = dr["DOC_FLOW"].ToString() != "" ? dr["DOC_FLOW"].ToString() : "-";
                        model.PLANT_DEP = dr["PLANT"].ToString() != "" ? dr["PLANT"].ToString() : "-";
                        docList.Add(model);
                    }

                    if (docList != null)
                    {
                        foreach (var i in docList)
                        {
                            dr.Close();
                            cmd.CommandText = " SELECT DA.APPROVE_ID,CONCAT(e.empTitleEng,' ',e.empNameEng) APPROVE_NAME, DA.STATUS, DA.COMMENT, DA.APPROVE_DATE , d.Dept_Remark " +
                                              " FROM TBL_TECH_IS_DOCINFO_APPROVE DA JOIN db_employee.dbo.tbl_employee e on e.empId = DA.APPROVE_ID " +
                                              " left JOIN db_employee.dbo.tbl_Dept d on e.deptId = d.Dept_ID " +
                                              " WHERE DOCINFO_ID = " + Fn.getSQL(i.ID) + " AND APPROVE_LEVEL IS NULL ";
                            dr = cmd.ExecuteReader();
                            List<DocInfoApprove> m = new List<DocInfoApprove>();
                            while (dr.Read())
                            {
                                DocInfoApprove da = new DocInfoApprove();
                                da.APPROVE_NAME = dr["APPROVE_NAME"].ToString() != "" ? dr["APPROVE_NAME"].ToString() : "-";
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

                            string html = "<p>Dear All</p><p>Update progress information documents No." + i.INFO_NO + "</p>";
                            html += "<table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'>" +
                               " <tr> <td height ='35' colspan='3' align='center' style='background-color:#65A4FF;'><b>Information System</b></td> </tr>" +
                               " <tr><td width='20%'>No.</td> <td colspan='2'><b>" + i.INFO_NO + "<b></td></tr>" +
                               " <tr><td width='20%'>Plant/Department</td> <td colspan='2'>" + i.PLANT_DEP + "</td></tr><tr><td width='20%'>Issue Name</td> <td colspan='2'>" + i.ISSUE_NAME + "</td></tr> " +
                               " <tr><td width='20%'>Document Type</td> <td colspan='2'>" + i.DOC_TYPE + "</td></tr>" +
                               " <tr><td width='20%'>Document flow</td> <td colspan='2'>" + i.DOC_FLOW_TEXT + "</td></tr>" +
                               " <tr><td width='20%'>Subject</td> <td colspan='2'>" + i.DOC_SUBJECT + "</td></tr>" +
                               " <tr><td width='20%'>Reson Explain</td> <td colspan='2'>" + i.REASON_EXPLAIN + "</td></tr>" +
                               " <tr><td width='20%'>Document Detail</td> <td colspan='2'>" + i.DOC_DETAIL + "</td></tr>" +
                               " </table><br/>" +
                               " <table border='1' width='60%' style='border-collapse: collapse;font-family:monospace;'> " +
                               " <thead style='background-color:#65A4FF;'><tr><th>Approve Name</th><th>Department</th><th>Comment</th><th>Approve Date</th><th>Status</th></tr></thead> " +
                               " <tbody class='table-body' id='tbody'>";
                            foreach (var l in m)
                            {
                                html += " <tr> <td> " + l.APPROVE_NAME + " </td><td>  " + l.APPROVE_DEPT + " </td><td>  " + l.COMMENT + " </td><td>  " + l.APPROVE_DATE + " </td><td>  " + l.STATUS + " </td></tr>";
                            }
                            html += "</tbody> </table><br/>" +
                               " <a href='"+ url + "'>Click link to Information System </a>" +
                               " <div>If you have any question please do not hesitate to contact me. Thank you & Best Regards</div><div>IT developer team (363)</div>"; ;
                            string subject = "Update progress information documents No." + i.INFO_NO;
                            List<string> mailTo = emailList(i.ID);
                            Fn.sendMail(subject, mailTo, html);

                        }
                    }
                }
                con.Close();
            }
        }
     
        private List<string> emailList(string doc_id)
        {
            List<string> list = new List<string>();
            using(SqlConnection con = new SqlConnection(Fn.conRTCStr))
            {
                con.Open();
                using(SqlCommand cmd = new SqlCommand(null, con))
                {
                    cmd.CommandText = " SELECT E.empEmail FROM TBL_TECH_IS_DOCINFO_APPROVE A " +
                                      " JOIN db_employee.dbo.tbl_employee E ON A.APPROVE_ID = E.empId " +
                                      " WHERE DOCINFO_ID = " + Fn.getSQL(doc_id) +
                                      " AND IS_FOLLOW = 1 ";
                    SqlDataReader dr = cmd.ExecuteReader();
                    while(dr.Read())
                    {
                        list.Add(dr["empEmail"].ToString());
                    }
                }
                con.Close();
            }
            return list;
        }


    }

    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<AutoSendMail>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(13, 25))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);

        }
    }

}