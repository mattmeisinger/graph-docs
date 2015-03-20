using System;
using System.Activities;
using System.Activities.DurableInstancing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Runtime.DurableInstancing;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting workflow...");
            var workflow = new ClassLibrary1.TestWorkflow();
            var wfApp = new WorkflowApplication(workflow);

            //var connStr = "Server=(localdb)\\v11.0;Integrated Security=true;";
            //var conn = new SqlConnection(connStr);
            //conn.Open();
            //conn.Execute("Create table MyTable (Id INT, Name VARCHAR(100))");
            //conn.Execute("INSERT INTO MyTable (Id,Name) VALUES (1,'Matt')");
            //conn.Execute("INSERT INTO MyTable (Id,Name) VALUES (2,'Kari')");
            //var i = conn.Query<int>("SELECT count(*) from MyTable").Single();
            //Console.WriteLine("Found rows: " + i);

            //var store = new SqlWorkflowInstanceStore(connStr);

            var neo4jConnStr = "";
            var store = new New4JWorkflowInstanceStore(neo4jConnStr);
            wfApp.InstanceStore = store;
            wfApp.Persist();
            
            var results = WorkflowInvoker.Invoke(workflow);
            System.Threading.Thread.Sleep(2000);
        }
    }
}
