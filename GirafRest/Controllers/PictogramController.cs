using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using GirafRest.Models.DTOs;
using GirafRest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using GirafRest.Services;
using GirafRest.Extensions;

namespace GirafRest.Controllers
{
    /// <summary>
    /// The pictogram controller fetches an delivers pictograms on request. It also has endpoints for fetching
    /// and uploading images to pictograms. Supported image-types are .png and .jpg.
    /// </summary>
    [Route("[controller]")]
    public class PictogramController : Controller
    {
        private const string IMAGE_TYPE_PNG = "image/png";
        private const string IMAGE_TYPE_JPEG = "image/jpeg";

        /// <summary>
        /// A reference to GirafService, that defines common functionality for all classes.
        /// </summary>
        private readonly IGirafService _giraf;

        /// <summary>
        /// Constructor for the Pictogram-controller. This is called by the asp.net runtime.
        /// </summary>
        /// <param name="giraf">A reference to the GirafService.</param>
        /// <param name="loggerFactory">A reference to an implementation of ILoggerFactory. Used to create a logger.</param>
        public PictogramController(IGirafService girafController, ILoggerFactory lFactory) 
        {
            _giraf = girafController;
            _giraf._logger = lFactory.CreateLogger("Pictogram");
        }

        #region PictogramHandling
        /// <summary>
        /// Get all public <see cref="Pictogram"/> pictograms available to the user
        /// (i.e the public pictograms and those owned by the user (PRIVATE) and his department (PROTECTED)).
        /// </summary>
        /// <returns> All the user's <see cref="Pictogram"/> pictograms.</returns>
        [HttpGet]
        public async Task<IActionResult> ReadPictograms()
        {
            int limit = int.MaxValue;
            int startFrom = 0;
            try
            {
                limit = parseQueryInteger("limit", int.MaxValue);
                startFrom = parseQueryInteger("start_from", 0);
            }
            catch
            {
                return BadRequest("The request query contained an invalid value.");
            }

            //Produce a list of all pictograms available to the user
            var userPictograms = await ReadAllPictograms();
            if (userPictograms == null)
                return BadRequest("There is most likely no pictograms available on the server.");

            //Filter out all that does not satisfy the query string, if such is present.
            var titleQuery = HttpContext.Request.Query["title"];
            if(!String.IsNullOrEmpty(titleQuery)) userPictograms = FilterByTitle(userPictograms, titleQuery);

            return Ok(await userPictograms.OfType<Pictogram>().Skip(startFrom).Take(limit).Select(p => new PictogramDTO(p)).ToListAsync());
        }

        /// <summary>
        /// Read the <see cref="Pictogram"/> pictogram with the specified <paramref name="id"/> id and
        /// check if the user is authorized to see it.
        /// </summary>
        /// <param name="id">The ID of the pictogram to fetch.</param>
        /// <returns> The <see cref="Pictogram"/> pictogram with the specified ID,
        /// NotFound (404) if no such <see cref="Pictogram"/> pictogram exists,
        /// BadRequest if the <see cref="Pictogram"/> was not succesfully uploaded to the server or
        /// Unauthorized if the pictogram is private and user does not own it.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> ReadPictogram(long id)
        {
            try
            {
                var usr = await _giraf.LoadUserAsync(HttpContext.User);
                //Fetch the pictogram and check that it actually exists
                var _pictogram = await _giraf._context.Pictograms
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
                if (_pictogram == null) return NotFound();

                //Check if the pictogram is public and return it if so
                if (_pictogram.AccessLevel == AccessLevel.PUBLIC) return Ok(new PictogramDTO(_pictogram, _pictogram.Image));

                bool ownsResource = false;
                if (_pictogram.AccessLevel == AccessLevel.PRIVATE)
                    ownsResource = await _giraf.CheckPrivateOwnership(_pictogram, usr);
                else if (_pictogram.AccessLevel == AccessLevel.PROTECTED)
                    ownsResource = await _giraf.CheckProtectedOwnership(_pictogram, usr);

                if (ownsResource)
                    return Ok(new PictogramDTO(_pictogram, _pictogram.Image));
                else
                    return Unauthorized();
            } catch (Exception e)
            {
                string exceptionMessage = $"Exception occured in read:\n{e}";
                _giraf._logger.LogError(exceptionMessage);
                return BadRequest("There is most likely no pictograms available on the server.\n\n" + exceptionMessage);
            }
        }

        /// <summary>
        /// Create a new <see cref="Pictogram"/> pictogram.
        /// </summary>
        /// <param name="pictogram">A <see cref="PictogramDTO"/> with all relevant information about the new pictogram.</param>
        /// <returns>The new pictogram with all database-generated information.</returns>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePictogram([FromBody]PictogramDTO pictogram)
        {
            if(pictogram == null) return BadRequest("The body of the request must contain a pictogram.");
            if (!ModelState.IsValid)
                return BadRequest("Some data was missing from the serialized user \n\n" +
                                  string.Join(",",
                                  ModelState.Values.Where(E => E.Errors.Count > 0)
                                  .SelectMany(E => E.Errors)
                                  .Select(E => E.ErrorMessage)
                                  .ToArray()));

            //Create the actual pictogram instance
            Pictogram pict = new Pictogram(pictogram.Title, (AccessLevel) pictogram.AccessLevel);
            pict.Image = pictogram.Image;

            var user = await _giraf.LoadUserAsync(HttpContext.User);

            if(pictogram.AccessLevel == AccessLevel.PRIVATE) {
                //Add the pictogram to the current user
                new UserResource(user, pict);
            }
            else if(pictogram.AccessLevel == AccessLevel.PROTECTED)
            {
                //Add the pictogram to the user's department
                new DepartmentResource(user.Department, pict);
            }

            //Stamp the pictogram with current time and add it to the database
            pict.LastEdit = DateTime.Now;
            await _giraf._context.Pictograms.AddAsync(pict);
            await _giraf._context.SaveChangesAsync();

            return Ok(new PictogramDTO(pict));
        }

        /// <summary>
        /// Update info of a <see cref="Pictogram"/> pictogram.
        /// </summary>
        /// <param name="pictogram">A <see cref="PictogramDTO"/> with all new information to update with.
        /// The Id found in this DTO is the target pictogram.
        /// </param>
        /// <returns>NotFound if there is no pictogram with the specified id or 
        /// the updated pictogram to maintain statelessness.</returns>
        [HttpPut("{id}")]
        [Authorize(Policy = GirafRole.RequireGuardianOrSuperUser)]
        public async Task<IActionResult> UpdatePictogramInfo(long id, [FromBody] PictogramDTO pictogram)
        {
            if (pictogram == null) return BadRequest("Unable to parse the request body.");
            if (!ModelState.IsValid)
                return BadRequest("Some data was missing from the serialized user \n\n" +
                                  string.Join(",",
                                  ModelState.Values.Where(E => E.Errors.Count > 0)
                                  .SelectMany(E => E.Errors)
                                  .Select(E => E.ErrorMessage)
                                  .ToArray()));

            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            //Fetch the pictogram from the database and check that it exists
            var pict = await _giraf._context.Pictograms
                .Where(pic => pic.Id == id)
                .FirstOrDefaultAsync();
            if(pict == null) return NotFound();

            if (!CheckOwnership(pict, usr).Result)
                return Unauthorized();
            //Ensure that Id is not changed.
            pictogram.Id = id;
            //Update the existing database entry and save the changes.
            pict.Merge(pictogram);
            _giraf._context.Pictograms.Update(pict);
            await _giraf._context.SaveChangesAsync();

            return Ok(new PictogramDTO(pict));
        }

        /// <summary>
        /// Delete the <see cref="Pictogram"/> pictogram with the specified id.
        /// </summary>
        /// <param name="id">The id of the pictogram to delete.</param>
        /// <returns>Ok if the pictogram was deleted and NotFound if no pictogram with the id exists.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = GirafRole.RequireGuardianOrSuperUser)]
        public async Task<IActionResult> DeletePictogram(int id)
        {
            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            //Fetch the pictogram from the database and check that it exists
            var pict = await _giraf._context.Pictograms.Where(pic => pic.Id == id).FirstOrDefaultAsync();
            if(pict == null) return NotFound();
            
            if (!CheckOwnership(pict, usr).Result)
                return Unauthorized();

            //Remove it and save changes
            _giraf._context.Pictograms.Remove(pict);
            await _giraf._context.SaveChangesAsync();
            return Ok();
        }
        #endregion
        #region ImageHandling
        /// <summary>
        /// Upload an image for the <see cref="Pictogram"/> pictogram with the given id.
        /// </summary>
        /// <param name="id">Id of the pictogram to upload an image for.</param>
        /// <returns>The pictogram's information along with its image.</returns>
        [HttpPost("image/{id}")]
        [Consumes(IMAGE_TYPE_PNG, IMAGE_TYPE_JPEG)]
        [Authorize]
        public async Task<IActionResult> CreateImage(long id)
        {
            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            //Fetch the image and check that it exists
            var pict = await _giraf._context
                .Pictograms
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            if(pict == null) return NotFound();
            else if(pict.Image != null) return BadRequest("The pictogram already has an image.");

            if (!CheckOwnership(pict, usr).Result)
                return Unauthorized();
            //Read the image from the request body
            byte[] image = await _giraf.ReadRequestImage(HttpContext.Request.Body);
            if(image.Length == 0)
            {
                return BadRequest("The request contained no image.");
            }

            pict.Image = image;

            var pictoResult = await _giraf._context.SaveChangesAsync();
            return Ok(new PictogramDTO(pict, image));
        }

        /// <summary>
        /// Update the image of a <see cref="Pictogram"/> pictogram with the given Id.
        /// </summary>
        /// <param name="id">Id of the pictogram to update the image for.</param>
        /// <returns>The updated pictogram along with its image.</returns>
        [Consumes(IMAGE_TYPE_PNG, IMAGE_TYPE_JPEG)]
        [HttpPut("image/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePictogramImage(long id) {
            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            //Attempt to fetch the pictogram from the database.
            var picto = await _giraf._context
                .Pictograms
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            if(picto == null) return NotFound();
            else if(picto.Image == null) return BadRequest("The pictogram does not have a image, please POST instead.");

            if (!CheckOwnership(picto, usr).Result)
                return Unauthorized();

            //Update the image
            byte[] image = await _giraf.ReadRequestImage(HttpContext.Request.Body);
            picto.Image = image;

            await _giraf._context.SaveChangesAsync();
            return Ok(new PictogramDTO(picto, image));
        }

        /// <summary>
        /// Read the image of a given pictogram.
        /// </summary>
        /// <param name="id">The id of the pictogram to read the image of.</param>
        /// <returns>A FileResult with the desired image.</returns>
        [HttpGet("image/{id}")]
        public async Task<IActionResult> ReadPictogramImage(long id) {
            var usr = await _giraf.LoadUserAsync(HttpContext.User);
            //Fetch the pictogram and check that it actually exists and has an image.
            var picto = await _giraf._context
                .Pictograms
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();
            if (picto == null)
                return NotFound($"There is no image with id {id}.");
            else if (picto.Image == null)
                return NotFound("The specified pictogram has no image.");

        
            if (!CheckOwnership(picto, usr).Result)
                return Unauthorized();

            return Ok(picto.Image);
        }
        #endregion

        #region helpers
        
        /// <summary>
        /// Checks if the user has some form of ownership of the pictogram.
        /// </summary>
        /// <param name="picto">The Pictogram in need of checking.</param>
        /// <param name="usr">The user in question.</param>
        /// <returns>A list of said pictograms.</returns>
        private async Task<bool> CheckOwnership(Pictogram picto, GirafUser usr)
        {
            var ownsPictogram = false;
            switch (picto.AccessLevel)
            {
                case AccessLevel.PUBLIC:
                    ownsPictogram = true;
                    break;
                case AccessLevel.PROTECTED:
                    ownsPictogram = await _giraf.CheckProtectedOwnership(picto, usr);
                    break;
                case AccessLevel.PRIVATE:
                    ownsPictogram = await _giraf.CheckPrivateOwnership(picto, usr);
                    break;
                default:
                    break;
            }
            if (!ownsPictogram)
                return false;
            return true;
        }

        /// <summary>
        /// Read all pictograms available to the current user (or only the PUBLIC ones if no user is authorized).
        /// </summary>
        /// <returns>A list of said pictograms.</returns>
        private async Task<IQueryable<Pictogram>> ReadAllPictograms() {
            //In this method .AsNoTracking is used due to a bug in EntityFramework Core, where we are not allowed to call a constructor in .Select,
            //i.e. convert the pictograms to PictogramDTOs.
            try
            {
                //Find the user and add his pictograms to the result
                var user = await _giraf.LoadUserAsync(HttpContext.User);
                
                if (user != null)
                {
                    if (user.Department != null)
                    {
                        _giraf._logger.LogInformation($"Fetching pictograms for department {user.Department.Name}");
                        return _giraf._context.Pictograms.AsNoTracking()
                            //All public pictograms
                            .Where(p => p.AccessLevel == AccessLevel.PUBLIC 
                            //All the users pictograms
                            || p.Users.Any(ur => ur.OtherKey == user.Id)
                            //All the department's pictograms
                            || p.Departments.Any(dr => dr.OtherKey == user.DepartmentKey));
                    }

                    return _giraf._context.Pictograms.AsNoTracking()
                            //All public pictograms
                            .Where(p => p.AccessLevel == AccessLevel.PUBLIC 
                            //All the users pictograms
                            || p.Users.Any(ur => ur.OtherKey == user.Id));
                }

                //Fetch all public pictograms as there is no user.
                return _giraf._context.Pictograms.AsNoTracking()
                    .Where(p => p.AccessLevel == AccessLevel.PUBLIC);
            } catch (Exception e)
            {
                _giraf._logger.LogError("An exception occurred when reading all pictograms.", $"Message: {e.Message}", $"Source: {e.Source}");
                return null;
            }
        }

        private int parseQueryInteger(string queryStringName, int fallbackValue)
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query[queryStringName]))
                return fallbackValue;
            return int.Parse(HttpContext.Request.Query[queryStringName]);
        }
        #endregion
        #region query filters
        /// <summary>
        /// Filter a list of pictograms by their title.
        /// </summary>
        /// <param name="pictos">A list of pictograms that should be filtered.</param>
        /// <param name="titleQuery">The string that specifies what to search for.</param>
        /// <returns>A list of all pictograms with 'titleQuery' as substring.</returns>
        public IQueryable<Pictogram> FilterByTitle(IQueryable<Pictogram> pictos, string titleQuery) { 
            return pictos
                .Where(p => p.Title.ToLower().Contains(titleQuery.ToLower()));
        }
        #endregion
    }
}