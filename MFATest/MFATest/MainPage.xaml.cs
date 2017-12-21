using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MFATest
{
    public partial class MainPage : ContentPage
    {
        public static string clientId = "4c2d7c93-9268-49ea-a31c-da3ca5032922";
        public static string authority = "https://login.windows.net/common";
        public static string returnUri = "http://MFATestPCL-redirect";
        private const string graphResourceUri = "https://graph.windows.net";
        private AuthenticationResult authResult = null;

        HttpClient client = new HttpClient();


        public MainPage()
        {
            InitializeComponent();
        }

        private async void btnLogin_Clicked(object sender, EventArgs e)
        {
            try
            {
                var auth = DependencyService.Get<IAuthenticator>();
                authResult = await auth.Authenticate(authority, graphResourceUri, clientId, returnUri);
                var userName = authResult.UserInfo.GivenName + " " + authResult.UserInfo.FamilyName;
                lblUserName.Text = userName;
                lblMessage.Text = "Access Token: " + authResult.AccessToken.ToString();
                await DisplayAlert("Token", userName, "Ok", "Cancel");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private void btnLogout_Clicked(object sender, EventArgs e)
        {
            if (authResult != null)
            {
                AuthenticationContext ac = new AuthenticationContext(authority);
                ac.TokenCache.Clear();
                var auth = DependencyService.Get<IAuthenticator>();
                auth.ClearAllCookies();
            }
        }

        private async void btnAddTaskRestAdal_Clicked(object sender, EventArgs e)
        {

            if (!(authResult is null))
            {
                client.MaxResponseContentBufferSize = 256000;
                var uri = new Uri(string.Format("http://centos-srv01.westeurope.cloudapp.azure.com:3000/tasks/sevstaluser01/task02", string.Empty));
                // string authHeader = authResult.CreateAuthorizationHeader();
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", authHeader);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                try
                {
                    string mydata = "test01";
                    StringContent queryString = new StringContent(mydata);
                    var response = await client.PostAsync(uri, queryString);
                    if (response.IsSuccessStatusCode)
                    {
                        //lblMessage.Text = "REST Call Success";
                        lblMessage.Text = await response.Content.ReadAsStringAsync();
                        //var content = await response.Content.ReadAsStringAsync();
                        //Items = JsonConvert.DeserializeObject<List<TodoItem>>(content);
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = $"ERROR: {ex.Message}";
                }
            }
            else
                lblMessage.Text = "Please press Login to Azure button first";
        }

    }
}
