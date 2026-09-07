using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace LittleFocus {
 // App display name. Lives in Model.cs (not Program.cs) so every compile
 // target -- app, UI_TEST build and the UiTests harness (which has its own
 // Main and omits Program.cs) -- can reference it.
 public static class Brand {
#if DEV
  public const string AppName="专注小站 (dev)";
#else
  public const string AppName="专注小站";
#endif
 }
 public class Project {
  public string Id = Guid.NewGuid().ToString("N");
  public string Name = "";
  public string NextStep = "";
  public string SmallStep="", ResumeNote="", DoneWhen="", LastCheckpoint="";
  public DateTime LastWorkedUtc;
  public bool Enabled = true;
  public bool Completed;
  public bool Deleted;
  public bool Challenge;
  public bool AllowDistraction;
  public bool AiEnabled=false;
  public DateTime UpdatedUtc=DateTime.UtcNow;
  public int ChallengeMinutes=120;
  public bool ChallengeMinutesCustomized;
  public int PomodoroMinutes;
  public List<Stage> Stages=new List<Stage>();
  public string PlanSource="待拆解";
  public override string ToString() { return Name; }
 }
 public class Stage {
  public string Id=Guid.NewGuid().ToString("N");
  public int Remaining=100;
  public int AllocatedSeconds;
  public string Title="";
  public string Action="";
  public bool Done;
  public override string ToString(){return Title;}
 }
 public class BreakIdea {
  public string Id=Guid.NewGuid().ToString("N");
  public DateTime UpdatedUtc=DateTime.UtcNow;
  public bool Deleted;
  public string Name = "";
  public string Detail = "";
  public bool Enabled = true;
  public override string ToString() { return Name; }
 }
 public class TimeRecord {
  public string SessionId="";
  public string RunId = "";
  public string ProjectId = "";
  public string ProjectName = "";
  public string StageId="",StageName="";
  public string StageNote="";
  public string Day = "";
  public DateTime StartUtc;
  public DateTime EndUtc;
  public double Seconds;
  public string Outcome = "计时中";
  public int Tomatoes;
  public bool Challenge;
  public DateTime UpdatedUtc=DateTime.UtcNow;
 }
 public class ChallengeAdjustment {
  public string Id=Guid.NewGuid().ToString("N"),ProjectId="",ProjectName="",ProjectDescription="",StageId="",StageName="",Kind="";
  public DateTime AtUtc=DateTime.UtcNow;
  public int DeltaSeconds,PlannedRemainingPercent,GoalSeconds,ElapsedSeconds;
  public string Reason="";
 }
 public class FreshContent {
  public string Id=Guid.NewGuid().ToString("N"),ProjectId="",ProjectName="",Kind="why",Title="",Body="",English="",Explanation="",Chinese="",Phonetics="",SourceUrl="";
  public DateTime SavedUtc=DateTime.UtcNow; public bool Favorite;
 }
 public class DistractionRecord {
  public string Id=Guid.NewGuid().ToString("N"),ProjectId="",ProjectName="",Kind="why",Topic="",Title="",Body="",English="",Explanation="",Chinese="",Phonetics="",SourceUrl="";
  public DateTime AtUtc=DateTime.UtcNow; public int CharCount; public bool Favorite;
 }
 public class RuntimeEvent { public string Id=Guid.NewGuid().ToString("N"); public DateTime AtUtc=DateTime.UtcNow; public string Kind="signal",Name="",RunId="",ProjectId="",State="",Detail=""; }
 public class AppData {
  public int MiniDimPercent=25;public bool MiniAiRest=false;public int RestLineSeconds=20;public int AiRestClearDays=7;public DateTime AiRestGeneratedUtc;public string MiniRestLines="( ˘ω˘ )\n慢一点也可以\n现在不用完成任何事\n离开屏幕，松松肩\n喝口水，给自己留白\n(๑•̀ㅂ•́)و✧";public List<string> AiRestLines=new List<string>();public bool LowStimulus=false;public string LastProjectId="";
  public bool AutoResumeSuggestions=false;
  public bool DefaultAllowDistraction=true;
  public bool DefaultProjectChallenge=false;
  public int DefaultProjectMinutes=25;
  public bool CloseToTray=true;
  public int AiPreferencesRevision=0;
  public string SupportHabits="";
  public string EncouragementPersona=AiDefaults.Persona;
  public bool EncourageOnBreak=true,EncourageOnTimerEnd=true,EncourageOnTaskComplete=true;
  public int Version = 1; public bool AutoEncouragement=false; public string EncouragementPrompt=AiDefaults.Prompt;
  public string DistractionTopic=""; public bool DistractionUseProjectName=false; public int DistractionMaxChars=300; public int DistractionCooldownMinutes=10; public DateTime LastFreshAtUtc; public string LastFreshProjectId=""; public FreshContent LastFresh;
  public string SelfReviewAnalysis=""; public DateTime SelfReviewAnalysisUtc;
  public string SelfReviewPrompt="结合项目名、子任务、实际用时和用户填写的理由，寻找重复出现的启动困难、时间估计偏差和顺利推进条件。优先采用跨多次记录的证据；每条规律给出适用场景、记录依据和一个下次可直接采用的小调整。不诊断人格，不把单次状态当成稳定能力。";
  public int FocusMinutes = 25;
  public bool Sound = true;  public string SoundPath = "";  public string SoundPreset = "短双音";
  public bool Rewards = true;
  public bool UseBuiltins = true;
  public List<Project> Projects = new List<Project>();
  public List<BreakIdea> Ideas = new List<BreakIdea>();
  public List<TimeRecord> Records = new List<TimeRecord>();
  public List<ChallengeAdjustment> ChallengeAdjustments=new List<ChallengeAdjustment>();
  public List<FreshContent> FreshContents=new List<FreshContent>();
  public List<DistractionRecord> DistractionLog=new List<DistractionRecord>();
  public List<RuntimeEvent> RuntimeEvents=new List<RuntimeEvent>();
 }
 public static class AiDefaults {
  public const string Persona="你是一个懂得 ADHD 启动与恢复困难的任务伙伴。以 i-have-adhd 为主要行动和表达规则，以 adhd-friendly-skill 补充去羞耻与恢复支持。像一个了解节奏的可靠同伴，说话直接但不冷淡，有具体依据，不用空泛的夸赞。不扮演医生，不推断我的情绪或诊断。默认称呼我为‘你’，不擅自使用亲昵称呼。人格服务于我当下的状态：专注时不岔题，休息时不催工作，收尾时允许结束。";
  public const string Prompt="根据触发状态写2–4句自然中文，通常60–140字，只选一个主动作，不必逐条复述规则。\n进入休息：第一句给一个可以立即做的离开座位动作；再具体认可已记录的投入，提醒这段时间可以暂停工作，不安排下一轮任务。\n结束计时：先给保存已有内容或留一句接续线索的收尾动作；若没有成果信息，只引用真实时长，不假称已经完成工作。最后允许今天先停在这里。\n完成任务：先给一个轻量收尾动作，例如保存结果、关掉当前材料；明确认可这项任务已标记完成，不立刻塞入新任务。\n手动请求：依据已知状态给一个2分钟内能开始的选择，不推断我正焦虑或疲惫。\n需要给动作时间时写‘约30秒’或‘约1分钟’，不要把估计说成事实。多步才编号，最多3步。不要用‘你太棒了’‘继续加油’‘坚持就是胜利’作为万能结尾，不排名、不比较、不制造连续打卡压力。可以温暖，但每句话都要与这一次的状态有关。";
  public static void Migrate(AppData data){if(data.AiPreferencesRevision<1){data.AutoResumeSuggestions=false;data.AiPreferencesRevision=1;}if(String.IsNullOrWhiteSpace(data.EncouragementPersona)||data.EncouragementPersona=="温和、克制的陪伴者；具体认可投入，不催促、不夸大。")data.EncouragementPersona=Persona;if(String.IsNullOrWhiteSpace(data.EncouragementPrompt)||data.EncouragementPrompt=="用温和、具体的中文鼓励我，认可已经投入的时间，不催促、不比较。最多三句话。")data.EncouragementPrompt=Prompt;if(String.IsNullOrWhiteSpace(data.SelfReviewPrompt))data.SelfReviewPrompt="结合项目名、子任务、实际用时和用户填写的理由，寻找重复出现的启动困难、时间估计偏差和顺利推进条件。优先采用跨多次记录的证据；每条规律给出适用场景、记录依据和一个下次可直接采用的小调整。不诊断人格，不把单次状态当成稳定能力。";if(data.DefaultProjectMinutes<1)data.DefaultProjectMinutes=25;if(data.DistractionTopic=="随机十万个为什么或英文新闻"){data.DistractionTopic="根据我的兴趣，给一条轻松、有用的中文内容";data.LastFresh=null;data.LastFreshProjectId="";}}
 }
 public static class TaskSupport {
  public static int StageSeconds(Project p,Stage stage,AppData data){if(p==null||stage==null||p.Stages==null)return 0;if(stage.AllocatedSeconds>0)return stage.AllocatedSeconds;int i=p.Stages.IndexOf(stage);if(i<0)return 0;int next=i+1<p.Stages.Count?p.Stages[i+1].Remaining:0;int total=p.Challenge?p.ChallengeMinutes:(p.PomodoroMinutes>0?p.PomodoroMinutes:(data==null?25:data.FocusMinutes));return Math.Max(0,(int)Math.Round(total*60*Math.Max(0,stage.Remaining-next)/100.0));}
  public static bool EncouragementEnabled(AppData data,string trigger){return data.AutoEncouragement&&(trigger=="break"?data.EncourageOnBreak:trigger=="timer-end"?data.EncourageOnTimerEnd:trigger=="task-complete"&&data.EncourageOnTaskComplete);}
  public static string Next(Project p){if(p==null)return "点击左侧“新建”，先写下想做的一件事。";if(p.Completed)return "这项任务已经完成。需要时，可以从左侧恢复。";if(!String.IsNullOrWhiteSpace(p.SmallStep))return p.SmallStep;var stage=p.Stages.FirstOrDefault(s=>!s.Done);if(stage!=null)return String.IsNullOrWhiteSpace(stage.Action)?"打开相关材料，先做："+stage.Title:stage.Action;if(p.Stages.Count>0)return "小步骤已经勾完。检查成果，准备好后点击“完成任务”。";return "打开与“"+p.Name+"”有关的文件，先写下一句要处理的问题。";}
  public static string Checkpoint(Project p){if(p==null)return "";if(!String.IsNullOrWhiteSpace(p.ResumeNote))return "接着这里："+p.ResumeNote;if(String.IsNullOrWhiteSpace(p.LastCheckpoint))return "";var stage=p.Stages.FirstOrDefault(s=>!s.Done)??p.Stages.LastOrDefault();if(stage!=null)return "上次停在："+stage.Title;string text=p.LastCheckpoint.StartsWith("上次停在：")?p.LastCheckpoint.Substring(5):p.LastCheckpoint;int cut=text.IndexOfAny(new[]{'，','。','；','\r','\n'});return "上次停在："+(cut>0?text.Substring(0,cut):text);}
  public static void Remember(Project p){if(p==null||p.Completed)return;p.LastCheckpoint="上次停在："+Next(p);p.LastWorkedUtc=DateTime.UtcNow;p.UpdatedUtc=DateTime.UtcNow;}
 }
 public class Store {
  public readonly string DirectoryPath;
  public string FilePath { get { return Path.Combine(DirectoryPath,"data.xml"); } }
  public string Warning = "";
  public Store(string directory) { DirectoryPath=directory; }
  public AppData Load() {
   if(!File.Exists(FilePath)) return new AppData();
   try { return Read(FilePath); }
   catch(Exception) {
    if(File.Exists(FilePath+".bak")) {
     try { var recovered=Read(FilePath+".bak"); Warning="主数据文件损坏，已从上一次备份恢复。原文件已另存，未覆盖。"; PreserveDamaged(); return recovered; } catch(Exception) { }
    }
    Warning="数据文件无法读取。已保留原文件，当前使用空白数据；可从备份恢复。";
    PreserveDamaged(); return new AppData();
   }
  }
  void PreserveDamaged() {
   File.Copy(FilePath,FilePath+".damaged-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"),false);
  }
  public static AppData Read(string path) {
   return Decode(File.ReadAllText(path));
  }
  public static string Encode(AppData data){using(var text=new StringWriter(System.Globalization.CultureInfo.InvariantCulture)){new XmlSerializer(typeof(AppData)).Serialize(text,data);return text.ToString();}}
  public static AppData Decode(string text) {
   var settings=new System.Xml.XmlReaderSettings { DtdProcessing=System.Xml.DtdProcessing.Prohibit, XmlResolver=null, MaxCharactersInDocument=100000000 };
   AppData data;
   using(var input=new StringReader(text))using(var reader=System.Xml.XmlReader.Create(input,settings)) data=(AppData)new XmlSerializer(typeof(AppData)).Deserialize(reader);
   if(data==null || data.Version!=1 || data.Projects==null || data.Ideas==null || data.Records==null) throw new InvalidDataException("不支持的数据格式");
   if(data.ChallengeAdjustments==null)data.ChallengeAdjustments=new List<ChallengeAdjustment>();if(data.FreshContents==null)data.FreshContents=new List<FreshContent>();if(data.DistractionLog==null)data.DistractionLog=new List<DistractionRecord>();if(data.RuntimeEvents==null)data.RuntimeEvents=new List<RuntimeEvent>();data.RuntimeEvents=data.RuntimeEvents.Where(x=>x!=null&&!String.IsNullOrWhiteSpace(x.Kind)&&!String.IsNullOrWhiteSpace(x.Name)).OrderBy(x=>x.AtUtc).Take(2000).ToList();
   AiDefaults.Migrate(data);
   data.DefaultProjectMinutes=Math.Max(1,Math.Min(720,data.DefaultProjectMinutes));
   data.FocusMinutes=Math.Max(1,Math.Min(240,data.FocusMinutes));
   if(data.Projects.Any(p=>p==null || String.IsNullOrWhiteSpace(p.Id) || String.IsNullOrWhiteSpace(p.Name)) || data.Projects.Select(p=>p.Id).Distinct().Count()!=data.Projects.Count) throw new InvalidDataException("任务数据无效");
   if(data.Ideas.Any(i=>i==null || String.IsNullOrWhiteSpace(i.Name))) throw new InvalidDataException("活动数据无效");
   foreach(var p in data.Projects){if(p.Stages==null)p.Stages=new List<Stage>();p.ChallengeMinutes=Math.Max(1,Math.Min(720,p.ChallengeMinutes));if(p.Stages.Any(s=>s==null || String.IsNullOrWhiteSpace(s.Title)))throw new InvalidDataException("子任务数据无效");}
   if(data.DistractionMaxChars==600)data.DistractionMaxChars=300;data.DistractionMaxChars=Math.Max(100,Math.Min(2000,data.DistractionMaxChars));data.DistractionCooldownMinutes=Math.Max(1,Math.Min(120,data.DistractionCooldownMinutes));data.RestLineSeconds=Math.Max(5,Math.Min(120,data.RestLineSeconds));data.AiRestClearDays=Math.Max(0,Math.Min(365,data.AiRestClearDays));if(data.AiRestLines==null)data.AiRestLines=new List<string>();data.AiRestLines=data.AiRestLines.Where(x=>!String.IsNullOrWhiteSpace(x)&&x.Length<=60).Distinct().Take(500).ToList();
   if(data.Records.Any(r=>r==null || r.Seconds<0 || Double.IsNaN(r.Seconds) || Double.IsInfinity(r.Seconds) || r.EndUtc<r.StartUtc || r.Tomatoes<0 || r.Tomatoes>1 || !ValidDay(r.Day))) throw new InvalidDataException("时间数据无效");
   data.Records.RemoveAll(r=>data.Projects.Any(p=>p.Deleted&&p.Id==r.ProjectId));return data;
  }
  static bool ValidDay(string s) { DateTime d;return DateTime.TryParseExact(s,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None,out d); }
  public void Save(AppData data) {
   Directory.CreateDirectory(DirectoryPath);
   string tmp=FilePath+".tmp";
   using(var stream=new FileStream(tmp,FileMode.Create,FileAccess.Write,FileShare.None)) { new XmlSerializer(typeof(AppData)).Serialize(stream,data); stream.Flush(true); }
   if(File.Exists(FilePath)) File.Replace(tmp,FilePath,FilePath+".bak",true); else File.Move(tmp,FilePath);
  }
 }
 public enum Phase { Idle, Focus, Paused, Break, Ready }
 public class FocusEngine {
  public AppData Data;
  public Phase State=Phase.Idle;
  public Project Current;
  public string RunId="";
  public double RunSeconds;
  public int GoalMinutes;
  public DateTime LastUtc;
  public DateTime BreakUntil;
  public double NextReminderSeconds;
  public bool ReminderSent;
  public int LastAward;
  public int OvertimeRequests;
  public bool ChallengeAtDeadline;
  public bool IsChallenge;
  public int ChallengeBaseMinutes;
  public double OvertimeSeconds,OvertimeStartRunSeconds,ChallengeShiftSeconds;
  public double ChallengeStartOffsetSeconds,NextBreakReminderSeconds;
  public bool BreakFromChallenge;
  public bool InOvertime { get { return IsChallenge && OvertimeSeconds>0; } }
  public List<Stage> Plan=new List<Stage>();
  public List<Stage> Reached=new List<Stage>();
  int stageIndex;
  public FocusEngine(AppData data) { Data=data; if(Data.RuntimeEvents==null)Data.RuntimeEvents=new List<RuntimeEvent>(); foreach(var r in Data.Records.Where(r=>r.Outcome=="计时中")) r.Outcome="意外退出（已保存部分）"; }
  void Log(string kind,string name,string detail="") { Data.RuntimeEvents.Add(new RuntimeEvent{AtUtc=DateTime.UtcNow,Kind=kind,Name=name,RunId=RunId,ProjectId=Current==null?"":Current.Id,State=State.ToString(),Detail=detail??""});if(Data.RuntimeEvents.Count>2000)Data.RuntimeEvents.RemoveAt(0); }
  string sessionId="";
  public double SessionSeconds{get{return String.IsNullOrEmpty(sessionId)?0:Data.Records.Where(r=>r.SessionId==sessionId).Sum(r=>r.Seconds);}}
  public void Start(Project project,DateTime now) {
   if(State!=Phase.Idle && State!=Phase.Ready) throw new InvalidOperationException("请先结束当前计时");
   if(project==null || project.Completed || project.Deleted) throw new InvalidOperationException("请选择一个未完成任务");
   if(State==Phase.Ready&&BreakFromChallenge){BreakFromChallenge=false;State=Phase.Focus;LastUtc=now;NextBreakReminderSeconds=RunSeconds+Math.Max(1,Data.FocusMinutes)*60;return;}
   if(State==Phase.Idle||String.IsNullOrEmpty(sessionId))sessionId=Guid.NewGuid().ToString("N");
   Current=project;RunId=Guid.NewGuid().ToString("N");RunSeconds=0;IsChallenge=project.Challenge;GoalMinutes=IsChallenge?project.ChallengeMinutes:(project.PomodoroMinutes>0?project.PomodoroMinutes:Data.FocusMinutes);ChallengeBaseMinutes=GoalMinutes;OvertimeSeconds=0;OvertimeStartRunSeconds=0;ChallengeShiftSeconds=0;LastUtc=now;State=Phase.Focus;ReminderSent=false;NextReminderSeconds=IsChallenge?GoalMinutes*60:InitialStageSeconds();NextBreakReminderSeconds=Math.Max(1,Data.FocusMinutes)*60;LastAward=0;OvertimeRequests=0;ChallengeAtDeadline=false;
   Log("signal","start",IsChallenge?"challenge":"pomodoro");Plan=project.Stages.ToList();stageIndex=Plan.FindIndex(s=>!s.Done);if(stageIndex<0)stageIndex=Plan.Count;Reached.Clear();ChallengeStartOffsetSeconds=0;if(IsChallenge&&stageIndex<Plan.Count){var stage=Plan[stageIndex];ChallengeStartOffsetSeconds=Plan.Take(stageIndex).Sum(s=>TaskSupport.StageSeconds(project,s,Data));double used=Data.Records.Where(r=>r.ProjectId==project.Id&&r.StageId==stage.Id).Sum(r=>r.Seconds);ChallengeStartOffsetSeconds+=Math.Min(TaskSupport.StageSeconds(project,stage,Data),Math.Max(0,used));}
  }
  public string Tick(DateTime now) {
   if(State==Phase.Focus) {
    double seconds=(now-LastUtc).TotalSeconds;
    if(seconds>120) { State=Phase.Paused;LastUtc=now;Log("signal","interrupted");return "interrupted"; }
    DateTime countedUntil=now;
    if(!IsChallenge&&!ReminderSent&&seconds>Math.Max(0,NextReminderSeconds-RunSeconds)){seconds=Math.Max(0,NextReminderSeconds-RunSeconds);countedUntil=LastUtc.AddSeconds(seconds);}
    double limit=IsChallenge&&InOvertime?OvertimeStartRunSeconds+OvertimeSeconds:ChallengeBaseMinutes*60+ChallengeShiftSeconds-ChallengeStartOffsetSeconds;if(IsChallenge&&seconds>Math.Max(0,limit-RunSeconds)){seconds=Math.Max(0,limit-RunSeconds);countedUntil=LastUtc.AddTicks((long)Math.Round(seconds*TimeSpan.TicksPerSecond));}
    if(seconds>0) { AddSpan(LastUtc,countedUntil);RunSeconds+=seconds; }
    LastUtc=now;
    if(IsChallenge) {
     double logical=ChallengeStartOffsetSeconds+RunSeconds;Reached.Clear();if(!InOvertime){while(stageIndex<Plan.Count){double boundary=Plan.Take(stageIndex+1).Sum(s=>TaskSupport.StageSeconds(Current,s,Data))+ChallengeShiftSeconds;if(logical<boundary)break;Reached.Add(Plan[stageIndex++]);}}
     if(RunSeconds>=limit){if(InOvertime)CommitOvertime();ChallengeAtDeadline=true;State=Phase.Paused;return "challenge-end";}
     if(Reached.Count>0){State=Phase.Paused;return "stage";}
     if(RunSeconds>=NextBreakReminderSeconds){State=Phase.Paused;return "challenge-break";}return null;
    }
    if(!ReminderSent && RunSeconds>=NextReminderSeconds) { ReminderSent=true;State=Phase.Paused;return "due"; }
   } else if(State==Phase.Break && now>=BreakUntil) { State=Phase.Ready;return "break-end"; }
   return null;
  }
  void AddSpan(DateTime start,DateTime end) {
   while(start<end) {
    string day=start.ToLocalTime().ToString("yyyy-MM-dd");
    DateTime boundary=start.ToLocalTime().Date.AddDays(1).ToUniversalTime();
    DateTime stop=end<boundary?end:boundary;
    if(stop<=start) stop=end;
    var stage=Current.Stages.FirstOrDefault(s=>!s.Done);string stageId=stage==null?"":stage.Id;
    TimeRecord last=Data.Records.LastOrDefault();
    if(last==null || last.RunId!=RunId || last.Day!=day || last.EndUtc!=start || last.StageId!=stageId) {
     last=new TimeRecord { SessionId=sessionId,RunId=RunId,ProjectId=Current.Id,ProjectName=Current.Name,StageId=stageId,StageName=stage==null?"":stage.Title,StageNote=stage==null?"":stage.Action,Day=day,StartUtc=start,EndUtc=start,Challenge=IsChallenge };
     Data.Records.Add(last);
    }
    last.Seconds+=(stop-start).TotalSeconds;last.EndUtc=stop;last.UpdatedUtc=DateTime.UtcNow;start=stop;
   }
  }
  public string Pause(DateTime now) { string evt=Tick(now);if(State==Phase.Focus){State=Phase.Paused;Log("signal","pause");}return evt; }
  public void Resume(DateTime now) { if(State!=Phase.Paused)return;LastUtc=now;State=Phase.Focus;Log("signal","resume"); }
  public void Snooze() { NextReminderSeconds=RunSeconds+300;ReminderSent=false; }
  public double ChallengeTotalRemaining(){double end=InOvertime?OvertimeStartRunSeconds+OvertimeSeconds:ChallengeBaseMinutes*60+ChallengeShiftSeconds-ChallengeStartOffsetSeconds;return Math.Max(0,end-RunSeconds);}
  public Stage CurrentStage(){return Current==null?null:Current.Stages.FirstOrDefault(s=>!s.Done);}
  double InitialStageSeconds(){var stage=CurrentStage();int value=TaskSupport.StageSeconds(Current,stage,Data);return value>0?value:GoalMinutes*60;}
  public double StageRemaining(){if(!IsChallenge)return Math.Max(0,NextReminderSeconds-RunSeconds);if(InOvertime)return Math.Max(0,OvertimeStartRunSeconds+OvertimeSeconds-RunSeconds);var stage=CurrentStage();if(stage==null)return ChallengeTotalRemaining();int i=Current.Stages.IndexOf(stage);double end=Current.Stages.Take(i+1).Sum(s=>TaskSupport.StageSeconds(Current,s,Data))+ChallengeShiftSeconds-ChallengeStartOffsetSeconds;return Math.Max(0,end-RunSeconds);}
  public void RefreshStageDeadline(){if(IsChallenge||Current==null)return;ReminderSent=false;NextReminderSeconds=RunSeconds+InitialStageSeconds();}
  public double PlannedStageRemaining(Stage stage){if(!IsChallenge||stage==null)return 0;if(InOvertime)return StageRemaining();int i=Current.Stages.IndexOf(stage);if(i<0)return 0;return Math.Max(0,Current.Stages.Take(i+1).Sum(s=>TaskSupport.StageSeconds(Current,s,Data))+ChallengeShiftSeconds-ChallengeStartOffsetSeconds-RunSeconds);}
  public bool AddChallengeTime(int minutes){if(!IsChallenge||minutes<1||CurrentStage()==null||CurrentStage().Done)return false;OvertimeRequests++;OvertimeStartRunSeconds=RunSeconds;OvertimeSeconds=minutes*60;ChallengeAtDeadline=false;LastUtc=DateTime.UtcNow;return true;}
  void CommitOvertime(){if(!InOvertime)return;ChallengeShiftSeconds+=Math.Max(0,Math.Min(OvertimeSeconds,RunSeconds-OvertimeStartRunSeconds));OvertimeSeconds=0;GoalMinutes=(int)Math.Ceiling((ChallengeBaseMinutes*60+ChallengeShiftSeconds)/60.0);}
  public void CompleteOvertimeStage(){CommitOvertime();ChallengeAtDeadline=false;}
  public void FinishChallenge(string outcome){if(!IsChallenge)return;Finish(outcome);State=Phase.Idle;ChallengeAtDeadline=false;}
  int Finish(string outcome) {
   var rows=Data.Records.Where(r=>r.RunId==RunId).ToList();
   foreach(var row in rows){row.Outcome=outcome;row.UpdatedUtc=DateTime.UtcNow;}
   int award=RunSeconds>=ChallengeBaseMinutes*60+ChallengeShiftSeconds && rows.Count>0?1:0;
   if(rows.Count>0)rows[rows.Count-1].Tomatoes=award;
   LastAward=award;return award;
  }
  public bool EndBreakEarly(DateTime now){if(State!=Phase.Break)return false;if(BreakFromChallenge){BreakFromChallenge=false;State=Phase.Focus;LastUtc=now;NextBreakReminderSeconds=RunSeconds+Math.Max(1,Data.FocusMinutes)*60;return true;}State=Phase.Ready;Start(Current,now);return true;}
  public bool BeginBreak(DateTime now) {
   if(State!=Phase.Focus && State!=Phase.Paused)return false;
   Tick(now);if(IsChallenge){BreakFromChallenge=true;State=Phase.Break;BreakUntil=now.AddMinutes(5);return true;}Finish("进入休息");State=Phase.Break;BreakUntil=now.AddMinutes(5);return true;
  }
  public void SkipChallengeBreak(DateTime now){if(!IsChallenge||State!=Phase.Paused)return;NextBreakReminderSeconds=RunSeconds+Math.Max(1,Data.FocusMinutes)*60;Resume(now);}
  public int Stop(DateTime now,string outcome) {
   if(State==Phase.Focus || State==Phase.Paused) { var evt=Tick(now);if(evt!="challenge-end")Finish(outcome); }
   else if(BreakFromChallenge&&(State==Phase.Break||State==Phase.Ready)){Finish(outcome);BreakFromChallenge=false;}
   else LastAward=0;
   State=Phase.Idle;Log("signal","stop",outcome);return LastAward;
  }
  public double BreakSeconds(DateTime now) {return Math.Max(0,(BreakUntil-now).TotalSeconds);}
  public void Complete(Project project,DateTime now){if(project==null)return;if(State!=Phase.Idle&&Current!=null&&Current.Id==project.Id)Stop(now,"任务完成");project.Completed=true;project.Enabled=false;project.UpdatedUtc=now;}
  public static string Duration(double seconds) { var s=(long)Math.Floor(Math.Max(0,seconds));return String.Format("{0:00}:{1:00}:{2:00}",s/3600,s/60%60,s%60); }
 }
}
