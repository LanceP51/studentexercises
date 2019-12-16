using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace StudentExercises.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        [StringLength(12, MinimumLength = 3)]
        public string SlackHandle { get; set; }

        // This is to hold the actual foreign key integer
        public int CohortId { get; set; }
        // This property is for storing the C# object representing the department
        public Cohort Cohort { get; set; }
        public List<Exercise> exercises { get; set; } = new List<Exercise>();
    }
}
