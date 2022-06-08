using CRUD.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CRUD.ViewModel
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(250)]
        public string Title { get; set; }
        public int Year { get; set; }
        [Range(1 , 10)]
        public double Rate { get; set; }

        [Required, MaxLength(2500)]
        public string StoryLine { get; set; }

        [Display(Name = "Select Poster")]
        public Byte[] Poster { get; set; }

        [Display(Name = "Gerne")]
        public int GenreId { get; set; }

        public IEnumerable<Genre> Genres { get; set; }
    }
}
