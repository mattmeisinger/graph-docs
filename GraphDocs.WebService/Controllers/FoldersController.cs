using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GraphDocs.Infrastructure;

namespace GraphDocs.WebService.Controllers
{
    public class FoldersController : BaseController
    {
        private FoldersDataService folders = new FoldersDataService();

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string path)
        {
            var folder = folders.Get(path);
            return Json(new
            {
                data = new
                {
                    id = folder.ID,
                    name = folder.Name,
                    path = folder.Path
                },
                childFolders = folder.ChildFolders.Select(a => new
                {
                    id = a.ID,
                    path = a.Path,
                    links = new
                    {
                        self = PathToController("folders", a.Path)
                    }
                }),
                childDocuments = folder.ChildDocuments.Select(a => new
                {
                    id = a.ID,
                    path = a.Path,
                    name = a.Name,
                    links = new
                    {
                        self = PathToController("documents", a.Path)
                    }
                }),
                links = new
                {
                    self = PathTo(folder.Path)
                }
            });
        }

        [ActionName("Index"), HttpPost]
        public ActionResult Post(string path, string name)
        {
            folders.Create(new Core.Models.Folder { Path = path, Name = name });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpPut]
        public ActionResult Put(string path, string name, string id)
        {
            folders.Save(new Core.Models.Folder { Path = path, Name = name, ID = id });
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpDelete]
        public ActionResult Delete(string path)
        {
            folders.Delete(path);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}