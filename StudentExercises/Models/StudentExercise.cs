using System;
using System.Collections.Generic;
using System.Text;

namespace StudentExercises.Models
{
    public class StudentExercise
    {
        public int Id { get; set; }

        // This is to hold the actual foreign key integer
        public int StudentId { get; set; }
        public int ExerciseId { get; set; }
        // This property is for storing the C# object representing the department
        public string Cohort { get; set; }
        public string Exercise { get; set; }
    }
}
