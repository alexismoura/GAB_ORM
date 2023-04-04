using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GAB.Doc
{
    class Program
    {
        /// <summary>
        /// Retorna o diretório raiz da aplicação
        /// </summary>
        public static string LocalPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0].FullyQualifiedName);
            }
        }

        static void Main(string[] args)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = Path.Combine(LocalPath,"docu.exe");
            startInfo.Arguments = String.Format(@"{0} {1}",
                                    Path.Combine(LocalPath, "GAB.dll"),
                                    Path.Combine(LocalPath, "GAB.xml")
                                   );
            process.StartInfo = startInfo;
            process.Start();
            Console.ReadLine();
        }
    }
}
