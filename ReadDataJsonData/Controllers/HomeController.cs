using DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nancy.Extensions;
using Nancy.Json;
using Newtonsoft.Json;
using ReadDataJsonData.Models;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ReadDataJsonData.Controllers
{
    public class HomeController : Controller
	{
        private readonly IConfiguration _configuration;
        public HomeController(IConfiguration configuration)
		{
			_configuration = configuration;

		}

		public IActionResult Index()
		{
            string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "form.json");
            List<Form> listForm = new List<Form>();
            // Check if the JSON file exists.
            if (!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound("JSON file not found.");
            }
            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
            WebRequest webRequest = WebRequest.Create(jsonFilePath);
            WebResponse webResponse = webRequest.GetResponse();
            using (Stream datastrem = webResponse.GetResponseStream())
            {
                StreamReader streamReader = new StreamReader(datastrem);
                string json = streamReader.ReadToEnd();

                Root root = JsonConvert.DeserializeObject<Root>(json); 

                foreach (Form item in root.form) {
					Form form = new Form();
					if (item != null) {

                        form.label = item.label;
                        form.type = item.type;
                        form.options = item.options;
                    }
                    listForm.Add(form); 
                }
            }
			return View(listForm);	
        }

		public ActionResult SaveForm(SaveForm saveForm)
		{
			if(saveForm.Name != null && saveForm.Gender !=null && saveForm.Hobbies != null)
			{
				string hobbies = string.Empty;
				if (saveForm.Hobbies.Count > 0)
				{
					hobbies = string.Join(",", saveForm.Hobbies.ToArray());
				}
				string myDb1ConnectionString = _configuration.GetConnectionString("Constring");
				using (SqlConnection con = new SqlConnection(myDb1ConnectionString))
				{
					using (SqlCommand command = new SqlCommand("Insertjsondata", con))
					{
						command.CommandType = CommandType.StoredProcedure;
						command.Parameters.AddWithValue("@Name", saveForm.Name);
						command.Parameters.AddWithValue("@Gender", saveForm.Gender);
						command.Parameters.AddWithValue("@Hobbies", hobbies);

						if (con.State == ConnectionState.Closed)
							con.Open();
						command.ExecuteNonQuery();
						if (con.State == ConnectionState.Open)
							con.Close();
					}
				}
			}
		
			return View();

		}
		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}

    public class Form
    {
        public string type { get; set; }
        public string label { get; set; }
        public List<string> options { get; set; }
    }
	public partial class SaveForm
	{
		public string Name { get; set; }
		public string Gender { get; set; }
		public List<string> Hobbies { get; set; }
	}
	public class Root
    {
        public List<Form> form { get; set; }
    }
}