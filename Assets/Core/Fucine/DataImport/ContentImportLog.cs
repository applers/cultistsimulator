﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Noon;

namespace Assets.Core.Fucine
{
    public class ContentImportLog
    {
        private IList<NoonLogMessage> _contentImportMessages= new List<NoonLogMessage>();

        public void LogProblem(string problemDesc)
        {
            _contentImportMessages.Add(new NoonLogMessage(problemDesc));
        }

        public void LogWarning(string desc)
        {
            _contentImportMessages.Add(new NoonLogMessage(desc, 1));
        }

        public void LogInfo(string desc)
        {
            _contentImportMessages.Add(new NoonLogMessage(desc,0));
        }

        public IList<NoonLogMessage> GetMessages()
        {
            return new List<NoonLogMessage>(_contentImportMessages);

        }
    }
}
