using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> Get( bool includeTalks = false )
        {
            try
            {
                var results = await _repository.GetAllCampsAsync(includeTalks);
                CampModel[] model = _mapper.Map<CampModel[]>(results);
                return Ok(model);
            } catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> Get( string moniker )
        {
            try 
            {
                var result = await _repository.GetCampAsync(moniker);
                
                if (result == null)
                {
                    return NotFound();
                }

                CampModel model = _mapper.Map<CampModel>(result);
                return Ok(model);
            }
            catch (Exception)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate( DateTime theDate, bool includeTalks = false )
        {
            try
            {
                var results = await _repository.GetAllCampsByEventDate(theDate, includeTalks);

                if (!results.Any())
                {
                    return NotFound();
                }

                CampModel[] model = _mapper.Map<CampModel[]>(results);

                return Ok(model);
            }
            catch ( Exception )
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        public async Task<ActionResult<CampModel>> Post(CampModel model)
        {
            try
            {
                var existing = await _repository.GetCampAsync(model.Moniker);
                if (existing != null)
                {
                    return BadRequest("Monkier already in Use.");
                }

                var location = _linkGenerator.GetPathByAction("Get", "Camps", new { moniker = model.Moniker });
                if (string.IsNullOrWhiteSpace(location))
                {
                    return BadRequest("Could not use current moniker");
                }

                var camp = _mapper.Map<Camp>(model);
                _repository.Add(camp);

                if (await _repository.SaveChangesAsync())
                {
                    return Created(location, _mapper.Map<CampModel>(camp));
                }

                return BadRequest("Could not update camp");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> Put( string moniker, CampModel model)
        {
            try
            {
                var oldCamp = await _repository.GetCampAsync(moniker);
                if (oldCamp == null) NotFound($"Could not find camp with moniker: {moniker}");

                //oldCamp.Name = model.Name;
                _mapper.Map(model, oldCamp);

                if ( await _repository.SaveChangesAsync() )
                {
                    CampModel campModel = _mapper.Map<CampModel>(oldCamp);
                    return Ok(campModel);
                }
                // to avoid warning of return.
                return BadRequest("Could not create camp");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> Delete( string moniker)
        {
            try
            {
                var campToDelete = await _repository.GetCampAsync(moniker);

                if (campToDelete == null) return NotFound($"Could not found camp with moniker: {moniker}");

                _repository.Delete(campToDelete);

                if ( await _repository.SaveChangesAsync())
                {
                    return Ok();
                }

                return BadRequest("Could not delete camp");
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }
    }
}