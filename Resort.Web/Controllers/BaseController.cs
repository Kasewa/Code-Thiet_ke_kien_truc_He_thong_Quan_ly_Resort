using Microsoft.AspNetCore.Mvc;
using Resort.Utilities;
using Resort.Web.Extensions;

namespace Resort.Web.Controllers
{
    public class BaseController : Controller
    {
        protected CurrentUser CurrentUser => HttpContext.User.ToCurrentUser();

        protected string CurrentUserId => CurrentUser.UserId;
        protected string CurrentUserEmail => CurrentUser.Email;
    }
}
