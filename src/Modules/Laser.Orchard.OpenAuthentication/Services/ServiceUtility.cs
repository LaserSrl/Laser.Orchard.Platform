using System.Web;

namespace Laser.Orchard.OpenAuthentication.Services {
    public class ServiceUtility {
        public bool RewriteRequestByState() {
            bool result = false;
            var ctx = HttpContext.Current;
            var stateString = System.Web.HttpUtility.UrlDecode(ctx.Request.QueryString["state"]);
            if (stateString != null && stateString.Contains("__provider__=")) {
                // this provider requires that all return data be packed into a "state" parameter
                var q = System.Web.HttpUtility.ParseQueryString(stateString);
                q.Add(ctx.Request.QueryString);
                q.Remove("state");
                ctx.RewritePath(ctx.Request.Path + "?" + q.ToString());
                result = true;
            }
            return result;
        }
    }
}