using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using GraphDocs.Infrastructure;

namespace GraphDocs.WebService.Controllers
{
    public class TagsController : BaseController
    {
        private TagsDataService tags = new TagsDataService();

        [ActionName("Index"), HttpGet]
        public ActionResult Get(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                // Display a list of all tags
                var allTags = tags.Get();
                return Json(new
                {
                    data = allTags.Select(a => new
                    {
                        name = a.Name,
                        links = new
                        {
                            self = PathTo(a.Name)
                        }
                    })
                });
            }
            else
            {
                var tag = tags.Get(path);
                return Json(new
                {
                    data = new
                    {
                        name = tag.Name
                    },
                    relatedDocuments = tag.RelatedDocuments.Select(a => new
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
                        self = PathTo(tag.Name)
                    }
                });
            }
        }

        [ActionName("Index"), HttpPost]
        public ActionResult Post(string path)
        {
            throw new NotImplementedException("Tags may only be created by saving them to a document.");
        }

        [ActionName("Index"), HttpPut]
        public ActionResult Put(string path, string name)
        {
            // This save just allows the user to change a tag name
            tags.Rename(oldName: path, newName: name);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [ActionName("Index"), HttpDelete]
        public ActionResult Delete(string path)
        {
            tags.Delete(path);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}