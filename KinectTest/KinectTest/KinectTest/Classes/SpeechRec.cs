using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using Microsoft.Research.Kinect.Nui;
using System.IO;

namespace KinectTest
{
    class SpeechRec
    {
        KinectAudioSource kinectSource;
        SpeechRecognitionEngine speechEngine;
        Stream stream;
        string speechMsg;
        string RecognizerId = "SR_MS_en-US_Kinect_10.0";
        public bool selected = false;


        public void initSpeech()
        {
            kinectSource = new KinectAudioSource();
            kinectSource.FeatureMode = true;
            kinectSource.AutomaticGainControl = false;
            kinectSource.SystemMode = SystemMode.OptibeamArrayOnly;

            var rec = (from r in SpeechRecognitionEngine.InstalledRecognizers() where r.Id == RecognizerId select r).FirstOrDefault();

            speechEngine = new SpeechRecognitionEngine(rec.Id);

            var choices = new Choices();
            choices.Add("scalpal");
            choices.Add("syringe");
            choices.Add("suction");
            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = rec.Culture;
            gb.Append(choices);

            var g = new Grammar(gb);

            speechEngine.LoadGrammar(g);
            speechEngine.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);
            speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            speechEngine.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            Console.WriteLine("Recognizing Speech");

            stream = kinectSource.Start();

            speechEngine.SetInputToAudioStream(stream,
                          new SpeechAudioFormatInfo(
                              EncodingFormat.Pcm, 16000, 16, 1,
                              32000, 2, null));


            speechEngine.RecognizeAsync(RecognizeMode.Multiple);
        }

        public string returnMsg()
        {
            return speechMsg;
        }

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Console.Write("\rSpeech Rejected: \t{0}", e.Result.Text);
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Text == "scalpal")
            {
                speechMsg = ": Scalpal Selected!";
                selected = true;    
            }
            else if (e.Result.Text == "syringe")
            {
                speechMsg = ": Syringe Selected!";
            }
            else if (e.Result.Text == "suction")
            {
                speechMsg = ": Suction Selected!";
            }
            Console.Write("\rSpeech Recognized: \t{0} \n", e.Result.Text);
        }
    }

}
