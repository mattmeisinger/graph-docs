using GraphDocs.Infrastructure.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GraphDocs.WebService.Controllers
{
    public abstract class BaseController : Controller
    {
        public string APIVersionNumber = "v1";

        public string PathTo(params string[] pathsToJoin)
        {
            var controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            return PathToController(controllerName, pathsToJoin);
        }
        public string PathToController(string controllerName, params string[] pathsToJoin)
        {
            var pathElements = new List<string>();
            pathElements.Add(APIVersionNumber);
            pathElements.Add(controllerName);
            if (pathsToJoin != null && pathsToJoin.Any())
                pathElements.AddRange(pathsToJoin);
            return this.GetCurrentDomain() + PathUtilities.Join(pathElements.ToArray());
        }

        public string GetCurrentDomain()
        {
            // From SO answer: http://stackoverflow.com/questions/61817/whats-the-best-method-in-asp-net-to-obtain-the-current-domain
            return Request.Url.Scheme + System.Uri.SchemeDelimiter + Request.Url.Host +
                (Request.Url.IsDefaultPort ? "" : ":" + Request.Url.Port);
        }

        // Handle exceptions that come up while handling a request. Send a nice, short, error message and an error message.
        protected override void OnException(ExceptionContext filterContext)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            if (filterContext.ExceptionHandled)
            {
                return;
            }
            filterContext.Result = Content(filterContext.Exception.ToString());
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = statusCode;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }

        // Override this method so that every time Json is returned, GET is allowed. The way our routes are
        // set up in this project, only Get methods will be accessible by the GET HTTP verb anyway.
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding)
        {
            return base.Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
        }
        protected override JsonResult Json(object data, string contentType, Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return base.Json(data, contentType, contentEncoding, JsonRequestBehavior.AllowGet);
        }
    }
}