using StudentExercises.Models;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace StudentExercisesTest
{
    public class StudentTestController
    {
        // This is going to be our test coffee instance that we create and delete to make sure everything works
        private Student dummyStudent { get; } = new Student
        {
            FirstName = "Lance",
            LastName = "Penn",
            SlackHandle = "Lancey55",
            CohortId = 1
        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/student";


        // Reusable method to create a new coffee in the database and return it
        public async Task<Student> CreateDummyStudent()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string lanceAsJSON = JsonConvert.SerializeObject(dummyStudent);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(lanceAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Student
                Student newlyCreatedStudent = JsonConvert.DeserializeObject<Student>(responseBody);

                return newlyCreatedStudent;
            }
        }

        // Reusable method to deelte a coffee from the database
        public async Task deleteDummyStudent(Student studentToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{studentToDelete.Id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Student()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new coffee in the db
                Student newLancePenn = await CreateDummyStudent();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newLancePenn.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Student newStudent = JsonConvert.DeserializeObject<Student>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyStudent.LastName, newStudent.LastName);
                Assert.Equal(dummyStudent.FirstName, newStudent.FirstName);

                // Clean up after ourselves
                await deleteDummyStudent(newStudent);

            }

        }


        [Fact]

        public async Task Delete_Student()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new coffee in the db
            Student newLancePenn = await CreateDummyStudent();

            // Delete it
            await deleteDummyStudent(newLancePenn);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newLancePenn.Id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Student()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the coffees from /api/coffees
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Student> students = JsonConvert.DeserializeObject<List<Student>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(students.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Student()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Student newLancePenn = await CreateDummyStudent();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newLancePenn.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Student LancePennFromDB = JsonConvert.DeserializeObject<Student>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyStudent.LastName, LancePennFromDB.LastName);
                Assert.Equal(dummyStudent.FirstName, LancePennFromDB.FirstName);

                // Clean up after ourselves-- delete the dummy coffee we just created
                await deleteDummyStudent(LancePennFromDB);

            }
        }




        [Fact]
        public async Task Update_Student()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Student newLancePenn = await CreateDummyStudent();

                // Make a new title and assign it to our dummy coffee
                string newLastName = "Pennington";
                newLancePenn.LastName = newLastName;

                // Convert it to JSON
                string modifiedLancePennAsJSON = JsonConvert.SerializeObject(newLancePenn);

                // Try to PUT the newly edited coffee
                var response = await client.PutAsync(
                    $"{url}/{newLancePenn.Id}",
                    new StringContent(modifiedLancePennAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited coffee back from the database after the PUT
                var getModifiedStudent = await client.GetAsync($"{url}/{newLancePenn.Id}");
                getModifiedStudent.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getStudentBody = await getModifiedStudent.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Student newlyEditedStudent = JsonConvert.DeserializeObject<Student>(getStudentBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedStudent.StatusCode);
                Assert.Equal(newLastName, newlyEditedStudent.LastName);

                // Clean up after yourself
                await deleteDummyStudent(newlyEditedStudent);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Student_Fails()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Try to get a coffee with an Id that could never exist
                HttpResponseMessage response = await client.GetAsync($"{url}/00000000");

                // It should bring back a 204 no content error
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            }
        }

        [Fact]
        public async Task Test_Delete_NonExistent_Student_Fails()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Try to delete an Id that shouldn't exist 
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}0000000000");

                Assert.False(deleteResponse.IsSuccessStatusCode);
                Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
            }
        }
    }
}
