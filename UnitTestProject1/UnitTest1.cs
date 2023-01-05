using Information_System;
using Information_System.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Web;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1 : ApproveController 
    {
        [TestMethod]
        public void TestMethod1()
        {
            AutoSendMail autoSend = new AutoSendMail();
            autoSend.Execute();
          
        }
        [TestMethod]
        public void TestMethod2()
        {
            
            getApproveList("0","");
        }
        [TestMethod]
        public void TestMethod3()
        {
            checkonemorenonApprove_Emailtosend(DateTime.Now, "0", "www", null, "", null, "");
        }
        [TestMethod]
        public void TestMethod4()
        {
            ApproveList();
        }
        [TestMethod]
        public void TestMethod5()
        {
            SendEmail("");
        }

        [TestMethod]
        public void TestMethod6()
        {
            getEmailTemplate(null);
        }   
        
        [TestMethod]
        public void TestMethod7()
        {
            ApproveRequirement();
        }
        [TestMethod]
        public void TestMethod8()
        {
            saveInformation_inprogress("");
        }

    }
    //CREATE CLASS
    [TestClass]
    public class Create : CreateController
    {
        [TestMethod]
        public void RequirementInfo()
        {
            RequirementInfoList();
        }  
        
        [TestMethod]
        public void ReceivedRequirment()
        {
            ReceivedRequirment("");
        } 
        
        [TestMethod]
        public void getRequirementInfoList()
        {
            getRequirementInfoList("","","","","","");
        }  
        
        [TestMethod]
        public void AddRequirementDoc()
        {
            AddRequirementDoc(null,"");
        } 
        
        [TestMethod]
        public void AddRequirementDoc2()
        {
            AddRequirementDoc();
        } 
        
        [TestMethod]
        public void getDocInfo()
        {
            getDocInfo("","");
        }  
        
        [TestMethod]
        public void createInfoDoc()
        {
            createInfoDoc(null,"","");
        }
    }
}
