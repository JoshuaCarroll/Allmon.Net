using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace AllmonNet.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public bool IsPost = false;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            

        }

        public void OnPost() 
        {
            IsPost = true;
        }

        public IActionResult OnGetConnectionData()
        {
            AsteriskResponse response = Allstar.RequestConn("499601");
            string json = JsonConvert.SerializeObject(response);
            JsonResult jr = new JsonResult(json);
            return jr;
        }
    }
}
