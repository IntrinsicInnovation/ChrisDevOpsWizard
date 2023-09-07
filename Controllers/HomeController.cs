using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebApp_OpenIDConnect_DotNet.Models;
using WebApp_OpenIDConnect_DotNet.Services.Arm;
using WebApp_OpenIDConnect_DotNet.Services.GraphOperations;
using Newtonsoft.Json.Linq;

namespace WebApp_OpenIDConnect_DotNet.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        static string[] devopsscopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation" }; //Constant value to target Azure DevOps. Do not change  


        readonly ITokenAcquisition tokenAcquisition;
        private readonly IGraphApiOperations graphApiOperations;
        private readonly IArmOperations armOperations;
        private readonly IArmOperationsWithImplicitAuth armOperationsWithImplicitAuth;

        public HomeController(ITokenAcquisition tokenAcquisition,
                              IGraphApiOperations graphApiOperations,
                              IArmOperations armOperations,
                              IArmOperationsWithImplicitAuth armOperationsWithImplicitAuth)
        {
            this.tokenAcquisition = tokenAcquisition;
            this.graphApiOperations = graphApiOperations;
            this.armOperations = armOperations;
            this.armOperationsWithImplicitAuth = armOperationsWithImplicitAuth;
        }

        public IActionResult Index()
        {
            return View();
        }

        private VssConnection Authenticate(string token)
        {
         //   string token = ""; // GetBearerToken(); // GetTokenConfig();
          //  string projectName = "";// GetProjectNameConfig();

            //var credentials = new VssBasicCredential(string.Empty, token);

            VssOAuthAccessTokenCredential credentials = new VssOAuthAccessTokenCredential(token);

            var connection = new VssConnection(new Uri("https://dev.azure.com/cj4599868"), credentials);

            return connection;
        }

        public List<GitRepository> GetGitRepos(string token)
        {
            var conn = Authenticate(token);

            var client = conn.GetClient<ProjectHttpClient>();
            var projects = client.GetProjects().Result;
            //return projects;

            //test builds here:
           // var definitions = ListBuildDefinitions(conn);



            //This doesn't work as your devops app is only scoped to projects!!!!
            using (GitHttpClient gitClient = conn.GetClient<GitHttpClient>())
            {
                var allRepos = gitClient.GetRepositoriesAsync().Result;
                return allRepos;
            }

            //var buildclient = conn.GetClient<BuildHttpClient>();
            //var a = buildclient;

            //var b = buildclient;


        }


        public IEnumerable<BuildDefinitionReference> ListBuildDefinitions(string token) // VssConnection connection)
        {

            var conn = Authenticate(token);

            string projectName = "project4"; // ClientSampleHelpers.FindAnyProject(this.Context).Name;

            // Get a build client instance
          //  VssConnection connection = Authenticate();
            BuildHttpClient buildClient = conn.GetClient<BuildHttpClient>();

            List<BuildDefinitionReference> buildDefinitions = new List<BuildDefinitionReference>();
            //buildClient.CreateDefinitionAsync(new BuildDefinition() { })
            // Iterate (as needed) to get the full set of build definitions
            string continuationToken = null;
            do
            {
                IPagedList<BuildDefinitionReference> buildDefinitionsPage = buildClient.GetDefinitionsAsync2(
                    project: projectName,
                    continuationToken: continuationToken).Result;

                buildDefinitions.AddRange(buildDefinitionsPage);

                continuationToken = buildDefinitionsPage.ContinuationToken;
            } while (!String.IsNullOrEmpty(continuationToken));

            // Show the build definitions
            foreach (BuildDefinitionReference definition in buildDefinitions)
            {
                Console.WriteLine("{0} {1}", definition.Id.ToString().PadLeft(6), definition.Name);
            }
            return buildDefinitions;
        }




        private async Task<string> GetDefinition(string token)
        {
            //Get existing Definition TEST:
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, "https://dev.azure.com/cj4599868/project4/_apis/build/definitions/10?api-version=7.0");
            HttpResponseMessage response =  client.SendAsync(requestMessage).Result;
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();
            return "";
        }



        private void CreateNewDefinition(string token)
        {

            //Create New Definition TEST:
            //var personalaccesstoken = "2ru3guvfvw3wnxmz3sds663ldzqbv4o6dedk5kdevpt5wkykqzxq";
            //var base64Token = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalaccesstoken}"));
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); // base64Token);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://dev.azure.com/cj4599868/project4/_apis/build/definitions?api-version=7.0");
            //requestMessage.Content = new StringContent("{\"process\": {  \"phases\": [{\"steps\": [], \"name\": \"Phase 1\",\"refName\": \"Phase_1\",\"condition\": \"succeeded()\",\"target\": { \"executionOptions\": { \"type\": 0 },\"allowScriptsAuthAccessOption\": false,  \"type\": 1  },  \"jobAuthorizationScope\": \"projectCollection\", \"jobCancelTimeoutInMinutes\": 1 }],\"type\": 1  }, \"repository\": { \"properties\": { \"cleanOptions\": \"0\",\"labelSources\": \"0\",\"labelSourcesFormat\": \"1\", \"reportBuildStatus\": \"true\",\"gitLfsSupport\": \"false\", \"skipSyncSource\": \"false\",\"checkoutNestedSubmodules\": \"false\", \"fetchDepth\": \"0\"},\"id\": \"096284f6-aabc-4252-8215-99825ebdd645\",\"type\": \"TfsGit\",\"name\": \"product1\", \"url\": \"https://dev.azure.com/cj4599868/project4/_git/project4\", \"defaultBranch\": \"refs/heads/master\",  \"clean\": \"false\",\"checkoutSubmodules\": false },\"processParameters\": {}, \"drafts\": [],\"queue\": { \"id\": null,  \"name\": \"Default\",\"pool\": {\"id\": 2, \"name\": \"Default\"}}, \"name\": \"definitionCreatedByRESTAPI7\", \"type\": \"build\",\"queueStatus\": \"enabled\"}", Encoding.UTF8, "application/json");
            var stringtoencode = @"{""options"":[{""enabled"":true,""definition"":{""id"":""5d58cc01-7c75-450c-be18-a388ddb129ec""},""inputs"":{""branchFilters"":""[\""+refs/heads/*\""]"",""additionalFields"":""{}""}},{""enabled"":false,""definition"":{""id"":""a9db38f9-9fdc-478c-b0f9-464221e58316""},""inputs"":{""workItemType"":""Issue"",""assignToRequestor"":""true"",""additionalFields"":""{}""}}],""variables"":{""BuildConfiguration"":{""value"":""Release"",""allowOverride"":true},""BuildPlatform"":{""value"":""any cpu"",""allowOverride"":true},""system.debug"":{""value"":""false"",""allowOverride"":true}},""properties"":{},""tags"":[],""_links"":{""self"":{""href"":""https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/Definitions/10?revision=1""},""web"":{""href"":""https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_build/definition?definitionId=10""},""editor"":{""href"":""https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_build/designer?id=10&_a=edit-build-definition""},""badge"":{""href"":""https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/status/10""}},""buildNumberFormat"":""$(date:yyyyMMdd)$(rev:.r)"",""jobAuthorizationScope"":2,""jobTimeoutInMinutes"":60,""jobCancelTimeoutInMinutes"":5,""process"":{""phases"":[{""steps"":[{""environment"":{},""enabled"":true,""continueOnError"":false,""alwaysRun"":false,""displayName"":""Restore"",""timeoutInMinutes"":0,""retryCountOnTaskFailure"":0,""condition"":""succeeded()"",""task"":{""id"":""5541a522-603c-47ad-91fc-a4b1d163081b"",""versionSpec"":""2.*"",""definitionType"":""task""},""inputs"":{""command"":""restore"",""publishWebProjects"":""true"",""projects"":""$(Parameters.RestoreBuildProjects)"",""custom"":"""",""arguments"":"""",""restoreArguments"":"""",""publishTestResults"":""true"",""testRunTitle"":"""",""zipAfterPublish"":""true"",""modifyOutputPath"":""true"",""selectOrConfig"":""select"",""feedRestore"":"""",""includeNuGetOrg"":""true"",""nugetConfigPath"":"""",""externalEndpoints"":"""",""noCache"":""false"",""packagesDirectory"":"""",""verbosityRestore"":""Detailed"",""searchPatternPush"":""$(Build.ArtifactStagingDirectory)/*.nupkg"",""nuGetFeedType"":""internal"",""feedPublish"":"""",""publishPackageMetadata"":""true"",""externalEndpoint"":"""",""searchPatternPack"":""**/*.csproj"",""configurationToPack"":""$(BuildConfiguration)"",""outputDir"":""$(Build.ArtifactStagingDirectory)"",""nobuild"":""false"",""includesymbols"":""false"",""includesource"":""false"",""versioningScheme"":""off"",""versionEnvVar"":"""",""requestedMajorVersion"":""1"",""requestedMinorVersion"":""0"",""requestedPatchVersion"":""0"",""buildProperties"":"""",""verbosityPack"":""Detailed"",""workingDirectory"":""""}},{""environment"":{},""enabled"":true,""continueOnError"":false,""alwaysRun"":false,""displayName"":""Build"",""timeoutInMinutes"":0,""retryCountOnTaskFailure"":0,""condition"":""succeeded()"",""task"":{""id"":""5541a522-603c-47ad-91fc-a4b1d163081b"",""versionSpec"":""2.*"",""definitionType"":""task""},""inputs"":{""command"":""build"",""publishWebProjects"":""true"",""projects"":""$(Parameters.RestoreBuildProjects)"",""custom"":"""",""arguments"":""--configuration $(BuildConfiguration)"",""restoreArguments"":"""",""publishTestResults"":""true"",""testRunTitle"":"""",""zipAfterPublish"":""true"",""modifyOutputPath"":""true"",""selectOrConfig"":""select"",""feedRestore"":"""",""includeNuGetOrg"":""true"",""nugetConfigPath"":"""",""externalEndpoints"":"""",""noCache"":""false"",""packagesDirectory"":"""",""verbosityRestore"":""Detailed"",""searchPatternPush"":""$(Build.ArtifactStagingDirectory)/*.nupkg"",""nuGetFeedType"":""internal"",""feedPublish"":"""",""publishPackageMetadata"":""true"",""externalEndpoint"":"""",""searchPatternPack"":""**/*.csproj"",""configurationToPack"":""$(BuildConfiguration)"",""outputDir"":""$(Build.ArtifactStagingDirectory)"",""nobuild"":""false"",""includesymbols"":""false"",""includesource"":""false"",""versioningScheme"":""off"",""versionEnvVar"":"""",""requestedMajorVersion"":""1"",""requestedMinorVersion"":""0"",""requestedPatchVersion"":""0"",""buildProperties"":"""",""verbosityPack"":""Detailed"",""workingDirectory"":""""}},{""environment"":{},""enabled"":true,""continueOnError"":false,""alwaysRun"":false,""displayName"":""Test"",""timeoutInMinutes"":0,""retryCountOnTaskFailure"":0,""condition"":""succeeded()"",""task"":{""id"":""5541a522-603c-47ad-91fc-a4b1d163081b"",""versionSpec"":""2.*"",""definitionType"":""task""},""inputs"":{""command"":""test"",""publishWebProjects"":""true"",""projects"":""$(Parameters.TestProjects)"",""custom"":"""",""arguments"":""--configuration $(BuildConfiguration)"",""restoreArguments"":"""",""publishTestResults"":""true"",""testRunTitle"":"""",""zipAfterPublish"":""true"",""modifyOutputPath"":""true"",""selectOrConfig"":""select"",""feedRestore"":"""",""includeNuGetOrg"":""true"",""nugetConfigPath"":"""",""externalEndpoints"":"""",""noCache"":""false"",""packagesDirectory"":"""",""verbosityRestore"":""Detailed"",""searchPatternPush"":""$(Build.ArtifactStagingDirectory)/*.nupkg"",""nuGetFeedType"":""internal"",""feedPublish"":"""",""publishPackageMetadata"":""true"",""externalEndpoint"":"""",""searchPatternPack"":""**/*.csproj"",""configurationToPack"":""$(BuildConfiguration)"",""outputDir"":""$(Build.ArtifactStagingDirectory)"",""nobuild"":""false"",""includesymbols"":""false"",""includesource"":""false"",""versioningScheme"":""off"",""versionEnvVar"":"""",""requestedMajorVersion"":""1"",""requestedMinorVersion"":""0"",""requestedPatchVersion"":""0"",""buildProperties"":"""",""verbosityPack"":""Detailed"",""workingDirectory"":""""}},{""environment"":{},""enabled"":true,""continueOnError"":false,""alwaysRun"":false,""displayName"":""Publish"",""timeoutInMinutes"":0,""retryCountOnTaskFailure"":0,""condition"":""succeeded()"",""task"":{""id"":""5541a522-603c-47ad-91fc-a4b1d163081b"",""versionSpec"":""2.*"",""definitionType"":""task""},""inputs"":{""command"":""publish"",""publishWebProjects"":""True"",""projects"":""$(Parameters.RestoreBuildProjects)"",""custom"":"""",""arguments"":""--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)"",""restoreArguments"":"""",""publishTestResults"":""true"",""testRunTitle"":"""",""zipAfterPublish"":""True"",""modifyOutputPath"":""true"",""selectOrConfig"":""select"",""feedRestore"":"""",""includeNuGetOrg"":""true"",""nugetConfigPath"":"""",""externalEndpoints"":"""",""noCache"":""false"",""packagesDirectory"":"""",""verbosityRestore"":""Detailed"",""searchPatternPush"":""$(Build.ArtifactStagingDirectory)/*.nupkg"",""nuGetFeedType"":""internal"",""feedPublish"":"""",""publishPackageMetadata"":""true"",""externalEndpoint"":"""",""searchPatternPack"":""**/*.csproj"",""configurationToPack"":""$(BuildConfiguration)"",""outputDir"":""$(Build.ArtifactStagingDirectory)"",""nobuild"":""false"",""includesymbols"":""false"",""includesource"":""false"",""versioningScheme"":""off"",""versionEnvVar"":"""",""requestedMajorVersion"":""1"",""requestedMinorVersion"":""0"",""requestedPatchVersion"":""0"",""buildProperties"":"""",""verbosityPack"":""Detailed"",""workingDirectory"":""""}},{""environment"":{},""enabled"":true,""continueOnError"":false,""alwaysRun"":true,""displayName"":""Publish Artifact"",""timeoutInMinutes"":0,""retryCountOnTaskFailure"":0,""condition"":""succeededOrFailed()"",""task"":{""id"":""2ff763a7-ce83-4e1f-bc89-0ae63477cebe"",""versionSpec"":""1.*"",""definitionType"":""task""},""inputs"":{""PathtoPublish"":""$(build.artifactstagingdirectory)"",""ArtifactName"":""drop"",""ArtifactType"":""Container"",""TargetPath"":""\\\\my\\share\\$(Build.DefinitionName)\\$(Build.BuildNumber)"",""Parallel"":""false"",""ParallelCount"":""8"",""StoreAsTar"":""false""}}],""name"":""Agent job 1"",""refName"":""Job_1"",""condition"":""succeeded()"",""target"":{""executionOptions"":{""type"":0},""allowScriptsAuthAccessOption"":false,""type"":1},""jobAuthorizationScope"":2}],""target"":{""agentSpecification"":{""identifier"":""windows-2019""}},""type"":1},""repository"":{""properties"":{""cleanOptions"":""0"",""labelSources"":""0"",""labelSourcesFormat"":""$(build.buildNumber)"",""reportBuildStatus"":""true"",""fetchDepth"":""1"",""gitLfsSupport"":""false"",""skipSyncSource"":""false"",""checkoutNestedSubmodules"":""false""},""id"":""096284f6-aabc-4252-8215-99825ebdd645"",""type"":""TfsGit"",""name"":""project4"",""url"":""https://dev.azure.com/cj4599868/project4/_git/project4"",""defaultBranch"":""refs/heads/master"",""clean"":""false"",""checkoutSubmodules"":false},""processParameters"":{""inputs"":[{""aliases"":[],""options"":{},""properties"":{},""name"":""RestoreBuildProjects"",""label"":""Project(s) to restore and build"",""defaultValue"":""**/*.csproj"",""type"":""multiline"",""helpMarkDown"":""Relative path of the .csproj file(s) from repo root. Wildcards can be used. For example, **/*.csproj for all .csproj files in all the subfolders."",""visibleRule"":"""",""groupName"":""""},{""aliases"":[],""options"":{},""properties"":{},""name"":""TestProjects"",""label"":""Project(s) to test"",""defaultValue"":""**/*[Tt]ests/*.csproj"",""type"":""multiline"",""helpMarkDown"":""Relative path of the .csproj file(s) from repo root. Wildcards can be used. For example, **/*.csproj for all .csproj files in all the subfolders."",""visibleRule"":"""",""groupName"":""""}]},""quality"":1,""authoredBy"":{""displayName"":""Chris Johnson"",""url"":""https://spsprodcca1.vssps.visualstudio.com/A36ee7258-ef28-466f-aaf2-f767eb692ed2/_apis/Identities/b370c683-bb42-626c-9a85-ec3a81c32fbf"",""_links"":{""avatar"":{""href"":""https://dev.azure.com/cj4599868/_apis/GraphProfile/MemberAvatars/aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm""}},""id"":""b370c683-bb42-626c-9a85-ec3a81c32fbf"",""uniqueName"":""cj4599868@gmail.com"",""imageUrl"":""https://dev.azure.com/cj4599868/_apis/GraphProfile/MemberAvatars/aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm"",""descriptor"":""aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm""},""drafts"":[],""queue"":{""_links"":{""self"":{""href"":""https://dev.azure.com/cj4599868/_apis/build/Queues/18""}},""id"":18,""name"":""Azure Pipelines"",""url"":""https://dev.azure.com/cj4599868/_apis/build/Queues/18"",""pool"":{""id"":9,""name"":""Azure Pipelines"",""isHosted"":true}},""id"":10,""name"":""pipelinefromAPIbyChrisJ"",""url"":""https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/Definitions/10?revision=1"",""uri"":""vstfs:///Build/Definition/10"",""path"":""\\"",""type"":2,""queueStatus"":0,""revision"":1,""createdDate"":""2023-06-23T01:11:47.667Z"",""project"":{""id"":""8475d613-bfde-40bc-92ce-883005ff4a17"",""name"":""project4"",""description"":""test project 4"",""url"":""https://dev.azure.com/cj4599868/_apis/projects/8475d613-bfde-40bc-92ce-883005ff4a17"",""state"":1,""revision"":19,""visibility"":0,""lastUpdateTime"":""2023-05-26T22:21:13.090Z""}}"; 
            requestMessage.Content = new StringContent(stringtoencode, Encoding.UTF8, "application/json");
            //  var create2 = "{ "options":[{ "enabled":true,"definition":{ "id":"5d58cc01-7c75-450c-be18-a388ddb129ec"},"inputs":{ "branchFilters":"[\"+refs/heads/*\"]","additionalFields":"{}"} },{ "enabled":false,"definition":{ "id":"a9db38f9-9fdc-478c-b0f9-464221e58316"},"inputs":{ "workItemType":"Issue","assignToRequestor":"true","additionalFields":"{}"} }],"variables":{ "BuildConfiguration":{ "value":"Release","allowOverride":true},"BuildPlatform":{ "value":"any cpu","allowOverride":true},"system.debug":{ "value":"false","allowOverride":true} },"properties":{ },"tags":[],"_links":{ "self":{ "href":"https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/Definitions/10?revision=1"},"web":{ "href":"https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_build/definition?definitionId=10"},"editor":{ "href":"https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_build/designer?id=10&_a=edit-build-definition"},"badge":{ "href":"https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/status/10"} },"buildNumberFormat":"$(date:yyyyMMdd)$(rev:.r)","jobAuthorizationScope":"project","jobTimeoutInMinutes":60,"jobCancelTimeoutInMinutes":5,"process":{ "phases":[{ "steps":[{ "environment":{ },"enabled":true,"continueOnError":false,"alwaysRun":false,"displayName":"Restore","timeoutInMinutes":0,"retryCountOnTaskFailure":0,"condition":"succeeded()","task":{ "id":"5541a522-603c-47ad-91fc-a4b1d163081b","versionSpec":"2.*","definitionType":"task"},"inputs":{ "command":"restore","publishWebProjects":"true","projects":"$(Parameters.RestoreBuildProjects)","custom":"","arguments":"","restoreArguments":"","publishTestResults":"true","testRunTitle":"","zipAfterPublish":"true","modifyOutputPath":"true","selectOrConfig":"select","feedRestore":"","includeNuGetOrg":"true","nugetConfigPath":"","externalEndpoints":"","noCache":"false","packagesDirectory":"","verbosityRestore":"Detailed","searchPatternPush":"$(Build.ArtifactStagingDirectory)/*.nupkg","nuGetFeedType":"internal","feedPublish":"","publishPackageMetadata":"true","externalEndpoint":"","searchPatternPack":"**/*.csproj","configurationToPack":"$(BuildConfiguration)","outputDir":"$(Build.ArtifactStagingDirectory)","nobuild":"false","includesymbols":"false","includesource":"false","versioningScheme":"off","versionEnvVar":"","requestedMajorVersion":"1","requestedMinorVersion":"0","requestedPatchVersion":"0","buildProperties":"","verbosityPack":"Detailed","workingDirectory":""} },{ "environment":{ },"enabled":true,"continueOnError":false,"alwaysRun":false,"displayName":"Build","timeoutInMinutes":0,"retryCountOnTaskFailure":0,"condition":"succeeded()","task":{ "id":"5541a522-603c-47ad-91fc-a4b1d163081b","versionSpec":"2.*","definitionType":"task"},"inputs":{ "command":"build","publishWebProjects":"true","projects":"$(Parameters.RestoreBuildProjects)","custom":"","arguments":"--configuration $(BuildConfiguration)","restoreArguments":"","publishTestResults":"true","testRunTitle":"","zipAfterPublish":"true","modifyOutputPath":"true","selectOrConfig":"select","feedRestore":"","includeNuGetOrg":"true","nugetConfigPath":"","externalEndpoints":"","noCache":"false","packagesDirectory":"","verbosityRestore":"Detailed","searchPatternPush":"$(Build.ArtifactStagingDirectory)/*.nupkg","nuGetFeedType":"internal","feedPublish":"","publishPackageMetadata":"true","externalEndpoint":"","searchPatternPack":"**/*.csproj","configurationToPack":"$(BuildConfiguration)","outputDir":"$(Build.ArtifactStagingDirectory)","nobuild":"false","includesymbols":"false","includesource":"false","versioningScheme":"off","versionEnvVar":"","requestedMajorVersion":"1","requestedMinorVersion":"0","requestedPatchVersion":"0","buildProperties":"","verbosityPack":"Detailed","workingDirectory":""} },{ "environment":{ },"enabled":true,"continueOnError":false,"alwaysRun":false,"displayName":"Test","timeoutInMinutes":0,"retryCountOnTaskFailure":0,"condition":"succeeded()","task":{ "id":"5541a522-603c-47ad-91fc-a4b1d163081b","versionSpec":"2.*","definitionType":"task"},"inputs":{ "command":"test","publishWebProjects":"true","projects":"$(Parameters.TestProjects)","custom":"","arguments":"--configuration $(BuildConfiguration)","restoreArguments":"","publishTestResults":"true","testRunTitle":"","zipAfterPublish":"true","modifyOutputPath":"true","selectOrConfig":"select","feedRestore":"","includeNuGetOrg":"true","nugetConfigPath":"","externalEndpoints":"","noCache":"false","packagesDirectory":"","verbosityRestore":"Detailed","searchPatternPush":"$(Build.ArtifactStagingDirectory)/*.nupkg","nuGetFeedType":"internal","feedPublish":"","publishPackageMetadata":"true","externalEndpoint":"","searchPatternPack":"**/*.csproj","configurationToPack":"$(BuildConfiguration)","outputDir":"$(Build.ArtifactStagingDirectory)","nobuild":"false","includesymbols":"false","includesource":"false","versioningScheme":"off","versionEnvVar":"","requestedMajorVersion":"1","requestedMinorVersion":"0","requestedPatchVersion":"0","buildProperties":"","verbosityPack":"Detailed","workingDirectory":""} },{ "environment":{ },"enabled":true,"continueOnError":false,"alwaysRun":false,"displayName":"Publish","timeoutInMinutes":0,"retryCountOnTaskFailure":0,"condition":"succeeded()","task":{ "id":"5541a522-603c-47ad-91fc-a4b1d163081b","versionSpec":"2.*","definitionType":"task"},"inputs":{ "command":"publish","publishWebProjects":"True","projects":"$(Parameters.RestoreBuildProjects)","custom":"","arguments":"--configuration $(BuildConfiguration) --output $(build.artifactstagingdirectory)","restoreArguments":"","publishTestResults":"true","testRunTitle":"","zipAfterPublish":"True","modifyOutputPath":"true","selectOrConfig":"select","feedRestore":"","includeNuGetOrg":"true","nugetConfigPath":"","externalEndpoints":"","noCache":"false","packagesDirectory":"","verbosityRestore":"Detailed","searchPatternPush":"$(Build.ArtifactStagingDirectory)/*.nupkg","nuGetFeedType":"internal","feedPublish":"","publishPackageMetadata":"true","externalEndpoint":"","searchPatternPack":"**/*.csproj","configurationToPack":"$(BuildConfiguration)","outputDir":"$(Build.ArtifactStagingDirectory)","nobuild":"false","includesymbols":"false","includesource":"false","versioningScheme":"off","versionEnvVar":"","requestedMajorVersion":"1","requestedMinorVersion":"0","requestedPatchVersion":"0","buildProperties":"","verbosityPack":"Detailed","workingDirectory":""} },{ "environment":{ },"enabled":true,"continueOnError":false,"alwaysRun":true,"displayName":"Publish Artifact","timeoutInMinutes":0,"retryCountOnTaskFailure":0,"condition":"succeededOrFailed()","task":{ "id":"2ff763a7-ce83-4e1f-bc89-0ae63477cebe","versionSpec":"1.*","definitionType":"task"},"inputs":{ "PathtoPublish":"$(build.artifactstagingdirectory)","ArtifactName":"drop","ArtifactType":"Container","TargetPath":"\\\\my\\share\\$(Build.DefinitionName)\\$(Build.BuildNumber)","Parallel":"false","ParallelCount":"8","StoreAsTar":"false"} }],"name":"Agent job 1","refName":"Job_1","condition":"succeeded()","target":{ "executionOptions":{ "type":0},"allowScriptsAuthAccessOption":false,"type":1},"jobAuthorizationScope":"project"}],"target":{ "agentSpecification":{ "identifier":"windows-2019"} },"type":1},"repository":{ "properties":{ "cleanOptions":"0","labelSources":"0","labelSourcesFormat":"$(build.buildNumber)","reportBuildStatus":"true","fetchDepth":"1","gitLfsSupport":"false","skipSyncSource":"false","checkoutNestedSubmodules":"false"},"id":"096284f6-aabc-4252-8215-99825ebdd645","type":"TfsGit","name":"project4","url":"https://dev.azure.com/cj4599868/project4/_git/project4","defaultBranch":"refs/heads/master","clean":"false","checkoutSubmodules":false},"processParameters":{ "inputs":[{ "aliases":[],"options":{ },"properties":{ },"name":"RestoreBuildProjects","label":"Project(s) to restore and build","defaultValue":"**/*.csproj","type":"multiline","helpMarkDown":"Relative path of the .csproj file(s) from repo root. Wildcards can be used. For example, **/*.csproj for all .csproj files in all the subfolders.","visibleRule":"","groupName":""},{ "aliases":[],"options":{ },"properties":{ },"name":"TestProjects","label":"Project(s) to test","defaultValue":"**/*[Tt]ests/*.csproj","type":"multiline","helpMarkDown":"Relative path of the .csproj file(s) from repo root. Wildcards can be used. For example, **/*.csproj for all .csproj files in all the subfolders.","visibleRule":"","groupName":""}]},"quality":"definition","authoredBy":{ "displayName":"Chris Johnson","url":"https://spsprodcca1.vssps.visualstudio.com/A36ee7258-ef28-466f-aaf2-f767eb692ed2/_apis/Identities/b370c683-bb42-626c-9a85-ec3a81c32fbf","_links":{ "avatar":{ "href":"https://dev.azure.com/cj4599868/_apis/GraphProfile/MemberAvatars/aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm"} },"id":"b370c683-bb42-626c-9a85-ec3a81c32fbf","uniqueName":"cj4599868@gmail.com","imageUrl":"https://dev.azure.com/cj4599868/_apis/GraphProfile/MemberAvatars/aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm","descriptor":"aad.YjM3MGM2ODMtYmI0Mi03MjZjLTlhODUtZWMzYTgxYzMyZmJm"},"drafts":[],"queue":{ "_links":{ "self":{ "href":"https://dev.azure.com/cj4599868/_apis/build/Queues/18"} },"id":18,"name":"Azure Pipelines","url":"https://dev.azure.com/cj4599868/_apis/build/Queues/18","pool":{ "id":9,"name":"Azure Pipelines","isHosted":true} },"id":10,"name":"pipeline3","url":"https://dev.azure.com/cj4599868/8475d613-bfde-40bc-92ce-883005ff4a17/_apis/build/Definitions/10?revision=1","uri":"vstfs:///Build/Definition/10","path":"\\","type":"build","queueStatus":"enabled","revision":1,"createdDate":"2023-06-23T01:11:47.667Z","project":{ "id":"8475d613-bfde-40bc-92ce-883005ff4a17","name":"project4","description":"test project 4","url":"https://dev.azure.com/cj4599868/_apis/projects/8475d613-bfde-40bc-92ce-883005ff4a17","state":"wellFormed","revision":19,"visibility":"private","lastUpdateTime":"2023-05-26T22:21:13.09Z"} }"
            HttpResponseMessage response = client.SendAsync(requestMessage).Result;
            response.EnsureSuccessStatusCode();
        }


        [AuthorizeForScopes(Scopes = new[] { "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation"})]
        public async Task<IActionResult> DevOps()
        {
            var accessToken = await tokenAcquisition.GetAccessTokenForUserAsync(devopsscopes);


            //ViewData["Photo"] = photo;
            //var me = await graphApiOperations.GetUserInformation(accessToken);
            //var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            //  ViewData["Me"] = me;
            //   ViewData["Photo"] = photo;

            CreateNewDefinition(accessToken);
            await  GetDefinition(accessToken);

            var repos = GetGitRepos(accessToken);
            ViewData["repos"] = repos;
            var definitions = ListBuildDefinitions(accessToken);
            ViewData["definitions"] = definitions;
            return View();
        }



        [AuthorizeForScopes(Scopes = new[] { WebApp_OpenIDConnect_DotNet.Infrastructure.Constants.ScopeUserRead })]
        public async Task<IActionResult> Profile()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { WebApp_OpenIDConnect_DotNet.Infrastructure.Constants.ScopeUserRead });

            var me = await graphApiOperations.GetUserInformation(accessToken);
            var photo = await graphApiOperations.GetPhotoAsBase64Async(accessToken);

            ViewData["Me"] = me;
            ViewData["Photo"] = photo;

            return View();
        }

        // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
        // the admin tenant does not require admin consent for ARM.
        [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation", "user.read", "directory.read.all" })]
        public async Task<IActionResult> Tenants()
        {
            var accessToken =
                await tokenAcquisition.GetAccessTokenForUserAsync(new[] { $"{ArmApiOperationService.ArmResource}user_impersonation" });

            var tenantIds = await armOperations.EnumerateTenantsIdsAccessibleByUser(accessToken);
            /*
                        var tenantsIdsAndNames =  await graphApiOperations.EnumerateTenantsIdAndNameAccessibleByUser(tenantIds,
                            async tenantId => { return await tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "Directory.Read.All" }, tenantId); });
            */
            ViewData["tenants"] = tenantIds;

            return View();
        }

        // Requires that the app has added the Azure Service Management / user_impersonation scope, and that
        // the admin tenant does not require admin consent for ARM.
        [AuthorizeForScopes(Scopes = new[] { "https://management.core.windows.net/user_impersonation" })]
        public async Task<IActionResult> TenantsWithImplicitAuth()
        {
            var tenantIds = await armOperationsWithImplicitAuth.EnumerateTenantsIds();
            /*
                        var tenantsIdsAndNames =  await graphApiOperations.EnumerateTenantsIdAndNameAccessibleByUser(tenantIds,
                            async tenantId => { return await tokenAcquisition.GetAccessTokenForUserAsync(new string[] { "Directory.Read.All" }, tenantId); });
            */
            ViewData["tenants"] = tenantIds;

            return View(nameof(Tenants));
        }

        [AuthorizeForScopes(Scopes = new[] { "https://storage.azure.com/user_impersonation" })]
        public async Task<IActionResult> Blob()
        {
            string message = "Blob failed to create";
            // replace the URL below with your storage account URL
            //Uri blobUri = new Uri("https://blobstorageazuread.blob.core.windows.net/sample-container/Blob1.txt");
            Uri blobUri = new Uri("https://chrisjstorageaccount2.blob.core.windows.net/blob1?sp=r&st=2023-06-02T01:31:36Z&se=2023-06-02T09:31:36Z&spr=https&sv=2022-11-02&sr=c&sig=7XIwvLdqMALP9q2NIQjXKI55dwD8mO5vZw5HxCG2jRk%3D");
            BlobClient blobClient = new BlobClient(blobUri, new TokenAcquisitionTokenCredential(tokenAcquisition));

            string blobContents = "Blob created by Azure AD authenticated user.";
            byte[] byteArray = Encoding.ASCII.GetBytes(blobContents);
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                try
                {
                    await blobClient.UploadAsync(stream);
                    message = "Blob successfully created";
                }
                catch (MicrosoftIdentityWebChallengeUserException ex)
                {
                    throw ex;
                }	
                catch (MsalUiRequiredException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    try
                    {
                        message += $". Reason - {((Azure.RequestFailedException)ex).ErrorCode}";
                    }
                    catch (Exception)
                    {
                        message += $". Reason - {ex.Message}";
                    }
                }
            }

            ViewData["Message"] = message;
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
