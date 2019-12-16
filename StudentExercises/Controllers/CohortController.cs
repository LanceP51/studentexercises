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
    public class CohortController : ControllerBase
    {
        private readonly IConfiguration _config;

        public CohortController(IConfiguration config)
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
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT Cohort.Id, cohort.Name, Student.Id AS 'StudentId', Student.FirstName AS 'StudentFirst' , Student.LastName AS 'StudentLast', Student.SlackHandle AS 'StudentSlack', Student.CohortId AS 'StudentCohort', Instructor.Id AS 'InstructorId', Instructor.FirstName AS 'InstructorFirst', Instructor.LastName AS 'InstructorLast', Instructor.SlackHandle AS 'InstructorSlack', Instructor.CohortId AS 'InstructorCohort' FROM Cohort RIGHT JOIN Student ON Cohort.Id=Student.CohortId RIGHT JOIN Instructor ON Cohort.Id=Instructor.CohortId";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Cohort> cohorts = new List<Cohort>();

                    while (reader.Read())
                    {
                        Cohort currentcohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        
                        };
                        Student currentStudent = new Student
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            FirstName = reader.GetString(reader.GetOrdinal("StudentFirst")),
                            LastName = reader.GetString(reader.GetOrdinal("StudentLast")),
                            SlackHandle = reader.GetString(reader.GetOrdinal("StudentSlack")),
                            CohortId = reader.GetInt32(reader.GetOrdinal("StudentCohort"))

                        };
                        Instructor currentInstructor = new Instructor
                        {
                         Id = reader.GetInt32(reader.GetOrdinal("InstructorId")),
                         FirstName = reader.GetString(reader.GetOrdinal("InstructorFirst")),
                         LastName = reader.GetString(reader.GetOrdinal("InstructorLast")),
                         SlackHandle = reader.GetString(reader.GetOrdinal("InstructorSlack")),
                         CohortId = reader.GetInt32(reader.GetOrdinal("InstructorCohort"))
                         };
                        if (cohorts.Any(c => c.Id == currentcohort.Id))
                        {
                            Cohort cohortToReference = cohorts.Where(c => c.Id == currentcohort.Id).FirstOrDefault();
                            if(!cohortToReference.students.Any(s=>s.Id==currentStudent.Id))
                            {
                                cohortToReference.students.Add(currentStudent);
                            }
                            if(!cohortToReference.instructors.Any(i=>i.Id==currentInstructor.Id))
                            {
                                cohortToReference.instructors.Add(currentInstructor);
                            }
                        }
                        else
                        {
                            currentcohort.students.Add(currentStudent);
                            currentcohort.instructors.Add(currentInstructor);
                            cohorts.Add(currentcohort);
                        }
                        
                    }
                    reader.Close();

                    return Ok(cohorts);
                }
            }
        }

        [HttpGet("{id}", Name = "GetCohort")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, Name
                        FROM Cohort
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Cohort cohort = null;

                    if (reader.Read())
                    {
                        cohort = new Cohort
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        };
                    }
                    reader.Close();

                    return Ok(cohort);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Cohort cohort)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Cohort (Name)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name)";
                    cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));

                    int newId = (int)cmd.ExecuteScalar();
                    cohort.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, cohort);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Cohort cohort)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE Cohort
                                            SET Name = @name
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@name", cohort.Name));
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
                if (!CohortExists(id))
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
                        cmd.CommandText = @"DELETE FROM Cohort WHERE Id = @id";
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
                if (!CohortExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool CohortExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name
                        FROM Cohort
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}