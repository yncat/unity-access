using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.EventSystems;
using System.Reflection;
[InitializeOnLoad]
public class UnityAccessibility{
#if UNITY_EDITOR_WIN
static int c_count=0;
static int m_count=0;
		static AudioClip errorSound = null;
		static AudioClip enterPlayModeSound = null;
		static AudioClip leavePlayModeSound = null;
		static AudioClip compilingSound = null;
		static AudioClip compiledSound = null;
static bool compilingState=false;
static bool isPlaying = false;
static string logfile_path="log.txt";
static int speech_pctalker=1;
static int speech_NVDA=0;
static int pctalker_active, NVDA_active;

//DLL imports
//NVDA
[DllImport("nvdaControllerClient")]
public static extern int nvdaController_testIfRunning();
[DllImport("nvdaControllerClient", CharSet = CharSet.Auto)]
public static extern int nvdaController_speakText(string text);
[DllImport("nvdaControllerClient")]
public static extern int nvdaController_cancelSpeech();

//pctalker
[DllImport("PCTKUSR")]
public static extern int PCTKStatus();
[DllImport("PCTKUSR")]
public static extern void PCTKVReset();
[DllImport("PCTKUSR", CharSet = CharSet.Auto)]
public static extern int PCTKPReadW(string lpszString,int priority,int analyze);
const int TTSPRIORITY_HIGH=4;

static UnityAccessibility(){
LoadSettings();
pctalker_active=PCTKStatus();
NVDA_active=nvdaController_testIfRunning();
Application.logMessageReceivedThreaded+=OnUnityLogCallback;
EditorApplication.playmodeStateChanged+=StateChange;
EditorApplication.update+=Update;
//			SceneView.onSceneGUIDelegate += OnSceneViewCallback;
errorSound=AssetDatabase.LoadAssetAtPath("Assets/Editor/UnityAccessibilitySND/error.wav",typeof(AudioClip)) as AudioClip;
enterPlayModeSound=AssetDatabase.LoadAssetAtPath("Assets/Editor/UnityAccessibilitySND/start.wav",typeof(AudioClip)) as AudioClip;
leavePlayModeSound=AssetDatabase.LoadAssetAtPath("Assets/Editor/UnityAccessibilitySND/exit.wav",typeof(AudioClip)) as AudioClip;
compilingSound=AssetDatabase.LoadAssetAtPath("Assets/Editor/UnityAccessibilitySND/compiling.wav",typeof(AudioClip)) as AudioClip;
compiledSound=AssetDatabase.LoadAssetAtPath("Assets/Editor/UnityAccessibilitySND/compiled.wav",typeof(AudioClip)) as AudioClip;
if(isFirstRun()){
 voice("Accessibility enabled");
}else{
voice("Done");
}
}

	~UnityAccessibility(){
		voice("Accessibility unloaded");
	}
private static bool isFirstRun(){
bool ret=false;
if(!System.IO.File.Exists("Temp/UnityAccessibility-loaded")){
ret=true;
System.IO.File.Create("Temp/UnityAccessibility-loaded");
}
return ret;
}

private static void LoadSettings(){
if(!System.IO.File.Exists("Assets/Editor/UnityAccessibilitySettings.ini")){
string ini="";
ini+="speech_pctalker="+speech_pctalker+"\n";
ini+="speech_NVDA="+speech_NVDA+"\n";
System.IO.File.AppendAllText("UnityAccessibilitySettings.ini",ini);
return;
}

string read=System.IO.File.ReadAllText("Assets/Editor/UnityAccessibilitySettings.ini");
string [] lines=read.Split('\n');
foreach(string line in lines){
if(line=="") continue;
int equal=line.IndexOf("=");

string name=line.Substring(0,equal);
string value=line.Substring(equal+1);
switch(name){
case "speech_pctalker":
speech_pctalker=int.Parse(value);
break;
case "speech_NVDA":
speech_NVDA=int.Parse(value);
break;
}
}
}

		private static void OnUnityLogCallback(string logString, string stackTrace, LogType type){
System.IO.File.AppendAllText(logfile_path,type.ToString()+" : "+logString+'\n');
if (type == LogType.Error){
 PlayClip(errorSound);
voice(logString);
}
}

static public void StateChange(){
if (isPlaying && !EditorApplication.isPlaying){
 PlayClip(leavePlayModeSound);
isPlaying=false;
}
if (!isPlaying && EditorApplication.isPlaying){
 PlayClip(enterPlayModeSound);
isPlaying=true;
}
return;
}

private static void voice(string str){
bool read=false;
if(speech_pctalker==1 && pctalker_active==1){
read=true;
PCTKPReadW(str,TTSPRIORITY_HIGH,1);
}
if(speech_NVDA==1 && NVDA_active==1){
read=true;
nvdaController_speakText(str);
}
if(!read) GUIUtility.systemCopyBuffer=str;
}

		public static void PlayClip(AudioClip clip){
if(clip==null) return;
var type=typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
var method=type.GetMethod("PlayClip",BindingFlags.Static|BindingFlags.Public,null,new System.Type[]{typeof(AudioClip)},null);
method.Invoke(null,new object[]{clip});
		}

static void OnSceneViewCallback(SceneView sceneView){
}

public static void Update(){
if(!compilingState && EditorApplication.isCompiling){
compilingState=true;
voice("Compiling...");
PlayClip(compilingSound);
c_count=0;
}
if(compilingState && !EditorApplication.isCompiling){
compilingState=false;
voice("Fix the errors before running your game");
}
if(compilingState){
c_count++;
if(c_count==100){
c_count=0;
voice("Compiling...");
PlayClip(compilingSound);
}
}
}
#endif
}
