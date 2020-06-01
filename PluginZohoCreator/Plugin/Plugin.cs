using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Naveego.Sdk.Plugins;
using Newtonsoft.Json;
using PluginZohoCreator.API.Discover;
using PluginZohoCreator.API.Read;
using PluginZohoCreator.API.Utility;
using PluginZohoCreator.DataContracts;
using PluginZohoCreator.Helper;


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
                Logger.Error(e, e.Message, context);
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
                Logger.Error(e, e.Message, context);
                return new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                };
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
                Logger.Error(e, e.Message, context);

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
            try
            {
                Logger.SetLogPrefix("discover");
                Logger.Info("Discovering Schemas...");

                DiscoverSchemasResponse discoverSchemasResponse = new DiscoverSchemasResponse();

                discoverSchemasResponse.Schemas.AddRange(await Discover.GetAllSchemas(_client));
                discoverSchemasResponse.Schemas.AddRange(
                    await Discover.GetAllCustomSchemas(_client, _server.Settings.CustomSchemaList));

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

                    Logger.Debug($"Refresh requested on schemas: {JsonConvert.SerializeObject(refreshSchemas)}");

                    Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");
                    return discoverSchemasResponse;
                }

                // return all schemas otherwise
                Logger.Info($"Schemas returned: {discoverSchemasResponse.Schemas.Count}");
                return discoverSchemasResponse;
            }
            catch (Exception e)
            {
                Logger.Error(e, e.Message, context);
                return new DiscoverSchemasResponse();
            }
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
            var recordsCount = 0;

            Logger.SetLogPrefix(request.JobId);
            Logger.Info($"Publishing records for schema: {schema.Name}");

            try
            {
                // get information from schema
                var publisherMetaJson = JsonConvert.DeserializeObject<PublisherMetaJson>(schema.PublisherMetaJson);

                var recordsResponse = await Read.GetAllRecordsAsync(_client, publisherMetaJson);

                // publish each record in the page
                foreach (var record in recordsResponse)
                {
                    try
                    {
                        foreach (var property in schema.Properties)
                        {
                            if (record.ContainsKey(property.Id))
                            {
                                if (record[property.Id] != null)
                                {
                                    switch (property.Type)
                                    {
                                        case PropertyType.String:
                                            var value = record[property.Id];
                                            if (!(value is string))
                                            {
                                                record[property.Id] = JsonConvert.SerializeObject(value);
                                            }

                                            continue;
                                    }
                                }
                            }
                            else
                            {
                                record[property.Id] = null;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "ReadStream Processing");
                        Logger.Error(e, e.Message);
                        continue;
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
                Logger.Error(e, "ReadStream");
                Logger.Error(e, e.Message, context);
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
    }
}