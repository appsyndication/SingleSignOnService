using System.Threading.Tasks;
using AppSyndication.SingleSignOnService.Web.Models;
using IdentityServer4.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppSyndication.SingleSignOnService.Web.Controllers
{
    public class Error : Controller
    {
        private readonly ErrorInteraction _errorInteraction;

        public Error(ErrorInteraction errorInteraction)
        {
            _errorInteraction = errorInteraction;
        }

        public async Task<IActionResult> Index(string id)
        {
            var vm = new ErrorViewModel();

            if (id != null)
            {
                vm.Error = await _errorInteraction.GetRequestAsync(id);
            }

            return View(vm);
        }
    }
}
