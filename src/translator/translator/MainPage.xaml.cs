using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using translator.Resources;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech.Recognition;
using System.Xml.Linq;
using translator.Model;


namespace languagetranslator
{
    public partial class MainPage : PhoneApplicationPage
    {

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            BuildLocalizedApplicationBar();
        }

        // Sample code for building a localized ApplicationBar
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            // Create a new button and set the text value to the localized string from AppResources.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/microphone.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarButtonText;
            appBarButton.Click += appBarButton_Click;

            ApplicationBar.Buttons.Add(appBarButton);
        }

        #region Buttons Actions

        private async void appBarButton_Click(object sender, EventArgs e)
        {
            //-- Objetos para reconocimiento de voz
            var recognizer = new SpeechRecognizerUI();
            recognizer.Settings.ShowConfirmation = true;
            recognizer.Settings.ReadoutEnabled = false;

            //-- Inicializando para escuchar la voz
            var result = await recognizer.RecognizeWithUIAsync();

            if (result.ResultStatus == SpeechRecognitionUIStatus.Succeeded)
            {
                //-- Colocamos el texto reconocido en la TextBox que tiene el texto a traducir
                this.txtOriginal.Text = result.RecognitionResult.Text;
                //-- Formando la URL de consulta a utilizar
                string uri = "";
                uri = String.Format("https://translate.yandex.net/api/v1.5/tr/translate?key=trnsl.1.1.20130705T161953Z.6bf2c28742bb8e29.605b3c4e4d852751bafc35a06b04713c60ac2d63&lang=en&text=" + this.txtOriginal.Text + "&text=" + this.txtOriginal.Text);
                //-- Consultamos el API
                WebClient clienteProxy;
                clienteProxy = new WebClient();
                clienteProxy.DownloadStringCompleted += (s, a) =>
                {
                    if (a.Error == null && !a.Cancelled)
                    {
                        var api_result = a.Result;
                        string resultado = api_result;
                        //-- Pasamos los resultados a un tipo de doc XML
                        var doc = XDocument.Parse(api_result);
                        var query = from c in doc.Descendants("Translation")
                                    select new Translation()
                                    {
                                        text = (string)c.Element("text")
                                    };
                        var results = query.ToList();

                        //-- Obtenemoos el texto y lo pasamos al objeto
                        Translation texto = new Translation();
                        texto = results.FirstOrDefault();
                        //-- Colocamos el texto traducido en el TextBox que contendrá la traducción
                        this.txtTraduccion.Text = texto.text;

                        //-- Llamamos al metodo para que el texto traducido sea hablado por el telefono
                        this.SpeakText();

                    }
                };
                clienteProxy.DownloadStringAsync(new Uri(uri, UriKind.Absolute));

                
            }
        }

        private async void SpeakText() 
        {
            //-- Objeto para el manejo de voz del telefono
            SpeechSynthesizer speech = new SpeechSynthesizer();
            //-- Bucamos el lenguaje con el cual sera hablado el texto
            IEnumerable<VoiceInformation> voices = from voice in InstalledVoices.All
                                                   where voice.Language == "en-US"
                                                   select voice;
            speech.SetVoice(voices.ElementAt(0));
            //-- Ejecutamos el metodo para que el telefono diga el texto
            await speech.SpeakTextAsync(this.txtTraduccion.Text);
        }

        #endregion

        }

}