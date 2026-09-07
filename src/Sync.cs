using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace LittleFocus {
 public class SyncConfig {
  public string Url="";
  public string User="";
  public string ProtectedPassword="";
  public bool AutoSync;
  public string Password(){return String.IsNullOrEmpty(ProtectedPassword)?"":Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(ProtectedPassword),null,DataProtectionScope.CurrentUser));}
  public void SetPassword(string password){ProtectedPassword=String.IsNullOrEmpty(password)?"":Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(password),null,DataProtectionScope.CurrentUser));}
  public static SyncConfig Load(string dir){string file=Path.Combine(dir,"sync.xml");if(!File.Exists(file))return new SyncConfig();using(var f=File.OpenRead(file))return (SyncConfig)new XmlSerializer(typeof(SyncConfig)).Deserialize(f);}
  public void Save(string dir){Directory.CreateDirectory(dir);string path=Path.Combine(dir,"sync.xml"),tmp=path+".tmp";using(var f=File.Create(tmp))new XmlSerializer(typeof(SyncConfig)).Serialize(f,this);if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);}
 }
 public static class DataMerge {
  public static AppData Merge(AppData local,AppData remote){
   var result=Store.Decode(Store.Encode(local));
   var projects=result.Projects.ToDictionary(p=>p.Id);foreach(var p in remote.Projects){Project old;if(!projects.TryGetValue(p.Id,out old)||p.Deleted||(!old.Deleted&&p.UpdatedUtc>old.UpdatedUtc))projects[p.Id]=p;}result.Projects=projects.Values.ToList();
   var ideas=result.Ideas.ToDictionary(i=>i.Id);foreach(var i in remote.Ideas){BreakIdea old;if(!ideas.TryGetValue(i.Id,out old)||i.UpdatedUtc>old.UpdatedUtc)ideas[i.Id]=i;}result.Ideas=ideas.Values.ToList();
   var rows=new Dictionary<string,TimeRecord>();foreach(var r in result.Records.Concat(remote.Records)){string key=r.RunId+"|"+r.StartUtc.ToUniversalTime().Ticks+"|"+r.Day;TimeRecord old;if(!rows.TryGetValue(key,out old)||r.Seconds>old.Seconds||(Math.Abs(r.Seconds-old.Seconds)<0.00001&&r.UpdatedUtc>old.UpdatedUtc))rows[key]=r;}result.Records=rows.Values.Where(r=>!result.Projects.Any(p=>p.Deleted&&p.Id==r.ProjectId)).OrderBy(r=>r.StartUtc).ToList();var adjustments=result.ChallengeAdjustments.Concat(remote.ChallengeAdjustments).Where(a=>a!=null&&!result.Projects.Any(p=>p.Deleted&&p.Id==a.ProjectId)).GroupBy(a=>a.Id).Select(g=>g.OrderByDescending(a=>a.AtUtc).First()).ToList();result.ChallengeAdjustments=adjustments;var fresh=result.FreshContents.Concat(remote.FreshContents).Where(x=>x!=null&&!result.Projects.Any(p=>p.Deleted&&p.Id==x.ProjectId)).GroupBy(x=>x.Id).Select(g=>{var item=g.OrderByDescending(x=>x.SavedUtc).First();item.Favorite=g.Any(x=>x.Favorite);return item;}).ToList();result.FreshContents=fresh;var distractions=result.DistractionLog.Concat(remote.DistractionLog).Where(x=>x!=null&&!result.Projects.Any(p=>p.Deleted&&p.Id==x.ProjectId)).GroupBy(x=>x.Id).Select(g=>g.OrderByDescending(x=>x.AtUtc).First()).ToList();result.DistractionLog=distractions;result.AiRestLines=result.AiRestLines.Concat(remote.AiRestLines).Where(x=>!String.IsNullOrWhiteSpace(x)).Distinct().Take(500).ToList();if(remote.AiRestGeneratedUtc>result.AiRestGeneratedUtc)result.AiRestGeneratedUtc=remote.AiRestGeneratedUtc;return result;
  }
 }
 public static class WebDavSync {
  public static async Task<AppData> Run(SyncConfig config,Func<AppData> snapshot,CancellationToken cancel){
   Uri uri=Planner.Endpoint(config.Url);ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
   using(var handler=new HttpClientHandler{AllowAutoRedirect=false})using(var client=new HttpClient(handler)){
    client.Timeout=TimeSpan.FromSeconds(45);client.MaxResponseContentBufferSize=50000000;
    if(!String.IsNullOrEmpty(config.User))client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Basic",Convert.ToBase64String(Encoding.UTF8.GetBytes(config.User+":"+config.Password())));
    for(int attempt=0;attempt<3;attempt++){
     bool exists;string etag=null;AppData remote;
     using(var get=new HttpRequestMessage(HttpMethod.Get,uri)){
      get.Headers.CacheControl=new CacheControlHeaderValue{NoCache=true};
      using(var response=await client.SendAsync(get,cancel)){
       exists=response.StatusCode!=HttpStatusCode.NotFound;
       if(exists&&!response.IsSuccessStatusCode)throw new InvalidOperationException("下载同步数据失败（HTTP "+(int)response.StatusCode+"）。请检查地址和账号。");
       if(exists){if(response.Headers.ETag==null||response.Headers.ETag.IsWeak)throw new InvalidOperationException("服务器未提供强 ETag，无法安全防止覆盖其他设备的数据；已停止同步。请使用支持条件更新的 WebDAV 服务。");etag=response.Headers.ETag.ToString();remote=Store.Decode(await response.Content.ReadAsStringAsync());}
       else remote=new AppData();
      }
     }
     AppData merged=DataMerge.Merge(snapshot(),remote);
     using(var put=new HttpRequestMessage(HttpMethod.Put,uri)){
      if(exists)put.Headers.TryAddWithoutValidation("If-Match",etag);else put.Headers.TryAddWithoutValidation("If-None-Match","*");
      put.Content=new StringContent(Store.Encode(merged).Replace("encoding=\"utf-16\"","encoding=\"utf-8\""),Encoding.UTF8,"application/xml");
      using(var response=await client.SendAsync(put,cancel)){
       if(response.StatusCode==HttpStatusCode.PreconditionFailed)continue;
       if(!response.IsSuccessStatusCode)throw new InvalidOperationException("上传同步数据失败（HTTP "+(int)response.StatusCode+"）。远端目录须已存在，并允许读写。");return merged;
      }
     }
    }
    throw new InvalidOperationException("其他设备正在更新数据。本地记录安全保留，请稍后再同步。");
   }
  }
 }
}

