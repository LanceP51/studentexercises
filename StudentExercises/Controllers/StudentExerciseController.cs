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
    public class StudentExerciseController : ControllerBase
    {
        private readonly IConfiguration _config;

        public StudentExerciseController(IConfiguration config)
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
                    cmd.CommandText = "SELECT Id, StudentId, ExerciseId FROM StudentExercise";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<StudentExercise> studentexercises = new List<StudentExercise>();

                    while (reader.Read())
                    {
                        StudentExercise studentexercise = new StudentExercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            ExerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"))
                        };

                        studentexercises.Add(studentexercise);
                    }
                    reader.Close();

                    return Ok(studentexercises);
                }
            }
        }

        [HttpGet("{id}", Name = "GetStudentExercise")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                            Id, StudentId, ExerciseId
                        FROM StudentExercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    StudentExercise studentexercise = null;

                    if (reader.Read())
                    {
                        studentexercise = new StudentExercise
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            StudentId = reader.GetInt32(reader.GetOrdinal("StudentId")),
                            ExerciseId = reader.GetInt32(reader.GetOrdinal("ExerciseId"))
                        };
                    }
                    reader.Close();

                    return Ok(studentexercise);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] StudentExercise studentexercise)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO StudentExercise (StudentId, ExerciseId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@studentId, @exerciseId)";
                    cmd.Parameters.Add(new SqlParameter("@studentId", studentexercise.StudentId));
                    cmd.Parameters.Add(new SqlParameter("@exerciseId", studentexercise.ExerciseId));

                    int newId = (int)cmd.ExecuteScalar();
                    studentexercise.Id = newId;
                    return CreatedAtRoute("GetExercise", new { id = newId }, studentexercise);
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] StudentExercise studentexercise)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE StudentExercise
                                            SET StudentId = @studentId,
                                                ExerciseId = @exerciseId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@studentId", studentexercise.StudentId));
                        cmd.Parameters.Add(new SqlParameter("@exerciseId", studentexercise.ExerciseId));
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
                if (!StudentExerciseExists(id))
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
                        cmd.CommandText = @"DELETE FROM StudentExercise WHERE Id = @id";
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
                if (!StudentExerciseExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool StudentExerciseExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, StudentId, ExerciseId
                        FROM StudentExercise
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
    }
}