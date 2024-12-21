using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VNTextPatch.Scripts.AdvHd;
using VNTextPatch.Scripts.ArcGameEngine;
using VNTextPatch.Scripts.Artemis;
using VNTextPatch.Scripts.Ethornell;
using VNTextPatch.Scripts.Kirikiri;
using VNTextPatch.Scripts.Majiro;
using VNTextPatch.Scripts.Mware;
using VNTextPatch.Scripts.Propeller;
using VNTextPatch.Scripts.RealLive;
using VNTextPatch.Scripts.ShSystem;
using VNTextPatch.Scripts.Silkys;
using VNTextPatch.Scripts.Softpal;
using VNTextPatch.Scripts.SystemNnn;
using VNTextPatch.Scripts.TmrHiroAdvSystem;
using VNTextPatch.Scripts.Yuris;

namespace VNTextPatch.Scripts
{
    public class FolderScriptCollection : IScriptCollection
    {
        private static readonly IScript[] TemporaryScripts;

        static FolderScriptCollection()
        {
            TemporaryScripts =
                new IScript[]
                {
                    new AdvHdScript(),
                    new ArtemisAsbScript(),
                    new ArtemisAstScript(),
                    new ArtemisTxtScript(),
                    new AgeScript(),
                    new CatSystemScript(),
                    new CSystemScript(),
                    new EthornellScript(),
                    new JsonScript(),
                    new KaguyaScript(),
                    new KirikiriKsScript(),
                    new KirikiriScnScript(),
                    new KirikiriSocScript(),
                    new KirikiriTjsScript(),
                    new MajiroScript(),
                    new MusicaScript(),
                    new MwareScript(),
                    new PropellerScript(),
                    new QlieScript(),
                    new RealLiveScript(),
                    new RenpyScript(),
                    new ShSystemScript(),
                    new SilkysMapScript(),
                    new SilkysMesScript(),
                    new SoftpalScript(),
                    new SystemNnnDevScript(),
                    new SystemNnnReleaseScript(),
                    new TmrHiroAdvSystemCodeScript(),
                    new TmrHiroAdvSystemTextScript(),
                    new WhaleScript(),
                    new YurisScript()
                };
        }

        public FolderScriptCollection(string folderPath, string extension, string format = null)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"{folderPath} does not exist");
            }

            FolderPath = folderPath;
            Extension = extension ?? string.Empty;
            Format = format;
        }

        public string Name
        {
            get { return FolderPath; }
        }

        public string FolderPath
        {
            get;
        }

        public string Extension
        {
            get;
        }

        public string Format
        {
            get;
        }

        public IScript GetTemporaryScript()
        {
            IScript script;
            if (Format != null)
            {
                string typeName = Format + "Script";
                script = TemporaryScripts.FirstOrDefault(f => f.GetType().Name.Equals(typeName, StringComparison.InvariantCultureIgnoreCase));
                if (script == null)
                {
                    throw new NotSupportedException($"Format {Format} is not supported");
                }
            }
            else
            {
                script = TemporaryScripts.FirstOrDefault(f => !string.IsNullOrEmpty(f.Extension) && f.Extension.Equals(Extension, StringComparison.InvariantCultureIgnoreCase));
                if (script == null)
                {
                    throw new NotSupportedException($"Extension {Extension} is not supported");
                }
            }
            return script;
        }

        public IEnumerable<string> Scripts
        {
            get
            {
                int folderPathLength = FolderPath.Length;
                if (!Name.EndsWith("\\"))
                {
                    folderPathLength++;
                }

                return Directory.EnumerateFiles(Name, "*" + Extension, SearchOption.AllDirectories)
                                .Select(f => f.Substring(folderPathLength));
            }
        }

        public bool Exists(string scriptName)
        {
            return File.Exists(Path.Combine(FolderPath, scriptName));
        }

        public void Add(string scriptName)
        {
            string filePath = Path.Combine(FolderPath, scriptName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.Create(filePath).Close();
        }

        public void Add(string scriptName, ScriptLocation copyFrom)
        {
            string sourceFilePath = copyFrom.ToFilePath();
            string destFilePath = Path.Combine(FolderPath, copyFrom.ScriptName);
            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
            File.Copy(sourceFilePath, destFilePath, true);
        }

        public override string ToString()
        {
            return FolderPath;
        }
    }
}
