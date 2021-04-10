using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Sibilus.Web.Server;

namespace Sibilus.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public async void OnGet()
        {
            string sessionId = Request.Cookies["session"];

            if (string.IsNullOrWhiteSpace(sessionId) || !await DbCache.DbClient.ValueExistsAsync("sessions", "id", sessionId))
                Response.Redirect("/login");
        }
    }
}
