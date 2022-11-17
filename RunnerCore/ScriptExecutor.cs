﻿using RunnerCore.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace RunnerCore
{
    public class ScriptExecutor
    {
        private static readonly List<bool> usedFiles = new List<bool>();

        private readonly ScriptLines currentScript;
        private readonly RunnerConfig config;

        public ScriptExecutor(ScriptLines currentScript, RunnerConfig config)
        {
            this.currentScript = currentScript;
            this.config = config;
        }

        public void ExecuteAsTask(object sender, EventArgs e)
        {
            Task.Run(() => Execute(sender, e));
        }

        public void Execute(object sender, EventArgs e)
        {
            var fileIndex = GetFileIndex();
            var fileName = GetFileName(fileIndex);
            var path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllLines(path, currentScript.ScriptLines);
            var process = Process.Start("powershell", path);
            process.WaitForExit();
            File.Delete(path);
            FreeFileIndex(fileIndex);
        }

        private int GetFileIndex()
        {
            lock (usedFiles)
            {
                for (int i = 0; i < usedFiles.Count; i++)
                {
                    if (!usedFiles[i])
                    {
                        usedFiles[i] = true;
                        return i;
                    }
                }

                var newIndex = usedFiles.Count;
                usedFiles.Add(true);
                return newIndex;
            }
        }

        private string GetFileName(int index)
        {
            return $"temp{index}.ps1";
        }

        private void FreeFileIndex(int index)
        {
            lock (usedFiles)
            {
                usedFiles[index] = false;
            }
        }
    }
}
