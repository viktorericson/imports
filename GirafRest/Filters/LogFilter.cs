﻿using GirafRest.Models.Responses;
using GirafRest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GirafRest.Filters
{
    public class LogFilter : IActionFilter
    {
        IGirafService _giraf;

        public LogFilter(IGirafService giraf)
        {
            _giraf = giraf;
        }

        public void OnActionExecuting(ActionExecutingContext context) {}
        public void OnActionExecuted(ActionExecutedContext context)
        {
            string path = "Logs/log-" + DateTime.Now.Year + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0') + ".txt";
            var controller = context.Controller as Controller;
            string userId = controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            string byId = controller.User.Claims.FirstOrDefault(c => c.Type == "impersonatedBy")?.Value;
            var user = _giraf._context.Users.FirstOrDefault(u => u.Id == userId)?.UserName;
            var by = _giraf._context.Users.FirstOrDefault(u => u.Id == byId)?.UserName;
            string p = context.HttpContext.Request.Path;
            string verb = context.HttpContext.Request.Method;
            var action = context.ActionDescriptor.DisplayName;
            var error = ((context.Result as ObjectResult)?.Value as Response)?.ErrorCode.ToString();
            string[] lines = new string[]
            {
                $"{DateTime.UtcNow}; {by}; {user}; {verb}; {p}; {error}; {byId}; {userId}"
            };
            Directory.CreateDirectory("Logs");
            File.AppendAllLines(path, lines);
        }
    }

}
