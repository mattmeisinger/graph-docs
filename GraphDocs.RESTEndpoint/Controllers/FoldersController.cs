using GraphDocs.DataServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.RESTEndpoint.Controllers
{
    public class FoldersController : Controller
    {
        private FoldersDataService folders = new FoldersDataService();

        [HttpGet]
        public ActionResult Index()
        {
            return Get("/");
        }

        [HttpGet]
        public ActionResult Get(string path)
        {
            var folder = folders.Get(path);
            return Json(new
            {
                path = path,
                data = new
                {
                    name = folder.Name
                },
                childFolders = folder.ChildFolders.Select(a => new
                {
                    path = a.Path,
                    links = new
                    {
                        self = "/folders" + a.Path
                    }
                }),
                childDocuments = folder.ChildDocuments.Select(a => new
                {
                    path = a.Path,
                    name = a.Name,
                    links = new
                    {
                        self = "/documents" + a.Path
                    }
                }),
                links = new
                {
                    self = "/folders" + path
                }
            }, JsonRequestBehavior.AllowGet);
        }
    }
}