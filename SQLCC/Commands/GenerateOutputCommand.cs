using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLCC.Core;
using SQLCC.Core.Interfaces;
using SQLCC.Core.Objects;

namespace SQLCC.Commands
{
    public class GenerateOutputCommand : ICommand
    {
        private DbProvider _dbProvider;
        private DbTraceCodeFormatter _dbCodeFormatter;
        private HighlightCodeProvider _codeHighlighter;
        private OutputProvider _outputProvider;
        private string _traceName;

        public GenerateOutputCommand(DbProvider dbProvider, DbTraceCodeFormatter dbCodeFormatter,
            HighlightCodeProvider codeHighlighter, OutputProvider outputProvider, string traceName)
        {
            _dbProvider = dbProvider;
            _dbCodeFormatter = dbCodeFormatter;
            _codeHighlighter = codeHighlighter;
            _outputProvider = outputProvider;
            _traceName = traceName;
        }

        public void Execute()
        {
            Console.WriteLine("Generating results...");

            var codeCoverageProcessor = new CodeCoverageProcessor(_dbCodeFormatter, _codeHighlighter);
            var codeCover = new DbCodeCoverage();

            Console.WriteLine("    Getting all database objects...");
            codeCover.Name = _traceName;
            codeCover.TotalObjects = _dbProvider.GetAllObjects();
            codeCover.TraceCodeSegments = _dbProvider.GetTraceCodeSegments(_traceName);

            Console.WriteLine("    Parsing coverage for {0} database objects...", codeCover.TotalObjects.Count);
            codeCoverageProcessor.ProcessAllCoverage(codeCover);

            Console.WriteLine("    Saving output results...");
            _outputProvider.SetUp(_traceName);
            _outputProvider.SaveResults(codeCover);

            Console.WriteLine("Results generated.");
        }
    }
}
