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
        
        public void initSpeech()
        {
            kinectSource = new KinectAudioSource();
            kinectSource.FeatureMode = true;
            kinectSource.AutomaticGainControl = false;
            kinectSource.SystemMode = SystemMode.OptibeamArrayOnly;

            var rec = (from r in SpeechRecognitionEngine.InstalledRecognizers() where r.Id == RecognizerId select r).FirstOrDefault();

            speechEngine = new SpeechRecognitionEngine(rec.Id);

            var choices = new Choices();
            choices.Add("select scalpal");
            choices.Add("select syringe");
            choices.Add("select suction");
            choices.Add("select hand");
            choices.Add("nurse scalpal");
            choices.Add("nurse syringe");
            choices.Add("nurse suction");
            choices.Add("nurse hand");
            choices.Add("show console");
            choices.Add("hide console");
            choices.Add("begin incision");
            choices.Add("end incision");

            choices.Add("inject");
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
            // used for button items
            if (e.Result.Text == "select scalpal")
            {
                speechMsg = ": Scalpal Recognized!";    
            }
            else if (e.Result.Text == "select syringe")
            {
                speechMsg = ": Syringe Recognized!";
            }
            else if (e.Result.Text == "select suction")
            {
                speechMsg = ": Suction Recognized!";
            }
            else if (e.Result.Text == "select hand")
            {
                speechMsg = ": Hand Recognized!";
            }

            // nurse items
            if (e.Result.Text == "nurse scalpal")
            {
                speechMsg = ": nurse scalpal";
            }
            else if (e.Result.Text == "nurse suction")
            {
                speechMsg = ": nurse suction";
            }
            else if (e.Result.Text == "nurse syringe")
            {
                speechMsg = ": nurse syringe";
            }
            else if (e.Result.Text == "nurse hand")
            {
                speechMsg = ": nurse hand";
            }

            if (e.Result.Text == "begin incision")
            {
                speechMsg = ": begin incision";
            }
            else if (e.Result.Text == "end incision")
            {
                speechMsg = ": end incision";
            }
            if (e.Result.Text == "show console")
            {
                speechMsg = ": show console";
            }
            else if (e.Result.Text == "hide console")
            {
                speechMsg = ": hide console";
            }

            if (e.Result.Text == "inject")
            {
                speechMsg = ": inject";
            }
        }
    }

}
