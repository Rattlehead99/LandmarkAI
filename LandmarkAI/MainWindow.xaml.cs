using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using LandmarkAI.Classes;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace LandmarkAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void ImageSelectButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            
            //Everything after the "|" is the actual filter functionality, setting what it should filter for.
            //Every odd following "|" adds another filter criteria, while every even is just text for the user.
            //This filter is the filter when you choose in the explorer tab the types of files to open/look for.
            dialog.Filter = "Image files (*.jpeg, *.png)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            //Sets the directory in which the dialog explorer window first opens at.
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (dialog.ShowDialog() == true)
            {
                string fileName = dialog.FileName;

                //Setting the source of the image in the XML to the file on the computer selected from the OpenFileDialog's .ShowDialog() method:
                SelectedImage.Source = new BitmapImage(new Uri(fileName));

                MakePrediction(fileName);
            }

            
        }

        //T63. Sending a Request to the REST Service:
        private async Task MakePrediction(string fileName)
        {
            //Setup for using the API
            string url = "https://westeurope.api.cognitive.microsoft.com/customvision/v3.0/Prediction/82c5d099-6f79-4f29-a146-0475fec7c19c/classify/iterations/Iteration1/image";
            string predictionKey = "e49623412ff741b1a2f687df4e7a32e2";
            string contentType = "application/octet-stream";
            byte[] imageFile = File.ReadAllBytes(fileName);

            //How to make an HTTP request - you need an HTTPClient class
            using (HttpClient httpClient = new HttpClient())
            {
                //According to customvision.ai's Hot to use Prediction API the Prediction-Key is a Header
                httpClient.DefaultRequestHeaders.Add("Prediction-Key", predictionKey);

                //The content type is a common header, so we don't need to add it to httpClient, because it already exists.
                //To set it to the content type that the API is requesting all you need to do is create the content
                //of type ByteArrayContent that needs ot be sent and set its ContentType property to the one needed:
                using (var content = new ByteArrayContent(imageFile))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                    //Send the request with the content/file and get a response:
                    HttpResponseMessage response = await httpClient.PostAsync(url, content);

                    //T64. Reading the Response as JSON
                    //Read the content of the response as a string, which in this case is a JSON formatted string:
                    string responseString = await response.Content.ReadAsStringAsync();

                    //Use https://jsonutils.com/ -> a tool that from a JSON string shows you what OOP POCO classes it is equivalent of(=> T64.1 CustomVision.cs):

                    //T65. Deserializing JSON
                    // Add the NewtonSoft.Json nuget
                    IList<Prediction> predictions = (JsonConvert.DeserializeObject<CustomVision>(responseString)).Predictions;//=>(T66.)

                    //T66. Displaying the result in a GridView
                    PredictionListView.ItemsSource = predictions;
                }
            }
        }
    }

   
}
