using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExercises.Models
{
    public class Instructor
    {
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string SlackHandle { get; set; }

        // This is to hold the actual foreign key integer
        public int CohortId { get; set; }

        // This property is for storing the C# object representing the department
        public Cohort Cohort { get; set; }

    }
}
