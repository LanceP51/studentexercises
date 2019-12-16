using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using StudentExercises.Models;
using Microsoft.AspNetCore.Http;

namespace StudentExercises.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(string include, string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //string query = @"SELECT Student.Id, Student.FirstName, Student.LastName, Student.SlackHandle, Student.CohortId, Cohort.Name FROM Student JOIN Cohort ON Student.CohortId=Cohort.Id";
                    string query = "";

                    string studentsColumns = @"
                        SELECT s.Id AS 'Student Id', 
                        s.firstName AS 'Student First Name', 
                        s.lastName AS 'Student Last Name',
                        s.slackHandle AS 'Slack Handle',
                        c.name AS 'Cohort Name', 
                        c.Id AS 'Cohort Id'";
                    string studentsTable = "FROM Student s JOIN Cohort c ON s.cohortId = c.Id";


                    if (include == "exercises")
                    {
                        string includeColumns = @", 
                        e.name AS 'Exercise Name', 
                        e.language AS 'Exercise Language', 
                        e.Id AS 'Exercise Id'";

                        string includeTables = @"
                        JOIN StudentExercise se ON s.Id = se.studentId 
                        JOIN Exercise e ON se.exerciseId=e.Id";

                        query = $@"{studentsColumns} 
                                    {includeColumns} 
                                    {studentsTable} 
                                    {includeTables}";

                    }
                    else
                    {
                        query = $"{studentsColumns} {studentsTable}";
                    }
                    
                    if (q != null)
                    {
                        query += $" WHERE s.FirstName LIKE '{q}' OR s.LastName LIKE '{q}' OR s.SlackHandle LIKE '{q}'";
                    }


                    cmd.CommandText = query;
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Student> students = new List<Student>();

                    while (reader.Read())
                    {
                        Student student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Student Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("Student First Name")),
                            LastName = reader.GetString(reader.GetOrdinal("Student Last Name")),
                            SlackHandle = reader.GetString(reader.GetOrdinal                            ("Slack Handle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("Cohort Id"))
                           
                        };

                        if (include == "exercises")

                        {
                            Exercise currentExercise = new Exercise
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Exercise Id")),
                                Name = reader.GetString(reader.GetOrdinal("Exercise Name")),
                                Language = reader.GetString(reader.GetOrdinal("Exercise Language"))

                            };


                            // If the student is already on the list, don't add them again!
                            if (students.Any(s => s.Id == student.Id))
                            {
                                Student thisStudent = students.Where(s => s.Id ==                       student.Id).FirstOrDefault();
                                thisStudent.exercises.Add(currentExercise);
                            }
                            else
                            {
                                student.exercises.Add(currentExercise);
                                students.Add(student);

                            }

                        }
                        else
                        {
                            students.Add(student);
                        }


                        students.Add(student);
                    }
                    reader.Close();

                    return Ok(students);
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudent")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Student.Id, Student.FirstName, Student.LastName, Student.SlackHandle, Student.CohortId, Cohort.Name FROM Student JOIN Cohort ON Student.CohortId=Cohort.Id
                        WHERE Student.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Student student = null;

                    if (reader.Read())
                    {
                        student = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("SlackHandle")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("CohortId")),
                            Cohort = new Cohort
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("CohortId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            }
                        };
                    }
                    reader.Close();

                    return Ok(student);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Student student)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Student (FirstName, LastName, SlackHandle, CohortId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@firstname, @lastname, @slackhandle, @cohortId)";
                    cmd.Parameters.Add(new SqlParameter("@firstname", student.FirstName));
                    cmd.Parameters.Add(new SqlParameter("@lastname", student.LastName));
                    cmd.Parameters.Add(new SqlParameter("@slackhandle", student.SlackHandle));
                    cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));

                    int newId = (int)cmd.ExecuteScalar();
                    student.Id = newId;
                    return CreatedAtRoute("GetStudent", new { id = newId }, student);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Student student)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Student
                                            SET FirstName = @firstname,
                                                LastName = @lastname,
                                                SlackHandle = @slackhandle,
                                                CohortId = @cohortId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@firstname", student.FirstName));
                        cmd.Parameters.Add(new SqlParameter("@lastname", student.LastName));
                        cmd.Parameters.Add(new SqlParameter("@slackhandle", student.SlackHandle));
                        cmd.Parameters.Add(new SqlParameter("@cohortId", student.CohortId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Student WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!StudentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool StudentExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, FirstName, LastName, SlackHandle, CohortId
                        FROM Student
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}