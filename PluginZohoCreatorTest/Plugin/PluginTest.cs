using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using PluginZohoCreator.DataContracts;
using Pub;
using RichardSzalay.MockHttp;
using Xunit;
using Record = Pub.Record;

namespace PluginZohoCreatorTest.Plugin
{
    public class PluginTest
    {
        private ConnectRequest GetConnectSettings()
        {
            return new ConnectRequest
            {
                SettingsJson = "{\"Token\" : \"mocktoken\"}",
                OauthConfiguration = null,
                OauthStateJson = ""
            };
        }

        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");

            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();
            var disconnectRequest = new DisconnectRequest();

            // act
            var response = client.ConnectSession(request);
            var responseStream = response.ResponseStream;
            var records = new List<ConnectResponse>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
                client.Disconnect(disconnectRequest);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ConnectTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");

            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");

            mockHttp.When("https://creator.zoho.com/api/json/event-management/formsandviews?scope=creatorapi&zc_ownername=wyattroehler&authtoken=mocktoken")
                .Respond("application/json", 
                    "{\"application-name\":[\"event-management\",{\"viewList\":[{\"viewCount\":8},{\"linkid\":71,\"componentid\":3836610000000031391,\"displayname\":\"BookEvents\",\"componentname\":\"Book_Events_Mobile\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":72,\"componentid\":3836610000000031569,\"displayname\":\"EventsCalendar\",\"componentname\":\"Events_Calendar\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":73,\"componentid\":3836610000000031573,\"displayname\":\"AllEventsList\",\"componentname\":\"All_Events_List\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":74,\"componentid\":3836610000000031577,\"displayname\":\"PaidEventBookings\",\"componentname\":\"Booked_Entries\",\"formlinkname\":\"Book_Paid_Events\"},{\"linkid\":75,\"componentid\":3836610000000031579,\"displayname\":\"FreeEventBookings\",\"componentname\":\"Booked_Free_Entries\",\"formlinkname\":\"Book_Free_Events1\"},{\"linkid\":76,\"componentid\":3836610000000031581,\"displayname\":\"AllVenues\",\"componentname\":\"All_Venues\",\"formlinkname\":\"Add_Venue\"},{\"linkid\":77,\"componentid\":3836610000000031583,\"displayname\":\"AllEventCategories\",\"componentname\":\"All_Event_Categories\",\"formlinkname\":\"Add_Event_Category\"},{\"linkid\":78,\"componentid\":3836610000000031585,\"displayname\":\"ViewAttendees\",\"componentname\":\"All_Customers\",\"formlinkname\":\"Add_Customer\"}],\"formList\":[{\"formCount\":1},{\"linkid\":49,\"displayname\":\"CreateNewEvent\",\"componentname\":\"Create_New_Event\"}]}]}");
            
            mockHttp.When("https://creator.zoho.com/api/json/event-management/Create_New_Event/fields?scope=creatorapi&zc_ownername=wyattroehler&authtoken=mocktoken")
                .Respond("application/json", 
                    "{\"application-name\":[\"event-management\",{\"form-name\":[\"Create_New_Event\",{\"Fields\":[{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"3836610000000030013\"}],\"Tooltip\":\"\",\"DisplayName\":\"EventCategory\",\"Unique\":false,\"FieldName\":\"Event_Category\",\"apiType\":12},{\"Reqd\":true,\"Type\":1,\"Tooltip\":\"\",\"DisplayName\":\"EventName\",\"Unique\":false,\"MaxChar\":255,\"FieldName\":\"Event_Name\",\"Initial\":\"\",\"apiType\":1},{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"3836610000000030019\"}],\"Tooltip\":\"\",\"DisplayName\":\"Venue\",\"Unique\":false,\"FieldName\":\"Venue\",\"apiType\":12},{\"Reqd\":true,\"Type\":22,\"Tooltip\":\"\",\"DisplayName\":\"EventStartTime\",\"Unique\":false,\"FieldName\":\"Event_Start_Time\",\"Initial\":\"\",\"apiType\":11},{\"Reqd\":true,\"Type\":22,\"Tooltip\":\"\",\"DisplayName\":\"EventEndTime\",\"Unique\":false,\"FieldName\":\"Event_End_Time\",\"Initial\":\"\",\"apiType\":11},{\"Reqd\":false,\"Type\":31,\"Tooltip\":\"\",\"DisplayName\":\"EventID\",\"Unique\":false,\"FieldName\":\"Event_ID\",\"apiType\":9},{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"Free\",\"choice2\":\"Paid\"}],\"Tooltip\":\"\",\"DisplayName\":\"TicketType\",\"Unique\":false,\"FieldName\":\"Subscription_Type\",\"apiType\":12},{\"Reqd\":true,\"altTxtReq\":false,\"Type\":20,\"imgLinkReq\":false,\"imgTitleReq\":false,\"Tooltip\":\"\",\"DisplayName\":\"EventImage\",\"Unique\":false,\"FieldName\":\"Event_Image\",\"apiType\":18},{\"Reqd\":true,\"Type\":6,\"Tooltip\":\"\",\"DisplayName\":\"EntryFees\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Entry_Fees\",\"Initial\":\"0\",\"CurrencyType\":\"USD\",\"apiType\":8},{\"Reqd\":true,\"Type\":5,\"Tooltip\":\"\",\"DisplayName\":\"Numberofentries\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Number_of_entries\",\"Initial\":\"0\",\"apiType\":5},{\"Reqd\":false,\"Type\":5,\"Tooltip\":\"\",\"DisplayName\":\"AvailableEntries\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Available_Entries\",\"Initial\":\"0\",\"apiType\":5},{\"Reqd\":true,\"Type\":3,\"Tooltip\":\"\",\"DisplayName\":\"EventDescription\",\"Unique\":false,\"FieldName\":\"Remarks\",\"apiType\":2},{\"Reqd\":false,\"Type\":101,\"Choices\":[{\"choice1\":\"Active\",\"choice2\":\"Ended\"}],\"Tooltip\":\"\",\"DisplayName\":\"EventStatus\",\"Unique\":false,\"FieldName\":\"Event_Status\",\"apiType\":13}],\"DisplayName\":\"CreateNewEvent\"}]}]}");

            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Equal(3, response.Schemas.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");

            mockHttp.When("https://creator.zoho.com/api/json/event-management/formsandviews?scope=creatorapi&zc_ownername=wyattroehler&authtoken=mocktoken")
                .Respond("application/json", 
                    "{\"application-name\":[\"event-management\",{\"viewList\":[{\"viewCount\":8},{\"linkid\":71,\"componentid\":3836610000000031391,\"displayname\":\"BookEvents\",\"componentname\":\"Book_Events_Mobile\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":72,\"componentid\":3836610000000031569,\"displayname\":\"EventsCalendar\",\"componentname\":\"Events_Calendar\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":73,\"componentid\":3836610000000031573,\"displayname\":\"AllEventsList\",\"componentname\":\"All_Events_List\",\"formlinkname\":\"Create_New_Event\"},{\"linkid\":74,\"componentid\":3836610000000031577,\"displayname\":\"PaidEventBookings\",\"componentname\":\"Booked_Entries\",\"formlinkname\":\"Book_Paid_Events\"},{\"linkid\":75,\"componentid\":3836610000000031579,\"displayname\":\"FreeEventBookings\",\"componentname\":\"Booked_Free_Entries\",\"formlinkname\":\"Book_Free_Events1\"},{\"linkid\":76,\"componentid\":3836610000000031581,\"displayname\":\"AllVenues\",\"componentname\":\"All_Venues\",\"formlinkname\":\"Add_Venue\"},{\"linkid\":77,\"componentid\":3836610000000031583,\"displayname\":\"AllEventCategories\",\"componentname\":\"All_Event_Categories\",\"formlinkname\":\"Add_Event_Category\"},{\"linkid\":78,\"componentid\":3836610000000031585,\"displayname\":\"ViewAttendees\",\"componentname\":\"All_Customers\",\"formlinkname\":\"Add_Customer\"}],\"formList\":[{\"formCount\":1},{\"linkid\":49,\"displayname\":\"CreateNewEvent\",\"componentname\":\"Create_New_Event\"}]}]}");
            
            mockHttp.When("https://creator.zoho.com/api/json/event-management/Create_New_Event/fields?scope=creatorapi&zc_ownername=wyattroehler&authtoken=mocktoken")
                .Respond("application/json", 
                    "{\"application-name\":[\"event-management\",{\"form-name\":[\"Create_New_Event\",{\"Fields\":[{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"3836610000000030013\"}],\"Tooltip\":\"\",\"DisplayName\":\"EventCategory\",\"Unique\":false,\"FieldName\":\"Event_Category\",\"apiType\":12},{\"Reqd\":true,\"Type\":1,\"Tooltip\":\"\",\"DisplayName\":\"EventName\",\"Unique\":false,\"MaxChar\":255,\"FieldName\":\"Event_Name\",\"Initial\":\"\",\"apiType\":1},{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"3836610000000030019\"}],\"Tooltip\":\"\",\"DisplayName\":\"Venue\",\"Unique\":false,\"FieldName\":\"Venue\",\"apiType\":12},{\"Reqd\":true,\"Type\":22,\"Tooltip\":\"\",\"DisplayName\":\"EventStartTime\",\"Unique\":false,\"FieldName\":\"Event_Start_Time\",\"Initial\":\"\",\"apiType\":11},{\"Reqd\":true,\"Type\":22,\"Tooltip\":\"\",\"DisplayName\":\"EventEndTime\",\"Unique\":false,\"FieldName\":\"Event_End_Time\",\"Initial\":\"\",\"apiType\":11},{\"Reqd\":false,\"Type\":31,\"Tooltip\":\"\",\"DisplayName\":\"EventID\",\"Unique\":false,\"FieldName\":\"Event_ID\",\"apiType\":9},{\"Reqd\":true,\"Type\":100,\"Choices\":[{\"choice1\":\"Free\",\"choice2\":\"Paid\"}],\"Tooltip\":\"\",\"DisplayName\":\"TicketType\",\"Unique\":false,\"FieldName\":\"Subscription_Type\",\"apiType\":12},{\"Reqd\":true,\"altTxtReq\":false,\"Type\":20,\"imgLinkReq\":false,\"imgTitleReq\":false,\"Tooltip\":\"\",\"DisplayName\":\"EventImage\",\"Unique\":false,\"FieldName\":\"Event_Image\",\"apiType\":18},{\"Reqd\":true,\"Type\":6,\"Tooltip\":\"\",\"DisplayName\":\"EntryFees\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Entry_Fees\",\"Initial\":\"0\",\"CurrencyType\":\"USD\",\"apiType\":8},{\"Reqd\":true,\"Type\":5,\"Tooltip\":\"\",\"DisplayName\":\"Numberofentries\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Number_of_entries\",\"Initial\":\"0\",\"apiType\":5},{\"Reqd\":false,\"Type\":5,\"Tooltip\":\"\",\"DisplayName\":\"AvailableEntries\",\"Unique\":false,\"MaxChar\":10,\"FieldName\":\"Available_Entries\",\"Initial\":\"0\",\"apiType\":5},{\"Reqd\":true,\"Type\":3,\"Tooltip\":\"\",\"DisplayName\":\"EventDescription\",\"Unique\":false,\"FieldName\":\"Remarks\",\"apiType\":2},{\"Reqd\":false,\"Type\":101,\"Choices\":[{\"choice1\":\"Active\",\"choice2\":\"Ended\"}],\"Tooltip\":\"\",\"DisplayName\":\"EventStatus\",\"Unique\":false,\"FieldName\":\"Event_Status\",\"apiType\":13}],\"DisplayName\":\"CreateNewEvent\"}]}]}");

            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {new Schema {Id = "2"}}
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Empty(response.Schemas);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");
            
            mockHttp.When(
                    "https://creator.zoho.com/api/json/event-management/view/Events_Calendar?scope=creatorapi&zc_ownername=wyattroehler&startindex=0&limit=200&raw=true&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"Create_New_Event\":[{\"Subscription_Type\":\"Free\",\"Entry_Fees\":\"$0.00\",\"Venue\":\"250EFrontStreet\",\"Number_of_entries\":10,\"Event_ID\":0,\"Event_End_Time\":\"14-Mar-201908:31:52\",\"Event_Status\":\"Active\",\"Event_Image\":\"<imgsrc=\\\"https://www.google.com/url?sa=i&source=images&cd=&cad=rja&uact=8&ved=2ahUKEwj-yLWBkP_gAhWF6IMKHdhUB0QQjRx6BAgBEAU&url=https%3A%2F%2Fwww.w3schools.com%2Fw3css%2Fw3css_images.asp&psig=AOvVaw3WmqPMreHGH_RWBkQigvgc&ust=1552566751508402\\\"border=\\\"0\\\"><\\/img>\",\"Event_Start_Time\":\"13-Mar-201908:31:49\",\"Event_Category\":\"Circus\",\"Remarks\":\"Thebestestevent\",\"ID\":\"3836610000000030023\",\"Available_Entries\":10,\"Event_Name\":\"GrandEvent\"},{\"Subscription_Type\":\"Free\",\"Entry_Fees\":\"$0.00\",\"Venue\":\"250EFrontStreet\",\"Number_of_entries\":10,\"Event_ID\":0,\"Event_End_Time\":\"14-Mar-201908:31:52\",\"Event_Status\":\"Active\",\"Event_Image\":\"<imgsrc=\\\"https://www.google.com/url?sa=i&source=images&cd=&cad=rja&uact=8&ved=2ahUKEwj-yLWBkP_gAhWF6IMKHdhUB0QQjRx6BAgBEAU&url=https%3A%2F%2Fwww.w3schools.com%2Fw3css%2Fw3css_images.asp&psig=AOvVaw3WmqPMreHGH_RWBkQigvgc&ust=1552566751508402\\\"border=\\\"0\\\"><\\/img>\",\"Event_Start_Time\":\"13-Mar-201908:31:49\",\"Event_Category\":\"Circus\",\"Remarks\":\"Thebestestevent\",\"ID\":\"3836610000000030023\",\"Available_Entries\":10,\"Event_Name\":\"GrandEvent\"}]}");

            mockHttp.When(
                    "https://creator.zoho.com/api/json/event-management/view/Events_Calendar?scope=creatorapi&zc_ownername=wyattroehler&startindex=200&limit=200&raw=true&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"Create_New_Event\":[]}");

            
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new ReadRequest()
            {
                Schema = new Schema
                {
                    Id = "test",
                    Name = "test",
                    PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                    {
                        ApplicationName = "event-management",
                        OwnerName = "wyattroehler",
                        ViewName = "Events_Calendar",
                        FormName = "Create_New_Event"
                    })
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(2, records.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamLimitTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://creator.zoho.com/api/json/applications?scope=creatorapi&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"result\":{\"application_list\":{\"applications\":[{\"application\":[{\"created_time\":\"2019-03-1305:29:27.0\",\"application_name\":\"EventManagement\",\"access\":\"private\",\"link_name\":\"event-management\",\"time_zone\":\"IST\",\"dateformat\":\"dd-MMM-yyyy\"}]}]},\"application_owner\":\"wyattroehler\"}}");
            
            mockHttp.When(
                    "https://creator.zoho.com/api/json/event-management/view/Events_Calendar?scope=creatorapi&zc_ownername=wyattroehler&startindex=0&limit=200&raw=true&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"Create_New_Event\":[{\"Subscription_Type\":\"Free\",\"Entry_Fees\":\"$0.00\",\"Venue\":\"250EFrontStreet\",\"Number_of_entries\":10,\"Event_ID\":0,\"Event_End_Time\":\"14-Mar-201908:31:52\",\"Event_Status\":\"Active\",\"Event_Image\":\"<imgsrc=\\\"https://www.google.com/url?sa=i&source=images&cd=&cad=rja&uact=8&ved=2ahUKEwj-yLWBkP_gAhWF6IMKHdhUB0QQjRx6BAgBEAU&url=https%3A%2F%2Fwww.w3schools.com%2Fw3css%2Fw3css_images.asp&psig=AOvVaw3WmqPMreHGH_RWBkQigvgc&ust=1552566751508402\\\"border=\\\"0\\\"><\\/img>\",\"Event_Start_Time\":\"13-Mar-201908:31:49\",\"Event_Category\":\"Circus\",\"Remarks\":\"Thebestestevent\",\"ID\":\"3836610000000030023\",\"Available_Entries\":10,\"Event_Name\":\"GrandEvent\"},{\"Subscription_Type\":\"Free\",\"Entry_Fees\":\"$0.00\",\"Venue\":\"250EFrontStreet\",\"Number_of_entries\":10,\"Event_ID\":0,\"Event_End_Time\":\"14-Mar-201908:31:52\",\"Event_Status\":\"Active\",\"Event_Image\":\"<imgsrc=\\\"https://www.google.com/url?sa=i&source=images&cd=&cad=rja&uact=8&ved=2ahUKEwj-yLWBkP_gAhWF6IMKHdhUB0QQjRx6BAgBEAU&url=https%3A%2F%2Fwww.w3schools.com%2Fw3css%2Fw3css_images.asp&psig=AOvVaw3WmqPMreHGH_RWBkQigvgc&ust=1552566751508402\\\"border=\\\"0\\\"><\\/img>\",\"Event_Start_Time\":\"13-Mar-201908:31:49\",\"Event_Category\":\"Circus\",\"Remarks\":\"Thebestestevent\",\"ID\":\"3836610000000030023\",\"Available_Entries\":10,\"Event_Name\":\"GrandEvent\"}]}");

            mockHttp.When(
                    "https://creator.zoho.com/api/json/event-management/view/Events_Calendar?scope=creatorapi&zc_ownername=wyattroehler&startindex=200&limit=200&raw=true&authtoken=mocktoken")
                .Respond("application/json",
                    "{\"Create_New_Event\":[]}");
            
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new ReadRequest()
            {
                Schema = new Schema
                {
                    Id = "test",
                    Name = "test",
                    PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                    {
                        ApplicationName = "event-management",
                        OwnerName = "wyattroehler",
                        ViewName = "Events_Calendar",
                        FormName = "Create_New_Event"
                    })
                },
                Limit = 1
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DisconnectTest()
        {
            // setup
            var mockHttp = new MockHttpMessageHandler();

            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginZohoCreator.Plugin.Plugin(mockHttp.ToHttpClient()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = new DisconnectRequest();

            // act
            var response = client.Disconnect(request);

            // assert
            Assert.IsType<DisconnectResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
    }
}