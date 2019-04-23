using System.Web.Mvc;
using Tweetinvi;
using Tweetinvi.Models;

namespace Examplinvi.ASP.NET.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult TwitterAuth()
        {
            //var appCredentials = new TwitterCredentials("7qDmmaoHHfR4r5ktwLdeYBCOZ", "8c1Dl5shUvLRO8P3KuQPo2EzbMIWns5ZgbqjM6TgmxiFYA2gLK");
            //var authenticationContext1 = AuthFlow.InitAuthentication(appCredentials);

            var appCreds = new ConsumerCredentials(MyCredentials.CONSUMER_KEY, MyCredentials.CONSUMER_SECRET);
            var redirectURL = "http://" + Request.Url.Authority + "/Home/ValidateTwitterAuth";
            //var redirectURL = "http://127.0.0.1:5000/";
            var authenticationContext = AuthFlow.InitAuthentication(appCreds, redirectURL);

            return new RedirectResult(authenticationContext.AuthorizationURL);
        }

        public ActionResult ValidateTwitterAuth()
        {
            var verifierCode = Request.Params.Get("oauth_verifier");
            var authorizationId = Request.Params.Get("authorization_id");

            if (verifierCode != null)
            {
                var userCreds = AuthFlow.CreateCredentialsFromVerifierCode(verifierCode, authorizationId);
                var user = Tweetinvi.User.GetAuthenticatedUser(userCreds);

                ViewBag.User = user;
            }

            return View();
        }
    }
}