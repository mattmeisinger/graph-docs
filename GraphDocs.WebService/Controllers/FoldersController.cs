using GraphDocs.Core.Interfaces;
using GraphDocs.WebService.Models;
using GraphDocs.WebService.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public class FoldersController : BaseController
    {
        private IFoldersDataService folders;
        public FoldersController(IFoldersDataService folders)
        {
            this.folders = folders;
        }

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
                    path = folder.Path,
                    workflowDefinitions = folder.WorkflowDefinitions.OrderBy(a => a.Order).Select(a => new
                    {
                        order = a.Order,
                        workflowName = a.WorkflowName,
                        settings = a.Settings
                    })
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
                    active = a.Active,
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
        [ReadJsonBody(Param = "model", JsonDataType = typeof(FolderItem))]
        public ActionResult Post(string path, FolderItem model)
        {
            var folder = model.ConvertToFolder();
            folder.Path = path;
            folders.Create(folder);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpPut]
        [ReadJsonBody(Param = "model", JsonDataType = typeof(FolderItem))]
        public ActionResult Put(string path, string id, FolderItem model)
        {
            var folder = model.ConvertToFolder();
            folder.Path = path;
            folder.ID = id;
            folders.Save(folder);
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