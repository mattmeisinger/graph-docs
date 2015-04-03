using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Utilities
{
    public class ReadJsonBody : ActionFilterAttribute
    {
        public string Param { get; set; }
        public Type JsonDataType { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            if (request.ContentType.Contains("application/json"))
            {
                string inputContent;
                if (request.InputStream.Position > 0)
                {
                    request.InputStream.Position = 0;
                }
                using (var sr = new StreamReader(request.InputStream))
                {
                    inputContent = sr.ReadToEnd();
                }
                var result = JsonConvert.DeserializeObject(inputContent, JsonDataType);
                filterContext.ActionParameters[Param] = result;
            }
        }
    }
}