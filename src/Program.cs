using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
[assembly: AssemblyTitle("专注小站")]
[assembly: AssemblyDescription("本地任务计时、番茄钟、分阶段挑战与可选 AI 拆解、个性化探索")]
[assembly: AssemblyVersion("2.0.10.0")]
namespace LittleFocus {
 static class Program {
  [STAThread] static void Main(){
   Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
   string directory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"LittleFocusDesktop");
#if UI_TEST
   directory=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"testdata");
#endif
   bool created;using(var mutex=new Mutex(true,"Local\\LittleFocusDesktop"+directory.GetHashCode(),out created)){
    if(!created){MessageBox.Show("专注小站已经运行，请从系统托盘打开。","专注小站");return;}
    try{Application.Run(new MainForm(new Store(directory)));}catch(Exception){MessageBox.Show("程序遇到问题，已保存的数据仍在本地。请重新打开；若重复出现，请提供复现步骤。","专注小站");}
   }
  }
 }
}
