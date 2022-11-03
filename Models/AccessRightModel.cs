using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace Information_System.Models
{
    public class AccessRightModel
    {
        InFunction Fn;
        
        //public void test()
        //{
        //    using(SqlConnection con =  new SqlConnection(Fn.connString))
        //    {
        //        con.Open();
        //        using(SqlCommand cmd = new SqlCommand(null, con))
        //        {
        //            cmd.CommandText = "select top 1 * from tbl_access_right ";
        //            SqlDataReader rd = cmd.ExecuteReader();
        //            if (rd.Read())
        //            {
        //                IS_ADMIN = Int32.Parse(rd["IS_ADMIN"].ToString());
        //                IS_RECEIVE = rd["IS_RECEIVE"].ToString();
        //            }

        //        }
        //        con.Close();
        //    }
        //}


        public bool IS_ADMIN { get; set; }
        public string IS_RECEIVE { get; set; }
        public bool IS_APP_LEVEL1 { get; set; }
        public bool IS_APP_LEVEL2 { get; set; }
        //public bool IS_APPROVE { get; set; }
        public bool IS_TE { get; set; }
        public bool IS_IT { get; set; }

    }
    
}