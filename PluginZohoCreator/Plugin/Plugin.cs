using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;
using Pub;

namespace PluginZohoCreator.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private RequestHelper _client;
        private readonly HttpClient _injectedClient;
        private readonly ServerStatus _server;
        private TaskCompletionSource<bool> _tcs;

        public Plugin(HttpClient client = null)
        {
            _injectedClient = client ?? new HttpClient();
            _server = new ServerStatus
            {
                Connected = false
            };
        }

        /// <summary>
        /// Establishes a connection with Zoho Creator. Creates an authenticated http client and tests it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>A message indicating connection success</returns>
        public override async Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            _server.Connected = false;

            Logger.Info("Connecting...");

            // validate settings passed in
            try
            {
                var settings = JsonConvert.DeserializeObject<Settings>(request.SettingsJson);
                _server.Settings = settings;
                _server.Settings.Validate();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
            }

            // create new authenticated request helper with validated settings
            try
            {
                _client = new RequestHelper(_server.Settings, _injectedClient);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // attempt to call the Zoho api
            try
            {
                var response =
                    await _client.GetAsync("https://creator.zoho.com/api/json/applications?scope=creatorapi");
                response.EnsureSuccessStatusCode();

                _server.Connected = true;

                Logger.Info("Connected to Zoho Creator");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);

                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = e.Message,
                    OauthError = "",
                    SettingsError = ""
                };
            }

            return new ConnectResponse
            {
                OauthStateJson = request.OauthStateJson,
                ConnectionError = "",
                OauthError = "",
                SettingsError = ""
            };
        }

        public override async Task ConnectSession(ConnectRequest request,
            IServerStreamWriter<ConnectResponse> responseStream, ServerCallContext context)
        {
            Logger.Info("Connecting session...");

            // create task to wait for disconnect to be called
            _tcs?.SetResult(true);
            _tcs = new TaskCompletionSource<bool>();

            // call connect method
            var response = await Connect(request, context);

            await responseStream.WriteAsync(response);

            Logger.Info("Session connected.");

            // wait for disconnect to be called
            await _tcs.Task;
        }

        /// <summary>
        /// Discovers schemas located in the users Zoho CRM instance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Discovered schemas</returns>
        public override async Task<DiscoverSchemasResponse> DiscoverSchemas(DiscoverSchemasRequest request,
            ServerCallContext context)
        {
            Logger.Info("Discovering Schemas...");

            DiscoverSchemasResponse discoverSchemasResponse = new DiscoverSchemasResponse();
            ApplicationsResponse applicationsResponse;

            // get the applications present in Zoho
            try
            {
                Logger.Debug("Getting applications...");
                var response =
                    await _client.GetAsync("https://creator.zoho.com/api/json/applications?scope=creatorapi");
                response.EnsureSuccessStatusCode();

                applicationsResponse =
                    JsonConvert.DeserializeObject<ApplicationsResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // attempt to get a schema for each view in each application
            try
            {
                Logger.Info(
                    $"Applicaitions attempted: {applicationsResponse.Result.ApplicationsList.Applications.Count}");

                var tasks = applicationsResponse.Result.ApplicationsList.Applications.Select(x =>
                        GetSchemasForApplication(x, applicationsResponse.Result.ApplicationOwner))
                    .ToArray();

                await Task.WhenAll(tasks);

                discoverSchemasResponse.Schemas.AddRange(tasks.SelectMany(x => x.Result).ToList());
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            Logger.Info($"Schemas found: {discoverSchemasResponse.Schemas.Count}");

            // only return requested schemas if refresh mode selected
            if (request.Mode == DiscoverSchemasRequest.Types.Mode.Refresh)
            {
                var refreshSchemas = request.ToRefresh;
                var schemas =
                    JsonConvert.DeserializeObject<Schema[]>(
                        JsonConvert.SerializeObject(discoverSchemasResponse.Schemas));
                discoverSchemasResponse.Schemas.Clear();
                discoverSchemasResponse.Schemas.AddRange(schemas.Join(refreshSchemas, schema => schema.Id,
                    refreshSchema => refreshSchema.Id,
                    (schema, refresh) => schema));

                Logger.Debug($"Schemas found: {JsonConvert.SerializeObject(schemas)}");
                Logger.Debug($"Refresh requested on schemas: {JsonConvert.SerializeObject(refreshSchemas)}");

                Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");
                return discoverSchemasResponse;
            }

            // return all schemas otherwise
            Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");
            return discoverSchemasResponse;
        }

        /// <summary>
        /// Publishes a stream of data for a given schema
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ReadStream(ReadRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            var schema = request.Schema;
            var limit = request.Limit;
            var limitFlag = request.Limit != 0;

            Logger.Info($"Publishing records for schema: {schema.Name}");

            // get information from schema
            var publisherMetaJson = JsonConvert.DeserializeObject<PublisherMetaJson>(schema.PublisherMetaJson);

            try
            {
                Dictionary<string, List<Dictionary<string, object>>> recordsResponse;
                int recordsCount = 0;
                // Publish records for the given schema

                // get records for schema page by page
                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/view/{1}?scope=creatorapi&zc_ownername={2}&raw=true",
                    publisherMetaJson.ApplicationName,
                    publisherMetaJson.ViewName,
                    publisherMetaJson.OwnerName
                );

                var response = await _client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                // if response is empty or call did not succeed return no records
                if (!IsSuccessAndNotEmpty(response))
                {
                    Logger.Info($"No records for: {schema.Name}");
                    return;
                }

                recordsResponse =
                    JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(
                        await response.Content.ReadAsStringAsync());

                Logger.Debug($"data: {JsonConvert.SerializeObject(recordsResponse[publisherMetaJson.FormName])}");

                // publish each record in the page
                foreach (var record in recordsResponse[publisherMetaJson.FormName])
                {
                    foreach (var property in schema.Properties)
                    {
                        if (property.Type == PropertyType.String)
                        {
                            var value = record[property.Id];
                            if (!(value is string))
                            {
                                record[property.Id] = JsonConvert.SerializeObject(value);
                            }
                        }
                    }

                    var recordOutput = new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(record)
                    };

                    // stop publishing if the limit flag is enabled and the limit has been reached
                    if ((limitFlag && recordsCount == limit) || !_server.Connected)
                    {
                        break;
                    }

                    // publish record
                    await responseStream.WriteAsync(recordOutput);
                    recordsCount++;
                }

                Logger.Info($"Published {recordsCount} records");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Handles disconnect requests from the agent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            // clear connection
            _server.Connected = false;
            _server.Settings = null;

            // alert connection session to close
            if (_tcs != null)
            {
                _tcs.SetResult(true);
                _tcs = null;
            }

            Logger.Info("Disconnected");
            return Task.FromResult(new DisconnectResponse());
        }

        /// <summary>
        /// Gets all schemas for a given application
        /// </summary>
        /// <param name="application"></param>
        /// <param name="applicationOwner"></param>
        /// <returns></returns>
        private async Task<List<Schema>> GetSchemasForApplication(Application application, string applicationOwner)
        {
            FormsAndViewsResponse formsAndViewsResponse;

            // get the forms and views present in application
            try
            {
                Logger.Debug($"Getting forms and views for {application.ApplicationName}...");

                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/formsandviews?scope=creatorapi&zc_ownername={1}",
                    application.LinkName,
                    applicationOwner
                );
                var response = await _client.GetAsync(uri);
                response.EnsureSuccessStatusCode();

                formsAndViewsResponse =
                    JsonConvert.DeserializeObject<FormsAndViewsResponse>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }

            // get all schemas for each application
            try
            {
                var tasks = formsAndViewsResponse.ApplicationName.First().ViewList
                    .Select(x => GetSchemaForView(x, application.LinkName, applicationOwner))
                    .ToArray();

                await Task.WhenAll(tasks);

                return tasks.Where(x => x.Result != null).Select(x => x.Result).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Gets a schema for a given form
        /// </summary>
        /// <param name="view"></param>
        /// /// <param name="applicationName"></param>
        /// <param name="applicationOwner"></param>
        /// <returns>returns a schema or null if unavailable</returns>
        private async Task<Schema> GetSchemaForView(View view, string applicationName, string applicationOwner)
        {
            // base schema to be added to
            var schema = new Schema
            {
                Id = view.ComponentName,
                Name = view.DisplayName,
                Description = "",
                PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                {
                    ApplicationName = applicationName,
                    OwnerName = applicationOwner,
                    ViewName = view.ComponentName,
                    FormName = view.FormLinkName
                }),
                DataFlowDirection = Schema.Types.DataFlowDirection.Read
            };

            try
            {
                Logger.Debug($"Getting fields for: {view.DisplayName}");

                // get fields for module
                var uri = String.Format(
                    "https://creator.zoho.com/api/json/{0}/{1}/fields?scope=creatorapi&zc_ownername={2}",
                    applicationName,
                    view.FormLinkName,
                    applicationOwner
                );
                var response = await _client.GetAsync(uri);

                // if response is empty or call did not succeed return null
                if (!IsSuccessAndNotEmpty(response))
                {
                    Logger.Debug($"No fields for: {view.FormLinkName}");
                    return null;
                }

                Logger.Debug($"Got fields for: {view.FormLinkName}");

                // for each field in the schema add a new property
                var fieldsResponse =
                    JsonConvert.DeserializeObject<FieldsResponse>(await response.Content.ReadAsStringAsync());

                foreach (var field in fieldsResponse.ApplicationName.First().FormName.First().Fields)
                {
                    var property = new Property
                    {
                        Id = field.FieldName,
                        Name = field.DisplayName,
                        Type = GetPropertyType(field),
                        IsKey = field.Unique,
                        IsCreateCounter = false,
                        IsUpdateCounter = false,
                        TypeAtSource = field.ApiType.ToString(),
                        IsNullable = true
                    };

                    schema.Properties.Add(property);
                }

                Logger.Debug($"Added schema for: {applicationName}");
                return schema;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the Naveego type from the provided Zoho information
        /// </summary>
        /// <param name="field"></param>
        /// <returns>The property type</returns>
        private PropertyType GetPropertyType(Field field)
        {
            switch (field.ApiType)
            {
                case 6:
                case 7:
                    return PropertyType.Float;
                case 5:
                case 9:
                    return PropertyType.Integer;
                case 10:
                case 11:
                    return PropertyType.Datetime;
                default:
                    if (field.MaxChar > 1024)
                    {
                        return PropertyType.Text;
                    }
                    else
                    {
                        return PropertyType.String;
                    }
            }
        }

        /// <summary>
        /// Checks if a http response message is not empty and did not fail
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private bool IsSuccessAndNotEmpty(HttpResponseMessage response)
        {
            return response.StatusCode != HttpStatusCode.NoContent && response.IsSuccessStatusCode;
        }
    }
}