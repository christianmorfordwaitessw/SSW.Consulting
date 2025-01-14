﻿using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SSW.Consulting.WebAPI.Filters;
using SSW.Consulting.WebAPI.Security;

namespace SSW.Consulting.WebAPI.Controllers
{
    [Authorize,Restricted]
    [CustomExceptionFilter]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        private IMediator _mediator;

        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}

