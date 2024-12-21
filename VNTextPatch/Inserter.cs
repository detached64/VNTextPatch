using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VNTextPatch
{
    public class Inserter
    {
        private readonly IScriptCollection _inputCollection;
        private readonly IScriptCollection _textCollection;
        private readonly IScriptCollection _outputCollection;

        private readonly IScript _inputOutputScript;
        private readonly IScript _textScript;

        public Inserter(IScriptCollection inputCollection, IScriptCollection textCollection, IScriptCollection outputCollection)
        {
            _inputCollection = inputCollection;
            _textCollection = textCollection;
            _outputCollection = outputCollection;

            if (inputCollection.GetType() != outputCollection.GetType())
            {
                throw new ArgumentException("Input and output collections must have the same type");
            }

            _inputOutputScript = inputCollection.GetTemporaryScript();
            _textScript = textCollection.GetTemporaryScript();
        }

        public ILineStatistics Statistics
        {
            get { return _textScript as ILineStatistics; }
        }

        public void InsertOne(string inputScriptName, string textScriptName, string outputScriptName)
        {
            Console.Write($"{inputScriptName}...");

            try
            {
                if (!_inputCollection.Exists(inputScriptName))
                {
                    Console.Write($"{inputScriptName} does not exist in {_inputCollection.Name}");
                    return;
                }
                if (!_textCollection.Exists(textScriptName))
                {
                    Console.Write($"{textScriptName} does not exist in {_textCollection.Name}");
                    return;
                }

                _textScript.Load(new ScriptLocation(_textCollection, textScriptName));
                IEnumerable<ScriptString> strings = _textScript.GetStrings();

                _inputOutputScript.Load(new ScriptLocation(_inputCollection, inputScriptName));

                _outputCollection.Add(outputScriptName);
                _inputOutputScript.WritePatched(strings, new ScriptLocation(_outputCollection, outputScriptName));

                Console.Write("Done");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"Error: {ex.Message}. Skip...");
                Console.ResetColor();
            }
            finally
            {
                Console.WriteLine();
            }
        }

        public void InsertAll()
        {
            foreach (string inputScriptName in _inputCollection.Scripts)
            {
                string textScriptName;
                if (!string.IsNullOrEmpty(_inputOutputScript.Extension))
                {
                    textScriptName = Path.ChangeExtension(inputScriptName, _textScript.Extension);
                }
                else
                {
                    textScriptName = inputScriptName + _textScript.Extension;
                }

                if (_textCollection.Exists(textScriptName))
                {
                    InsertOne(inputScriptName, textScriptName, inputScriptName);
                }
                else
                {
                    _outputCollection.Add(inputScriptName, new ScriptLocation(_inputCollection, inputScriptName));
                    AddInputScriptMessageCount(inputScriptName);
                }
            }
        }

        private void AddInputScriptMessageCount(string scriptName)
        {
            if (Statistics == null)
            {
                return;
            }

            _inputOutputScript.Load(new ScriptLocation(_inputCollection, scriptName));
            Statistics.Total += _inputOutputScript.GetStrings().Count(s => s.Type == ScriptStringType.Message);
        }
    }
}
