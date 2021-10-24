using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountOwnerServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IRepositoryWrapper repositoryWrapper;

        public WeatherForecastController(IRepositoryWrapper repositoryWrapper)
        {
            this.repositoryWrapper = repositoryWrapper;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<Owner> Get()
        {
            var domesticAccounts = repositoryWrapper.Account.FindByCondition(x => x.AccountType.Equals("Domestic"));
            var owners = repositoryWrapper.Owner.FindAll();
            return owners;
        }

    }
}
