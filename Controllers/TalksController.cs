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
    [Route("api/camps/{moniker}/[controller]")]
    [ApiController]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _repository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository repository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _repository = repository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get( string moniker)
        {
            try
            {
                var results = await _repository.GetTalksByMonikerAsync(moniker);
                if ( !results.Any() )
                {
                    return NotFound("No talks found");
                }

                TalkModel[] model = _mapper.Map<TalkModel[]>(results);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get( string moniker, int id)
        {
            try
            {
                var result = await _repository.GetTalkByMonikerAsync(moniker, id);
                if (result == null)
                {
                    return NotFound("No talks found");
                }

                TalkModel model = _mapper.Map<TalkModel>(result);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post( string moniker, TalkModel model)
        {
            try
            {
                var camp = await _repository.GetCampAsync(moniker);
                // if there is no camp with moniker return
                if (camp == null) return BadRequest("Camp does not exist");

                // map the posted talk to Talk model
                var talk = _mapper.Map<Talk>(model);
                // assign camp that we get above to the talk.
                talk.Camp = camp;

                // check if speaker id exist in the post data
                if (model.Speaker == null) return BadRequest("Speaker ID is required");
                var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                // return if speaker with speaker id does not exist
                if (speaker == null) return BadRequest("Speaker could not be found");
                // add speaker to the talk
                talk.Speaker = speaker;

                // add talk with camp associated
                _repository.Add(talk);

                if ( await _repository.SaveChangesAsync() )
                {
                    // get location to set in response header
                    var location = _linkGenerator.GetPathByAction(HttpContext,
                        "Get",
                        values: new { moniker, id = talk.TalkId});
                    // convert talk back to TalkModel
                    TalkModel talkModel = _mapper.Map<TalkModel>(talk);
                    return Created(location, talkModel);
                } else
                {
                    return BadRequest("Failed to save new talk");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex}");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put( string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _repository.GetTalkByMonikerAsync(moniker, id, true); // true to get speakers

                if (talk == null) return NotFound("Could not find the talk");

                //
                // 
                /**
                 * similar to : model.Title = talk.Title; 
                 * map everything in the model to everything in the talk : not needed, modified CampsProfile
                 * speaker and camp is ignored in the CampProfile
                 */
                _mapper.Map(model, talk);

                // attaching speaker manuall
                if ( model.Speaker != null )
                {
                    var speaker = await _repository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if ( speaker != null )
                    {
                        talk.Speaker = speaker;
                    }
                }

                if ( await _repository.SaveChangesAsync() )
                {
                    var modal = _mapper.Map<TalkModel>(talk);
                    return Ok(modal);
                } else
                {
                    return BadRequest("Failed to update talk");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Database Failure {ex.Message}");
            }
        }
    }
}