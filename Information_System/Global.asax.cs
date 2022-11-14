using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Timers;
using System.Web.Configuration;
using System.Diagnostics;
using System.Net;
using Quartz;

namespace Information_System
{
    public class MvcApplication : System.Web.HttpApplication
    {
        //private static double TimerIntervalInMilliseconds = Convert.ToDouble(WebConfigurationManager.AppSettings["TimerIntervalInMilliseconds"]);
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //JobScheduler.Start();

            //AutoSendMail autoSend = new AutoSendMail();
            //autoSend.Autosend();
            //Timer timer = new Timer(TimerIntervalInMilliseconds);  //60000
            //Timer timer = new Timer();
            //timer.Interval = 86400000; 
            //timer.Enabled = true;
            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            //timer.Start();
        }

        //static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //DateTime MyScheduledRunTime = DateTime.Parse(WebConfigurationManager.AppSettings["TimerStartTime"]);
        //    //DateTime CurrentSystemTime = DateTime.Now;
        //    //Debug.WriteLine(string.Concat("Timer Event Handler Called: ", CurrentSystemTime.ToString()));
        //    //DateTime LatestRunTime = MyScheduledRunTime.AddMilliseconds(TimerIntervalInMilliseconds);
        //    //if ((CurrentSystemTime.CompareTo(MyScheduledRunTime) >= 0) && (CurrentSystemTime.CompareTo(LatestRunTime) <= 0))
        //    //{
        //    //    Debug.WriteLine(String.Concat("Timer Event Handling MyScheduledRunTime Actions: ", DateTime.Now.ToString()));
        //        AutoSendMail autoSend = new AutoSendMail();
        //        //autoSend.Autosend();
        //    //}
        //}
    }
}
