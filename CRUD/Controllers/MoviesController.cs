using CRUD.Models;
using CRUD.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;
        private readonly IToastNotification _toastNotification;
        public MoviesController(ApplicationDbContext context , IToastNotification toastNotification)
        {
            this._context = context;
            this._toastNotification = toastNotification;
        }

        public async Task<IActionResult>  Index()
        {
            var movies = await _context.movies.OrderByDescending(m => m.Rate).ToListAsync();
            return View(movies);
        }

        #region  create

        public async Task<IActionResult> Create()
        {
            var viewModel = new MovieFormViewModel
            {
                Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
          public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            //case one same data in get method

            if (!ModelState.IsValid)
            {
                model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                return View(model);
            }

            //case two if user no select image

            var files = Request.Form.Files;

            if (!files.Any())
            {
                model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster","Please select movie poster !");
                return View(model);
            }

            //case three i control user what can select type of image like jpg or png
            var poster = files.FirstOrDefault();
            var allowedExtenstions = new List<string> { ".jpg", ".png" };
            if (! _allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                return View(model);
            }

            //case four  limt siza 
            if (poster.Length > _maxAllowedPosterSize)
            {
                model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                return View(model);
            }

            using var dataStream = new MemoryStream();

            await poster.CopyToAsync(dataStream);

            var movies = new Movie
            {
                Title = model.Title,
                GenreId = model.GenreId,
                Year = model.Year,
                Rate = model.Rate,
                StoryLine = model.StoryLine,
                Poster = dataStream.ToArray()
            };

            _context.movies.Add(movies);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Created successfully");

            model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();



            return RedirectToAction(nameof(Index));
        }

        #endregion




        public async Task<IActionResult> Edit(int ? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var movie = await _context.movies.FindAsync(id);

            if (movie == null)
            {
                return NotFound();
            }

            var viewModel = new MovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                GenreId = movie.GenreId,
                Rate = movie.Rate,
                Year = movie.Year,
                StoryLine = movie.StoryLine,
                Poster = movie.Poster,
                Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync()
            };

            return View(viewModel);  

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel model)
        {
            //case one same data in get method

            if (!ModelState.IsValid)
            {
                model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                return View(model);
            }
            var movie = await _context.movies.FindAsync(model.Id);

            if (movie == null)
            {
                return NotFound();
            }

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();

                using var dataStream = new MemoryStream();

                await poster.CopyToAsync(dataStream);

                model.Poster = dataStream.ToArray();

                //case three i control user what can select type of image like jpg or png
             

                var allowedExtenstions = new List<string> { ".jpg", ".png" };
                if (!_allowedExtenstions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Only .PNG, .JPG images are allowed!");
                    return View(model);
                }

                //case four  limt siza 
                if (poster.Length > _maxAllowedPosterSize)
                {
                    model.Genres = await _context.genres.OrderBy(m => m.Name).ToListAsync();
                    ModelState.AddModelError("Poster", "Poster cannot be more than 1 MB!");
                    return View(model);
                }



                movie.Poster = model.Poster;
            }


            movie.Title = model.Title;
            movie.GenreId = model.GenreId;
            movie.Year = model.Year;
            movie.Rate = model.Rate;
            movie.StoryLine = model.StoryLine;

            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Updated successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.movies.Include(m => m.Genre).SingleOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            return View(movie);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.movies.FindAsync(id);

            if (movie == null)
                return NotFound();

            _context.movies.Remove(movie);
            _context.SaveChanges();

            return Ok();
        }

    }
}
