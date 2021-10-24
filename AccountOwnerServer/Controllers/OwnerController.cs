using AutoMapper;
using Contracts;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountOwnerServer.Controllers
{
    [Route("api/owner")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private ILogger<OwnerController> logger;
        private IRepositoryWrapper repository;
        private readonly IMapper mapper;

        public OwnerController(ILogger<OwnerController> logger, IRepositoryWrapper repository, IMapper mapper)
        {
            this.logger = logger;
            this.repository = repository;
            this.mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetAllOwners()
        {
            try
            {
                var owners = repository.Owner.GetAllOwners();
                logger.LogInformation($"Returned all owners from database.");

                var ownerResult = mapper.Map<IEnumerable<OwnerDto>>(owners);

                return Ok(ownerResult);
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong inside GetAllOwners action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetOwnerById(string id)
        {
            try
            {
                var owner = repository.Owner.GetOwnerById(id);

                if (owner == null)
                {
                    logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    logger.LogInformation($"Returned owner with id: {id}");

                    var ownerResult = mapper.Map<OwnerDto>(owner);
                    return Ok(ownerResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong inside GetOwnerById action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}/account")]
        public IActionResult GetOwnerWithDetails(string id)
        {
            try
            {
                var owner = repository.Owner.GetOwnerWithDetails(id);

                if (owner == null)
                {
                    logger.LogError($"Owner with id: {id}, hasn't been found in db.");
                    return NotFound();
                }
                else
                {
                    logger.LogInformation($"Returned owner with details for id: {id}");

                    var ownerResult = mapper.Map<OwnerDto>(owner);
                    return Ok(ownerResult);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong inside GetOwnerWithDetails action: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }


    }
}
