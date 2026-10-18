using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LoanSystem.Core.Exceptions;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NikanBase.Infrastructure.Framework;

namespace LoanSystem.Site.Areas.AppApi.Controllers;

[Authorize(Roles = "AppApiUser", AuthenticationSchemes = "Bearer,Identity.Application")]
[Area("AppApi")]
public class BaseApiController : BaseController
{
    protected ILogger Logger { get; private set; }

    public BaseApiController(ILogger logger)
    {
        Logger = logger;
    }

    protected DefaultResponseState Success(string message = "")
    {
        return new DefaultResponseState
        {
            Success = true,
            Message = message,
            MessageType = ResponseStateMessageTypes.Success
        };
    }

    protected DefaultResponseState Error(string message = "")
    {
        return new DefaultResponseState
        {
            Success = false,
            Message = message,
            MessageType = ResponseStateMessageTypes.Error
        };
    }

    protected DefaultResponseState Info(string message = "")
    {
        return new DefaultResponseState
        {
            Success = true,
            Message = message,
            MessageType = ResponseStateMessageTypes.Info
        };
    }

    protected DefaultResponseState Warning(string message = "")
    {
        return new DefaultResponseState
        {
            Success = true,
            Message = message,
            MessageType = ResponseStateMessageTypes.Warning
        };
    }

    protected ResponseState<T> Success<T>(T data = default(T), string message = "")
    {
        return MakeResponse(success: true, message, ResponseStateMessageTypes.Success, data);
    }

    protected ResponseState<T> Error<T>(T data = default(T), string message = "")
    {
        return MakeResponse(success: false, message, ResponseStateMessageTypes.Error, data);
    }

    protected ResponseState<T> Info<T>(T data = default(T), string message = "")
    {
        return MakeResponse(success: true, message, ResponseStateMessageTypes.Info, data);
    }

    protected ResponseState<T> Warning<T>(T data = default(T), string message = "")
    {
        return MakeResponse(success: true, message, ResponseStateMessageTypes.Warning, data);
    }

    protected ResponseState<T> MakeResponse<T>(bool success, string message, ResponseStateMessageTypes messageType, T data)
    {
        return new ResponseState<T>
        {
            Success = success,
            Message = message,
            MessageType = messageType,
            Data = data
        };
    }

    protected async Task<ResponseState<T>> TryCatch<T>(Func<Task<ResponseState<T>>> func, bool addModelState = false)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                string text = JoinErrors();
                BaseApiController baseApiController = this;
                string message = text;
                return baseApiController.Error<T>(default, message);
            }
            return await func();
        }
        catch (CustomException exception)
        {
            string text2 = HandleException(exception, addModelState);
            BaseApiController baseApiController2 = this;
            string message = text2;
            return baseApiController2.Error<T>(default, message);
        }
        catch (Exception exception2)
        {
            string text3 = HandleException(exception2, addModelState);
            BaseApiController baseApiController3 = this;
            string message = text3;
            return baseApiController3.Error<T>(default, message);
        }
    }

    protected async Task<DefaultResponseState> TryCatch(Func<Task<DefaultResponseState>> func, bool addModelState = false)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                string message = JoinErrors();
                return Error(message);
            }
            return await func();
        }
        catch (Exception exception)
        {
            string message2 = HandleException(exception, addModelState);
            return Error(message2);
        }
    }

    protected string JoinErrors()
    {
        Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
        if (!ModelState.IsValid && ModelState.ErrorCount > 0)
        {
            ModelStateEntry[] array = ModelState.Values.Where((ModelStateEntry x) => x.ValidationState == ModelValidationState.Invalid).ToArray();
            for (int num = 0; num < ModelState.Values.Count(); num++)
            {
                string key = ModelState.Keys.ElementAt(num);
                ModelStateEntry modelStateEntry = ModelState.Values.ElementAt(num);
                if (modelStateEntry.ValidationState == ModelValidationState.Invalid)
                {
                    dictionary.Add(key, modelStateEntry.Errors.Select((ModelError x) => (!string.IsNullOrEmpty(x.ErrorMessage)) ? x.ErrorMessage : x.Exception?.Message).ToList());
                }
            }
        }
        return string.Join("<br/>", dictionary.Select((KeyValuePair<string, List<string>> x) => string.Join(" - ", x.Value) ?? ""));
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    protected async Task<string> PartialViewToString(PartialViewResult partialView, ActionContext actionContext)
    {
        using StringWriter writer = new StringWriter();
        IServiceProvider requestServices = actionContext.HttpContext.RequestServices;
        PartialViewResultExecutor requiredService = requestServices.GetRequiredService<PartialViewResultExecutor>();
        IView view = requiredService.FindView(actionContext, partialView).View;
        ViewContext context = new ViewContext(actionContext, view, partialView.ViewData, partialView.TempData, writer, new HtmlHelperOptions());
        await view.RenderAsync(context);
        return writer.ToString();
    }

    private void HandleExceptionRelatedErrors(CustomException ex, ref string userMsg)
    {
        if (ex.RelatedErrors == null || !ex.RelatedErrors.Any())
        {
            return;
        }
        userMsg += "<br/>";
        foreach (string relatedError in ex.RelatedErrors)
        {
            userMsg = userMsg + "- " + relatedError + "<br/>";
        }
    }

    private string HandleException(Exception exception, bool addModelState)
    {
        string text = "خطا در پردازش اطلاعات.";
        if (exception.InnerException != null && exception.InnerException is SqlException)
        {
            SqlException ex = exception.InnerException as SqlException;
            if (ex.Number == 2601 || ex.Number == 2627)
            {
                text = "خطا در پردازش اطلاعات.";
            }
        }
        Logger.LogError(exception, exception.Message);
        if (addModelState)
        {
            ModelState.AddModelError("err", text);
        }
        return text;
    }

    private string HandleException(CustomException exception, bool addModelState)
    {
        string userMsg = exception.Message;
        HandleExceptionRelatedErrors(exception, ref userMsg);
        Logger.LogError(exception, exception.Message);
        if (addModelState)
        {
            ModelState.AddModelError("err", userMsg);
        }
        return userMsg;
    }
}
