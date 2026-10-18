using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace NikanBase.Infrastructure.Framework;

public class BaseController : Controller
{
    private const string SUCCESS = "success";

    private const string ERROR = "error";

    protected virtual void NotifySuccess(string message, bool persistForTheNextRequest = true)
    {
        AddNotification("success", message, persistForTheNextRequest);
    }

    protected virtual void NotifyError(string message, bool persistForTheNextRequest = true)
    {
        AddNotification("error", message, persistForTheNextRequest);
    }

    protected virtual void NotifyError(Exception exception, bool persistForTheNextRequest = true)
    {
        AddNotification("error", exception.Message, persistForTheNextRequest);
    }

    protected virtual void AddNotification(string type, string message, bool persistForTheNextRequest)
    {
        string text = $"nk.notify.{type}";
        if (persistForTheNextRequest)
        {
            if (TempData[text] == null)
            {
                TempData[text] = new List<string>();
            }
            if (TempData[text] is string[])
            {
                string[] collection = TempData[text] as string[];
                List<string> value = new List<string>(collection);
                TempData[text] = value;
            }
            ((List<string>)TempData[text]).Add(message);
        }
        else
        {
            if (ViewData[text] == null)
            {
                ViewData[text] = new List<string>();
            }
            if (ViewData[text] is string[])
            {
                string[] collection2 = ViewData[text] as string[];
                List<string> value2 = new List<string>(collection2);
                ViewData[text] = value2;
            }
            ((List<string>)ViewData[text]).Add(message);
        }
    }
}
