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
    public class CohortTestController
    {
        // This is going to be our test coffee instance that we create and delete to make sure everything works
        private Cohort dummyCohort { get; } = new Cohort
        {
            Name = "Fifty-Two"
        };
        // We'll store our base url for this route as a private field to avoid typos
        private string url { get; } = "/api/cohort";


        // Reusable method to create a new coffee in the database and return it
        public async Task<Cohort> CreateDummyCohort()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Serialize the C# object into a JSON string
                string cohortAsJSON = JsonConvert.SerializeObject(dummyCohort);


                // Use the client to send the request and store the response
                HttpResponseMessage response = await client.PostAsync(
                    url,
                    new StringContent(cohortAsJSON, Encoding.UTF8, "application/json")
                );

                // Store the JSON body of the response
                string responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON into an instance of Cohort
                Cohort newlyCreatedCohort = JsonConvert.DeserializeObject<Cohort>(responseBody);

                return newlyCreatedCohort;
            }
        }

        // Reusable method to deelte a coffee from the database
        public async Task deleteDummyCohort(Cohort cohortToDelete)
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                HttpResponseMessage deleteResponse = await client.DeleteAsync($"{url}/{cohortToDelete.Id}");

            }

        }


        /* TESTS START HERE */


        [Fact]
        public async Task Create_Cohort()
        {
            using (var client = new APIClientProvider().Client)
            {
                // Create a new coffee in the db
                Cohort newCohort55 = await CreateDummyCohort();

                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}/{newCohort55.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Cohort newCohort = JsonConvert.DeserializeObject<Cohort>(responseBody);

                // Make sure it's really there
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCohort.Name, newCohort.Name);

                // Clean up after ourselves
                await deleteDummyCohort(newCohort);

            }

        }


        [Fact]

        public async Task Delete_Cohort()
        {
            // Note: with many of these methods, I'm creating dummy data and then testing to see if I can delete it. I'd rather do that for now than delete something else I (or a user) created in the database, but it's not essential-- we could test deleting anything 

            // Create a new coffee in the db
            Cohort newCohort55 = await CreateDummyCohort();

            // Delete it
            await deleteDummyCohort(newCohort55);

            using (var client = new APIClientProvider().Client)
            {
                // Try to get it again
                HttpResponseMessage response = await client.GetAsync($"{url}{newCohort55.Id}");

                // Make sure it's really gone
                Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            }
        }

        [Fact]
        public async Task Get_All_Cohort()
        {

            using (var client = new APIClientProvider().Client)
            {

                // Try to get all of the coffees from /api/coffees
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                // Convert to JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Convert from JSON to C#
                List<Cohort> cohorts = JsonConvert.DeserializeObject<List<Cohort>>(responseBody);

                // Make sure we got back a 200 OK Status and that there are more than 0 coffees in our database
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.True(cohorts.Count > 0);

            }
        }

        [Fact]
        public async Task Get_Single_Cohort()
        {
            using (HttpClient client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Cohort newCohort55 = await CreateDummyCohort();

                // Try to get it
                HttpResponseMessage response = await client.GetAsync($"{url}/{newCohort55.Id}");
                response.EnsureSuccessStatusCode();

                // Turn the response into JSON
                string responseBody = await response.Content.ReadAsStringAsync();

                // Turn the JSON into C#
                Cohort CohortFromDB = JsonConvert.DeserializeObject<Cohort>(responseBody);

                // Did we get back what we expected to get back? 
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(dummyCohort.Name, CohortFromDB.Name);

                // Clean up after ourselves-- delete the dummy coffee we just created
                await deleteDummyCohort(CohortFromDB);

            }
        }




        [Fact]
        public async Task Update_Cohort()
        {

            using (var client = new APIClientProvider().Client)
            {
                // Create a dummy coffee
                Cohort newCohort = await CreateDummyCohort();

                // Make a new title and assign it to our dummy coffee
                string newName = "Fifty-five";
                newCohort.Name = newName;

                // Convert it to JSON
                string modifiedCohort55AsJSON = JsonConvert.SerializeObject(newCohort);

                // Try to PUT the newly edited coffee
                var response = await client.PutAsync(
                    $"{url}/{newCohort.Id}",
                    new StringContent(modifiedCohort55AsJSON, Encoding.UTF8, "application/json")
                );

                // See what comes back from the PUT. Is it a 204? 
                string responseBody = await response.Content.ReadAsStringAsync();
                Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

                // Get the edited coffee back from the database after the PUT
                var getModifiedCohort = await client.GetAsync($"{url}/{newCohort.Id}");
                getModifiedCohort.EnsureSuccessStatusCode();

                // Convert it to JSON
                string getCohortBody = await getModifiedCohort.Content.ReadAsStringAsync();

                // Convert it from JSON to C#
                Cohort newlyEditedCohort = JsonConvert.DeserializeObject<Cohort>(getCohortBody);

                // Make sure the title was modified correctly
                Assert.Equal(HttpStatusCode.OK, getModifiedCohort.StatusCode);
                Assert.Equal(newName, newlyEditedCohort.Name);

                // Clean up after yourself
                await deleteDummyCohort(newlyEditedCohort);
            }
        }

        [Fact]
        public async Task Test_Get_NonExitant_Cohort_Fails()
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
        public async Task Test_Delete_NonExistent_Cohort_Fails()
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
