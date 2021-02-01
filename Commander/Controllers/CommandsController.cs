using System;
using System.Collections.Generic;
using AutoMapper;
using Commander.Data;
using Commander.Dtos;
using Commander.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using Commander.Exceptions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Commander.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly ICommanderRepo _repository;
        private IMapper _mapper;

        public CommandsController(ICommanderRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        /// <summary>
        /// Gets all commands
        /// </summary>
        [HttpGet]
        [Produces("application/json")]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }


        /// <summary>
        /// Get the command with the specified ID
        /// </summary>
        /// <response code="200">Returns the command</response>
        /// <response code="404">There isn't a command with the specified ID</response> 
        [HttpGet("{id}", Name = "GetCommandById")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem != null)
            {
                return Ok(_mapper.Map<CommandReadDto>(commandItem));
            }

            return NotFound();
        }

        /// <summary>
        /// Gets all the commands of the specified platform
        /// </summary>
        [HttpGet("platform")]
        [Produces("application/json")]
        public ActionResult<IEnumerable<CommandReadDto>> GetCommandsByPlatform([FromQuery] string platform)
        {
            var commandItems = _repository.GetCommandsByPlatform(platform);
            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));

        }

        /// <summary>
        /// Creates a command
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /Command
        ///     {
        ///        "howTo": "Run a .NET project,
        ///        "line": "dotnet run",
        ///        "platform": ".Net"
        ///     }
        ///
        /// </remarks>
        /// <returns>A newly created Command</returns>
        /// <response code="201">Returns the newly created command</response>
        /// <response code="400">The field howTo and/or line must be unique</response> 
        [HttpPost]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<CommandReadDto> CreateCommand(CommandCreateDto cmd)
        {
            var commandModel = _mapper.Map<Command>(cmd);
            try
            {
                _repository.CreateCommand(commandModel);
                _repository.SaveChanges();
                var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);
                return CreatedAtRoute(nameof(GetCommandById), new { Id = commandReadDto.Id }, commandReadDto);
            }
            catch (AppException ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        /// <summary>
        /// Updates a command information
        /// </summary>
        /// <response code="204">Command updated</response>
        /// <response code="400">Fields missing</response>
        /// <response code="404">There isn't a command with the specified ID</response> 
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public ActionResult UpdateCommand(int id, CommandUpdateDto cmd)
        {
            var commandModelRepo = _repository.GetCommandById(id);
            if (commandModelRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(cmd, commandModelRepo);
            _repository.UpdateCommand(commandModelRepo);
            _repository.SaveChanges();

            return NoContent();

        }

        /// <summary>
        /// Partially updates a command
        /// </summary>
        [HttpPatch("{id}")]
        public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
        {
            var commandModelRepo = _repository.GetCommandById(id);
            if (commandModelRepo == null)
            {
                return NotFound();
            }

            var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelRepo);
            patchDoc.ApplyTo(commandToPatch, ModelState);

            if (!TryValidateModel(commandToPatch))
            {
                return ValidationProblem(ModelState);
            }

            _mapper.Map(commandToPatch, commandModelRepo);
            _repository.UpdateCommand(commandModelRepo);
            _repository.SaveChanges();

            return NoContent();
        }

        /// <summary>
        /// Deletes the command with the specified ID
        /// </summary>
        /// <response code="204">Command deleted</response>
        /// <response code="404">There isn't a command with the specified ID</response> 
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Produces("application/json")]
        public ActionResult DeleteCommand(int id)
        {
            var commandModelRepo = _repository.GetCommandById(id);
            if (commandModelRepo == null)
            {
                return NotFound();
            }

            _repository.DeleteCommand(commandModelRepo);
            _repository.SaveChanges();

            return NoContent();
        }
    }
}