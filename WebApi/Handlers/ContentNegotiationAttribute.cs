using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http.Filters;
using System.Web.Http;
using System.Web.Http.Controllers;
public class ContentNegotiationAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
    {
        var acceptHeader = actionExecutedContext.Request.Headers.Accept;
        if (acceptHeader != null)
        {
            if (acceptHeader.Any(h => h.MediaType == "application/xml"))
            {
                // Return XML response
                var xmlFormatter = new XmlMediaTypeFormatter();
                var content = actionExecutedContext.ActionContext.Response.Content.ReadAsAsync(actionExecutedContext.ActionContext.ActionDescriptor.ReturnType).Result;
                actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.OK, content, xmlFormatter);
                actionExecutedContext.Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            }
        }

        base.OnActionExecuted(actionExecutedContext);
    }
}
