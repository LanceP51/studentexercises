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
    public class ExercisesTestController
    {
        // This is going to be our test coffee instance that we create and delete to make sure everything works
        private Exercise dummyExercise { get; } = new Exercise
        {
            Name = "Moosehead",
            Language = "French"
        };

        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/exercises";


        // Reusable method to create a new coffee in the database and return it
        public async Task<Exercise> CreateDummyExercise()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string mooseheadAsJSON = JsonConvert.SerializeObject(dummyExercise);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(mooseheadAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Exercise
                Exercise newlyCreatedExercise = JsonConvert.DeserializeObject<Exercise>(responseBody);

                return newlyCreatedExercise;
            }
        }

        // Reusable method to deelte a coffee from the database
        public async Task deleteDummyExercise(Exercise exerciseToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{exerciseToDelete.Id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Exercise()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new coffee in the db
                Exercise moosehead = await CreateDummyExercise();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{moosehead.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Exercise newExercise = JsonConvert.DeserializeObject<Exercise>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyExercise.Name, newExercise.Name);
                Assert.Equal(dummyExercise.Language, newExercise.Language);

                // Clean up after ourselves
                await deleteDummyExercise(newExercise);

            }

        }


        [Fact]

        public async Task Delete_Exercise()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new coffee in the db
            Exercise moosehead = await CreateDummyExercise();

            // Delete it
            await deleteDummyExercise(moosehead);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{moosehead.Id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Exercise()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the coffees from /api/coffees
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Exercise> exercises = JsonConvert.DeserializeObject<List<Exercise>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(exercises.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Exercise()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Exercise moosehead = await CreateDummyExercise();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{moosehead.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Exercise mooseheadFromDB = JsonConvert.DeserializeObject<Exercise>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyExercise.Name, mooseheadFromDB.Name);
                Assert.Equal(dummyExercise.Language, mooseheadFromDB.Language);

                // Clean up after ourselves-- delete the dummy coffee we just created
                await deleteDummyExercise(mooseheadFromDB);

            }
        }




        [Fact]
        public async Task Update_Exercise()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Exercise moosehead = await CreateDummyExercise();

                // Make a new title and assign it to our dummy coffee
                string newName = "Moosehead";
                moosehead.Name = newName;

                // Convert it to JSON
                string modifiedmooseheadAsJSON = JsonConvert.SerializeObject(moosehead);

                // Try to PUT the newly edited coffee
                var response = await client.PutAsync(
                    $"{url}/{moosehead.Id}",
                    new StringContent(modifiedmooseheadAsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited coffee back from the database after the PUT
                var getModifiedExercise = await client.GetAsync($"{url}/{moosehead.Id}");
                getModifiedExercise.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getExerciseBody = await getModifiedExercise.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Exercise newlyEditedExercise = JsonConvert.DeserializeObject<Exercise>(getExerciseBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedExercise.StatusCode);
                Assert.Equal(newName, newlyEditedExercise.Name);

                // Clean up after yourself
                await deleteDummyExercise(newlyEditedExercise);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Instructor_Fails()
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
        public async Task Test_Delete_NonExistent_Instructor_Fails()
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
