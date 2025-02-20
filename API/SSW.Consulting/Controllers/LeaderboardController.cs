﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class LeaderboardController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<LeaderboardListViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
        }
    }
}