using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
#if DEV
[assembly: AssemblyTitle("专注小站 (dev)")]
#else
[assembly: AssemblyTitle("专注小站")]
#endif
[assembly: AssemblyDescription("本地任务计时、番茄钟、分阶段挑战与可选 AI 拆解、个性化探索")]
[assembly: AssemblyVersion("2.0.10.0")]
namespace LittleFocus {
 static class Program {
  [STAThread] static void Main(){
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   string directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LittleFocusDesktop");
#if UI_TEST
   directory=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"testdata");
#elif DEV
   directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LittleFocusDesktop-dev");
#endif
   bool created;using(var mutex=new Mutex(true,"Local\\LittleFocusDesktop"+directory.GetHashCode(),out created)){
    if(!created){MessageBox.Show(Brand.AppName+"已经运行，请从系统托盘打开。",Brand.AppName);return;}
    try{Application.Run(new MainForm(new Store(directory)));}catch(Exception){MessageBox.Show("程序遇到问题，已保存的数据仍在本地。请重新打开；若重复出现，请提供复现步骤。",Brand.AppName);}
   }
  }
 }
}
