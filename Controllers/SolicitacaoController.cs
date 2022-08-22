using System.Diagnostics;
using System.Json;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MvcSite.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MvcSite.Controllers;

public class SolicitacaoController : Controller
{
    private readonly ILogger<SolicitacaoController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private string url = "";
    private string urlcamunda = "";
    private readonly IConfiguration _config;

    public SolicitacaoController(ILogger<SolicitacaoController> logger, IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;
        url = _config.GetSection("AppSettings")["WebApiUrl"];
        urlcamunda = _config.GetSection("AppSettings")["CamundaUrl"];
    }
    public IActionResult Index()
    {
        _logger.LogInformation("List Solicitacao");
        IEnumerable<SolicitacaoViewModel> students;

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(url);
        //HTTP GET
        var responseTask = client.GetAsync("solicitacao");
        responseTask.Wait();

        var result = responseTask.Result;
        if (result.IsSuccessStatusCode)
        {
            var readTask = result.Content.ReadAsAsync<IList<SolicitacaoViewModel>>();
            readTask.Wait();

            students = readTask.Result;
        }
        else
        {
            students = Enumerable.Empty<SolicitacaoViewModel>();
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }

        using var activitySource = new ActivitySource(AppDomain.CurrentDomain.FriendlyName, "1.0.0");
        using var activity = activitySource.StartActivity("Call Status");
        activity?.SetTag("startPoint", "SolicitacaoController.cs");

        _logger.LogInformation("List Status");
        responseTask = client.GetAsync("status");
        responseTask.Wait();

        result = responseTask.Result;
        if (!result.IsSuccessStatusCode)
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");

        activity?.Dispose();

        _logger.LogInformation("BIP - Api zosconnect");
        // client = _httpClientFactory.CreateClient();
        // client.BaseAddress = new Uri("https://apiibmdev.mercantil.com.br:9444/mb.api.fab.cadastromodelo/");
        // //HTTP GET
        // responseTask = client.GetAsync("bip");
        // responseTask.Wait();

        // result = responseTask.Result;
        // if (result.IsSuccessStatusCode)
        // {
        //     var readTask = result.Content.ReadAsStringAsync();
        //     readTask.Wait();
        // }
        // else
        // {
        //     ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        // }

        return View(students);
    }

    public IActionResult Create()
    {

        _logger.LogInformation("List Status");
        IEnumerable<StatusViewModel> students;
        var client = _httpClientFactory.CreateClient();

        client.BaseAddress = new Uri(url);
        var responseTask = client.GetAsync("status");
        responseTask.Wait();

        var result = responseTask.Result;
        if (!result.IsSuccessStatusCode)
        {
            students = Enumerable.Empty<StatusViewModel>();
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }
        else
        {
            var readTask = result.Content.ReadAsAsync<IList<StatusViewModel>>();
            readTask.Wait();

            students = readTask.Result;
        }


        ViewData["idStatus"] = new SelectList(students, "idStatus", "descricaoStatus");
        return View();
    }

    [HttpPost]
    public IActionResult Create(SolicitacaoViewModel solicitacao)
    {
        _logger.LogInformation("Post Solicitacao");
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(url);
        //HTTP GET            
        var responseTask = client.PostAsync("solicitacao?id_status=" + solicitacao.idStatus + "&descricao_solicitacao=" + solicitacao.descricaoSolicitacao, null);
        responseTask.Wait();
        var result = responseTask.Result;
        if (result.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }
        else //web api sent error response 
        {
            //log response status here..
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }

        return View(solicitacao);
    }

    [HttpDelete]
    public IActionResult Delete(long id)
    {
        _logger.LogInformation("Delete Solicitacao");
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri(url);
        //HTTP GET            
        var responseTask = client.DeleteAsync("solicitacao?id_solicitacao=" + id);
        responseTask.Wait();
        var result = responseTask.Result;
        if (result.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }
        else //web api sent error response 
        {
            //log response status here..
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }

        return RedirectToAction("Index");
    }


    public IActionResult CreateProcess()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreateProcess(ProcessoViewModel processo)
    {
        _logger.LogInformation("CreateProcess-Cadastro");

        var client = _httpClientFactory.CreateClient();
        string json = @"{ 'variables': { 'nome' : { 'value' : '" + processo.Nome + @"', 'type': 'String'}}, 'businessKey' : '" + processo.Key + "'}";
        HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(JObject.Parse(json)), Encoding.UTF8);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client.BaseAddress = new Uri(urlcamunda);
        var responseTask = client.PostAsync("process-definition/key/processo-teste/start", httpContent);
        responseTask.Wait();
        var result = responseTask.Result;
        if (!result.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }
        else
        {
            var readTask = result.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(readTask.GetAwaiter().GetResult());
            processo.id = data.id;
        }
        return View(processo);
    }

    public IActionResult ListProcess()
    {
        _logger.LogInformation("List Process");
        List<ProcessoViewModel> students = new List<ProcessoViewModel>();
        ;
        var client = _httpClientFactory.CreateClient();
        string json = @"{ 'workerId':'mvcsite', 'maxTasks':10, 'usePriority':true, 'topics': [{'topicName': 'topico-dados','lockDuration': 10000}] }";

        HttpContent httpContent = new StringContent(JsonConvert.SerializeObject(JObject.Parse(json)), Encoding.UTF8);
        httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client.BaseAddress = new Uri(urlcamunda);
        var responseTask = client.PostAsync("external-task/fetchAndLock", httpContent);
        responseTask.Wait();
        var result = responseTask.Result;
        if (!result.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
        }
        else
        {
            var readTask = result.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(readTask.GetAwaiter().GetResult());
            for (int i = 0; i < data.Count; i++)
                students.Add(new ProcessoViewModel() { id = data[i].id, Key = data[i].businessKey, Nome = data[i].variables.nome.value });
        }
        return View(students);

    }

    [HttpPut]
    public IActionResult CompleteProcess(string id, string key)
    {
        Random r = new Random();

        _logger.LogInformation("CreateProcess-Complete");

        var client2 = _httpClientFactory.CreateClient();
        var json2 = @"{'workerId': 'mvcsite', 'variables': {'idade': {'value': " + r.Next(10, 55) + "}}}";

        var httpContent2 = new StringContent(JsonConvert.SerializeObject(JObject.Parse(json2)), Encoding.UTF8);
        httpContent2.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client2.BaseAddress = new Uri(urlcamunda);
        var responseTask2 = client2.PostAsync("external-task/" + id + "/complete", httpContent2);
        responseTask2.Wait();
        var result2 = responseTask2.Result;
        if (!result2.IsSuccessStatusCode)
        {
            ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");

        }

        return RedirectToAction("ListProcess");
    }

}